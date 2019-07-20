using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.Systems.Anim;
using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.Systems.Fade;
using SpicyTemple.Core.Systems.ObjScript;
using SpicyTemple.Core.Systems.Raycast;
using SpicyTemple.Core.Time;

namespace SpicyTemple.Core.Systems.Teleport
{
    public class TeleportSystem : IGameSystem, IResetAwareSystem, ITimeAwareSystem
    {
        [TempleDllLocation(0x10AB74C0)]
        private bool _active;

        [TempleDllLocation(0x10AB74C8)]
        private FadeAndTeleportArgs _currentArgs;

        private string _currentArgsSourceFile;
        private int _currentArgsLineNumber;

        [TempleDllLocation(0x10084ae0)]
        public bool IsProcessing { get; private set; }

        public void Dispose()
        {
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10086480)]
        public void AdvanceTime(TimePoint time)
        {
            if (_active)
            {
                IsProcessing = true;
                Process();
                IsProcessing = false;
                _active = false;
                if (_currentArgs.flags.HasFlag(FadeAndTeleportFlags.unk80000000))
                {
                    if (_currentArgs.flags.HasFlag(FadeAndTeleportFlags.unk20))
                    {
                        GameDrawing.EnableDrawing();
                    }
                }
            }
        }

        [TempleDllLocation(0x10085aa0)]
        private void Process()
        {
            if (_currentArgs.flags.HasFlag(FadeAndTeleportFlags.FadeOut))
            {
                PerformFadeOut(_currentArgs.FadeOutArgs);
            }

            if (_currentArgs.flags.HasFlag(FadeAndTeleportFlags.play_sound))
            {
                GameSystems.SoundGame.Sound(_currentArgs.soundId, 1);
            }

            if (_currentArgs.flags.HasFlag(FadeAndTeleportFlags.play_movie))
            {
                GameSystems.GMovie.PlayMovieId(_currentArgs.movieId, _currentArgs.movieFlags, 0);
                // TODO save_movies_seen(_currentArgs.movieId, v1);
            }

            if (_currentArgs.flags.HasFlag(FadeAndTeleportFlags.play_movie2))
            {
                GameSystems.GMovie.PlayMovieId(_currentArgs.movieId2, _currentArgs.movieFlags2, 0);
            }

            if (_currentArgs.flags.HasFlag(FadeAndTeleportFlags.advance_time))
            {
                GameSystems.TimeEvent.AddGameTime(TimeSpan.FromSeconds(_currentArgs.timeToAdvance));
            }

            if (_currentArgs.destMap == -1)
            {
                _currentArgs.destMap = GameSystems.Map.GetCurrentMapId();
            }

            var leader = GameSystems.Party.GetConsciousLeader();
            if (leader != null)
            {
                GameSystems.Combat.CritterLeaveCombat(leader);
            }

            AddTeleportingObject(_currentArgs.somehandle, _currentArgs.destLoc);

            if (_currentArgs.somehandle.IsCritter())
            {
                foreach (var member in GameSystems.Party.PartyMembers)
                {
                    if (member != _currentArgs.somehandle)
                    {
                        AddTeleportingObject(member, _currentArgs.destLoc);
                    }
                }
            }

            if (_isTeleportingPc)
            {
                GameSystems.MapObject.ClearRectList();
            }

            if (_currentArgs.destMap != GameSystems.Map.GetCurrentMapId())
            {
                if (GameSystems.Party.IsInParty(_currentArgs.somehandle))
                {
                    GameSystems.D20.ObjectRegistry.SendSignalAll(D20DispatcherKey.SIG_Teleport_Prepare);
                    GameSystems.TimeEvent.SaveForMap(_currentArgs.destMap);
                    GameUiBridge.SaveUiFocus();
                    GameSystems.Spell.PrepareSpellTransport();
                }
            }

            if (!TeleportMoveObjects())
            {
                return;
            }

            var currentMapId = GameSystems.Map.GetCurrentMapId();

            if (_isTeleportingPc)
            {
                // Save the ID of the top-level teleport target
                var topLevelTarget = ObjectId.CreateNull();
                if (GameSystems.Party.IsInParty(_currentArgs.somehandle))
                {
                    topLevelTarget = _currentArgs.somehandle.id;
                }
                else if (_currentArgs.somehandle.IsNPC())
                {
                    var obj = GameSystems.Critter.GetLeaderRecursive(_currentArgs.somehandle);
                    topLevelTarget = obj.id;
                }

                var destMapId = _currentArgs.destMap;
                var pGuidOut = _currentArgs.somehandle.id;
                _isTeleportingPc = false;

                if (destMapId != currentMapId)
                {
                    GameSystems.AI.MoveExFollowers();
                    ProcessDayNightTransfer(_currentArgs.destMap, currentMapId);
                    if (!GameSystems.Map.OpenMap(_currentArgs.destMap, false, false))
                    {
                        return;
                    }

                    GameSystems.Location.CenterOn(_currentArgs.destLoc.locx, _currentArgs.destLoc.locy);
                    GameSystems.D20.ObjectRegistry.SendSignalAll(D20DispatcherKey.SIG_Teleport_Reconnect);
                    GameSystems.Player.Restore();
                    GameUiBridge.RestoreUiFocus();
                    GameSystems.Spell.RestoreSpellTransport();
                }

                TryMovePartyMembersToFreeSpots();

                if (topLevelTarget)
                {
                    var savedObj = GameSystems.Object.GetObject(topLevelTarget);
                    if (savedObj != null)
                    {
                        GameSystems.Map.PreloadSectorsAround(_currentArgs.destLoc);
                        GameSystems.Map.MarkVisitedMap(savedObj);
                        GameSystems.Secretdoor.AfterTeleportStuff(_currentArgs.destLoc);
                    }
                }

                if (_currentArgs.destMap != currentMapId)
                {
                    if (pGuidOut)
                    {
                        var savedObj = GameSystems.Object.GetObject(pGuidOut);

                        if (savedObj != null)
                        {
                            foreach (var partyMember in GameSystems.Party.PartyMembers)
                            {
                                GameSystems.Script.ExecuteObjectScript(savedObj,
                                    partyMember, ObjScriptEvent.NewMap);
                            }

                            GameSystems.RandomEncounter.UpdateSleepStatus();
                            GameSystems.Dialog.OnAfterTeleport(_currentArgs.destMap);
                        }
                    }
                }
            }

            if (_currentArgs.flags.HasFlag(FadeAndTeleportFlags.EnableCallback))
            {
                var callback = _currentArgs.callback;
                if (callback != null)
                {
                    var callbackArg = 0;
                    if (_currentArgs.flags.HasFlag(FadeAndTeleportFlags.EnableCallbackArg))
                    {
                        callbackArg = _currentArgs.callbackArg;
                    }

                    callback(callbackArg);
                }
            }

            if (_currentArgs.flags.HasFlag(FadeAndTeleportFlags.CenterOnPartyLeader))
            {
                var firstPartyMember = GameSystems.Party.GetLeader();
                var centerOn = firstPartyMember.GetLocation();
                GameSystems.Location.CenterOn(centerOn.locx, centerOn.locy);
            }

            if (_currentArgs.destMap != currentMapId)
            {
                if (Globals.GameLib.IsIronmanGame)
                {
                    Globals.GameLib.IronmanSave();
                }
                else if (Globals.GameLib.IsAutosaveBetweenMaps)
                {
                    Globals.GameLib.MakeAutoSave();
                }
            }

            if (_currentArgs.flags.HasFlag(FadeAndTeleportFlags.FadeIn))
            {
                PerformFadeIn(_currentArgs.FadeInArgs);
            }

            GameSystems.Map.ShowGameTip();

            FinishFleeing();
        }

