
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
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.Utils;
using static OpenTemple.Core.Systems.Script.ScriptUtilities;

namespace Scripts;

[ObjectScript(444)]
public class SpecialNpcDeath : BaseObjectScript
{
    public override bool OnDying(GameObject attachee, GameObject triggerer)
    {
        if (CombatStandardRoutines.should_modify_CR(attachee))
        {
            CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
        }

        if (attachee.GetNameId() == 14248)
        {
            // Ogre Chief
            ScriptDaemon.record_time_stamp(505);
            ScriptDaemon.record_time_stamp(508);
            ScriptDaemon.set_v(499, ScriptDaemon.get_v(499) + 1);
            if (Math.Pow((ScriptDaemon.get_v(498) / 75f), 3) + Math.Pow((ScriptDaemon.get_v(499) / 38f), 3) + Math.Pow((ScriptDaemon.get_v(500) / 13f), 3) >= 1)
            {
                ScriptDaemon.record_time_stamp(510);
            }

        }
        else if (attachee.GetNameId() == 14156)
        {
            // Earth Commander
            ScriptDaemon.record_time_stamp(506);
            ScriptDaemon.record_time_stamp(507);
            ScriptDaemon.set_v(500, ScriptDaemon.get_v(500) + 1);
            if (Math.Pow((ScriptDaemon.get_v(498) / 75f), 3) + Math.Pow((ScriptDaemon.get_v(499) / 38f), 3) + Math.Pow((ScriptDaemon.get_v(500) / 13f), 3) >= 1)
            {
                ScriptDaemon.record_time_stamp(511);
            }

        }
        else if (attachee.GetNameId() == 8045)
        {
            // Romag
            // actually, he already has a san_dying of his own, so no need for this one
            // we'll keep it as "infrastructure"
            ScriptDaemon.record_time_stamp(456);
        }
        else if (attachee.GetNameId() == 14154)
        {
            // Hartsch
            ScriptDaemon.record_time_stamp(506);
            ScriptDaemon.set_v(500, ScriptDaemon.get_v(500) + 1);
            if (ScriptDaemon.get_v(500) >= 10)
            {
                ScriptDaemon.record_time_stamp(511);
            }

        }
        else if (attachee.GetNameId() == 14244)
        {
            // Juggernaut; makes it go kaboom and fade out
            attachee.SetObjectFlag(ObjectFlag.SEE_THROUGH);
            attachee.SetObjectFlag(ObjectFlag.FLAT);
            attachee.SetObjectFlag(ObjectFlag.TRANSLUCENT);
            attachee.SetObjectFlag(ObjectFlag.CLICK_THROUGH);
            attachee.SetObjectFlag(ObjectFlag.NOHEIGHT);
            attachee.FadeTo(0, 1, 10);
            // attachee.scripts[14] = 444 # so that upon end of combat the Jug will become click-through
            AttachParticles("sp-polymorph other", attachee);
            AttachParticles("sp-unholy water", attachee);
            AttachParticles("sp-enervation-hit", attachee);
            AttachParticles("sp-pyrotechnics-fireworks", attachee);
            AttachParticles("sp-ray of enfeeblement-end", attachee);
            AttachParticles("mon-earthelem-unconceal", attachee);
            AttachParticles("Ass Sunburst", attachee);
            AttachParticles("ef-minocloud", attachee);
            AttachParticles("sp-flare", attachee);
            AttachParticles("sp-mage hand", attachee);
            Sound(15122, 1);
            Sound(15122, 1);
        }

        return RunDefault;
    }
    public override bool OnExitCombat(GameObject attachee, GameObject triggerer)
    {
        if (attachee.GetNameId() == 14244)
        {
            attachee.SetObjectFlag(ObjectFlag.CLICK_THROUGH);
            attachee.FadeTo(110, 1, 3);
        }

        return RunDefault;
    }

}