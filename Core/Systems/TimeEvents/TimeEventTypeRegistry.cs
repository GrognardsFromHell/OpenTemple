using System;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.Systems.ObjScript;
using OpenTemple.Core.Ui;

namespace OpenTemple.Core.Systems.TimeEvents;

internal static class TimeEventTypeRegistry
{
    /// Get the argument types for a specific type of time event
    public static ref readonly TimeEventTypeSpec Get(TimeEventType type)
    {
        return ref TimeEventTypeSpecs[(int) type];
    }

    private static readonly TimeEventTypeSpec[] TimeEventTypeSpecs =
    {
        // Debug
        new(
            GameClockType.RealTime,
            ExpireDebug,
            null,
            false,
            TimeEventArgType.Int,
            TimeEventArgType.Int
        ),

        // Anim
        new(
            GameClockType.GameTimeAnims,
            ExpireAnimEvent,
            null,
            true,
            TimeEventArgType.Int
        ),

        // Bkg Anim
        new(
            GameClockType.RealTime,
            ExpireBkgAnim,
            null,
            false,
            TimeEventArgType.Int,
            TimeEventArgType.Int,
            TimeEventArgType.Int
        ),

        // Fidget Anim
        new(
            GameClockType.RealTime,
            ExpireFidgetAnim,
            null,
            false
        ),

        // Script
        new(
            GameClockType.GameTime,
            ExpireScript,
            null,
            true,
            TimeEventArgType.Int,
            TimeEventArgType.Int,
            TimeEventArgType.Object,
            TimeEventArgType.Object
        ),

        // PythonScript
        new(
            GameClockType.GameTime,
            ExpirePythonScript,
            null,
            true,
            TimeEventArgType.PythonObject,
            TimeEventArgType.PythonObject
        ),

        // Poison
        new(
            GameClockType.GameTime,
            ExpirePoison,
            null,
            true,
            TimeEventArgType.Int,
            TimeEventArgType.Object,
            TimeEventArgType.Int
        ),

        // Normal Healing
        new(
            GameClockType.GameTime,
            ExpireNormalHealing,
            null,
            true,
            TimeEventArgType.Object,
            TimeEventArgType.Int
        ),

        // Subdual Healing
        new(
            GameClockType.GameTime,
            ExpireSubdualHealing,
            null,
            true,
            TimeEventArgType.Object,
            TimeEventArgType.Int
        ),

        // Aging
        new(
            GameClockType.GameTime,
            ExpireAging,
            null,
            true
        ),

        // AI
        new(
            GameClockType.GameTime,
            ExpireAI,
            null,
            false,
            TimeEventArgType.Object,
            TimeEventArgType.Int
        ),

        // AI Delay
        new(
            GameClockType.GameTime,
            ExpireAIDelay,
            null,
            true,
            TimeEventArgType.Object
        ),

        // Combat
        new(
            GameClockType.GameTime,
            ExpireCombat,
            null,
            true
        ),

        // TB Combat
        new(
            GameClockType.RealTime,
            ExpireTBCombat,
            null,
            true
        ),

        // Ambient Lighting
        new(
            GameClockType.GameTime,
            ExpireAmbientLighting,
            null,
            true
        ),

        // WorldMap
        new(
            GameClockType.RealTime,
            ExpireWorldMap,
            null,
            true
        ),

        // Sleeping
        new(
            GameClockType.RealTime,
            ExpireSleeping,
            null,
            false,
            TimeEventArgType.Int
        ),

        // Clock
        new(
            GameClockType.GameTime,
            ExpireClock,
            null,
            true
        ),

        // NPC Wait Here
        new(
            GameClockType.GameTime,
            ExpireNPCWaitHere,
            null,
            true,
            TimeEventArgType.Object
        ),

        // MainMenu
        new(
            GameClockType.RealTime,
            ExpireMainMenu,
            null,
            false,
            TimeEventArgType.Int
        ),

        // Light
        new(
            GameClockType.GameTimeAnims,
            ExpireLight,
            null,
            false,
            TimeEventArgType.Int,
            TimeEventArgType.Int
        ),

        // Lock
        new(
            GameClockType.GameTime,
            ExpireLock,
            null,
            true,
            TimeEventArgType.Object
        ),

        // NPC Respawn
        new(
            GameClockType.GameTime,
            ExpireNPCRespawn,
            null,
            true,
            TimeEventArgType.Object
        ),

        // Decay Dead Bodies
        new(
            GameClockType.GameTime,
            ExpireDecayDeadBodies,
            null,
            true,
            TimeEventArgType.Object,
            TimeEventArgType.Int
        ),

        // Item Decay
        new(
            GameClockType.GameTime,
            ExpireItemDecay,
            null,
            true,
            TimeEventArgType.Object,
            TimeEventArgType.Int
        ),

        // Combat-Focus Wipe
        new(
            GameClockType.GameTime,
            ExpireCombatFocusWipe,
            null,
            true,
            TimeEventArgType.Object,
            TimeEventArgType.Int
        ),

        // Fade
        new(
            GameClockType.GameTime,
            ExpireFade,
            null,
            true,
            TimeEventArgType.Int,
            TimeEventArgType.Int,
            TimeEventArgType.Float,
            TimeEventArgType.Int
        ),

        // GFadeControl
        new(
            GameClockType.RealTime,
            ExpireGFadeControl,
            null,
            true
        ),

        // Teleported
        new(
            GameClockType.GameTime,
            ExpireTeleported,
            null,
            false,
            TimeEventArgType.Object
        ),

        // Scenery Respawn
        new(
            GameClockType.GameTime,
            ExpireSceneryRespawn,
            null,
            true,
            TimeEventArgType.Object
        ),

        // Random Encounters
        new(
            GameClockType.GameTime,
            ExpireRandomEncounters,
            null,
            true
        ),

        // objfade
        new(
            GameClockType.GameTimeAnims,
            GameSystems.ObjFade.TimeEventExpired,
            GameSystems.ObjFade.TimeEventRemoved,
            true,
            TimeEventArgType.Int,
            TimeEventArgType.Object
        ),

        // Action Queue
        new(
            GameClockType.GameTime,
            ExpireActionQueue,
            null,
            true,
            TimeEventArgType.Object,
            TimeEventArgType.Int
        ),

        // Search
        new(
            GameClockType.GameTime,
            ExpireSearch,
            null,
            true,
            TimeEventArgType.Object
        ),

        // intgame_turnbased
        new(
            GameClockType.RealTime,
            ExpireIntgameTurnbased,
            null,
            false,
            TimeEventArgType.Int,
            TimeEventArgType.Int
        ),

        // python_dialog
        new(
            GameClockType.RealTime,
            ExpirePythonDialog,
            null,
            true,
            TimeEventArgType.Object,
            TimeEventArgType.Object,
            TimeEventArgType.Int
        ),

        // encumbered complain
        new(
            GameClockType.GameTime,
            ExpireEncumberedComplain,
            null,
            true,
            TimeEventArgType.Object
        ),

        // PythonRealtime
        new(
            GameClockType.RealTime,
            ExpirePythonRealtime,
            null,
            true,
            TimeEventArgType.PythonObject,
            TimeEventArgType.PythonObject
        ),
    };

