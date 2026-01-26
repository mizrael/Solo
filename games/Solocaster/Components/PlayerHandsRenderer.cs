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
    private Texture2D? _rightHandTexture;
    private Texture2D? _leftHandTexture;

    private float _bobPhase;
    private float _rightBobOffset;
    private float _leftBobOffset;
    private float _rightHorizontalOffset;
    private float _leftHorizontalOffset;

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

    /// <summary>
    /// Horizontal offset from screen edge for each hand.
    /// </summary>
    public float HandHorizontalOffset { get; set; } = 300;

    /// <summary>
    /// Phase offset between left and right hand bobbing (0 = in sync, PI = alternating).
    /// </summary>
    public float HandPhaseOffset { get; set; } = MathF.PI * 0.7f;

    /// <summary>
    /// Scale multiplier for the left hand to make it appear closer to the viewer.
    /// </summary>
    public float LeftHandScaleMultiplier { get; set; } = 1.12f;

    /// <summary>
    /// Vertical offset for the left hand (positive = lower on screen, appearing closer).
    /// </summary>
    public float LeftHandVerticalOffset { get; set; } = 20f;

    /// <summary>
    /// Horizontal sway amplitude for subtle hand movement.
    /// </summary>
    public float HorizontalSwayAmplitude { get; set; } = 4f;

    /// <summary>
    /// Frequency multiplier for horizontal sway (different from vertical bob for natural feel).
    /// </summary>
    public float HorizontalSwayFrequency { get; set; } = 0.6f;

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
        _handTextures["empty"] = _game.Content.Load<Texture2D>("player/hand_empty");
        _handTextures["longsword"] = _game.Content.Load<Texture2D>("player/hand_longsword");
        _handTextures["axe"] = _game.Content.Load<Texture2D>("player/hand_axe");
        _handTextures["morningstar"] = _game.Content.Load<Texture2D>("player/hand_morningstar");
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
        var rightWeapon = _inventory.GetEquippedItem(EquipSlot.RightHand);
        var leftWeapon = _inventory.GetEquippedItem(EquipSlot.LeftHand);

        var rightKey = MapWeaponToTextureKey(rightWeapon);
        var leftKey = MapWeaponToTextureKey(leftWeapon);

        _rightHandTexture = _handTextures.GetValueOrDefault(rightKey) ?? _handTextures["empty"];
        _leftHandTexture = _handTextures.GetValueOrDefault(leftKey) ?? _handTextures["empty"];
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

        // Calculate bob offsets for each hand with phase offset for alternating motion
        if (moveSpeed > 0.001f)
        {
            // Stepping feel when moving - hands alternate with slight desync
            _rightBobOffset = MathF.Abs(MathF.Sin(_bobPhase * MathF.PI)) * amplitude * Scale;
            _leftBobOffset = MathF.Abs(MathF.Sin((_bobPhase * MathF.PI) + HandPhaseOffset)) * amplitude * Scale * 1.15f;
        }
        else
        {
            // Gentle idle sway - slightly different timing for each hand
            float rightSway = MathF.Sin(_bobPhase * MathF.PI * 2) * amplitude * Scale;
            float leftSway = MathF.Sin((_bobPhase * MathF.PI * 2) + 0.4f) * amplitude * Scale * 1.2f;
            _rightBobOffset = rightSway;
            _leftBobOffset = leftSway;
        }

        // Horizontal sway with different phase for each hand (creates natural jitter)
        float swayPhase = _bobPhase * HorizontalSwayFrequency;
        _rightHorizontalOffset = MathF.Sin(swayPhase * MathF.PI * 2) * HorizontalSwayAmplitude * Scale;
        _leftHorizontalOffset = MathF.Sin((swayPhase * MathF.PI * 2) + MathF.PI * 0.4f) * HorizontalSwayAmplitude * Scale * 1.3f;
    }

    public void Render(SpriteBatch spriteBatch)
    {
        if (Hidden)
            return;

        var viewport = _game.GraphicsDevice.Viewport;

        // Render right hand (on the right side of screen)
        if (_rightHandTexture != null)
        {
            int scaledWidth = (int)(_rightHandTexture.Width * Scale);
            int scaledHeight = (int)(_rightHandTexture.Height * Scale);

            int x = viewport.Width - scaledWidth - (int)(HandHorizontalOffset * Scale) + (int)_rightHorizontalOffset;
            int y = viewport.Height - scaledHeight + (int)_rightBobOffset + scaledHeight / 8;

            var destRect = new Rectangle(x, y, scaledWidth, scaledHeight);

            spriteBatch.Draw(
                _rightHandTexture,
                destRect,
                Color.White
            );
        }

        // Render left hand (mirrored, on the left side of screen, slightly closer/larger)
        if (_leftHandTexture != null)
        {
            float leftScale = Scale * LeftHandScaleMultiplier;
            int scaledWidth = (int)(_leftHandTexture.Width * leftScale);
            int scaledHeight = (int)(_leftHandTexture.Height * leftScale);

            int x = (int)(HandHorizontalOffset * Scale) + (int)_leftHorizontalOffset;
            int y = viewport.Height - scaledHeight + (int)_leftBobOffset + scaledHeight / 8 + (int)(LeftHandVerticalOffset * Scale);

            var destRect = new Rectangle(x, y, scaledWidth, scaledHeight);

            // Draw mirrored using SpriteEffects.FlipHorizontally
            spriteBatch.Draw(
                _leftHandTexture,
                destRect,
                null,
                Color.White,
                0f,
                Vector2.Zero,
                SpriteEffects.FlipHorizontally,
                0f
            );
        }
    }
}
