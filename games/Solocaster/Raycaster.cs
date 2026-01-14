using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Solo.Components;
using Solo.Services;
using Solocaster.Components;
using Solocaster.Entities;
using Solocaster.Services;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Solocaster;

public unsafe class Raycaster : IDisposable
{
    private readonly Color[][] _texturesData;
    private readonly int _texWidth;
    private readonly int _texHeight;
    private readonly int _mask;
    private readonly GCHandle[] _textureHandles;
    private readonly uint*[] _texturePointers;

    private const uint ceilingColor = 0xFF383838; // Dark gray ceiling
    private const uint floorColor = 0xFF707070;   // Lighter gray floor
    private const float stepTreshold = 0.15f;

    // Distance-based shading parameters
    private const float fogDistance = 15.0f; // Distance at which objects fade to black
    private const float fogStart = 1.0f;     // Distance at which fog starts

    public readonly Color[] FrameBuffer;

    private readonly Map _map;

    private readonly int _frameWidth;
    private readonly int _frameHeight;

    private readonly float[] _zBuffer;
    private readonly Dictionary<Texture2D, (Color[] data, GCHandle handle)> _spriteTextureCache;

    public Raycaster(
        Map map,
        int screenWidth,
        int screenHeight,
        Texture2D[] textures
        )
    {
        _frameWidth = screenWidth;
        _frameHeight = screenHeight;

        _map = map;

        FrameBuffer = new Color[screenWidth * screenHeight];
        _zBuffer = new float[screenHeight];
        _spriteTextureCache = new Dictionary<Texture2D, (Color[], GCHandle)>();

        _texWidth = textures[0].Width;
        _texHeight = textures[0].Height;
        _mask = _texWidth - 1;

        _texturesData = new Color[textures.Length][];
        _textureHandles = new GCHandle[textures.Length];
        _texturePointers = new uint*[textures.Length];

        for (int i = 0; i != textures.Length; i++)
        {
            _texturesData[i] = new Color[_texWidth * _texHeight];
            textures[i].GetData(_texturesData[i]);

            // Pin the array and get a pointer
            _textureHandles[i] = GCHandle.Alloc(_texturesData[i], GCHandleType.Pinned);
            _texturePointers[i] = (uint*)_textureHandles[i].AddrOfPinnedObject();
        }
    }

