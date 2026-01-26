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
    private readonly Game _game;
    private readonly InventoryComponent _inventory;
    private readonly PlayerBrain _playerBrain;

    private Dictionary<string, Texture2D> _handTextures = new();
    private Texture2D? _currentTexture;
    private string _currentWeaponKey = "empty";

    private float _bobPhase;
    private float _bobOffset;

    public int LayerIndex { get; set; } = 10;
    public bool Hidden { get; set; }

    /// <summary>
    /// Scale factor for rendering the hands. Default is 0.5 (half size).
    /// </summary>
    public float Scale { get; set; } = 0.5f;

    /// <summary>
    /// Vertical bobbing amplitude in pixels (at scale 1.0).
    /// </summary>
    public float BobAmplitude { get; set; } = 12f;

    /// <summary>
    /// Base bobbing frequency in Hz when moving at normal speed.
    /// </summary>
    public float BobFrequency { get; set; } = 2f;

    /// <summary>
    /// Multiplier to convert player move speed to bob speed.
    /// </summary>
    public float BobSpeedMultiplier { get; set; } = 8f;

    /// <summary>
    /// Bobbing frequency when idle (not moving).
    /// </summary>
    public float IdleBobFrequency { get; set; } = 1.5f;

    /// <summary>
    /// Bobbing amplitude when idle (not moving).
    /// </summary>
    public float IdleBobAmplitude { get; set; } = 4f;

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
        UpdateCurrentTexture();
    }

    private void LoadTextures()
    {
        _handTextures["empty"] = _game.Content.Load<Texture2D>("player/hands_empty");
        _handTextures["longsword"] = _game.Content.Load<Texture2D>("player/hands_longsword");
        _handTextures["axe"] = _game.Content.Load<Texture2D>("player/hands_axe");
        _handTextures["morningstar"] = _game.Content.Load<Texture2D>("player/hands_morningstar");
    }

    private void OnEquipmentChanged(ItemInstance item, EquipSlot slot)
    {
        if (slot == EquipSlot.RightHand || slot == EquipSlot.LeftHand)
        {
            UpdateCurrentTexture();
        }
    }

    private void UpdateCurrentTexture()
    {
        var weapon = _inventory.GetEquippedItem(EquipSlot.RightHand)
                  ?? _inventory.GetEquippedItem(EquipSlot.LeftHand);

        _currentWeaponKey = MapWeaponToTextureKey(weapon);
        _currentTexture = _handTextures.GetValueOrDefault(_currentWeaponKey)
                       ?? _handTextures["empty"];
    }

    private static string MapWeaponToTextureKey(ItemInstance? weapon)
    {
        if (weapon == null)
            return "empty";

        var id = weapon.TemplateId.ToLowerInvariant();
        var name = weapon.Template.Name.ToLowerInvariant();

        // Check in priority order (more specific first)
        if (id.Contains("morningstar") || name.Contains("morningstar") ||
            id.Contains("mace") || name.Contains("mace"))
            return "morningstar";

        if (id.Contains("axe") || name.Contains("axe"))
            return "axe";

        if (id.Contains("sword") || name.Contains("sword") ||
            id.Contains("blade") || name.Contains("blade"))
            return "longsword";

        return "empty";
    }

    protected override void UpdateCore(GameTime gameTime)
    {
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        float moveSpeed = _playerBrain.CurrentMoveSpeed;

        // Calculate bob speed: faster when moving, gentle idle sway when stationary
        float bobSpeed = moveSpeed > 0.001f
            ? moveSpeed * BobSpeedMultiplier * BobFrequency
            : IdleBobFrequency;

        _bobPhase += deltaTime * bobSpeed;

        // Calculate amplitude: full when moving, reduced for idle
        float amplitude = moveSpeed > 0.001f
            ? BobAmplitude
            : IdleBobAmplitude;

        // Use abs(sin) for stepping feel when moving, regular sin for gentle idle sway
        _bobOffset = moveSpeed > 0.001f
            ? MathF.Abs(MathF.Sin(_bobPhase * MathF.PI)) * amplitude * Scale
            : MathF.Sin(_bobPhase * MathF.PI * 2) * amplitude * Scale;
    }

    public void Render(SpriteBatch spriteBatch)
    {
        if (_currentTexture == null || Hidden)
            return;

        var viewport = _game.GraphicsDevice.Viewport;

        int scaledWidth = (int)(_currentTexture.Width * Scale);
        int scaledHeight = (int)(_currentTexture.Height * Scale);

        var x = (viewport.Width - scaledWidth) / 2;
        var y = viewport.Height - scaledHeight + (int)_bobOffset + scaledHeight / 8;

        var destRect = new Rectangle(x, y, scaledWidth, scaledHeight);

        spriteBatch.Draw(
            _currentTexture,
            destRect,
            Color.White
        );
    }
}
