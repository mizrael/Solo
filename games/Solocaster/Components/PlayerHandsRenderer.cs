using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Solo;
using Solo.Components;
using Solo.Services;
using Solocaster.Inventory;

namespace Solocaster.Components;

public class PlayerHandsRenderer : Component, IRenderable
{
    private const string TextureKeyEmpty = "empty";
    private const string TexturePathPrefix = "player/hand_";
    private const float TransitionSpeed = 6.67f;
    private const float Scale = 0.5f;
    private const float HorizontalOffset = 300f;
    private const float LeftHandScale = 0.56f; // Scale * 1.12
    private const float LeftHandVerticalOffset = 20f;
    private const float ArmedVerticalOffset = 220f;
    private const float HideOffsetMultiplier = 0.7f;
    private const float VisibilityThreshold = 0.99f;
    private const float BobAmplitude = 6f;
    private const float BobSpeedNormal = 1.5f;
    private const float BobSpeedRunning = 3f;
    private const float LeftHandBobMultiplier = 1.15f;

    private static readonly Dictionary<string, string[]> HandVariations = new()
    {
        { "empty", Array.Empty<string>() },
        { "longsword", ["sword", "blade"] },
        { "axe", ["axe"] },
        { "morningstar", ["morningstar", "mace"] },
        { "shield", ["shield", "buckler"] }
    };

    private readonly Game _game;
    private readonly InventoryComponent _inventory;
    private readonly PlayerBrain _playerBrain;

    private Dictionary<string, Texture2D> _handTextures = new();
    private Texture2D? _rightHandTexture;
    private Texture2D? _leftHandTexture;
    private string _rightHandKey = TextureKeyEmpty;
    private string _leftHandKey = TextureKeyEmpty;

    private float _visibilityOffset = 1f; // 0 = visible, 1 = hidden below screen
    private float _bobPhase;

    public int LayerIndex { get; set; } = 10;
    public bool Hidden { get; set; }

    private bool ShouldShowHands =>
        _playerBrain.State == PlayerState.Combat ||
        _playerBrain.State == PlayerState.Running;

    public PlayerHandsRenderer(GameObject owner, Game game, InventoryComponent inventory, PlayerBrain playerBrain) : base(owner)
    {
        _game = game;
        _inventory = inventory;
        _playerBrain = playerBrain;
    }

    protected override void InitCore()
    {
        LoadTextures();
        _inventory.OnItemEquipped += OnEquipmentChanged;
        _inventory.OnItemUnequipped += OnEquipmentChanged;
        UpdateHandTextures();
    }

    private void LoadTextures()
    {
        foreach (var variation in HandVariations.Keys)
        {
            _handTextures[variation] = _game.Content.Load<Texture2D>(TexturePathPrefix + variation);
        }
    }

    private void OnEquipmentChanged(ItemInstance item, EquipSlot slot)
    {
        if (slot == EquipSlot.RightHand || slot == EquipSlot.LeftHand)
        {
            UpdateHandTextures();
        }
    }

    private void UpdateHandTextures()
    {
        var rightItem = _inventory.GetEquippedItem(EquipSlot.RightHand);
        var leftItem = _inventory.GetEquippedItem(EquipSlot.LeftHand);

        _rightHandKey = MapItemToTextureKey(rightItem);
        _leftHandKey = MapItemToTextureKey(leftItem);

        _rightHandTexture = _handTextures.GetValueOrDefault(_rightHandKey) ?? _handTextures[TextureKeyEmpty];
        _leftHandTexture = _handTextures.GetValueOrDefault(_leftHandKey) ?? _handTextures[TextureKeyEmpty];
    }

    private static string MapItemToTextureKey(ItemInstance? item)
    {
        if (item == null)
            return TextureKeyEmpty;

        var id = item.TemplateId.ToLowerInvariant();
        var name = item.Template.Name.ToLowerInvariant();

        foreach (var (textureKey, keywords) in HandVariations)
        {
            foreach (var keyword in keywords)
            {
                if (id.Contains(keyword) || name.Contains(keyword))
                    return textureKey;
            }
        }

        return TextureKeyEmpty;
    }

    protected override void UpdateCore(GameTime gameTime)
    {
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        float targetVisibility = ShouldShowHands ? 0.4f : 1f;
        _visibilityOffset = MathHelper.Lerp(_visibilityOffset, targetVisibility, TransitionSpeed * deltaTime);

        float bobSpeed = _playerBrain.State == PlayerState.Running ? BobSpeedRunning : BobSpeedNormal;
        _bobPhase += deltaTime * bobSpeed;
    }

    public void Render(SpriteBatch spriteBatch)
    {
        if (Hidden || _visibilityOffset > VisibilityThreshold)
            return;

        var viewport = _game.GraphicsDevice.Viewport;
        int hideOffset = (int)(_visibilityOffset * viewport.Height * HideOffsetMultiplier);
        float bob = MathF.Sin(_bobPhase * MathF.PI * 2) * BobAmplitude;

        // Right hand
        if (_rightHandTexture != null)
        {
            int w = (int)(_rightHandTexture.Width * Scale);
            int h = (int)(_rightHandTexture.Height * Scale);
            int armedOffset = _rightHandKey != TextureKeyEmpty ? (int)(ArmedVerticalOffset * Scale) : 0;

            int x = viewport.Width - w - (int)(HorizontalOffset * Scale);
            int y = viewport.Height - h + (int)bob + armedOffset + hideOffset;

            spriteBatch.Draw(_rightHandTexture, new Rectangle(x, y, w, h), Color.White);
        }

        // Left hand (mirrored, slightly larger/lower)
        if (_leftHandTexture != null)
        {
            int w = (int)(_leftHandTexture.Width * LeftHandScale);
            int h = (int)(_leftHandTexture.Height * LeftHandScale);
            int armedOffset = _leftHandKey != TextureKeyEmpty ? (int)(ArmedVerticalOffset * Scale) : 0;

            int x = (int)(HorizontalOffset * Scale);
            int y = viewport.Height - h + (int)(-bob * LeftHandBobMultiplier) + (int)(LeftHandVerticalOffset * Scale) + armedOffset + hideOffset;

            spriteBatch.Draw(_leftHandTexture, new Rectangle(x, y, w, h), null, Color.White, 0f, Vector2.Zero, SpriteEffects.FlipHorizontally, 0f);
        }
    }
}