    public void Update(TransformComponent playerTransform, PlayerBrain playerBrain)
    {
        fixed (Color* frameBufferPtr = FrameBuffer)
        {
            uint* pixels = (uint*)frameBufferPtr;

            for (int y = 0; y < _frameHeight; y++)
            {
                uint* columnPtr = pixels + y * _frameWidth;

                //calculate ray position and direction
                float cameraY = 2 * y / (float)_frameHeight - 1; //y-coordinate in camera space
                float rayDirX = playerTransform.World.Direction.X + playerBrain.Plane.X * cameraY;
                float rayDirY = playerTransform.World.Direction.Y + playerBrain.Plane.Y * cameraY;

                //which box of the map we're in
                int mapX = (int)playerTransform.World.Position.X;
                int mapY = (int)playerTransform.World.Position.Y;

                float deltaDistX = (rayDirX == 0) ? 1e30f : Math.Abs(1 / rayDirX);
                float deltaDistY = (rayDirY == 0) ? 1e30f : Math.Abs(1 / rayDirY);

                int stepX;
                int stepY;
                float sideDistX;
                float sideDistY;

                if (rayDirX < 0)
                {
                    stepX = -1;
                    sideDistX = (playerTransform.World.Position.X - mapX) * deltaDistX;
                }
                else
                {
                    stepX = 1;
                    sideDistX = (mapX + 1.0f - playerTransform.World.Position.X) * deltaDistX;
                }

                if (rayDirY < 0)
                {
                    stepY = -1;
                    sideDistY = (playerTransform.World.Position.Y - mapY) * deltaDistY;
                }
                else
                {
                    stepY = 1;
                    sideDistY = (mapY + 1.0f - playerTransform.World.Position.Y) * deltaDistY;
                }

                // Store door hit info for later rendering
                Door? doorHit = null;
                int doorMapX = 0, doorMapY = 0;
                int doorSide = 0;
                float doorPerpWallDist = 0;

                //DDA
                bool hit = false;
                int side = 0;
                while (!hit)
                {
                    if (sideDistX < sideDistY)
                    {
                        sideDistX += deltaDistX;
                        mapX += stepX;
                        side = 0;
                    }
                    else
                    {
                        sideDistY += deltaDistY;
                        mapY += stepY;
                        side = 1;
                    }

                    var cell = _map.Cells[mapY][mapX];
                    if (cell == TileTypes.Floor || cell == TileTypes.StartingPosition)
                        continue;

                    if (cell == TileTypes.Door)
                    {
                        var door = _map.GetDoor(mapX, mapY);
                        
                        // Store door information if it's not fully open
                        if (door != null && door.IsBlocking && doorHit == null)
                        {
                            doorHit = door;
                            doorMapX = mapX;
                            doorMapY = mapY;
                            doorSide = side;
                            doorPerpWallDist = side == 0
                                ? (sideDistX - deltaDistX)
                                : (sideDistY - deltaDistY);
                        }
                    }
                    else
                    {
                        hit = true;
                    }
                }

                float perpWallDist = side == 0
                    ? (sideDistX - deltaDistX)
                    : (sideDistY - deltaDistY);

                // Store distance for sprite depth testing (rotated: y acts as ray index)
                _zBuffer[y] = perpWallDist;

                int lineWidth = (int)(_frameWidth / perpWallDist);

                int drawStart = (-lineWidth + _frameWidth) / 2;
                if (drawStart < 0)
                    drawStart = 0;

                int drawEnd = (lineWidth + _frameWidth) / 2;
                if (drawEnd >= _frameWidth)
                    drawEnd = _frameWidth - 1;

                // Render the farthest wall first (what's behind the door)
                int length = drawEnd - drawStart + 1;
                if (length > 0)
                {
                    UpdateRow(columnPtr, playerTransform, mapX, mapY, side, drawStart, drawEnd, perpWallDist, rayDirX, rayDirY, lineWidth);
                }

                // If we hit a partially open door, render it on top
                if (doorHit != null && doorHit.IsBlocking)
                {
                    // Calculate door hit position to check if ray hits the visible door portion
                    float doorWallX = (doorSide == 0) ?
                        playerTransform.World.Position.Y + doorPerpWallDist * rayDirY :
                        playerTransform.World.Position.X + doorPerpWallDist * rayDirX;
                    doorWallX -= MathF.Floor(doorWallX);

                    // Apply door offset
                    float doorOffset = 0;
                    if (doorHit.IsVertical)
                    {
                        if (doorSide == 0) // Viewing from E-W
                            doorOffset = doorHit.OpenAmount;
                    }
                    else
                    {
                        if (doorSide == 1) // Viewing from N-S
                            doorOffset = doorHit.OpenAmount;
                    }
                    doorWallX -= doorOffset;

                    // Only update z-buffer if ray actually hits the visible door portion (not the opening)
                    if (doorWallX >= 0 && doorWallX <= 1)
                    {
                        if (doorPerpWallDist < _zBuffer[y])
                            _zBuffer[y] = doorPerpWallDist;
                    }

                    int doorLineWidth = (int)(_frameWidth / doorPerpWallDist);
                    int doorDrawStart = (-doorLineWidth + _frameWidth) / 2;
                    if (doorDrawStart < 0)
                        doorDrawStart = 0;

                    int doorDrawEnd = (doorLineWidth + _frameWidth) / 2;
                    if (doorDrawEnd >= _frameWidth)
                        doorDrawEnd = _frameWidth - 1;

                    if (doorDrawEnd - doorDrawStart + 1 > 0)
                    {
                        RenderDoor(columnPtr, playerTransform, doorMapX, doorMapY, doorSide, doorDrawStart, doorDrawEnd,
                                  doorPerpWallDist, rayDirX, rayDirY, doorLineWidth, doorHit);
                    }
                }
            }

            // Render billboards after walls
            RenderBillboards(playerTransform, playerBrain, pixels);
        }
    }

