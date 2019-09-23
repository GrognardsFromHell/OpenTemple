
using System;
using System.Collections.Generic;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Systems;
using SpicyTemple.Core.Systems.Dialog;
using SpicyTemple.Core.Systems.Feats;
using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.Systems.Script;
using SpicyTemple.Core.Systems.Spells;
using SpicyTemple.Core.Systems.GameObjects;
using SpicyTemple.Core.Systems.D20.Conditions;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.Systems.ObjScript;
using SpicyTemple.Core.Ui;
using System.Linq;
using SpicyTemple.Core.Systems.Script.Extensions;
using SpicyTemple.Core.Utils;
using static SpicyTemple.Core.Systems.Script.ScriptUtilities;

namespace Scripts
{
    [ObjectScript(258)]
    public class MarkMapTemple : BaseObjectScript
    {
        public override bool OnUse(GameObjectBody attachee, GameObjectBody triggerer)
        {
            MakeAreaKnown(4);
            StoryState = 4;
            if (attachee.GetNameId() == 11002)
            {
                // note from smigmal Redhand, as opposed to Alira's Temple map
                SetGlobalFlag(425, true);
            }
            else if (attachee.GetNameId() == 11299) // Temple navigational map
            {
                if (!GameSystems.Combat.IsCombatActive())
                {
                    GameObjectBody talk_dude = null;
                    if (SelectedPartyLeader.type == ObjectType.pc && !SelectedPartyLeader.IsUnconscious())
                    {
                        talk_dude = SelectedPartyLeader;
                    }
                    else
                    {
                        foreach (var dude in GameSystems.Party.PartyMembers)
                        {
                            if (dude.type == ObjectType.pc && !dude.IsUnconscious() && talk_dude == null)
                            {
                                talk_dude = dude;
                            }

                        }

                    }

                    if (talk_dude != null)
                    {
                        talk_dude.SetScriptId(ObjScriptEvent.Dialog, 435);
                        if ((talk_dude.GetArea() == 4 || (new[] { 5064, 5065, 5066, 5067, 5079, 5080, 5092, 5105, 5110, 5111, 5112 }).Contains(talk_dude.GetMap())) && !((new[] { 5081, 5082, 5083, 5084 }).Contains(talk_dude.GetMap())))
                        {
                            talk_dude.BeginDialog(talk_dude, 1000);
                        }
                        else
                        {
                            talk_dude.BeginDialog(talk_dude, 1210);
                        }

                    }

                }

                return SkipDefault;
            }

            return RunDefault;
        }

    }
}
