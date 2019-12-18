using System;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Systems.GameObjects;

namespace SpicyTemple.Core.Systems.D20
{

    [Flags]
    public enum D20CAF : uint
    {
        NONE = 0,
        HIT = 0x1,
        CRITICAL = 0x2,
        RANGED = 0x4,
        ACTIONFRAME_PROCESSED = 0x8,
        NEED_PROJECTILE_HIT = 0x10,
        NEED_ANIM_COMPLETED = 0x20,
        ATTACK_OF_OPPORTUNITY = 0x40,
        CONCEALMENT_MISS = 0x80,
        TOUCH_ATTACK = 0x100, // confirmed
        FREE_ACTION = 0x200,
        CHARGE = 0x400,
        REROLL = 0x800,
        REROLL_CRITICAL = 0x1000,
        TRAP = 0x2000, // TODO: This should mean flatfooted (it might also mean being hit by a TRAP implied FLATFOOTED)
        ALTERNATE = 0x4000,
        NO_PRECISION_DAMAGE = 0x8000,
        FLANKED = 0x10000,
        DEFLECT_ARROWS = 0x20000,
        FULL_ATTACK = 0x40000,
        AOO_MOVEMENT = 0x80000,
        BONUS_ATTACK = 0x100000,
        THROWN = 0x200000,

        SAVE_SUCCESSFUL = 0x400000,// was 0x800000 in python dict, that that was actually D20CAF_SECONDARY_WEAPON
        SECONDARY_WEAPON = 0x800000, // 16777216,
        MANYSHOT = 0x1000000 ,//33554432,
        ALWAYS_HIT = 0x2000000 , //67108864,
        COVER =             0x4000000, //134217728,   // CONFIRMED in GlobalGetArmorClass
        COUNTERSPELLED =    0x8000000, //268435456,
        THROWN_GRENADE =    0x10000000, //536870912,
        FINAL_ATTACK_ROLL = 0x20000000, //1073741824,
        TRUNCATED =         0x40000000, //0x40000000,  indicates that the path has been truncated (since the destination was too far away)
        UNNECESSARY =       0x80000000  // affects pathfinding time allotment
    };


    public struct AttackPacket
    {
        public const int ATTACK_CODE_PRIMARY = 0;
        public const int ATTACK_CODE_OFFHAND = 99; // originally 4
        public const int ATTACK_CODE_NATURAL_ATTACK = 999; //originally 9

        public GameObjectBody attacker;
        public GameObjectBody victim;
        public D20ActionType d20ActnType;
        public int dispKey; // This isn'T the "dispKey"... It's the attack code
        public D20CAF flags;
        public int field_1C;
        public GameObjectBody weaponUsed;
        public GameObjectBody ammoItem;

        [TempleDllLocation(0x1004dfa0)]
        public GameObjectBody GetWeaponUsed()
        {
            if ( !flags.HasFlag(D20CAF.TOUCH_ATTACK) || flags.HasFlag(D20CAF.THROWN_GRENADE) ) {
                return weaponUsed;
            }
            return null;
        }

        public bool IsOffhandAttack()
        {
            if (flags.HasFlag(D20CAF.SECONDARY_WEAPON)) {
                return true;
            }

            if (dispKey == ATTACK_CODE_OFFHAND + 2 )
                return true;

            return false;
        }

        public static readonly AttackPacket Default = new AttackPacket
        {
            flags = D20CAF.NONE,
            d20ActnType = D20ActionType.STANDARD_ATTACK,
            dispKey = 0
        };

        public void SetAttacker(GameObjectBody attacker, bool includeWeapon = true)
        {
            this.attacker = attacker;

            if (includeWeapon)
            {
                GameObjectBody weapon = weaponUsed;
                if (attacker != null)
                {
                    weapon = GameSystems.Item.ItemWornAt(attacker, EquipSlot.WeaponPrimary);
                }

                if (weapon != null && weapon.type != ObjectType.weapon)
                {
                    weapon = null;
                }

                weaponUsed = weapon;

                if (attacker != null)
                {
                    ammoItem = GameSystems.Item.CheckRangedWeaponAmmo(attacker);
                }
                else
                {
                    ammoItem = null;
                }
            }
        }
    }

    public static class AttackPacketExtensions
    {

        public static bool IsRangedWeaponAttack(this AttackPacket attackPacket)
        {
            var weaponUsed = attackPacket.GetWeaponUsed();
            return weaponUsed != null && GameSystems.Item.IsRangedWeapon(weaponUsed);
        }

    }

}