    private void RenderDoor(
        uint* columnPtr,
        TransformComponent playerTransform,
        int mapX,
        int mapY,
        int side,
        int drawStart,
        int drawEnd,
        float perpWallDist,
        float rayDirX,
        float rayDirY,
        int lineWidth,
        Door door)
    {
        float wallY = (side == 0) ?
            playerTransform.World.Position.Y + perpWallDist * rayDirY :
            playerTransform.World.Position.X + perpWallDist * rayDirX;
        wallY -= MathF.Floor(wallY);

        // Apply door sliding offset based on which side we're viewing from
        float doorOffset = 0;
        if (door.IsVertical)
        {
            if (side == 0) // Viewing from E-W
                doorOffset = door.OpenAmount;
        }
        else
        {
            if (side == 1) // Viewing from N-S
                doorOffset = door.OpenAmount;
        }

        wallY -= doorOffset;

        // Only render the visible part of the door
        if (wallY >= 0 && wallY <= 1)
        {
            float shadingFactor = CalculateShadingFactor(perpWallDist);
            UpdateRow(side, drawStart, drawEnd, rayDirX, rayDirY, lineWidth, columnPtr, wallY, texNum: 0, shadingFactor);
        }
    }

    private void UpdateRow(
        uint* columnPtr,
        TransformComponent playerTransform,
        int mapX,
        int mapY,
        int side,
        int drawStart,
        int drawEnd,
        float perpWallDist,
        float rayDirX,
        float rayDirY,
        int lineWidth)
    {
        // Render ceiling (from top of screen to wall start)
        if (drawStart > 0)
            new Span<uint>(columnPtr, drawStart).Fill(ceilingColor);

        float wallY = (side == 0) ?
            playerTransform.World.Position.Y + perpWallDist * rayDirY :
            playerTransform.World.Position.X + perpWallDist * rayDirX;
        wallY -= MathF.Floor(wallY);

        int tileType = _map.Cells[mapY][mapX];

        // Regular wall rendering (doors are handled separately now)
        if (tileType != TileTypes.Floor && tileType != TileTypes.Door && tileType != TileTypes.StartingPosition)
        {
            float shadingFactor = CalculateShadingFactor(perpWallDist);
            UpdateRow(side, drawStart, drawEnd, rayDirX, rayDirY, lineWidth, columnPtr, wallY, texNum: tileType - 1, shadingFactor);
        }

        // Render floor (from wall end to bottom of screen)
        int floorStart = drawEnd + 1;
        int floorCount = _frameWidth - floorStart;
        if (floorCount > 0)
            new Span<uint>(columnPtr + floorStart, floorCount).Fill(floorColor);
    }

