using System;
using System.Collections.Generic;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.Dialog;
using OpenTemple.Core.Systems.Feats;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Systems.Script;
using OpenTemple.Core.Systems.Spells;
using OpenTemple.Core.Systems.GameObjects;
using OpenTemple.Core.Systems.D20.Conditions;
using OpenTemple.Core.Location;
using OpenTemple.Core.Systems.ObjScript;
using OpenTemple.Core.Ui;
using System.Linq;
using OpenTemple.Core.Startup.Discovery;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.Utils;
using static OpenTemple.Core.Systems.Script.ScriptUtilities;

namespace OpenTemple.Core.Systems.D20.Conditions.TemplePlus;

[AutoRegister]
public class StrongheartHalfling
{
    private const RaceId Id = RaceId.halfling + (3 << 5);

    public static readonly RaceSpec RaceSpec = new(Id, RaceBase.halfling, Subrace.strongheart_halfling)
    {
        effectiveLevel = 0,
        helpTopic = "TAG_STRONGHEART_HALFLING",
        flags = RaceDefinitionFlags.RDF_ForgottenRealms,
        conditionName = "Strongheart",
        heightMale = (32, 40),
        heightFemale = (30, 38),
        weightMale = (32, 34),
        weightFemale = (27, 29),
        statModifiers = {(Stat.strength, -2), (Stat.dexterity, 2)},
        ProtoId = 13038,
        materialOffset = 12, // offset into rules/material_ext.mes file
        bonusFirstLevelFeat = true,
        useBaseRaceForDeity = true
    };

    public static readonly ConditionSpec Condition = ConditionSpec.Create(RaceSpec.conditionName, 0, UniquenessType.NotUnique)
        .Configure(builder => builder
            .AddAbilityModifierHooks(RaceSpec)
            .AddSkillBonuses(
                (SkillId.listen, 2),
                (SkillId.move_silently, 2),
                (SkillId.climb, 2),
                (SkillId.jump, 2),
                (SkillId.hide, 4)
            )
            .AddBaseMoveSpeed(20)
            .AddFavoredClassHook(Stat.level_rogue)
            .AddHandler(DispatcherType.SaveThrowLevel, HalflingFearSaveBonus)
            .AddHandler(DispatcherType.ToHitBonus2, OnGetToHitBonusSlingsThrownWeapons)
        );

    public static void HalflingFearSaveBonus(in DispatcherCallbackArgs evt)
    {
        var dispIo = evt.GetDispIoSavingThrow();
        var flags = dispIo.flags;
        if ((flags & D20SavingThrowFlag.SPELL_DESCRIPTOR_FEAR) != 0)
        {
            dispIo.bonlist.AddBonus(2, 13, 139);
        }
    }
    // +1 with thrown weapons and slings

    public static void OnGetToHitBonusSlingsThrownWeapons(in DispatcherCallbackArgs evt)
    {
        var dispIo = evt.GetDispIoAttackBonus();
        var thrownWeapon = (dispIo.attackPacket.flags & D20CAF.THROWN) != 0;
        var isSling = false;
        var wpn = dispIo.attackPacket.GetWeaponUsed();
        if (wpn != null)
        {
            var weaponType = wpn.GetWeaponType();
            if (weaponType == WeaponType.sling)
            {
                isSling = true;
            }
        }

        // Check for sling or thrown weapon
        if (thrownWeapon || isSling)
        {
            dispIo.bonlist.AddBonus(1, 0, 139);
        }
    }

    // Note:  Adding the size +4 bonus to hide as a racial bonus since setting size to small does not grant the bonus
}