
using System;
using System.Collections.Generic;
using OpenTemple.Core.GameObject;
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

namespace Scripts
{
    [ObjectScript(212)]
    public class Burnebadger : BaseObjectScript
    {
        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((GetGlobalVar(501) == 4 || GetGlobalVar(501) == 5 || GetGlobalVar(501) == 6))
            {
                attachee.FloatLine(23000, triggerer);
            }
            else if ((GetGlobalFlag(835) && !GetGlobalFlag(37) && !GetGlobalFlag(842) && !GetGlobalFlag(839)))
            {
                triggerer.BeginDialog(attachee, 300);
            }
            else if ((PartyLeader.HasReputation(27)))
            {
                triggerer.BeginDialog(attachee, 120);
            }
            else if ((attachee.HasMet(triggerer)))
            {
                triggerer.BeginDialog(attachee, 130);
            }
            else
            {
                triggerer.BeginDialog(attachee, 1);
            }

            return SkipDefault;
        }
        public override bool OnFirstHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((attachee.GetMap() == 5007 || attachee.GetMap() == 5016 || attachee.GetMap() == 5017 || attachee.GetMap() == 5018))
            {
                if ((GetGlobalVar(501) == 4 || GetGlobalVar(501) == 5 || GetGlobalVar(501) == 6 || GetGlobalVar(510) == 2))
                {
                    attachee.SetObjectFlag(ObjectFlag.OFF);
                }
                else
                {
                    attachee.ClearObjectFlag(ObjectFlag.OFF);
                }

            }
            else if ((attachee.GetMap() == 5006 || attachee.GetMap() == 5014 || attachee.GetMap() == 5015))
            {
                if ((GetGlobalVar(510) != 2))
                {
                    if ((GetGlobalVar(501) == 4 || GetGlobalVar(501) == 5 || GetGlobalVar(501) == 6))
                    {
                        attachee.ClearObjectFlag(ObjectFlag.OFF);
                    }

                }
                else
                {
                    attachee.SetObjectFlag(ObjectFlag.OFF);
                }

            }
            else if ((attachee.GetMap() == 5042))
            {
                if ((GetGlobalVar(510) == 2))
                {
                    attachee.SetObjectFlag(ObjectFlag.OFF);
                }

            }

