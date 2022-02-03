
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.Utils;
using static OpenTemple.Core.Systems.Script.ScriptUtilities;

namespace Scripts.Dialog
{
    [DialogScript(342)]
    public class TarahDialog : Tarah, IDialogScript
    {
        public bool CheckPrecondition(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 11:
                case 351:
                    originalScript = "pc.stat_level_get( stat_charisma ) <= 14";
                    return pc.GetStat(Stat.charisma) <= 14;
                case 12:
                case 352:
                    originalScript = "pc.stat_level_get( stat_charisma ) >= 15 and pc.stat_level_get( stat_charisma ) <= 17";
                    return pc.GetStat(Stat.charisma) >= 15 && pc.GetStat(Stat.charisma) <= 17;
                case 13:
                case 353:
                    originalScript = "pc.stat_level_get( stat_charisma ) >= 18";
                    return pc.GetStat(Stat.charisma) >= 18;
                case 111:
                    originalScript = "game.party[2].stat_level_get( stat_gender ) == gender_male";
                    return GameSystems.Party.GetPartyGroupMemberN(2).GetGender() == Gender.Male;
                case 112:
                    originalScript = "game.party[2].stat_level_get( stat_gender ) == gender_female";
                    return GameSystems.Party.GetPartyGroupMemberN(2).GetGender() == Gender.Female;
                case 171:
                    originalScript = "game.party_size() == 3";
                    return GameSystems.Party.PartySize == 3;
                case 172:
                    originalScript = "game.party_size() == 4";
                    return GameSystems.Party.PartySize == 4;
                case 173:
                    originalScript = "game.party_size() == 5";
                    return GameSystems.Party.PartySize == 5;
                case 174:
                    originalScript = "game.party_size() == 6";
                    return GameSystems.Party.PartySize == 6;
                case 175:
                    originalScript = "game.party_size() == 7";
                    return GameSystems.Party.PartySize == 7;
                case 176:
                    originalScript = "game.party_size() == 8";
                    return GameSystems.Party.PartySize == 8;
                case 221:
                case 291:
                case 381:
                case 431:
                    originalScript = "(game.party_alignment == LAWFUL_GOOD or game.party_alignment == NEUTRAL_GOOD or game.party_alignment == CHAOTIC_GOOD or game.party_alignment == LAWFUL_NEUTRAL or game.party_alignment == TRUE_NEUTRAL)";
                    return (PartyAlignment == Alignment.LAWFUL_GOOD || PartyAlignment == Alignment.NEUTRAL_GOOD || PartyAlignment == Alignment.CHAOTIC_GOOD || PartyAlignment == Alignment.LAWFUL_NEUTRAL || PartyAlignment == Alignment.NEUTRAL);
                case 222:
                case 382:
                    originalScript = "game.global_flags[801] == 1 and (game.party_alignment == CHAOTIC_NEUTRAL or game.party_alignment == LAWFUL_EVIL)";
                    return GetGlobalFlag(801) && (PartyAlignment == Alignment.CHAOTIC_NEUTRAL || PartyAlignment == Alignment.LAWFUL_EVIL);
                case 223:
                case 383:
                    originalScript = "(game.party_alignment == NEUTRAL_EVIL or game.party_alignment == CHAOTIC_EVIL)";
                    return (PartyAlignment == Alignment.NEUTRAL_EVIL || PartyAlignment == Alignment.CHAOTIC_EVIL);
                case 281:
                    originalScript = "game.global_vars[992] == 1";
                    return GetGlobalVar(992) == 1;
                case 282:
                    originalScript = "game.global_vars[992] == 2";
                    return GetGlobalVar(992) == 2;
                case 283:
                    originalScript = "game.global_vars[992] == 3";
                    return GetGlobalVar(992) == 3;
                case 284:
                    originalScript = "game.global_vars[992] == 4";
                    return GetGlobalVar(992) == 4;
                case 285:
                    originalScript = "game.global_vars[992] == 5";
                    return GetGlobalVar(992) == 5;
                case 286:
                    originalScript = "game.global_vars[992] == 6";
                    return GetGlobalVar(992) == 6;
                case 292:
                case 432:
                    originalScript = "(game.party_alignment == CHAOTIC_NEUTRAL or game.party_alignment == LAWFUL_EVIL or game.party_alignment == NEUTRAL_EVIL or game.party_alignment == CHAOTIC_EVIL)";
                    return (PartyAlignment == Alignment.CHAOTIC_NEUTRAL || PartyAlignment == Alignment.LAWFUL_EVIL || PartyAlignment == Alignment.NEUTRAL_EVIL || PartyAlignment == Alignment.CHAOTIC_EVIL);
                default:
                    originalScript = null;
                    return true;
            }
        }
        public void ApplySideEffect(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 1:
                    originalScript = "game.sound( 4012 ); dom_mon( npc, pc )";
                    Sound(4012);
                    dom_mon(npc, pc);
                    ;
                    break;
                case 21:
                case 61:
                    originalScript = "pick_to_grope( npc, pc )";
                    pick_to_grope(npc, pc);
                    break;
                case 70:
                    originalScript = "game.global_vars[992] = 1";
                    SetGlobalVar(992, 1);
                    break;
                case 71:
                    originalScript = "meleny_see_tarah( npc, pc ); game.sound( 4018 ); switch_to_meleny( npc, pc, 340)";
                    meleny_see_tarah(npc, pc);
                    Sound(4018);
                    switch_to_meleny(npc, pc, 340);
                    ;
                    break;
                case 80:
                    originalScript = "game.global_vars[992] = 3";
                    SetGlobalVar(992, 3);
                    break;
                case 81:
                    originalScript = "fruella_see_tarah( npc, pc ); game.sound( 4018 ); switch_to_fruella( npc, pc, 410)";
                    fruella_see_tarah(npc, pc);
                    Sound(4018);
                    switch_to_fruella(npc, pc, 410);
                    ;
                    break;
                case 90:
                    originalScript = "game.global_vars[992] = 4";
                    SetGlobalVar(992, 4);
                    break;
                case 91:
                    originalScript = "serena_see_tarah( npc, pc ); game.sound( 4018 ); switch_to_serena( npc, pc, 250)";
                    serena_see_tarah(npc, pc);
                    Sound(4018);
                    switch_to_serena(npc, pc, 250);
                    ;
                    break;
                case 100:
                    originalScript = "game.global_vars[992] = 2";
                    SetGlobalVar(992, 2);
                    break;
                case 101:
                    originalScript = "riana_see_tarah( npc, pc ); game.sound( 4018 ); switch_to_riana( npc, pc, 200)";
                    riana_see_tarah(npc, pc);
                    Sound(4018);
                    switch_to_riana(npc, pc, 200);
                    ;
                    break;
                case 111:
                case 112:
                    originalScript = "pc2_see_tarah( npc, pc ); game.sound( 4018 )";
                    pc2_see_tarah(npc, pc);
                    Sound(4018);
                    ;
                    break;
                case 121:
                    originalScript = "switch_to_kenan( npc, pc, 90)";
                    switch_to_kenan(npc, pc, 90);
                    break;
                case 131:
                    originalScript = "switch_to_sharar( npc, pc, 10)";
                    switch_to_sharar(npc, pc, 10);
                    break;
                case 141:
                    originalScript = "switch_to_gadham( npc, pc, 10)";
                    switch_to_gadham(npc, pc, 10);
                    break;
                case 151:
                    originalScript = "switch_to_abaddon( npc, pc, 10)";
                    switch_to_abaddon(npc, pc, 10);
                    break;
                case 161:
                    originalScript = "switch_to_gershom( npc, pc, 10)";
                    switch_to_gershom(npc, pc, 10);
                    break;
                case 171:
                case 173:
                    originalScript = "game.particles( 'cast-Necromancy-cast', npc ); game.sound( 4014 ); kill_pc_3( npc, pc )";
                    AttachParticles("cast-Necromancy-cast", npc);
                    Sound(4014);
                    kill_pc_3(npc, pc);
                    ;
                    break;
                case 172:
                    originalScript = "game.particles( 'cast-Necromancy-cast', npc ); game.sound( 4014 ); kill_pc_4( npc, pc )";
                    AttachParticles("cast-Necromancy-cast", npc);
                    Sound(4014);
                    kill_pc_4(npc, pc);
                    ;
                    break;
                case 174:
                    originalScript = "game.particles( 'cast-Necromancy-cast', npc ); game.sound( 4014 ); kill_pc_5( npc, pc )";
                    AttachParticles("cast-Necromancy-cast", npc);
                    Sound(4014);
                    kill_pc_5(npc, pc);
                    ;
                    break;
                case 175:
                    originalScript = "game.particles( 'cast-Necromancy-cast', npc ); game.sound( 4014 ); kill_pc_7( npc, pc )";
                    AttachParticles("cast-Necromancy-cast", npc);
                    Sound(4014);
                    kill_pc_7(npc, pc);
                    ;
                    break;
                case 176:
                    originalScript = "game.particles( 'cast-Necromancy-cast', npc ); game.sound( 4014 ); kill_pc_6( npc, pc )";
                    AttachParticles("cast-Necromancy-cast", npc);
                    Sound(4014);
                    kill_pc_6(npc, pc);
                    ;
                    break;
                case 181:
                    originalScript = "switch_to_gershom( npc, pc, 20)";
                    switch_to_gershom(npc, pc, 20);
                    break;
                case 191:
                case 371:
                    originalScript = "daniel_see_tarah( npc, pc ); game.sound( 4018 )";
                    daniel_see_tarah(npc, pc);
                    Sound(4018);
                    ;
                    break;
                case 201:
                    originalScript = "switch_to_daniel( npc, pc, 10)";
                    switch_to_daniel(npc, pc, 10);
                    break;
                case 231:
                case 241:
                case 391:
                case 401:
                    originalScript = "create_skel( npc, pc ); game.sound( 4015 )";
                    create_skel(npc, pc);
                    Sound(4015);
                    ;
                    break;
                case 251:
                case 421:
                    originalScript = "dom_mon_end( npc, pc ); game.sound( 4016 )";
                    dom_mon_end(npc, pc);
                    Sound(4016);
                    ;
                    break;
                case 260:
                    originalScript = "game.global_vars[992] = 5";
                    SetGlobalVar(992, 5);
                    break;
                case 261:
                    originalScript = "pishella_see_tarah( npc, pc ); game.sound( 4018 ); switch_to_pishella( npc, pc, 240)";
                    pishella_see_tarah(npc, pc);
                    Sound(4018);
                    switch_to_pishella(npc, pc, 240);
                    ;
                    break;
                case 270:
                    originalScript = "game.global_vars[992] = 6";
                    SetGlobalVar(992, 6);
                    break;
                case 271:
                    originalScript = "kella_see_tarah( npc, pc ); game.sound( 4018 ); switch_to_kella( npc, pc, 230)";
                    kella_see_tarah(npc, pc);
                    Sound(4018);
                    switch_to_kella(npc, pc, 230);
                    ;
                    break;
                case 281:
                    originalScript = "switch_to_meleny( npc, pc, 350)";
                    switch_to_meleny(npc, pc, 350);
                    break;
                case 282:
                    originalScript = "switch_to_riana( npc, pc, 210)";
                    switch_to_riana(npc, pc, 210);
                    break;
                case 283:
                    originalScript = "switch_to_fruella( npc, pc, 420)";
                    switch_to_fruella(npc, pc, 420);
                    break;
                case 284:
                    originalScript = "switch_to_serena( npc, pc, 260)";
                    switch_to_serena(npc, pc, 260);
                    break;
                case 285:
                    originalScript = "switch_to_pishella( npc, pc, 320)";
                    switch_to_pishella(npc, pc, 320);
                    break;
                case 286:
                    originalScript = "switch_to_kella( npc, pc, 240)";
                    switch_to_kella(npc, pc, 240);
                    break;
                case 290:
                case 430:
                    originalScript = "npc.cast_spell( spell_improved_invisibility, npc )";
                    npc.CastSpell(WellKnownSpells.ImprovedInvisibility, npc);
                    break;
                case 291:
                case 292:
                case 431:
                case 432:
                    originalScript = "destroy_skel( npc, pc ); game.sound( 4015 )";
                    destroy_skel(npc, pc);
                    Sound(4015);
                    ;
                    break;
                case 311:
                    originalScript = "switch_to_daniel( npc, pc, 1)";
                    switch_to_daniel(npc, pc, 1);
                    break;
                case 321:
                case 322:
                    originalScript = "switch_to_abaddon( npc, pc, 20)";
                    switch_to_abaddon(npc, pc, 20);
                    break;
                case 330:
                case 440:
                    originalScript = "npc.cast_spell( spell_lesser_globe_of_invulnerability, npc )";
                    npc.CastSpell(WellKnownSpells.LesserGlobeOfInvulnerability, npc);
                    break;
                case 331:
                    originalScript = "start_fight( npc, pc )";
                    start_fight(npc, pc);
                    break;
                case 441:
                    originalScript = "npc.attack( pc )";
                    npc.Attack(pc);
                    break;
                default:
                    originalScript = null;
                    return;
            }
        }
        public bool TryGetSkillChecks(int lineNumber, out DialogSkillChecks skillChecks)
        {
            switch (lineNumber)
            {
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