        private void FinishFleeing()
        {
            Stub.TODO();

            if (GameSystems.Map.GetFleeInfo(out var fleeInfo))
            {
                throw new NotImplementedException();
                /*
              Obj_List_Vicinity(fleeInfo.enterLocation, 8, (objlist_result *)&it);
              v41 = v65;
              v42 = (int)v65;
              if ( v65 )
              {
                while ( 1 )
                {
                  v43 = obj_get_int32((ObjHndl)v41.id, obj_f.scenery_teleport_to);
                  if ( v43 )
                  {
                      var v51 = 0;
                    if ( GetJumpPoint(v43, 0, 0, &v51, 0) && v51 == pGuidOut.prevMapId )
                      break;
                  }
                  v42 = *(_DWORD *)(v42 + 8);
                  if ( !v42 )
                    goto LABEL_86;
                  v41 = v65;
                }
              }
              else
              {
          LABEL_86:
                GameSystems.Map.ResetFleeTo();
              }
              ObjListFree((objlist_result *)&it);
            }
                       */
            }
        }

        private static void PerformFadeOut(FadeArgs fadeArgs)
        {
            GameSystems.GFade.PerformFade(ref fadeArgs);

            var durationOfFade = fadeArgs.transitionTime * 600.0f;

            var start = TimePoint.Now;
            do
            {
                Globals.GameLoop.RenderFrame();

                GameSystems.GFade.AdvanceTime(TimePoint.Now);
            } while ((TimePoint.Now - start).TotalMilliseconds < durationOfFade);

            GameSystems.GFade.SetGameOpacity(1.0f);
        }

