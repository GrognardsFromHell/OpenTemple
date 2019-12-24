
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
    [ObjectScript(322)]
    public class CaptainAsaph : BaseObjectScript
    {
        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            attachee.TurnTowards(triggerer);
            if ((attachee.GetLeader() != null))
            {
                triggerer.BeginDialog(attachee, 90);
            }
            else
            {
                triggerer.TurnTowards(attachee);
                triggerer.BeginDialog(attachee, 1);
            }

            return SkipDefault;
        }
        public override bool OnEnterCombat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            var leader = attachee.GetLeader();
            leader.RemoveFollower(attachee);
            AttachParticles("CounterSpell", attachee);
            Sound(4016, 1);
            SetGlobalVar(989, 2);
            attachee.SetObjectFlag(ObjectFlag.OFF);
            return RunDefault;
        }
        public override bool OnStartCombat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            var asaph = Utilities.find_npc_near(attachee, 14781);
            AttachParticles("CounterSpell", asaph);
            Sound(4016, 1);
            SetGlobalVar(989, 3);
            asaph.SetObjectFlag(ObjectFlag.OFF);
            var guard = Utilities.find_npc_near(attachee, 14716);
            AttachParticles("CounterSpell", guard);
            guard.SetObjectFlag(ObjectFlag.OFF);
            guard = Utilities.find_npc_near(attachee, 14716);
            AttachParticles("CounterSpell", guard);
            guard.SetObjectFlag(ObjectFlag.OFF);
            guard = Utilities.find_npc_near(attachee, 14716);
            AttachParticles("CounterSpell", guard);
            guard.SetObjectFlag(ObjectFlag.OFF);
            guard = Utilities.find_npc_near(attachee, 14716);
            AttachParticles("CounterSpell", guard);
            guard.SetObjectFlag(ObjectFlag.OFF);
            guard = Utilities.find_npc_near(attachee, 14716);
            AttachParticles("CounterSpell", guard);
            guard.SetObjectFlag(ObjectFlag.OFF);
            return false;
        }
        public override bool OnJoin(GameObjectBody attachee, GameObjectBody triggerer)
        {
            var cloak = attachee.FindItemByName(6269);
            cloak.SetItemFlag(ItemFlag.NO_TRANSFER);
            var armor = attachee.FindItemByName(6047);
            armor.SetItemFlag(ItemFlag.NO_TRANSFER);
            var boots = attachee.FindItemByName(6020);
            boots.SetItemFlag(ItemFlag.NO_TRANSFER);
            var gloves = attachee.FindItemByName(6021);
            gloves.SetItemFlag(ItemFlag.NO_TRANSFER);
            var helm = attachee.FindItemByName(6035);
            helm.SetItemFlag(ItemFlag.NO_TRANSFER);
            var shield = attachee.FindItemByName(6499);
            shield.SetItemFlag(ItemFlag.NO_TRANSFER);
            var sword = attachee.FindItemByName(4037);
            sword.SetItemFlag(ItemFlag.NO_TRANSFER);
            return RunDefault;
        }
        public override bool OnDisband(GameObjectBody attachee, GameObjectBody triggerer)
        {
            AttachParticles("CounterSpell", attachee);
            Sound(4016, 1);
            SetGlobalVar(989, 5);
            attachee.SetObjectFlag(ObjectFlag.OFF);
            return SkipDefault;
        }
        public override bool OnNewMap(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((attachee.GetMap() == 5192 || attachee.GetMap() == 5193))
            {
                SelectedPartyLeader.BeginDialog(attachee, 80);
            }
            else if ((attachee.GetMap() != 5093 && attachee.GetMap() != 5192 && attachee.GetMap() != 5193))
            {
                var leader = attachee.GetLeader();
                leader.RemoveFollower(attachee);
                AttachParticles("CounterSpell", attachee);
                Sound(4016, 1);
                SetGlobalVar(989, 4);
                attachee.SetObjectFlag(ObjectFlag.OFF);
            }

            return SkipDefault;
        }
        public override bool OnHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((!GameSystems.Combat.IsCombatActive()))
            {
                foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                {
                    if ((Utilities.is_safe_to_talk(attachee, obj)))
                    {
                        if ((!attachee.HasMet(obj)))
                        {
                            attachee.TurnTowards(obj);
                            obj.BeginDialog(attachee, 1);
                            DetachScript();
                        }

                    }

                }

            }

            return RunDefault;
        }
        public static void kill_asaph(GameObjectBody attachee, GameObjectBody triggerer)
        {
            AttachParticles("CounterSpell", attachee);
            SetGlobalVar(989, 1);
            attachee.SetObjectFlag(ObjectFlag.OFF);
            var guard = Utilities.find_npc_near(attachee, 14716);
            AttachParticles("CounterSpell", guard);
            guard.SetObjectFlag(ObjectFlag.OFF);
            guard = Utilities.find_npc_near(attachee, 14716);
            AttachParticles("CounterSpell", guard);
            guard.SetObjectFlag(ObjectFlag.OFF);
            guard = Utilities.find_npc_near(attachee, 14716);
            AttachParticles("CounterSpell", guard);
            guard.SetObjectFlag(ObjectFlag.OFF);
            guard = Utilities.find_npc_near(attachee, 14716);
            AttachParticles("CounterSpell", guard);
            guard.SetObjectFlag(ObjectFlag.OFF);
            guard = Utilities.find_npc_near(attachee, 14716);
            AttachParticles("CounterSpell", guard);
            guard.SetObjectFlag(ObjectFlag.OFF);
            return;
        }
        public static void kill_asaph_guards(GameObjectBody attachee, GameObjectBody triggerer)
        {
            var guard = Utilities.find_npc_near(attachee, 14716);
            AttachParticles("CounterSpell", guard);
            guard.SetObjectFlag(ObjectFlag.OFF);
            guard = Utilities.find_npc_near(attachee, 14716);
            AttachParticles("CounterSpell", guard);
            guard.SetObjectFlag(ObjectFlag.OFF);
            guard = Utilities.find_npc_near(attachee, 14716);
            AttachParticles("CounterSpell", guard);
            guard.SetObjectFlag(ObjectFlag.OFF);
            guard = Utilities.find_npc_near(attachee, 14716);
            AttachParticles("CounterSpell", guard);
            guard.SetObjectFlag(ObjectFlag.OFF);
            guard = Utilities.find_npc_near(attachee, 14716);
            AttachParticles("CounterSpell", guard);
            guard.SetObjectFlag(ObjectFlag.OFF);
            return;
        }

    }
}
