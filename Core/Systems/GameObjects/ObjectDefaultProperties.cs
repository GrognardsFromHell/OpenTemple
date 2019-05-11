using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.Utils;

namespace SpicyTemple.Core.Systems.GameObjects
{
    public static class ObjectDefaultProperties
    {
        [TempleDllLocation(0x100a1620)]
        public static void SetDefaultProperties(GameObjectBody obj)
        {
            obj.SetInt32(obj_f.shadow_art_2d, -1);
            obj.SetInt32(obj_f.blit_color, 0xFFFFFF);
            obj.SetInt32(obj_f.transparency, 255);
            obj.SetInt32(obj_f.model_scale, 100);
            obj.SetInt32(obj_f.light_color, 0xFFFFFF);
            switch (obj.type)
            {
                case ObjectType.weapon:
                case ObjectType.ammo:
                case ObjectType.armor:
                case ObjectType.money:
                case ObjectType.food:
                case ObjectType.scroll:
                case ObjectType.key:
                case ObjectType.written:
                case ObjectType.generic:
                    if (obj.type == ObjectType.key)
                    {
                        obj.SetInt32(obj_f.item_weight, 0);
                    }
                    else if (obj.type == ObjectType.money)
                    {
                        obj.SetInt32(obj_f.item_weight, 1);
                    }
                    else
                    {
                        obj.SetInt32(obj_f.item_weight, 10);
                    }

                    obj.SetFlags(obj.GetFlags() | ObjectFlag.NO_BLOCK | ObjectFlag.SHOOT_THROUGH
                                 | ObjectFlag.SEE_THROUGH | ObjectFlag.FLAT);
                    break;
                case ObjectType.pc:
                case ObjectType.npc:
                    ItemSystem.SetWeaponSlotDefaults(obj);
                    D20StatSystem.SetDefaultPlayerHP(obj);
                    obj.SetFlags(obj.GetFlags() | ObjectFlag.PROVIDES_COVER | ObjectFlag.SEE_THROUGH
                                 | ObjectFlag.SHOOT_THROUGH);
                    break;
            }

            switch (obj.type)
            {
                case ObjectType.portal:
                    obj.SetInt32(obj_f.hp_pts, 100);
                    obj.SetFlags(obj.GetFlags() | ObjectFlag.PROVIDES_COVER);
                    break;
                case ObjectType.container:
                case ObjectType.scenery:
                    obj.SetInt32(obj_f.hp_pts, 100);
                    obj.SetFlags(obj.GetFlags() | ObjectFlag.PROVIDES_COVER | ObjectFlag.SEE_THROUGH
                                 | ObjectFlag.SHOOT_THROUGH);
                    break;
                case ObjectType.projectile:
                    obj.SetFlags(obj.GetFlags() | ObjectFlag.NO_BLOCK | ObjectFlag.SEE_THROUGH
                                 | ObjectFlag.SHOOT_THROUGH);
                    break;
                case ObjectType.weapon:
                    obj.SetInt32(obj_f.hp_pts, 8);
                    obj.SetInt32(obj_f.item_worth, 8);
                    obj.SetInt32(obj_f.weapon_ammo_type, (int) WeaponAmmoType.no_ammo);
                    obj.SetInt32(obj_f.weapon_damage_dice, new Dice(1, 4).ToPacked());
                    obj.SetInt32(obj_f.weapon_crit_hit_chart, 2);
                    obj.SetInt32(obj_f.weapon_crit_range, 1);
                    obj.SetInt32(obj_f.weapon_type, 0);
                    obj.SetInt32(obj_f.weapon_missile_aid, -1);
                    break;
                case ObjectType.ammo:
                    obj.SetInt32(obj_f.hp_pts, 5);
                    obj.SetInt32(obj_f.item_worth, 1);
                    break;
                case ObjectType.armor:
                    obj.SetInt32(obj_f.hp_pts, 8);
                    obj.SetInt32(obj_f.item_worth, 8);
                    break;
                case ObjectType.money:
                    obj.SetInt32(obj_f.hp_pts, 10);
                    obj.SetInt32(obj_f.item_worth, 1);
                    obj.SetInt32(obj_f.money_quantity, 1);
                    break;
                case ObjectType.food:
                    obj.SetInt32(obj_f.hp_pts, 2);
                    obj.SetInt32(obj_f.item_worth, 2);
                    break;
                case ObjectType.scroll:
                    obj.SetInt32(obj_f.hp_pts, 1);
                    obj.SetInt32(obj_f.item_worth, 6);
                    break;
                case ObjectType.key:
                    obj.SetInt32(obj_f.hp_pts, 3);
                    obj.SetInt32(obj_f.item_worth, 2);
                    break;
                case ObjectType.written:
                    obj.SetInt32(obj_f.hp_pts, 1);
                    obj.SetInt32(obj_f.item_worth, 1);
                    break;
                case ObjectType.generic:
                    obj.SetInt32(obj_f.hp_pts, 3);
                    obj.SetInt32(obj_f.item_worth, 2);
                    break;
                case ObjectType.trap:
                    obj.SetInt32(obj_f.hp_pts, 100);
                    obj.SetFlags(obj.GetFlags()
                                 | ObjectFlag.DONTLIGHT
                                 | ObjectFlag.NO_BLOCK
                                 | ObjectFlag.SHOOT_THROUGH
                                 | ObjectFlag.SEE_THROUGH
                                 | ObjectFlag.FLAT);
                    break;
            }
        }
    }
}