        private static void PerformFadeIn(FadeArgs fadeArgs)
        {
            var durationOfFade = fadeArgs.transitionTime * 600.0f;

            GameSystems.GFade.SetGameOpacity(0.0f);
            GameSystems.GFade.PerformFade(ref fadeArgs);

            var start = TimePoint.Now;
            do
            {
                Globals.GameLoop.RenderFrame();

                GameSystems.GFade.AdvanceTime(TimePoint.Now);
            } while ((TimePoint.Now - start).TotalMilliseconds < durationOfFade);

            GameSystems.GFade.SetGameOpacity(1.0f);
        }

        private void TryMovePartyMembersToFreeSpots()
        {
            foreach (var partyMember in GameSystems.Party.PartyMembers)
            {
                var loc = partyMember.GetLocationFull();
                var objRadius = partyMember.GetRadius();

                // Try moving party members to suitable free locations
                for (var i = 0; i < 18; i++)
                {
                    var iOffset = i * locXY.INCH_PER_SUBTILE;

                    for (var j = -i; j < i; j++)
                    {
                        var jOffset = j * locXY.INCH_PER_SUBTILE;

                        if (TryMoveTo(partyMember, objRadius, loc, jOffset, -iOffset))
                        {
                            return;
                        }

                        if (TryMoveTo(partyMember, objRadius, loc, -jOffset, iOffset))
                        {
                            return;
                        }

                        if (TryMoveTo(partyMember, objRadius, loc, -iOffset, -jOffset))
                        {
                            return;
                        }

                        if (TryMoveTo(partyMember, objRadius, loc, iOffset, jOffset))
                        {
                            return;
                        }
                    }
                }
            }
        }

        private bool TryMoveTo(GameObjectBody partyMember, float radius, LocAndOffsets currentLocation,
            float xOffset, float yOffset)
        {
            var loc = new LocAndOffsets(
                currentLocation.location,
                currentLocation.off_x + xOffset,
                currentLocation.off_y + yOffset
            );
            loc.Normalize();

            const RaycastFlag raycastFlags = RaycastFlag.StopAfterFirstBlockerFound
                                             | RaycastFlag.StopAfterFirstFlyoverFound
                                             | RaycastFlag.HasRadius
                                             | RaycastFlag.HasSourceObj
                                             | RaycastFlag.ExcludeItemObjects;

            using var it = new RaycastPacket();
            it.flags |= raycastFlags;
            it.origin = loc;
            it.targetLoc = loc;
            it.sourceObj = partyMember;
            it.radius = radius;
            if (it.RaycastShortRange() == 0)
            {
                GameSystems.MapObject.Move(partyMember, loc);
                return true;
            }

            return false;
        }

        [TempleDllLocation(0x10085370)]
        private bool TeleportMoveObjects()
        {
            var currentMapId = GameSystems.Map.GetCurrentMapId();
            if (currentMapId != _currentArgs.destMap)
            {
                var destMapName = GameSystems.Map.GetMapName(_currentArgs.destMap);
                var destMapFolder = Path.Join(Globals.GameFolders.CurrentSaveFolder, "maps", destMapName);
                Directory.CreateDirectory(destMapFolder);

                foreach (var teleportingObject in _teleportingObjects)
                {
                    RemoveObjectFromCurrentMap(teleportingObject.Item1);
                }

                foreach (var teleportingObject in _teleportingObjects)
                {
                    teleportingObject.Item1.FreezeIds();
                }

                foreach (var (obj, location) in _teleportingObjects)
                {
                    var flags = obj.GetFlags();
                    obj.SetFlags(flags | ObjectFlag.TELEPORTED | ObjectFlag.DYNAMIC);
                    obj.SetLocation(_currentArgs.destLoc);
                    obj.SetFloat(obj_f.offset_x, 0.0f);
                    obj.SetFloat(obj_f.offset_y, 0.0f);

                    var mobileMdyPath = Path.Join(destMapFolder, "mobile.mdy");

                    using (var writer = new BinaryWriter(new FileStream(mobileMdyPath, FileMode.Append)))
                    {
                        obj.Write(writer);
                    }

                    obj.SetFlags(flags | ObjectFlag.EXTINCT | ObjectFlag.DESTROYED);
                }

                foreach (var teleportingObject in _teleportingObjects)
                {
                    teleportingObject.Item1.UnfreezeIds();
                }

                _teleportingObjects.Clear();
                return true;
            }
            else
            {
                foreach (var (obj, location) in _teleportingObjects)
                {
                    var flags = obj.GetFlags();
                    if (!flags.HasFlag(ObjectFlag.INVENTORY))
                    {
                        ResetObject(obj);
                        GameSystems.MapObject.Move(obj, new LocAndOffsets(location, 0.0f, 0.0f));
                    }
                }

                return true;
            }
        }

