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
using SpicyTemple.Core.Startup.Discovery;
using SpicyTemple.Core.Systems.Script.Extensions;
using SpicyTemple.Core.Utils;
using static SpicyTemple.Core.Systems.Script.ScriptUtilities;

namespace SpicyTemple.Core.Systems.D20.Conditions.TemplePlus
{
    public class ExtraMusic
    {
        public static readonly FeatId ExtraMusicId = (FeatId) ElfHash.Hash("Extra Music");

        public static void EMNewDay(in DispatcherCallbackArgs evt)
        {
            var bardicMusicCount = GameSystems.Feat.HasFeatCount(evt.objHndCaller, ExtraMusicId);
            // Extra Music grants 4 additional uses of Bardic Music each time the feat is taken
            evt.SetConditionArg1(evt.GetConditionArg1() + bardicMusicCount * 4);
        }

        public static void QueryMaxBardicMusic(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Query();
            // Total uses = bard level + extra music count * 4
            var bardicMusicCount = GameSystems.Feat.HasFeatCount(evt.objHndCaller, ExtraMusicId);
            dispIo.return_val += bardicMusicCount * 4;
        }

        [AutoRegister]
        public static readonly ConditionSpec eSF = ConditionSpec.Extend(BardicMusic.Condition)
            .AddHandler(DispatcherType.NewDay, D20DispatcherKey.NEWDAY_REST, EMNewDay)
            .AddQueryHandler("Max Bardic Music", QueryMaxBardicMusic)
            .Build();
    }
}