    public const uint EmptyStub = 0x101f5850;

    [TempleDllLocation(0x10061250)]
    private static bool ExpireDebug(TimeEvent evt)
    {
        throw new NotImplementedException();
    }

    private static bool ExpireAnimEvent(TimeEvent evt)
    {
        return GameSystems.Anim.ProcessAnimEvent(evt);
    }

    [TempleDllLocation(0x10173830)]
    private static bool ExpireBkgAnim(TimeEvent evt)
    {
        return UiSystems.Anim.BkgAnimTimeEventExpires(evt);
    }

    [TempleDllLocation(0x100144c0)]
    private static bool ExpireFidgetAnim(TimeEvent evt)
    {
        Stub.TODO();
        return true;
    }

    [TempleDllLocation(0x1000bea0)]
    private static bool ExpireScript(TimeEvent evt)
    {
        throw new NotImplementedException();
    }

    [TempleDllLocation(0x100ad560)]
    private static bool ExpirePythonScript(TimeEvent evt)
    {
        throw new NotImplementedException();
    }

    [TempleDllLocation(EmptyStub)]
    private static bool ExpirePoison(TimeEvent evt)
    {
        throw new NotImplementedException();
    }

    [TempleDllLocation(0x1007efb0)]
    private static bool ExpireNormalHealing(TimeEvent evt)
    {
        GameSystems.Critter.ExpireNormalHealing(evt);
        return true;
    }

    [TempleDllLocation(0x1007eca0)]
    private static bool ExpireSubdualHealing(TimeEvent evt)
    {
        GameSystems.Critter.ExpireSubdualHealing(evt);
        return true;
    }

    [TempleDllLocation(EmptyStub)]
    private static bool ExpireAging(TimeEvent evt)
    {
        throw new NotImplementedException();
    }

    [TempleDllLocation(0x1005f090)]
    private static bool ExpireAI(TimeEvent evt)
    {
        var critter = evt.arg1.handle;
        var isFirstHeartbeat = evt.arg2.int32 != 0;
        GameSystems.AI.ExpireTimeEvent(critter, isFirstHeartbeat);
        return true;
    }

    [TempleDllLocation(0x100584b0)]
    private static bool ExpireAIDelay(TimeEvent evt)
    {
        throw new NotImplementedException();
    }

    [TempleDllLocation(EmptyStub)]
    private static bool ExpireCombat(TimeEvent evt)
    {
        throw new NotImplementedException();
    }