            return RunDefault;
        }
        public override bool OnDying(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (CombatStandardRoutines.should_modify_CR(attachee))
            {
                CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
            }

            return RunDefault;
        }
        public override bool OnEnterCombat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((PartyLeader.HasReputation(92)))
            {
                attachee.FloatLine(1000, triggerer);
            }

            if (triggerer.GetPartyMembers().Any(o => o.HasFollowerByName(8000)))
            {
                var elmo = Utilities.find_npc_near(triggerer, 8000);
                if ((elmo != null))
                {
                    triggerer.RemoveFollower(elmo);
                    elmo.FloatLine(20000, triggerer);
                    elmo.Attack(triggerer);
                }

            }

            if (triggerer.GetPartyMembers().Any(o => o.HasFollowerByName(8001)))
            {
                var paida = Utilities.find_npc_near(triggerer, 8001);
                if ((paida != null))
                {
                    triggerer.RemoveFollower(paida);
                    paida.FloatLine(20000, triggerer);
                    paida.Attack(triggerer);
                }

            }

            if (triggerer.GetPartyMembers().Any(o => o.HasFollowerByName(8014)))
            {
                var otis = Utilities.find_npc_near(triggerer, 8014);
                if ((otis != null))
                {
                    triggerer.RemoveFollower(otis);
                    otis.FloatLine(20000, triggerer);
                    otis.Attack(triggerer);
                }

            }

            if (triggerer.GetPartyMembers().Any(o => o.HasFollowerByName(8015)))
            {
                var meleny = Utilities.find_npc_near(triggerer, 8015);
                if ((meleny != null))
                {
                    triggerer.RemoveFollower(meleny);
                    meleny.FloatLine(20000, triggerer);
                    meleny.Attack(triggerer);
                }

            }

            if (triggerer.GetPartyMembers().Any(o => o.HasFollowerByName(8021)))
            {
                var ydey = Utilities.find_npc_near(triggerer, 8021);
                if ((ydey != null))
                {
                    triggerer.RemoveFollower(ydey);
                    ydey.FloatLine(20000, triggerer);
                    ydey.Attack(triggerer);
                }

            }

            if (triggerer.GetPartyMembers().Any(o => o.HasFollowerByName(8022)))
            {
                var murfles = Utilities.find_npc_near(triggerer, 8022);
                if ((murfles != null))
                {
                    triggerer.RemoveFollower(murfles);
                    murfles.FloatLine(20000, triggerer);
                    murfles.Attack(triggerer);
                }

            }

            if (triggerer.GetPartyMembers().Any(o => o.HasFollowerByName(8031)))
            {
                var thrommel = Utilities.find_npc_near(triggerer, 8031);
                if ((thrommel != null))
                {
                    triggerer.RemoveFollower(thrommel);
                    thrommel.FloatLine(20000, triggerer);
                    thrommel.Attack(triggerer);
                }

            }

            if (triggerer.GetPartyMembers().Any(o => o.HasFollowerByName(8039)))
            {
                var taki = Utilities.find_npc_near(triggerer, 8039);
                if ((taki != null))
                {
                    triggerer.RemoveFollower(taki);
                    taki.FloatLine(20000, triggerer);
                    taki.Attack(triggerer);
                }

            }

            if (triggerer.GetPartyMembers().Any(o => o.HasFollowerByName(8054)))
            {
                var burne = Utilities.find_npc_near(triggerer, 8054);
                if ((burne != null))
                {
                    triggerer.RemoveFollower(burne);
                    burne.FloatLine(20000, triggerer);
                    burne.Attack(triggerer);
                }

            }

            if (triggerer.GetPartyMembers().Any(o => o.HasFollowerByName(8060)))
            {
                var morgan = Utilities.find_npc_near(triggerer, 8060);
                if ((morgan != null))
                {
                    triggerer.RemoveFollower(morgan);
                    morgan.FloatLine(20000, triggerer);
                    morgan.Attack(triggerer);
                }

            }

            if (triggerer.GetPartyMembers().Any(o => o.HasFollowerByName(8069)))
            {
                var pishella = Utilities.find_npc_near(triggerer, 8069);
                if ((pishella != null))
                {
                    triggerer.RemoveFollower(pishella);
                    pishella.FloatLine(20000, triggerer);
                    pishella.Attack(triggerer);
                }

            }

            if (triggerer.GetPartyMembers().Any(o => o.HasFollowerByName(8071)))
            {
                var rufus = Utilities.find_npc_near(triggerer, 8071);
                if ((rufus != null))
                {
                    triggerer.RemoveFollower(rufus);
                    rufus.FloatLine(20000, triggerer);
                    rufus.Attack(triggerer);
                }

            }

            if (triggerer.GetPartyMembers().Any(o => o.HasFollowerByName(8072)))
            {
                var spugnoir = Utilities.find_npc_near(triggerer, 8072);
                if ((spugnoir != null))
                {
                    triggerer.RemoveFollower(spugnoir);
                    spugnoir.FloatLine(20000, triggerer);
                    spugnoir.Attack(triggerer);
                }

            }

            if (triggerer.GetPartyMembers().Any(o => o.HasFollowerByName(8714)))
            {
                var holly = Utilities.find_npc_near(triggerer, 8714);
                if ((holly != null))
                {
                    triggerer.RemoveFollower(holly);
                    holly.FloatLine(20000, triggerer);
                    holly.Attack(triggerer);
                }

            }

            if (triggerer.GetPartyMembers().Any(o => o.HasFollowerByName(8730)))
            {
                var ronald = Utilities.find_npc_near(triggerer, 8730);
                if ((ronald != null))
                {
                    triggerer.RemoveFollower(ronald);
                    ronald.FloatLine(20000, triggerer);
                    ronald.Attack(triggerer);
                }

            }

            return RunDefault;
        }
        public override bool OnHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((attachee.GetMap() == 5001))
            {
                if ((GetGlobalVar(501) == 4 || GetGlobalVar(501) == 5 || GetGlobalVar(501) == 6 || GetGlobalVar(510) == 2))
                {
                    attachee.SetObjectFlag(ObjectFlag.OFF);
                }
                else
                {
                    attachee.ClearObjectFlag(ObjectFlag.OFF);
                }

            }

            if ((attachee.GetMap() == 5014 || attachee.GetMap() == 5015 || attachee.GetMap() == 5016 || attachee.GetMap() == 5017 || attachee.GetMap() == 5018 || attachee.GetMap() == 5019))
            {
                foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_CRITTERS))
                {
                    if ((obj.GetNameId() == 14614 && attachee.HasLineOfSight(obj)))
                    {
                        attachee.Attack(obj);
                        return RunDefault;
                    }

                }

            }

            if ((attachee.GetMap() == 5015 && !GetGlobalFlag(845) && !GetGlobalFlag(846)))
            {
                if ((attachee.HasLineOfSight(PartyLeader)))
                {
                    PartyLeader.BeginDialog(attachee, 200);
                }

                foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                {
                    if ((attachee.HasLineOfSight(obj)))
                    {
                        obj.BeginDialog(attachee, 200);
                    }

                }

            }

            return RunDefault;
        }
        public override bool OnWillKos(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((!PartyLeader.HasReputation(92)))
            {
                return SkipDefault;
            }

            return RunDefault;
        }

    }
}