        [TempleDllLocation(0x10084fb0)]
        private void RemoveObjectFromCurrentMap(GameObjectBody obj)
        {
            ResetObject(obj);

            if (obj.IsCritter())
            {
                GameSystems.Combat.CritterLeaveCombat(obj);
            }

            GameSystems.D20.RemoveDispatcher(obj);
            if (obj.IsNPC())
            {
                GameSystems.Critter.SaveTeleportMap(obj);
            }

            GameSystems.AI.RemoveAiTimer(obj);
            GameUiBridge.OnObjectDestroyed(obj);
            GameSystems.MapObject.FreeRenderState(obj);
        }

        [TempleDllLocation(0x10084b30)]
        private void ResetObject(GameObjectBody obj)
        {
            GameSystems.Anim.Interrupt(obj, AnimGoalPriority.AGP_5, false);

            if (obj.IsCritter())
            {
                if (obj.IsNPC() && !GameSystems.Party.IsInParty(obj))
                {
                    GameSystems.Critter.SaveTeleportMap(obj);
                    obj.SetObject(obj_f.npc_combat_focus, null);
                    obj.SetObject(obj_f.npc_who_hit_me_last, null);
                    obj.ClearArray(obj_f.npc_ai_list_idx);
                    obj.ClearArray(obj_f.npc_ai_list_type_idx);
                    obj.SetObject(obj_f.npc_substitute_inventory, null);
                }

                obj.SetObject(obj_f.critter_fleeing_from, null);
            }

            GameSystems.Light.RemoveAttachedTo(obj);
        }

        private bool _isTeleportingPc = false;

        private List<Tuple<GameObjectBody, locXY>> _teleportingObjects = new List<Tuple<GameObjectBody, locXY>>();

        [TempleDllLocation(0x100856d0)]
        private void AddTeleportingObject(GameObjectBody obj, locXY location)
        {
            var flags = obj.GetFlags();
            if (flags.HasFlag(ObjectFlag.TEXT))
            {
                GameSystems.TextBubble.Remove(obj);
            }

            if (flags.HasFlag(ObjectFlag.TEXT_FLOATER))
            {
                GameSystems.TextFloater.Remove(obj);
            }

            foreach (var item in obj.EnumerateChildren())
            {
                AddTeleportingObject(item, item.GetLocation());
            }

            _teleportingObjects.Add(Tuple.Create(obj, location));

            if (obj.IsPC())
            {
                _isTeleportingPc = true;
            }
        }

        [TempleDllLocation(0x10084A50)]
        public bool FadeAndTeleport(in FadeAndTeleportArgs tpArgs, [CallerFilePath]
            string sourceFile = "",
            [CallerLineNumber]
            int lineNumber = -1)
        {
            Trace.Assert(tpArgs.somehandle != null);

            if (_active && _currentArgs.flags.HasFlag(FadeAndTeleportFlags.unk80000000))
            {
                return false;
            }
            else
            {
                GameSystems.Map.ResetFleeTo();

                _currentArgs = tpArgs;
                _currentArgsSourceFile = sourceFile;
                _currentArgsLineNumber = lineNumber;
                _active = true;
                if (GameSystems.Party.IsInParty(tpArgs.somehandle))
                {
                    _currentArgs.flags |= FadeAndTeleportFlags.unk80000000;
                    if (_currentArgs.flags.HasFlag(FadeAndTeleportFlags.unk20))
                    {
                        GameDrawing.DisableDrawing();
                    }
                }

                // Map switch means close all UI???
                if (_currentArgs.destMap != -1)
                {
                    GameUiBridge.OnMapChangeBegin(_currentArgs.destMap);
                }

                return true;
            }
        }

        [TempleDllLocation(0x100850a0)]
        public bool HasDayNightTransfer(GameObjectBody obj)
        {
            Stub.TODO();
            return false;
        }

        [TempleDllLocation(0x10085020)]
        public int GetCurrentDayNightTransferMap(GameObjectBody obj)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10085880)]
        private void ProcessDayNightTransfer(int toMap, int fromMap)
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x10085110)]
        public void RemoveDayNightTransfer(GameObjectBody critter)
        {
            Stub.TODO();
        }
    }
}