    private void UpdateRow(int side, int drawStart, int drawEnd, float rayDirX, float rayDirY, int lineWidth, uint* columnPtr, float wallY, int texNum, float shadingFactor = 1.0f)
    {
        uint* texturePtr = _texturePointers[texNum];

        int texY = (int)(wallY * _texWidth);

        int flipMask = ((side == 0 && rayDirX > 0) || (side == 1 && rayDirY < 0)) ? _mask : 0;
        texY = texY ^ flipMask;

        float step = 1.0f * _texWidth / lineWidth;
        float texPos = (drawStart - _frameWidth * .5f + lineWidth * .5f) * step;

        uint* sourcePtr = texturePtr + (_texHeight * texY);
        int drawLen = drawEnd - drawStart + 1;

        uint* destPtr = columnPtr + drawStart;

        // Use simplified rendering when wall is extremely close
        if (step < stepTreshold)
        {
            int i = 0;

            // Process larger chunks when sampling the same texel multiple times
            while (i < drawLen)
            {
                int currentTexX = ((int)texPos) & _mask;
                uint color = sourcePtr[currentTexX];
                uint shadedColor = ApplyShading(color, shadingFactor);

                // Calculate how many pixels will use this same texel
                int nextTexX = ((int)(texPos + step)) & _mask;
                int pixelsToFill = (nextTexX != currentTexX) ?
                    Math.Min((int)(1.0f / step), drawLen - i) : 1;

                // Fill multiple pixels with the same color
                int fillEnd = Math.Min(i + pixelsToFill, drawLen);
                for (int j = i; j < fillEnd; j++)
                    destPtr[j] = shadedColor;

                i = fillEnd;
                texPos += step * pixelsToFill;
            }
        }
        else
        {
            // rounds down to the nearest multiple of 4
            int unrollCount = drawLen & ~3;
            int i = 0;
            for (; i < unrollCount; i += 4)
            {
                destPtr[i] = ApplyShading(sourcePtr[((int)texPos) & _mask], shadingFactor);
                texPos += step;
                destPtr[i + 1] = ApplyShading(sourcePtr[((int)texPos) & _mask], shadingFactor);
                texPos += step;
                destPtr[i + 2] = ApplyShading(sourcePtr[((int)texPos) & _mask], shadingFactor);
                texPos += step;
                destPtr[i + 3] = ApplyShading(sourcePtr[((int)texPos) & _mask], shadingFactor);
                texPos += step;
            }

            // Handle remaining pixels
            for (; i < drawLen; i++)
            {
                destPtr[i] = ApplyShading(sourcePtr[((int)texPos) & _mask], shadingFactor);
                texPos += step;
            }
        }
    }

    private float CalculateShadingFactor(float distance)
    {
        if (distance <= fogStart)
            return 1.0f;
        if (distance >= fogDistance)
            return 0.0f;

        // Linear interpolation between fogStart and fogDistance
        return 1.0f - ((distance - fogStart) / (fogDistance - fogStart));
    }

    private uint ApplyShading(uint color, float shadingFactor)
    {
        if (shadingFactor >= 1.0f)
            return color;
        if (shadingFactor <= 0.0f)
            return 0xFF000000; // Black with full alpha

        byte a = (byte)((color >> 24) & 0xFF);
        byte r = (byte)(((color >> 16) & 0xFF) * shadingFactor);
        byte g = (byte)(((color >> 8) & 0xFF) * shadingFactor);
        byte b = (byte)((color & 0xFF) * shadingFactor);

        return ((uint)a << 24) | ((uint)r << 16) | ((uint)g << 8) | b;
    }

    private uint* GetOrCacheSpriteTexture(Texture2D texture)
    {
        if (!_spriteTextureCache.TryGetValue(texture, out var cached))
        {
            var data = new Color[texture.Width * texture.Height];
            texture.GetData(data);
            var handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            cached = (data, handle);
            _spriteTextureCache[texture] = cached;
        }
        return (uint*)cached.handle.AddrOfPinnedObject();
    }

    private struct SpriteProjection
    {
        public float Distance;
        public int ScreenY;
        public int SpriteSizeY;
        public int SpriteSizeX;
        public int DrawStartY;
        public int DrawEndY;
        public int DrawStartX;
        public int DrawEndX;
        public uint* TexturePtr;
        public int TexWidth;
        public int TexHeight;
        public Rectangle SpriteBounds;
        public BillboardAnchor Anchor;
    }

