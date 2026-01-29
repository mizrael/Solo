using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Solo;
using Solocaster.Character;
using Solocaster.Inventory;
using System;

namespace Solocaster.AI.Player;

public record PlayerCombatState : Solo.AI.State
{
    private const float RaisingDuration = 0.08f;
    private const float HeldDuration = 0.05f;
    private const float LoweringDuration = 0.12f;
    private const float BaseCooldown = 0.8f;
    private const float AgilityModifierRate = 0.02f;

    private readonly PlayerStateContext _ctx;

    private HandActionState _leftHandState = HandActionState.Idle;
    private HandActionState _rightHandState = HandActionState.Idle;
    private float _leftHandTimer;
    private float _rightHandTimer;
    private float _leftCooldown;
    private float _rightCooldown;

    private enum HandActionState
    {
        Idle,
        Raising,
        Held,
        Lowering
    }


    public PlayerCombatState(GameObject owner, PlayerStateContext context) : base(owner)
    {
        _ctx = context;
    }

    protected override void OnEnter()
    {
        _ctx.ShowsHands = true;
        _ctx.SpeedMultiplier = 1.0f;
        _ctx.BobSpeed = 1.5f;

        _leftHandState = HandActionState.Idle;
        _rightHandState = HandActionState.Idle;
        _leftHandTimer = 0f;
        _rightHandTimer = 0f;
        _leftCooldown = 0f;
        _rightCooldown = 0f;
    }

    protected override void OnExecute(GameTime gameTime)
    {
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        var mouseState = Mouse.GetState();

        // Update cooldowns
        _leftCooldown = MathF.Max(0, _leftCooldown - deltaTime);
        _rightCooldown = MathF.Max(0, _rightCooldown - deltaTime);

        // Handle hand actions
        HandleLeftHandAction(mouseState, deltaTime);
        HandleRightHandAction(mouseState, deltaTime);

        // Update raise amounts for renderer
        _ctx.LeftHandRaiseAmount = CalculateRaiseAmount(_leftHandState, _leftHandTimer);
        _ctx.RightHandRaiseAmount = CalculateRaiseAmount(_rightHandState, _rightHandTimer);

        _ctx.PreviousMouseState = mouseState;

        // Stamina regen in combat
        _ctx.Stats.UpdateStamina(deltaTime, false);
    }

    private void HandleLeftHandAction(MouseState mouseState, float deltaTime)
    {
        var leftItem = _ctx.Inventory.GetEquippedItem(EquipSlot.LeftHand);
        bool isShield = IsShield(leftItem);
        bool isWeapon = IsWeapon(leftItem);
        bool mouseHeld = mouseState.LeftButton == ButtonState.Pressed;

        if (isShield)
        {
            HandleParry(ref _leftHandState, ref _leftHandTimer, mouseHeld, deltaTime);
        }
        else if (isWeapon)
        {
            HandleAttack(ref _leftHandState, ref _leftHandTimer, ref _leftCooldown,
                         _rightHandState, mouseHeld, deltaTime, leftItem!);
        }
        else
        {
            HandleAttack(ref _leftHandState, ref _leftHandTimer, ref _leftCooldown,
                         _rightHandState, mouseHeld, deltaTime);
        }
    }

    private void HandleRightHandAction(MouseState mouseState, float deltaTime)
    {
        var rightItem = _ctx.Inventory.GetEquippedItem(EquipSlot.RightHand);
        bool isShield = IsShield(rightItem);
        bool isWeapon = IsWeapon(rightItem);
        bool mouseHeld = mouseState.RightButton == ButtonState.Pressed;

        if (isShield)
        {
            HandleParry(ref _rightHandState, ref _rightHandTimer, mouseHeld, deltaTime);
        }
        else if (isWeapon)
        {
            HandleAttack(ref _rightHandState, ref _rightHandTimer, ref _rightCooldown,
                         _leftHandState, mouseHeld, deltaTime, rightItem!);
        }
        else
        {
            HandleAttack(ref _rightHandState, ref _rightHandTimer, ref _rightCooldown,
                         _leftHandState, mouseHeld, deltaTime);
        }
    }

    private void HandleParry(ref HandActionState state, ref float timer, bool mouseHeld, float deltaTime)
    {
        if (mouseHeld)
        {
            switch (state)
            {
                case HandActionState.Idle:
                    state = HandActionState.Raising;
                    timer = 0f;
                    break;
                case HandActionState.Raising:
                    timer += deltaTime;
                    if (timer >= RaisingDuration)
                    {
                        state = HandActionState.Held;
                        timer = 0f;
                    }
                    break;
                case HandActionState.Held:
                    break;
                case HandActionState.Lowering:
                    state = HandActionState.Raising;
                    timer = 0f;
                    break;
            }
        }
        else
        {
            if (state == HandActionState.Held || state == HandActionState.Raising)
            {
                state = HandActionState.Lowering;
                timer = 0f;
            }
            else if (state == HandActionState.Lowering)
            {
                timer += deltaTime;
                if (timer >= LoweringDuration)
                {
                    state = HandActionState.Idle;
                    timer = 0f;
                }
            }
        }
    }

    private void HandleAttack(ref HandActionState state, ref float timer, ref float cooldown,
                              HandActionState otherHandState, bool mouseHeld, float deltaTime,
                              ItemInstance? weapon = null)
    {
        switch (state)
        {
            case HandActionState.Raising:
                timer += deltaTime;
                if (timer >= RaisingDuration)
                {
                    state = HandActionState.Held;
                    timer = 0f;
                }
                break;
            case HandActionState.Held:
                timer += deltaTime;
                if (timer >= HeldDuration)
                {
                    state = HandActionState.Lowering;
                    timer = 0f;
                    _ctx.Stats.OnCombatAction();
                }
                break;
            case HandActionState.Lowering:
                timer += deltaTime;
                if (timer >= LoweringDuration)
                {
                    state = HandActionState.Idle;
                    timer = 0f;
                }
                break;
        }

        if (mouseHeld && cooldown <= 0 &&
            state == HandActionState.Idle &&
            otherHandState != HandActionState.Raising && otherHandState != HandActionState.Held)
        {
            state = HandActionState.Raising;
            timer = 0f;
            cooldown = weapon != null ? CalculateCooldown(weapon.Template) : BaseCooldown;
        }
    }

    private float CalculateCooldown(ItemTemplate weapon)
    {
        float attackSpeed = weapon.AttackSpeed;
        float agility = _ctx.Stats.GetTotalStat(Stats.Agility);
        float weaponModifier = 1f / attackSpeed;
        float agilityModifier = 1f / (1f + agility * AgilityModifierRate);
        return BaseCooldown * weaponModifier * agilityModifier;
    }

    private float CalculateRaiseAmount(HandActionState state, float timer)
    {
        return state switch
        {
            HandActionState.Idle => 0f,
            HandActionState.Raising => MathF.Min(1f, timer / RaisingDuration),
            HandActionState.Held => 1f,
            HandActionState.Lowering => MathF.Max(0f, 1f - (timer / LoweringDuration)),
            _ => 0f
        };
    }

    private static bool IsShield(ItemInstance? item)
    {
        if (item == null) return false;
        var name = item.Template.Name;
        var id = item.TemplateId;
        return name.Contains("shield", StringComparison.OrdinalIgnoreCase) ||
               name.Contains("buckler", StringComparison.OrdinalIgnoreCase) ||
               id.Contains("shield", StringComparison.OrdinalIgnoreCase) ||
               id.Contains("buckler", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsWeapon(ItemInstance? item)
    {
        if (item == null) return false;
        return item.Template.ItemType == ItemType.Weapon;
    }
}