    [TempleDllLocation(EmptyStub)]
    private static bool ExpireTBCombat(TimeEvent evt)
    {
        // TODO: Remove this entire time event type since it has an empty callback in Vanilla
        return true;
    }

    private static bool ExpireAmbientLighting(TimeEvent evt)
    {
        UiSystems.Anim.ExpireAmbientLighting(evt);
        return true;
    }

    [TempleDllLocation(EmptyStub)]
    private static bool ExpireWorldMap(TimeEvent evt)
    {
        throw new NotImplementedException();
    }

    [TempleDllLocation(EmptyStub)]
    private static bool ExpireSleeping(TimeEvent evt)
    {
        throw new NotImplementedException();
    }

    [TempleDllLocation(EmptyStub)]
    private static bool ExpireClock(TimeEvent evt)
    {
        throw new NotImplementedException();
    }

    [TempleDllLocation(0x100579a0)]
    private static bool ExpireNPCWaitHere(TimeEvent evt)
    {
        throw new NotImplementedException();
    }

    [TempleDllLocation(0x101119B0)]
    private static bool ExpireMainMenu(TimeEvent evt)
    {
        // TODO: This was seemingly a "UI" callback which was set later due to UI being in a different .lib file
        throw new NotImplementedException();
    }

    [TempleDllLocation(0x100a8010)]
    private static bool ExpireLight(TimeEvent evt)
    {
        throw new NotImplementedException();
    }

    [TempleDllLocation(0x10021230)]
    private static bool ExpireLock(TimeEvent evt)
    {
        throw new NotImplementedException();
    }

    [TempleDllLocation(0x1006deb0)]
    private static bool ExpireNPCRespawn(TimeEvent evt)
    {
        throw new NotImplementedException();
    }

    [TempleDllLocation(0x1007f230)]
    private static bool ExpireDecayDeadBodies(TimeEvent evt)
    {
        throw new NotImplementedException();
    }

    [TempleDllLocation(EmptyStub)]
    private static bool ExpireItemDecay(TimeEvent evt)
    {
        throw new NotImplementedException();
    }

    [TempleDllLocation(0x10080510)]
    private static bool ExpireCombatFocusWipe(TimeEvent evt)
    {
        var npc = evt.arg1.handle;
        if ( GameSystems.Critter.IsDeadNullDestroyed(npc) )
        {
            var combatFocus = npc.GetObject(obj_f.npc_combat_focus);
            if ( combatFocus != null )
            {
                if ( npc.GetLocation().EstimateDistance(combatFocus.GetLocation()) < 30 )
                {
                    GameSystems.Critter.QueueWipeCombatFocus(npc);
                    return true;
                }
                npc.SetObject(obj_f.npc_combat_focus, null);
            }
        }
        return true;
    }

    [TempleDllLocation(0x10051c10)]
    private static bool ExpireFade(TimeEvent evt)
    {
        throw new NotImplementedException();
    }

    [TempleDllLocation(EmptyStub)]
    private static bool ExpireGFadeControl(TimeEvent evt)
    {
        throw new NotImplementedException();
    }

    [TempleDllLocation(0x10025250)]
    private static bool ExpireTeleported(TimeEvent evt)
    {
        var obj = evt.arg1.handle;
        obj?.UpdateRenderingState(true);

        return true;
    }

    [TempleDllLocation(EmptyStub)]
    private static bool ExpireSceneryRespawn(TimeEvent evt)
    {
        throw new NotImplementedException();
    }

    [TempleDllLocation(0x1009a610)]
    private static bool ExpireRandomEncounters(TimeEvent evt)
    {
        throw new NotImplementedException();
    }

    [TempleDllLocation(0x100151f0)]
    private static bool ExpireActionQueue(TimeEvent evt)
    {
        throw new NotImplementedException();
    }

    [TempleDllLocation(0x10046f00)]
    private static bool ExpireSearch(TimeEvent evt)
    {
        Stub.TODO();
        return true;
    }

    [TempleDllLocation(0x1009a880)]
    private static bool ExpireIntgameTurnbased(TimeEvent evt)
    {
        throw new NotImplementedException();
    }

    [TempleDllLocation(0x100acc60)]
    private static bool ExpirePythonDialog(TimeEvent evt)
    {
        var pc = evt.arg1.handle;
        var npc = evt.arg2.handle;
        var line = evt.arg3.int32;
        var script = npc.GetScript(obj_f.scripts_idx, (int) ObjScriptEvent.Dialog);
        GameUiBridge.InitiateDialog(pc, npc, script.scriptId, 0, line);
        return true;
    }

    [TempleDllLocation(0x10037df0)]
    private static bool ExpireEncumberedComplain(TimeEvent evt)
    {
        throw new NotImplementedException();
    }

    [TempleDllLocation(0x100ad560)]
    private static bool ExpirePythonRealtime(TimeEvent evt)
    {
        throw new NotImplementedException();
    }
}