    private void RenderBillboards(TransformComponent playerTransform, PlayerBrain playerBrain, uint* pixels)
    {
        var entityManager = GameServicesManager.Instance.GetRequired<EntityManager>();
        var billboards = entityManager.GetVisibleEntities(e => e.Components.Has<BillboardComponent>());

        var projections = new List<SpriteProjection>();

        foreach (var entity in billboards)
        {
            var transform = entity.Components.Get<TransformComponent>();
            var billboard = entity.Components.Get<BillboardComponent>();
            var sprite = billboard.Sprite;

            var spritePos = transform.Local.Position;

            // Relative position
            var relX = spritePos.X - playerTransform.World.Position.X;
            var relY = spritePos.Y - playerTransform.World.Position.Y;

            // Transform to camera space (accounting for 90° rotation)
            var invDet = 1.0f / (playerBrain.Plane.X * playerTransform.World.Direction.Y - playerTransform.World.Direction.X * playerBrain.Plane.Y);
            var transformX = invDet * (playerTransform.World.Direction.Y * relX - playerTransform.World.Direction.X * relY);
            var transformY = invDet * (-playerBrain.Plane.Y * relX + playerBrain.Plane.X * relY);

            if (transformY <= 0) 
                continue; // Behind camera

            var screenY = (int)((_frameHeight / 2) * (1 + transformX / transformY));
            var baseSpriteSize = Math.Abs((int)(_frameHeight / transformY));

            var spriteSizeY = (int)(baseSpriteSize * billboard.Scale.X);
            var spriteSizeX = (int)(baseSpriteSize * billboard.Scale.Y);

            var drawStartY = Math.Max(0, screenY - spriteSizeY / 2);
            var drawEndY = Math.Min(_frameHeight - 1, screenY + spriteSizeY / 2);

            // Apply anchor (rotated: X is vertical, _frameWidth/2 is horizon)
            int drawStartX, drawEndX;
            switch (billboard.Anchor)
            {
                case BillboardAnchor.Bottom:
                    // Sits on floor (below horizon)
                    drawStartX = _frameWidth / 2;
                    drawEndX = _frameWidth / 2 + spriteSizeX;
                    break;
                case BillboardAnchor.Top:
                    // Hangs from ceiling (above horizon)
                    drawStartX = _frameWidth / 2 - spriteSizeX;
                    drawEndX = _frameWidth / 2;
                    break;
                case BillboardAnchor.Center:
                default:
                    // Centered at horizon
                    drawStartX = _frameWidth / 2 - spriteSizeX / 2;
                    drawEndX = _frameWidth / 2 + spriteSizeX / 2;
                    break;
            }
            drawStartX = Math.Max(0, drawStartX);
            drawEndX = Math.Min(_frameWidth - 1, drawEndX);

            var texturePtr = GetOrCacheSpriteTexture(sprite.Texture);

            projections.Add(new SpriteProjection
            {
                Distance = transformY,
                ScreenY = screenY,
                SpriteSizeY = spriteSizeY,
                SpriteSizeX = spriteSizeX,
                DrawStartY = drawStartY,
                DrawEndY = drawEndY,
                DrawStartX = drawStartX,
                DrawEndX = drawEndX,
                TexturePtr = texturePtr,
                TexWidth = sprite.Texture.Width,
                TexHeight = sprite.Texture.Height,
                SpriteBounds = sprite.Bounds,
                Anchor = billboard.Anchor
            });
        }

        projections.Sort((a, b) => b.Distance.CompareTo(a.Distance));

        foreach (var proj in projections)
        {
            for (int y = proj.DrawStartY; y < proj.DrawEndY; y++)
            {
                if (proj.Distance >= _zBuffer[y])
                    continue;

                uint* rowPtr = pixels + y * _frameWidth;

                int localTexX = (int)((y - proj.ScreenY + proj.SpriteSizeY / 2) * proj.SpriteBounds.Width / proj.SpriteSizeY);
                if (localTexX < 0 || localTexX >= proj.SpriteBounds.Width) continue;
                int texX = proj.SpriteBounds.X + localTexX;

                for (int x = proj.DrawStartX; x < proj.DrawEndX; x++)
                {
                    int localTexY = (int)((x - proj.DrawStartX) * proj.SpriteBounds.Height / proj.SpriteSizeX);
                    if (localTexY < 0 || localTexY >= proj.SpriteBounds.Height) continue;
                    int texY = proj.SpriteBounds.Y + localTexY;

                    uint color = proj.TexturePtr[texY * proj.TexWidth + texX];
                    if ((color & 0xFF000000) != 0)
                    {
                        rowPtr[x] = color;
                    }
                }
            }
        }
    }

    public void Dispose()
    {
        foreach (var handle in _textureHandles)
        {
            if (handle.IsAllocated)
                handle.Free();
        }

        foreach (var (_, handle) in _spriteTextureCache.Values)
        {
            if (handle.IsAllocated)
                handle.Free();
        }
    }
}