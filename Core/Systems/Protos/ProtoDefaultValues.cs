using System;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.Systems.D20;

namespace OpenTemple.Core.Systems.Protos;

public static class ProtoDefaultValues
{
    // These are the proto IDs of the ingame icons (such as doors, stairs, etc.)
    private static readonly int[] SceneryIconProtoIds =
    {
        2011, 2012, 2013, 2014, 2015, 2035, 2036, 2037, 2038, 2039
    };

    public static void SetDefaultValues(int protoId, GameObject obj)
    {
        obj.SetInt32(obj_f.description, protoId);
        obj.SetFloat(obj_f.radius, 0);
        obj.SetFloat(obj_f.render_height_3d, 0);

        ObjectScript a4 = default;

        if (obj.type.IsEquipment())
        {
            obj.SetInt32(obj_f.item_material_slot, -1);
            obj.SetInt32(obj_f.item_quantity, 1);
        }

        switch (obj.type)
        {
            case ObjectType.container:
                if (protoId == 1000) // Junk pile
                {
                    obj.SetInt32(obj_f.material, 2);
                    obj.SetFlags(obj.GetFlags() | ObjectFlag.INVULNERABLE | ObjectFlag.NO_BLOCK);
                    obj.SetInt32(obj_f.sound_effect, 8025);
                }

                return;
            case ObjectType.scenery:
                // Scenery that's not an icon will be click-through
                if (Array.IndexOf(SceneryIconProtoIds, protoId) == -1)
                {
                    obj.SetFlags(obj.GetFlags() | ObjectFlag.NO_BLOCK | ObjectFlag.CLICK_THROUGH);
                }

                if (protoId == 2000) // Blood pool
                {
                    obj.SetFlag(ObjectFlag.FLAT, true);
                    obj.SetFlag(ObjectFlag.SEE_THROUGH, true);
                    obj.SetFlag(ObjectFlag.SHOOT_THROUGH, true);
                    obj.SetFlag(ObjectFlag.NO_BLOCK, true);
                    obj.SetFlag(ObjectFlag.CLICK_THROUGH, true);
                    obj.SetFlag(ObjectFlag.INVULNERABLE, true);

                    obj.SetInt32(obj_f.material, 8);

                    obj.SetSceneryFlags(obj.GetSceneryFlags() | SceneryFlag.UNDER_ALL | SceneryFlag.NO_AUTO_ANIMATE);
                    obj.SetFlag(ObjectFlag.PROVIDES_COVER, false);
                }

                return;
            case ObjectType.projectile:
                obj.SetInt32(obj_f.material, 2);
                obj.SetFloat(obj_f.speed_walk, 500.0f);
                obj.SetFloat(obj_f.speed_run, 500.0f);
                obj.SetFloat(obj_f.projectile_acceleration_x, 0.0f);
                obj.SetFloat(obj_f.projectile_acceleration_y, 0.0f);
                obj.SetFloat(obj_f.projectile_acceleration_z, 0.0f);
                return;
            case ObjectType.weapon:
                obj.SetInt32(obj_f.item_flags, 0x400000);
                obj.SetInt32(obj_f.item_description_unknown, protoId);
                obj.SetInt32(obj_f.item_description_effects, protoId);
                return;
            case ObjectType.ammo:
                obj.SetInt32(obj_f.item_description_unknown, protoId);
                obj.SetInt32(obj_f.item_description_effects, protoId);
                if (protoId == 5001) // DO NOT USE arrow
                {
                    obj.SetInt32(obj_f.ammo_type, 0);
                    obj.SetInt32(obj_f.ammo_quantity, 10);
                    obj.SetInt32(obj_f.item_weight, 1);
                    obj.SetInt32(obj_f.hp_pts, 1);
                    obj.SetInt32(obj_f.material, 2);
                    obj.SetInt32(obj_f.item_worth, 1);
                    obj.SetInt32(obj_f.category, 1);
                }

                return;
            case ObjectType.armor:
                obj.SetInt32(obj_f.item_description_unknown, protoId);
                obj.SetInt32(obj_f.item_description_effects, protoId);
                return;
            case ObjectType.money:
                obj.SetInt32(obj_f.item_description_unknown, protoId);
                obj.SetInt32(obj_f.item_description_effects, protoId);
                obj.SetInt32(obj_f.material, 5);
                obj.SetInt32(obj_f.category, 1);
                obj.SetInt32(obj_f.money_type, protoId - 7000);
                return;
            case ObjectType.food:
                obj.SetInt32(obj_f.item_description_unknown, protoId);
                obj.SetInt32(obj_f.item_description_effects, protoId);
                obj.SetInt32(obj_f.item_flags, 384);
                if (protoId == 8000) // bp_food_poison
                {
                    obj.SetInt32(obj_f.material, 8);
                    obj.SetInt32(obj_f.item_worth, 40);
                    obj.SetInt32(obj_f.hp_pts, 1);
                    obj.SetInt32(obj_f.item_weight, 5);
                    obj.SetInt32(obj_f.category, 1);
                }

                return;
            case ObjectType.scroll:
                obj.SetInt32(obj_f.item_description_unknown, protoId);
                obj.SetInt32(obj_f.item_description_effects, protoId);
                obj.SetInt32(obj_f.item_flags, 384);
                obj.SetInt32(obj_f.material, 9);
                obj.SetInt32(obj_f.item_worth, 100);
                obj.SetInt32(obj_f.category, 1);
                if (protoId == 9000) // Red scroll
                    obj.SetInt32(obj_f.item_worth, 50);
                else
                    obj.SetInt32(obj_f.item_worth, 750 * (protoId - 9000));
                obj.SetInt32(obj_f.item_weight, 0);
                return;
            case ObjectType.key:
                obj.SetInt32(obj_f.item_description_unknown, protoId);
                obj.SetInt32(obj_f.item_description_effects, protoId);
                obj.SetInt32(obj_f.material, 5);
                obj.SetInt32(obj_f.item_worth, 0);
                obj.SetInt32(obj_f.item_weight, 1);
                obj.SetInt32(obj_f.hp_pts, 200);
                if (protoId == 10000) // Gold key
                    obj.SetInt32(obj_f.category, 1);
                return;
            case ObjectType.written:
                obj.SetInt32(obj_f.item_description_unknown, protoId);
                obj.SetInt32(obj_f.item_description_effects, protoId);
                obj.SetInt32(obj_f.item_flags, 128);
                obj.SetInt32(obj_f.material, 9);
                if (protoId == 11000) // Book
                {
                    obj.SetInt32(obj_f.item_worth, 10);
                    obj.SetInt32(obj_f.item_weight, 30);
                    obj.SetInt32(obj_f.hp_pts, 20);
                    obj.SetInt32(obj_f.category, 1);
                    return;
                }

                if (protoId == 11001) // Note
                {
                    obj.SetInt32(obj_f.item_worth, 2);
                    obj.SetInt32(obj_f.item_weight, 20);
                    obj.SetInt32(obj_f.hp_pts, 5);
                    obj.SetInt32(obj_f.category, 1);
                    return;
                }

                return;
            case ObjectType.generic:
                obj.SetInt32(obj_f.item_description_unknown, protoId);
                obj.SetInt32(obj_f.item_description_effects, protoId);
                obj.SetInt32(obj_f.material, 2);
                obj.SetInt32(obj_f.category, 1);
                return;
            case ObjectType.pc:
                obj.SetInt32(obj_f.hp_pts, D20StatSystem.UninitializedHitPoints);
                obj.SetInt32(obj_f.material, 4);
                GameSystems.Object.SetGenderRace(obj, Gender.Male, RaceId.human);

                obj.SetString(obj_f.pc_player_name, "None");
                obj.SetInt32(obj_f.critter_portrait, 1005);
                obj.SetFloat(obj_f.speed_walk, 1.0f);
                obj.SetFloat(obj_f.speed_run, 1.0f);
                obj.SetInt32(obj_f.base_mesh, 1);
                obj.SetInt32(obj_f.base_anim, 1);
                obj.SetInt32(obj_f.hp_damage, 0);
                var descId = obj.GetInt32(obj_f.description);
                var name = GameSystems.Description.Get(descId);
                if (name != null)
                {
                    obj.SetString(obj_f.pc_player_name, name);
                }

                return;
            case ObjectType.npc:
                obj.SetInt32(obj_f.hp_pts, D20StatSystem.UninitializedHitPoints);
                obj.SetInt32(obj_f.npc_ac_bonus, 0);
                obj.SetFloat(obj_f.speed_walk, 1.0f);
                obj.SetFloat(obj_f.speed_run, 1.0f);
                obj.SetInt32(obj_f.material, 4);
                obj.SetInt32(obj_f.description, protoId);
                obj.SetInt32(obj_f.critter_description_unknown, protoId);
                obj.SetInt32(obj_f.hp_damage, 0);
                obj.SetInt32(obj_f.npc_reaction_base, 50);
                return;
            case ObjectType.trap:
                obj.SetInt32(obj_f.material, 5);
                obj.SetInt32(obj_f.hp_pts, 100);
                obj.SetFlag(ObjectFlag.DONTDRAW, true);

                var category = 1;
                switch (protoId)
                {
                    case 15000: // bp_trap_magical
                        a4.scriptId = 30000;
                        break;
                    case 15001: // bp_trap_mechanical
                        a4.scriptId = 30001;
                        break;
                    case 15002: // bp_trap_arrow
                        a4.scriptId = 30002;
                        break;
                    case 15003: // bp_trap_bullet
                        a4.scriptId = 30003;
                        break;
                    case 15004: // bp_trap_fire
                        a4.scriptId = 30004;
                        break;
                    case 15005: // bp_trap_electrical
                        a4.scriptId = 30005;
                        break;
                    case 15006: // bp_trap_poison
                        a4.scriptId = 30006;
                        break;
                    case 15007: // bp_trap_source
                        a4.scriptId = 30007;
                        category = 2;
                        break;
                    default:
                        return;
                }

                obj.SetInt32(obj_f.category, category);
                GameSystems.Script.GetLegacyHeader(ref a4);
                obj.SetScript(obj_f.scripts_idx, 1, a4);
                return;
            case ObjectType.bag:
                obj.SetInt32(obj_f.bag_flags, 0);
                obj.SetInt32(obj_f.bag_size, 1);
                return;
        }
    }
}