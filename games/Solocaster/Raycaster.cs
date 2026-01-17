using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Solo.Components;
using Solo.Services;
using Solo;
using Solocaster.Components;
using Solocaster.Entities;
using Solocaster.Services;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Solocaster;

public unsafe class Raycaster : IDisposable
{
    private readonly Color[][] _rotatedWallSpriteData;
    private readonly GCHandle[] _wallSpriteHandles;
    private readonly uint*[] _wallSpritePointers;

    private readonly Color[][] _rotatedDoorSpriteData;
    private readonly GCHandle[] _doorSpriteHandles;
    private readonly uint*[] _doorSpritePointers;

    private readonly int _texWidth;
    private readonly int _texHeight;
    private readonly int _mask;

    // Pre-computed floor/ceiling data for performance - pinned for direct pointer access
    private readonly float[] _floorRowDistances;
    private readonly float[] _ceilingRowDistances;
    private readonly uint[] _floorShadingFactors;    // Pre-multiplied as 8.8 fixed point
    private readonly uint[] _ceilingShadingFactors;  // Pre-multiplied as 8.8 fixed point

    // Pinned handles and pointers for zero-overhead access
    private readonly GCHandle _floorRowDistancesHandle;
    private readonly GCHandle _ceilingRowDistancesHandle;
    private readonly GCHandle _floorShadingHandle;
    private readonly GCHandle _ceilingShadingHandle;
    private readonly float* _floorRowDistancesPtr;
    private readonly float* _ceilingRowDistancesPtr;
    private readonly uint* _floorShadingPtr;
    private readonly uint* _ceilingShadingPtr;
    private readonly uint* _floorCeilTexPtr;  // Cached texture pointer
    private readonly float _texWidthF;        // Pre-converted to float

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
        Level level,
        int screenWidth,
        int screenHeight)
    {
        _frameWidth = screenWidth;
        _frameHeight = screenHeight;

        _map = level.Map;

        FrameBuffer = new Color[screenWidth * screenHeight];
        _zBuffer = new float[screenHeight];
        _spriteTextureCache = new Dictionary<Texture2D, (Color[], GCHandle)>();

        // Assume all sprites are the same size (first sprite's bounds)
        var wallSprites = level.WallSprites;
        _texWidth = wallSprites[0].Bounds.Width;
        _texHeight = wallSprites[0].Bounds.Height;
        _mask = _texWidth - 1;

        // Initialize wall sprites
        _rotatedWallSpriteData = new Color[wallSprites.Length][];
        _wallSpriteHandles = new GCHandle[wallSprites.Length];
        _wallSpritePointers = new uint*[wallSprites.Length];

        for (int i = 0; i < wallSprites.Length; i++)
        {
            var sprite = wallSprites[i];

            var spriteData = new Color[sprite.Bounds.Width * sprite.Bounds.Height];
            sprite.Texture.GetData(0, sprite.Bounds, spriteData, 0, spriteData.Length);

            _rotatedWallSpriteData[i] = spriteData.Rotate90(sprite.Bounds.Width, sprite.Bounds.Height);
            _wallSpriteHandles[i] = GCHandle.Alloc(_rotatedWallSpriteData[i], GCHandleType.Pinned);
            _wallSpritePointers[i] = (uint*)_wallSpriteHandles[i].AddrOfPinnedObject();
        }

        // Initialize door sprites
        var doorSprites = level.DoorSprites;
        _rotatedDoorSpriteData = new Color[doorSprites.Length][];
        _doorSpriteHandles = new GCHandle[doorSprites.Length];
        _doorSpritePointers = new uint*[doorSprites.Length];

        for (int i = 0; i < doorSprites.Length; i++)
        {
            var sprite = doorSprites[i];

            var spriteData = new Color[sprite.Bounds.Width * sprite.Bounds.Height];
            sprite.Texture.GetData(0, sprite.Bounds, spriteData, 0, spriteData.Length);

            _rotatedDoorSpriteData[i] = spriteData.Rotate90(sprite.Bounds.Width, sprite.Bounds.Height);
            _doorSpriteHandles[i] = GCHandle.Alloc(_rotatedDoorSpriteData[i], GCHandleType.Pinned);
            _doorSpritePointers[i] = (uint*)_doorSpriteHandles[i].AddrOfPinnedObject();
        }

        // Pre-compute floor/ceiling row distances and shading factors
        _floorRowDistances = new float[screenWidth];
        _ceilingRowDistances = new float[screenWidth];
        _floorShadingFactors = new uint[screenWidth];
        _ceilingShadingFactors = new uint[screenWidth];

        float posZ = screenWidth * 0.5f;
        float centerOffset = screenWidth * 0.5f;

        for (int x = 0; x < screenWidth; x++)
        {
            float p = x - centerOffset;
            if (MathF.Abs(p) < 0.001f)
            {
                _floorRowDistances[x] = 0;
                _ceilingRowDistances[x] = 0;
                _floorShadingFactors[x] = 256;  // 1.0 in 8.8 fixed point
                _ceilingShadingFactors[x] = 256;
            }
            else
            {
                _floorRowDistances[x] = posZ / p;
                _ceilingRowDistances[x] = -posZ / p;
                // Convert to 8.8 fixed point (0-256 range)
                _floorShadingFactors[x] = (uint)(CalculateShadingFactor(MathF.Abs(_floorRowDistances[x])) * 256);
                _ceilingShadingFactors[x] = (uint)(CalculateShadingFactor(MathF.Abs(_ceilingRowDistances[x])) * 256);
            }
        }

        // Pin arrays and store pointers for zero-overhead access
        _floorRowDistancesHandle = GCHandle.Alloc(_floorRowDistances, GCHandleType.Pinned);
        _ceilingRowDistancesHandle = GCHandle.Alloc(_ceilingRowDistances, GCHandleType.Pinned);
        _floorShadingHandle = GCHandle.Alloc(_floorShadingFactors, GCHandleType.Pinned);
        _ceilingShadingHandle = GCHandle.Alloc(_ceilingShadingFactors, GCHandleType.Pinned);

        _floorRowDistancesPtr = (float*)_floorRowDistancesHandle.AddrOfPinnedObject();
        _ceilingRowDistancesPtr = (float*)_ceilingRowDistancesHandle.AddrOfPinnedObject();
        _floorShadingPtr = (uint*)_floorShadingHandle.AddrOfPinnedObject();
        _ceilingShadingPtr = (uint*)_ceilingShadingHandle.AddrOfPinnedObject();

        // Cache floor/ceiling texture pointer and float texture width
        _floorCeilTexPtr = _wallSpritePointers[0];
        _texWidthF = _texWidth;
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

                    if (mapY < 0 || mapY >= _map.Cells.Length || mapX < 0 || mapX >= _map.Cells[mapY].Length)
                    {
                        hit = true;
                        break;
                    }

                    var cell = _map.Cells[mapY][mapX];
                    if (cell == TileTypes.Floor || cell == TileTypes.StartingPosition)
                        continue;

                    if (cell == TileTypes.DoorVertical || cell == TileTypes.DoorHorizontal)
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

                    // Determine if viewing from front based on door orientation
                    // Vertical door: front from E-W (doorSide == 0)
                    // Horizontal door: front from N-S (doorSide == 1)
                    bool isViewingFromFront = doorHit.IsVertical ? (doorSide == 0) : (doorSide == 1);

                    // Apply door offset only when viewing from front
                    if (isViewingFromFront)
                    {
                        doorWallX -= doorHit.OpenAmount;
                    }

                    // Update z-buffer:
                    // - When viewing from front: only if ray hits visible door portion (not the opening)
                    // - When viewing from side: always (door frame is solid)
                    bool shouldUpdateZBuffer = !isViewingFromFront || (doorWallX >= 0 && doorWallX <= 1);
                    if (shouldUpdateZBuffer && doorPerpWallDist < _zBuffer[y])
                    {
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

        // Determine if viewing from front or side based on door orientation
        // Vertical door (spans N-S): front view from E-W (side == 0)
        // Horizontal door (spans E-W): front view from N-S (side == 1)
        bool isViewingFromFront = door.IsVertical ? (side == 0) : (side == 1);

        float shadingFactor = CalculateShadingFactor(perpWallDist);

        if (isViewingFromFront)
        {
            // Viewing door from front - show door texture with sliding
            float doorOffset = door.OpenAmount;
            wallY -= doorOffset;

            // Only render the visible part of the door
            if (wallY >= 0 && wallY <= 1)
            {
                UpdateRow(side, drawStart, drawEnd, rayDirX, rayDirY, lineWidth, columnPtr, wallY, texNum: door.SpriteIndex, shadingFactor, _doorSpritePointers);
            }
        }
        else
        {
            // Viewing door from side - show wall texture (door frame)
            // Use first wall sprite as the door frame texture
            UpdateRow(side, drawStart, drawEnd, rayDirX, rayDirY, lineWidth, columnPtr, wallY, texNum: 0, shadingFactor, _wallSpritePointers);
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
            RenderFloorCeiling(columnPtr, playerTransform, rayDirX, rayDirY, 0, drawStart, isCeiling: true);

        float wallY = (side == 0) ?
            playerTransform.World.Position.Y + perpWallDist * rayDirY :
            playerTransform.World.Position.X + perpWallDist * rayDirX;
        wallY -= MathF.Floor(wallY);

        int tileType = _map.Cells[mapY][mapX];

        // Regular wall rendering (doors are handled separately now)
        if (tileType != TileTypes.Floor && tileType != TileTypes.DoorVertical && tileType != TileTypes.DoorHorizontal && tileType != TileTypes.StartingPosition)
        {
            float shadingFactor = CalculateShadingFactor(perpWallDist);
            UpdateRow(side, drawStart, drawEnd, rayDirX, rayDirY, lineWidth, columnPtr, wallY, texNum: tileType, shadingFactor, _wallSpritePointers);
        }

        // Render floor (from wall end to bottom of screen)
        int floorStart = drawEnd + 1;
        int floorCount = _frameWidth - floorStart;
        if (floorCount > 0)
            RenderFloorCeiling(columnPtr, playerTransform, rayDirX, rayDirY, floorStart, _frameWidth, isCeiling: false);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void RenderFloorCeiling(
        uint* columnPtr,
        TransformComponent playerTransform,
        float rayDirX,
        float rayDirY,
        int startX,
        int endX,
        bool isCeiling)
    {
        // Use cached pointers - no array access overhead
        uint* texturePtr = _floorCeilTexPtr;
        float* rowDistPtr = isCeiling ? _ceilingRowDistancesPtr : _floorRowDistancesPtr;
        uint* shadingPtr = isCeiling ? _ceilingShadingPtr : _floorShadingPtr;

        float posX = playerTransform.World.Position.X;
        float posY = playerTransform.World.Position.Y;

        // Cache field access in locals
        int texWidth = _texWidth;
        int mask = _mask;
        float texWidthF = _texWidthF;

        int x = startX;

        // SIMD path using AVX2 for coordinate calculation (8 pixels at a time)
        if (Avx2.IsSupported && endX - startX >= 8)
        {
            Vector256<float> vPosX = Vector256.Create(posX);
            Vector256<float> vPosY = Vector256.Create(posY);
            Vector256<float> vRayDirX = Vector256.Create(rayDirX);
            Vector256<float> vRayDirY = Vector256.Create(rayDirY);
            Vector256<float> vTexWidth = Vector256.Create(texWidthF);
            Vector256<int> vMask = Vector256.Create(mask);
            Vector256<int> vTexWidthI = Vector256.Create(texWidth);

            int simdEnd = startX + ((endX - startX) & ~7);  // Round down to multiple of 8

            // Allocate temp buffers once outside loop
            int* colors = stackalloc int[8];
            int* shading = stackalloc int[8];

            for (; x < simdEnd; x += 8)
            {
                // Load 8 row distances
                Vector256<float> vRowDist = Avx.LoadVector256(rowDistPtr + x);

                // Calculate world coordinates: posX + rowDistance * rayDirX
                Vector256<float> vFloorX = Avx.Add(vPosX, Avx.Multiply(vRowDist, vRayDirX));
                Vector256<float> vFloorY = Avx.Add(vPosY, Avx.Multiply(vRowDist, vRayDirY));

                // Convert to texture coordinates: (int)(floorX * texWidth) & mask
                Vector256<float> vTexXf = Avx.Multiply(vFloorX, vTexWidth);
                Vector256<float> vTexYf = Avx.Multiply(vFloorY, vTexWidth);

                Vector256<int> vTexX = Avx2.And(Avx.ConvertToVector256Int32(vTexXf), vMask);
                Vector256<int> vTexY = Avx2.And(Avx.ConvertToVector256Int32(vTexYf), vMask);

                // Calculate texture offsets: texY * texWidth + texX
                Vector256<int> vOffset = Avx2.Add(Avx2.MultiplyLow(vTexY, vTexWidthI), vTexX);

                // Gather texture samples using AVX2 gather
                Vector256<int> vColors = Avx2.GatherVector256((int*)texturePtr, vOffset, 4);

                // Load 8 shading factors (fixed point 8.8)
                Vector256<int> vShading = Avx.LoadVector256((int*)(shadingPtr + x));

                // Store to temp buffers for per-channel processing
                Avx.Store(colors, vColors);
                Avx.Store(shading, vShading);

                // Unrolled loop for 8 pixels
                uint* destPtr = columnPtr + x;
                for (int i = 0; i < 8; i++)
                {
                    uint color = (uint)colors[i];
                    uint sf = (uint)shading[i];

                    // Fixed-point multiply: (channel * sf) >> 8
                    uint r = (((color >> 16) & 0xFF) * sf) >> 8;
                    uint g = (((color >> 8) & 0xFF) * sf) >> 8;
                    uint b = ((color & 0xFF) * sf) >> 8;

                    destPtr[i] = 0xFF000000 | (r << 16) | (g << 8) | b;
                }
            }
        }
        // SSE2 path for 4 pixels at a time (no gather instruction, so scalar texture lookup)
        else if (Sse2.IsSupported && endX - startX >= 4)
        {
            Vector128<float> vPosX = Vector128.Create(posX);
            Vector128<float> vPosY = Vector128.Create(posY);
            Vector128<float> vRayDirX = Vector128.Create(rayDirX);
            Vector128<float> vRayDirY = Vector128.Create(rayDirY);
            Vector128<float> vTexWidth = Vector128.Create(texWidthF);
            Vector128<int> vMask = Vector128.Create(mask);

            int simdEnd = startX + ((endX - startX) & ~3);  // Round down to multiple of 4

            // Allocate temp buffers once outside loop
            int* texXBuf = stackalloc int[4];
            int* texYBuf = stackalloc int[4];

            for (; x < simdEnd; x += 4)
            {
                // Load 4 row distances
                Vector128<float> vRowDist = Sse.LoadVector128(rowDistPtr + x);

                // Calculate world coordinates: posX + rowDistance * rayDirX
                Vector128<float> vFloorX = Sse.Add(vPosX, Sse.Multiply(vRowDist, vRayDirX));
                Vector128<float> vFloorY = Sse.Add(vPosY, Sse.Multiply(vRowDist, vRayDirY));

                // Convert to texture coordinates: (int)(floorX * texWidth) & mask
                Vector128<float> vTexXf = Sse.Multiply(vFloorX, vTexWidth);
                Vector128<float> vTexYf = Sse.Multiply(vFloorY, vTexWidth);

                Vector128<int> vTexX = Sse2.And(Sse2.ConvertToVector128Int32(vTexXf), vMask);
                Vector128<int> vTexY = Sse2.And(Sse2.ConvertToVector128Int32(vTexYf), vMask);

                // Store texture coordinates for scalar lookup (no gather in SSE)
                Sse2.Store(texXBuf, vTexX);
                Sse2.Store(texYBuf, vTexY);

                // Scalar texture lookups and shading (4 pixels unrolled)
                uint* destPtr = columnPtr + x;
                uint* shadingLocal = shadingPtr + x;

                int offset0 = texYBuf[0] * texWidth + texXBuf[0];
                int offset1 = texYBuf[1] * texWidth + texXBuf[1];
                int offset2 = texYBuf[2] * texWidth + texXBuf[2];
                int offset3 = texYBuf[3] * texWidth + texXBuf[3];

                uint c0 = texturePtr[offset0], sf0 = shadingLocal[0];
                uint c1 = texturePtr[offset1], sf1 = shadingLocal[1];
                uint c2 = texturePtr[offset2], sf2 = shadingLocal[2];
                uint c3 = texturePtr[offset3], sf3 = shadingLocal[3];

                destPtr[0] = 0xFF000000 | ((((c0 >> 16) & 0xFF) * sf0 >> 8) << 16) | ((((c0 >> 8) & 0xFF) * sf0 >> 8) << 8) | (((c0 & 0xFF) * sf0) >> 8);
                destPtr[1] = 0xFF000000 | ((((c1 >> 16) & 0xFF) * sf1 >> 8) << 16) | ((((c1 >> 8) & 0xFF) * sf1 >> 8) << 8) | (((c1 & 0xFF) * sf1) >> 8);
                destPtr[2] = 0xFF000000 | ((((c2 >> 16) & 0xFF) * sf2 >> 8) << 16) | ((((c2 >> 8) & 0xFF) * sf2 >> 8) << 8) | (((c2 & 0xFF) * sf2) >> 8);
                destPtr[3] = 0xFF000000 | ((((c3 >> 16) & 0xFF) * sf3 >> 8) << 16) | ((((c3 >> 8) & 0xFF) * sf3 >> 8) << 8) | (((c3 & 0xFF) * sf3) >> 8);
            }
        }

        // Scalar fallback for remaining pixels
        for (; x < endX; x++)
        {
            float rowDistance = rowDistPtr[x];

            // Skip center pixel (distance is 0)
            if (rowDistance == 0) continue;

            // Calculate world coordinates
            float floorX = posX + rowDistance * rayDirX;
            float floorY = posY + rowDistance * rayDirY;

            // Get texture coordinates
            int texX = (int)(floorX * texWidthF) & mask;
            int texY = (int)(floorY * texWidthF) & mask;

            // Sample texture
            uint color = texturePtr[texY * texWidth + texX];

            // Apply pre-computed shading (fixed-point multiply)
            uint sf = shadingPtr[x];
            uint r = (((color >> 16) & 0xFF) * sf) >> 8;
            uint g = (((color >> 8) & 0xFF) * sf) >> 8;
            uint b = ((color & 0xFF) * sf) >> 8;

            columnPtr[x] = 0xFF000000 | (r << 16) | (g << 8) | b;
        }
    }

    private void UpdateRow(int side, int drawStart, int drawEnd, float rayDirX, float rayDirY, int lineWidth, uint* columnPtr, float wallY, int texNum, float shadingFactor, uint*[] spritePointers)
    {
        uint* texturePtr = spritePointers[texNum];

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
        public float ShadingFactor;
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
            // Floor/ceiling positions depend on distance - a 1-unit wall at this distance
            // spans from horizon-baseSpriteSize/2 to horizon+baseSpriteSize/2
            var floorPos = _frameWidth / 2 + baseSpriteSize / 2;
            var ceilingPos = _frameWidth / 2 - baseSpriteSize / 2;

            int drawStartX, drawEndX;
            switch (billboard.Anchor)
            {
                case BillboardAnchor.Bottom:
                    // Sits on floor - bottom of sprite aligns with floor at this distance
                    drawStartX = floorPos - spriteSizeX;
                    drawEndX = floorPos;
                    break;
                case BillboardAnchor.Top:
                    // Hangs from ceiling - top of sprite aligns with ceiling at this distance
                    drawStartX = ceilingPos;
                    drawEndX = ceilingPos + spriteSizeX;
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
                ShadingFactor = CalculateShadingFactor(transformY),
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
            // Pre-compute fixed-point shading factor (8.8 format)
            uint shadingFixed = (uint)(proj.ShadingFactor * 256);

            // Pre-compute texture stepping values (16.16 fixed-point)
            int texStepY = (proj.SpriteBounds.Height << 16) / Math.Max(1, proj.SpriteSizeX);
            int texStepX = (proj.SpriteBounds.Width << 16) / Math.Max(1, proj.SpriteSizeY);

            int baseTexYStart = proj.SpriteBounds.Y * proj.TexWidth;

            int* texYCoords = stackalloc int[8];
            int* texOffsets = stackalloc int[8];
            int* resultBuf = stackalloc int[8];
            int* alphaBuf = stackalloc int[8];

            for (int y = proj.DrawStartY; y < proj.DrawEndY; y++)
            {
                if (proj.Distance >= _zBuffer[y])
                    continue;

                uint* rowPtr = pixels + y * _frameWidth;

                // Calculate texX using fixed-point
                int localTexXFixed = (y - proj.ScreenY + proj.SpriteSizeY / 2) * texStepX;
                int localTexX = localTexXFixed >> 16;
                if (localTexX < 0 || localTexX >= proj.SpriteBounds.Width) continue;
                int texX = proj.SpriteBounds.X + localTexX;

                int x = proj.DrawStartX;
                int endX = proj.DrawEndX;
                int texYFixed = 0; // Starting texture Y in 16.16 fixed-point

                // AVX2 path: process 8 pixels at a time
                if (Avx2.IsSupported && endX - x >= 8)
                {
                    Vector256<int> vShadingR = Vector256.Create((int)shadingFixed);
                    Vector256<int> vShadingG = Vector256.Create((int)shadingFixed);
                    Vector256<int> vShadingB = Vector256.Create((int)shadingFixed);
                    Vector256<int> vAlphaMask = Vector256.Create(unchecked((int)0xFF000000));
                    Vector256<int> vZero = Vector256<int>.Zero;
                    Vector256<int> v255 = Vector256.Create(255);

                    for (; x <= endX - 8; x += 8)
                    {
                        for (int i = 0; i < 8; i++)
                        {
                            int localTexY = texYFixed >> 16;
                            texYCoords[i] = proj.SpriteBounds.Y + Math.Clamp(localTexY, 0, proj.SpriteBounds.Height - 1);
                            texOffsets[i] = texYCoords[i] * proj.TexWidth + texX;
                            texYFixed += texStepY;
                        }

                        // Gather 8 colors
                        Vector256<int> vOffsets = Avx.LoadVector256(texOffsets);
                        Vector256<int> vColors = Avx2.GatherVector256((int*)proj.TexturePtr, vOffsets, 4);

                        // Check alpha (skip if all transparent)
                        Vector256<int> vAlpha = Avx2.And(vColors, vAlphaMask);
                        if (Avx2.MoveMask(Avx2.CompareEqual(vAlpha, vZero).AsByte()) == -1)
                            continue;

                        // Extract and shade RGB channels
                        Vector256<int> vR = Avx2.And(Avx2.ShiftRightLogical(vColors, 16), v255);
                        Vector256<int> vG = Avx2.And(Avx2.ShiftRightLogical(vColors, 8), v255);
                        Vector256<int> vB = Avx2.And(vColors, v255);

                        // Apply shading (fixed-point multiply, then shift)
                        vR = Avx2.ShiftRightLogical(Avx2.MultiplyLow(vR, vShadingR), 8);
                        vG = Avx2.ShiftRightLogical(Avx2.MultiplyLow(vG, vShadingG), 8);
                        vB = Avx2.ShiftRightLogical(Avx2.MultiplyLow(vB, vShadingB), 8);

                        // Recombine
                        Vector256<int> vResult = Avx2.Or(Avx2.Or(
                            Avx2.Or(vAlpha, Avx2.ShiftLeftLogical(vR, 16)),
                            Avx2.ShiftLeftLogical(vG, 8)), vB);

                        // Store with alpha check per pixel
                        Avx.Store(resultBuf, vResult);
                        Avx.Store(alphaBuf, vAlpha);

                        for (int i = 0; i < 8; i++)
                        {
                            if (alphaBuf[i] != 0)
                                rowPtr[x + i] = (uint)resultBuf[i];
                        }
                    }
                }

                // Scalar fallback with loop unrolling (4 pixels)
                for (; x <= endX - 4; x += 4)
                {
                    uint c0, c1, c2, c3;
                    int ty0 = proj.SpriteBounds.Y + Math.Clamp(texYFixed >> 16, 0, proj.SpriteBounds.Height - 1);
                    texYFixed += texStepY;
                    int ty1 = proj.SpriteBounds.Y + Math.Clamp(texYFixed >> 16, 0, proj.SpriteBounds.Height - 1);
                    texYFixed += texStepY;
                    int ty2 = proj.SpriteBounds.Y + Math.Clamp(texYFixed >> 16, 0, proj.SpriteBounds.Height - 1);
                    texYFixed += texStepY;
                    int ty3 = proj.SpriteBounds.Y + Math.Clamp(texYFixed >> 16, 0, proj.SpriteBounds.Height - 1);
                    texYFixed += texStepY;

                    c0 = proj.TexturePtr[ty0 * proj.TexWidth + texX];
                    c1 = proj.TexturePtr[ty1 * proj.TexWidth + texX];
                    c2 = proj.TexturePtr[ty2 * proj.TexWidth + texX];
                    c3 = proj.TexturePtr[ty3 * proj.TexWidth + texX];

                    if ((c0 & 0xFF000000) != 0) rowPtr[x] = ApplyShadingFixed(c0, shadingFixed);
                    if ((c1 & 0xFF000000) != 0) rowPtr[x + 1] = ApplyShadingFixed(c1, shadingFixed);
                    if ((c2 & 0xFF000000) != 0) rowPtr[x + 2] = ApplyShadingFixed(c2, shadingFixed);
                    if ((c3 & 0xFF000000) != 0) rowPtr[x + 3] = ApplyShadingFixed(c3, shadingFixed);
                }

                // Handle remaining pixels
                for (; x < endX; x++)
                {
                    int localTexY = texYFixed >> 16;
                    if (localTexY >= 0 && localTexY < proj.SpriteBounds.Height)
                    {
                        int texY = proj.SpriteBounds.Y + localTexY;
                        uint color = proj.TexturePtr[texY * proj.TexWidth + texX];
                        if ((color & 0xFF000000) != 0)
                        {
                            rowPtr[x] = ApplyShadingFixed(color, shadingFixed);
                        }
                    }
                    texYFixed += texStepY;
                }
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint ApplyShadingFixed(uint color, uint shadingFixed)
    {
        if (shadingFixed >= 256) return color;
        if (shadingFixed == 0) return color & 0xFF000000;

        uint a = color & 0xFF000000;
        uint r = ((color >> 16) & 0xFF) * shadingFixed >> 8;
        uint g = ((color >> 8) & 0xFF) * shadingFixed >> 8;
        uint b = (color & 0xFF) * shadingFixed >> 8;

        return a | (r << 16) | (g << 8) | b;
    }

    public void Dispose()
    {
        foreach (var handle in _wallSpriteHandles)
        {
            if (handle.IsAllocated)
                handle.Free();
        }

        foreach (var handle in _doorSpriteHandles)
        {
            if (handle.IsAllocated)
                handle.Free();
        }

        // Free floor/ceiling pinned handles
        if (_floorRowDistancesHandle.IsAllocated)
            _floorRowDistancesHandle.Free();
        if (_ceilingRowDistancesHandle.IsAllocated)
            _ceilingRowDistancesHandle.Free();
        if (_floorShadingHandle.IsAllocated)
            _floorShadingHandle.Free();
        if (_ceilingShadingHandle.IsAllocated)
            _ceilingShadingHandle.Free();

        foreach (var (_, handle) in _spriteTextureCache.Values)
        {
            if (handle.IsAllocated)
                handle.Free();
        }
    }
}