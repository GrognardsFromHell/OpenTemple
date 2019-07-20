using System;
using System.Collections.Generic;
using System.Diagnostics;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.Logging;
using SpicyTemple.Core.Systems.Feats;
using SpicyTemple.Core.Systems.Pathfinding;

namespace SpicyTemple.Core.Systems.D20.Actions
{
    public enum CursorType
    {
        Sword = 1,
        Arrow = 2,
        FeetGreen = 3,
        FeetYellow = 4,
        SlidePortraits = 5,
        Locked = 6,
        HaveKey = 7,
        UseSkill = 8,
        UsePotion = 9,
        UseSpell = 10,
        UseTeleportIcon = 11,
        HotKeySelection = 12,
        Talk = 13,
        IdentifyCursor = 14,
        IdentifyCursor2 = 15,
        ArrowInvalid = 16,
        SwordInvalid = 17,
        ArrowInvalid2 = 18,
        FeetRed = 19,
        FeetRed2 = 20,
        InvalidSelection = 21,
        Locked2 = 22,
        HaveKey2 = 23,
        UseSkillInvalid = 24,
        UsePotionInvalid = 25,
        UseSpellInvalid = 26,
        PlaceFlag = 27,
        HotKeySelectionInvalid = 28,
        InvalidSelection2 = 29,
        InvalidSelection3 = 30,
        InvalidSelection4 = 31,
        AttackOfOpportunity = 32,
        AttackOfOpportunityGrey = 33,
    }

    public class D20ActionSystem : IDisposable
    {
        private static readonly ILogger Logger = new ConsoleLogger();

        private static readonly Dictionary<CursorType, string> CursorPaths = new Dictionary<CursorType, string>
        {
            {CursorType.AttackOfOpportunity, "art/interface/combat_ui/attack-of-opportunity.tga"},
            {CursorType.AttackOfOpportunityGrey, "art/interface/combat_ui/attack-of-opportunity-grey.tga"},
            {CursorType.Sword, "art/interface/cursors/sword.tga"},
            {CursorType.Arrow, "art/interface/cursors/arrow.tga"},
            {CursorType.FeetGreen, "art/interface/cursors/feet_green.tga"},
            {CursorType.FeetYellow, "art/interface/cursors/feet_yellow.tga"},
            {CursorType.SlidePortraits, "art/interface/cursors/SlidePortraits.tga"},
            {CursorType.Locked, "art/interface/cursors/Locked.tga"},
            {CursorType.HaveKey, "art/interface/cursors/havekey.tga"},
            {CursorType.UseSkill, "art/interface/cursors/useskill.tga"},
            {CursorType.UsePotion, "art/interface/cursors/usepotion.tga"},
            {CursorType.UseSpell, "art/interface/cursors/usespell.tga"},
            {CursorType.UseTeleportIcon, "art/interface/cursors/useteleporticon.tga"},
            {CursorType.HotKeySelection, "art/interface/cursors/hotkeyselection.tga"},
            {CursorType.Talk, "art/interface/cursors/talk.tga"},
            {CursorType.IdentifyCursor, "art/interface/cursors/IdentifyCursor.tga"},
            {CursorType.IdentifyCursor2, "art/interface/cursors/IdentifyCursor.tga"},
            {CursorType.ArrowInvalid, "art/interface/cursors/arrow_invalid.tga"},
            {CursorType.SwordInvalid, "art/interface/cursors/sword_invalid.tga"},
            {CursorType.ArrowInvalid2, "art/interface/cursors/arrow_invalid.tga"},
            {CursorType.FeetRed, "art/interface/cursors/feet_red.tga"},
            {CursorType.FeetRed2, "art/interface/cursors/feet_red.tga"},
            {CursorType.InvalidSelection, "art/interface/cursors/InvalidSelection.tga"},
            {CursorType.Locked2, "art/interface/cursors/locked.tga"},
            {CursorType.HaveKey2, "art/interface/cursors/havekey.tga"},
            {CursorType.UseSkillInvalid, "art/interface/cursors/useskill_invalid.tga"},
            {CursorType.UsePotionInvalid, "art/interface/cursors/usepotion_invalid.tga"},
            {CursorType.UseSpellInvalid, "art/interface/cursors/usespell_invalid.tga"},
            {CursorType.PlaceFlag, "art/interface/cursors/placeflagcursor.tga"},
            {CursorType.HotKeySelectionInvalid, "art/interface/cursors/hotkeyselection_invalid.tga"},
            {CursorType.InvalidSelection2, "art/interface/cursors/invalidSelection.tga"},
            {CursorType.InvalidSelection3, "art/interface/cursors/invalidSelection.tga"},
            {CursorType.InvalidSelection4, "art/interface/cursors/invalidSelection.tga"},
        };

        private readonly Dictionary<int, PythonActionSpec> _pythonActions = new Dictionary<int, PythonActionSpec>();

        public D20Action globD20Action = new D20Action();

        [TempleDllLocation(0x1186A8F0)]
        internal ActionSequence CurrentSequence { get; private set; }

        [TempleDllLocation(0x118CD2A0)]
        internal int actSeqTargetsIdx { get; set; }

        [TempleDllLocation(0x10B3D5A0)]
        internal bool actSeqPickerActive { get; private set; }

        [TempleDllLocation(0x1186A900)]
        private List<ReadiedActionPacket> _readiedActions = new List<ReadiedActionPacket>();

        [TempleDllLocation(0x10092800)]
        public D20ActionSystem()
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x10089ef0)]
        public void Dispose()
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x10097be0)]
        public void ActionSequencesResetOnCombatEnd()
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x10095fd0)]
        public bool TurnBasedStatusInit(GameObjectBody obj)
        {
            // NOTE: TEMPLEPLUS REPLACED
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10089f70)]
        public TurnBasedStatus curSeqGetTurnBasedStatus()
        {
            throw new NotImplementedException();
            return null;
        }


        // describes the new hourglass state when current state is i after doing an action that costs j
        [TempleDllLocation(0x102CC538)]
        private static readonly int[,] TurnBasedStatusTransitionMatrix = new int[7, 5]
        {
            {0, -1, -1, -1, -1},
            {1, 0, -1, -1, -1},
            {2, 0, 0, -1, -1},
            {3, 0, 0, 0, -1},
            {4, 2, 1, -1, 0},
            {5, 2, 2, -1, 0},
            {6, 5, 2, -1, 3}
        };

        [TempleDllLocation(0x1008b020)]
        public HourglassState GetHourglassTransition(HourglassState hourglassCurrent, int hourglassCost)
        {
            if (hourglassCurrent == HourglassState.INVALID)
            {
                return hourglassCurrent;
            }

            return (HourglassState) TurnBasedStatusTransitionMatrix[(int) hourglassCurrent, hourglassCost];
        }

        [TempleDllLocation(0x10094A00)]
        public void CurSeqReset(GameObjectBody obj)
        {
            // NOTE: TEMPLEPLUS REPLACED
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x100949e0)]
        public void GlobD20ActnInit()
        {
            // NOTE: TEMPLEPLUS REPLACED
            // D20ActnInit(globD20Action->d20APerformer, globD20Action);
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10089f80)]
        public void GlobD20ActnSetTypeAndData1(D20ActionType type, int data1)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10097C20)]
        public void ActionAddToSeq()
        {
            // NOTE: TEMPLEPLUS REPLACED
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x100961C0)]
        public void sequencePerform()
        {
            // NOTE: TEMPLEPLUS REPLACED
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10098c90)]
        public bool DoUseItemAction(GameObjectBody holder, GameObjectBody aiObj, GameObjectBody item)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10099cf0)]
        public void PerformOnAnimComplete(GameObjectBody obj, int uniqueId)
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x100933F0)]
        public void ActionFrameProcess(GameObjectBody obj)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10089f00)]
        public bool WasInterrupted(GameObjectBody obj)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x1004e790)]
        public void dispatchTurnBasedStatusInit(GameObjectBody obj, DispIOTurnBasedStatus dispIo)
        {
            var dispatcher = obj.GetDispatcher();
            if (dispatcher != null)
            {
                dispatcher.Process(DispatcherType.TurnBasedStatusInit, D20DispatcherKey.NONE, dispIo);
                if (dispIo.tbStatus != null)
                {
                    if (dispIo.tbStatus.hourglassState > 0)
                    {
                        GameSystems.D20.D20SendSignal(obj, D20DispatcherKey.SIG_BeginTurn);
                    }
                }
            }
        }

        [TempleDllLocation(0x1004ED70)]
        public int dispatch1ESkillLevel(GameObjectBody critter, SkillId skill, ref BonusList bonusList,
            GameObjectBody opposingObj, int flag)
        {
            var dispatcher = critter.GetDispatcher();
            if (dispatcher == null)
            {
                return 0;
            }

            DispIoObjBonus dispIO = DispIoObjBonus.Default;
            dispIO.flags = flag;
            dispIO.obj = opposingObj;
            dispIO.bonlist = bonusList;
            dispatcher.Process(DispatcherType.SkillLevel, (D20DispatcherKey) (skill + 20), dispIO);
            bonusList = dispIO.bonlist;
            return dispIO.bonlist.OverallBonus;
        }

        [TempleDllLocation(0x1004ED70)]
        public int dispatch1ESkillLevel(GameObjectBody critter, SkillId skill, GameObjectBody opposingObj, int flag)
        {
            var noBonus = BonusList.Default;
            return dispatch1ESkillLevel(critter, skill, ref noBonus, opposingObj, flag);
        }

        [TempleDllLocation(0x10099b10)]
        public void ProjectileHit(GameObjectBody projectile, GameObjectBody critter)
        {
            throw new NotImplementedException();
        }


        [TempleDllLocation(0x118CD3B8)]
        internal D20TargetClassification seqPickerTargetingType; // init to -1

        [TempleDllLocation(0x118A0980)]
        internal D20ActionType seqPickerD20ActnType; // init to 1

        [TempleDllLocation(0x118CD570)]
        internal int seqPickerD20ActnData1; // init to 0

        [TempleDllLocation(0x1008a0f0)]
        public bool SeqPickerHasTargetingType()
        {
            return seqPickerTargetingType != D20TargetClassification.Invalid;
        }

        [TempleDllLocation(0x1008a0b0)]
        public void SeqPickerTargetingTypeReset()
        {
            seqPickerTargetingType = D20TargetClassification.Invalid;
            seqPickerD20ActnType = D20ActionType.UNSPECIFIED_ATTACK;
            seqPickerD20ActnData1 = 0;
        }

        [TempleDllLocation(0x1008a050)]
        public bool IsCurrentlyPerforming(GameObjectBody actor)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10092e50)]
        public void GlobD20ActnSetTarget(GameObjectBody target, LocAndOffsets? location)
        {
            throw new NotImplementedException();
        }

        public void ActionTypeAutomatedSelection(GameObjectBody obj)
        {
            void SetGlobD20Action(D20ActionType actType, int data1)
            {
                globD20Action.d20ActType = actType;
                globD20Action.data1 = data1;
            }

            D20TargetClassification targetingType = D20TargetClassification.Movement; // default
            if (obj != null)
            {
                switch (obj.type)
                {
                    case ObjectType.portal:
                    case ObjectType.scenery:
                        targetingType = D20TargetClassification.Movement;
                        break;

                    case ObjectType.container:
                    case ObjectType.projectile:
                    case ObjectType.weapon:
                    case ObjectType.ammo:
                    case ObjectType.armor:
                    case ObjectType.money:
                    case ObjectType.food:
                    case ObjectType.scroll:
                    case ObjectType.key:
                    case ObjectType.written:
                    case ObjectType.generic:
                    case ObjectType.trap:
                    case ObjectType.bag:
                        targetingType = D20TargetClassification.ItemInteraction;
                        break;
                    case ObjectType.pc:
                    case ObjectType.npc:
                        targetingType = D20TargetClassification.SingleExcSelf;
                        break;
                    default:
                        targetingType = D20TargetClassification.Movement;
                        break;
                }
            }

            if (seqPickerTargetingType == targetingType
                || seqPickerTargetingType == D20TargetClassification.SingleIncSelf &&
                targetingType == D20TargetClassification.SingleExcSelf
                || seqPickerTargetingType != D20TargetClassification.Invalid)
            {
                SetGlobD20Action(seqPickerD20ActnType, seqPickerD20ActnData1);
                return;
            }


            // if no targeting type defined for picker:
            if (obj == null)
            {
                SetGlobD20Action(D20ActionType.UNSPECIFIED_MOVE, 0);
                return;
            }

            switch (obj.type)
            {
                case ObjectType.portal:
                case ObjectType.scenery:
                    SetGlobD20Action(D20ActionType.UNSPECIFIED_MOVE, 0);
                    return;
                case ObjectType.projectile:
                case ObjectType.weapon:
                case ObjectType.ammo:
                case ObjectType.armor:
                case ObjectType.money:
                case ObjectType.food:
                case ObjectType.scroll:
                case ObjectType.key:
                case ObjectType.written:
                case ObjectType.generic:
                case ObjectType.trap:
                case ObjectType.bag:
                    SetGlobD20Action(D20ActionType.PICKUP_OBJECT, 0);
                    return;
                case ObjectType.container:
                    SetGlobD20Action(D20ActionType.OPEN_CONTAINER, 0);
                    return;
                case ObjectType.pc:
                case ObjectType.npc:
                    break;
                default:
                    return;
            }

            // Critters

            var d20a = globD20Action;
            var performer = d20a.d20APerformer;
            if (GameSystems.Critter.IsFriendly(obj, performer))
            {
                // || GameSystems.Critter.NpcAllegianceShared(handle, performer)){ // NpcAllegianceShared check is currently not a good idea since ToEE has poor Friend or Foe tracking when it comes to faction mates...

                var performerLeader = GameSystems.Critter.GetLeader(performer);
                if (performerLeader == null || GameSystems.Critter.IsFriendly(obj, performerLeader))
                {
                    if (GameSystems.D20.D20Query(performer, D20DispatcherKey.QUE_HoldingCharge) == 0)
                    {
                        SetGlobD20Action(D20ActionType.UNSPECIFIED_MOVE, 0);
                        return;
                    }
                }
            }
            else if (GameSystems.Critter.IsDeadNullDestroyed(obj) && GameSystems.Critter.IsLootableCorpse(obj))
            {
                SetGlobD20Action(D20ActionType.OPEN_CONTAINER, 0);
                return;
            }

            SetGlobD20Action(D20ActionType.UNSPECIFIED_ATTACK, 0);
        }

        private struct ProjectileEntry
        {
            public D20Action d20a;
            public int pad4;
            public GameObjectBody projectile;
            public GameObjectBody ammoItem;
        }

        [TempleDllLocation(0x118A0720)]
        private List<ProjectileEntry> _projectiles = new List<ProjectileEntry>();

        [TempleDllLocation(0x1008B1E0)]
        public bool ProjectileAppend(D20Action action, GameObjectBody projHndl, GameObjectBody thrownItem)
        {
            if (projHndl == null)
            {
                return false;
            }

            _projectiles.Add(new ProjectileEntry
            {
                d20a = action,
                ammoItem = thrownItem,
                projectile = projHndl
            });

            return true;
        }

        public D20ADF GetActionFlags(D20ActionType d20ActionType)
        {
            throw new NotImplementedException();
        }

        public int DispatchD20ActionCheck(D20Action action, TurnBasedStatus turnBasedStatus,
            DispatcherType dispType)
        {
            var dispatcherKey = D20DispatcherKey.D20A_UNSPECIFIED_MOVE + (int) action.d20ActType;

            var dispatcher = action.d20APerformer.GetDispatcher();
            if (dispatcher != null)
            {
                var dispIo = new DispIoD20ActionTurnBased(action);
                dispIo.tbStatus = turnBasedStatus;
                if (dispType == DispatcherType.GetBonusAttacks)
                {
                    dispIo.bonlist = BonusList.Default;
                    dispatcher.Process(dispType, dispatcherKey, dispIo);
                    return (int) (dispIo.returnVal + dispIo.bonlist.OverallBonus);
                }
                else
                {
                    dispatcher.Process(dispType, dispatcherKey, dispIo);
                    return (int) dispIo.returnVal;
                }
            }

            return 0;
        }

        [TempleDllLocation(0x1008bfa0)]
        public ActionErrorCode AddToSeqSimple(D20Action action, ActionSequence actSeq, TurnBasedStatus tbStatus)
        {
            actSeq.d20ActArray.Add(action);
            return ActionErrorCode.AEC_OK;
        }

        public PythonActionSpec GetPythonAction(int id) => _pythonActions[id];

        [TempleDllLocation(0x10095450)]
        public ActionErrorCode AddToSeqWithTarget(D20Action action, ActionSequence sequence, TurnBasedStatus tbStatus)
        {
            var target = action.d20ATarget;
            var actNum = sequence.d20ActArrayNum;
            if (target == null)
            {
                return ActionErrorCode.AEC_TARGET_INVALID;
            }

            // check if target is within reach
            var reach = action.d20APerformer.GetReach(action.d20ActType);
            if (action.d20APerformer.DistanceToObjInFeet(action.d20ATarget) < reach)
            {
                sequence.d20ActArray.Add(action);
                return ActionErrorCode.AEC_OK;
            }

            // if not, add a move sequence
            var d20aCopy = action.Copy();
            d20aCopy.d20ActType = D20ActionType.UNSPECIFIED_MOVE;
            d20aCopy.destLoc = target.GetLocationFull();
            var result = MoveSequenceParse(d20aCopy, sequence, tbStatus, 0.0f, reach, true);
            if (result == ActionErrorCode.AEC_OK)
            {
                var tbStatusCopy = tbStatus.Copy();
                sequence.d20ActArray.Add(action);
                if (actNum < sequence.d20ActArrayNum)
                {
                    for (; actNum < sequence.d20ActArrayNum; actNum++)
                    {
                        var otherAction = sequence.d20ActArray[actNum];
                        result = TurnBasedStatusUpdate(otherAction, tbStatusCopy);
                        if (result != ActionErrorCode.AEC_OK)
                        {
                            tbStatusCopy.errCode = result;
                            return result;
                        }

                        var actionType = otherAction.d20ActType;
                        var actionCheckFunc = D20ActionDefs.GetActionDef(actionType).actionCheckFunc;
                        if (actionCheckFunc != null)
                        {
                            result = actionCheckFunc(otherAction, tbStatusCopy);
                            if (result != ActionErrorCode.AEC_OK)
                            {
                                return result;
                            }
                        }
                    }

                    if (actNum >= sequence.d20ActArrayNum)
                        return ActionErrorCode.AEC_OK;
                    tbStatusCopy.errCode = result;
                    if (result != ActionErrorCode.AEC_OK)
                        return result;
                }

                return ActionErrorCode.AEC_OK;
            }

            return result;
        }

        [TempleDllLocation(0x10B3D5C8)]
        [TempleDllLocation(0x100927f0)]
        public bool rollbackSequenceFlag { get; private set; }

        [TempleDllLocation(0x10B3D5C0)]
        public bool performingDefaultAction { get; set; }

        [TempleDllLocation(0x10094f70)]
        internal ActionErrorCode MoveSequenceParse(D20Action d20aIn, ActionSequence actSeq, TurnBasedStatus tbStat,
            float distToTgtMin, float reach, bool nonspecificMoveType)
        {
            D20Action d20aCopy;
            TurnBasedStatus tbStatCopy = tbStat.Copy();
            LocAndOffsets locAndOffCopy = d20aIn.destLoc;

            seqCheckFuncs(tbStatCopy);

            // if prone, add a standup action
            if (GameSystems.D20.D20Query(d20aIn.d20APerformer, D20DispatcherKey.QUE_Prone) != 0)
            {
                d20aCopy = d20aIn.Copy();
                d20aCopy.d20ActType = D20ActionType.STAND_UP;
                actSeq.d20ActArray.Add(d20aCopy);
            }

            d20aCopy = d20aIn.Copy();
            var d20a = d20aIn;

            var pathQ = new PathQuery();
            pathQ.critter = d20aCopy.d20APerformer;
            pathQ.from = actSeq.performerLoc;

            if (d20a.d20ATarget != null)
            {
                pathQ.targetObj = d20a.d20ATarget;
                if (pathQ.critter == pathQ.targetObj)
                    return ActionErrorCode.AEC_TARGET_INVALID;

                pathQ.flags = PathQueryFlags.PQF_TO_EXACT | PathQueryFlags.PQF_HAS_CRITTER | PathQueryFlags.PQF_800
                              | PathQueryFlags.PQF_TARGET_OBJ | PathQueryFlags.PQF_ADJUST_RADIUS |
                              PathQueryFlags.PQF_ADJ_RADIUS_REQUIRE_LOS;

                if (reach < 0.1f)
                {
                    reach = 3.0f; // 3 feet minimum reach
                }

                actSeq.targetObj = d20a.d20ATarget;
                pathQ.distanceToTargetMin = distToTgtMin * 12.0f; // Feet to inches
                var fourPointSevenPlusEight = locXY.INCH_PER_SUBTILE / 2 + 8.0f;
                pathQ.tolRadius = reach * 12.0f - fourPointSevenPlusEight; // Feet to inches
            }
            else
            {
                pathQ.to = d20aIn.destLoc;
                pathQ.flags = PathQueryFlags.PQF_TO_EXACT | PathQueryFlags.PQF_HAS_CRITTER | PathQueryFlags.PQF_800
                              | PathQueryFlags.PQF_ALLOW_ALTERNATIVE_TARGET_TILE;
            }

            if (d20a.d20Caf.HasFlag(D20CAF.UNNECESSARY))
            {
                pathQ.flags |= PathQueryFlags.PQF_A_STAR_TIME_CAPPED;
            }

            // frees the last path used in the d20a
            ReleasePooledPathQueryResult(ref d20aCopy.path);

            // get new path result slot
            // find path
            rollbackSequenceFlag = false;
            if ((d20aCopy.d20Caf.HasFlag(D20CAF.CHARGE)) || d20a.d20ActType == D20ActionType.RUN ||
                d20a.d20ActType == D20ActionType.DOUBLE_MOVE)
            {
                pathQ.flags |= PathQueryFlags.PQF_DONT_USE_PATHNODES; // so it runs in a straight line
            }

            var pathResult = GameSystems.PathX.FindPath(pathQ, out d20aCopy.path);
            var pqResult = d20aCopy.path;
            if (!pathResult)
            {
                if (pqResult.flags.HasFlag(PathFlags.PF_TIMED_OUT))
                {
                    rollbackSequenceFlag = true;
                }

                if (pathQ.targetObj != null)
                    Logger.Debug("MoveSequenceParse: FAILED PATH... {0} attempted from {1} to {2} ({3})",
                        GameSystems.MapObject.GetDisplayName(pqResult.mover), pqResult.from, pqResult.to,
                        GameSystems.MapObject.GetDisplayName(pathQ.targetObj));
                else
                    Logger.Debug("MoveSequenceParse: FAILED PATH... {0} attempted from {1} to {2}",
                        GameSystems.MapObject.GetDisplayName(pqResult.mover), pqResult.from, pqResult.to);

                ReleasePooledPathQueryResult(ref d20aCopy.path);
                return ActionErrorCode.AEC_TARGET_INVALID;
            }

            var pathLength = pqResult.GetPathResultLength();
            d20aCopy.destLoc = pqResult.to;
            d20aCopy.distTraversed = pathLength;

            if (pathLength < 0.1f)
            {
                ReleasePooledPathQueryResult(ref d20aCopy.path);
                return ActionErrorCode.AEC_OK;
            }

            if (!GameSystems.Combat.IsCombatActive())
            {
                d20aCopy.distTraversed = 0;
                pathLength = 0.0f;
            }

            // deducting moves that have already been spent, but also a raw calculation (not taking 5' step and such into account)
            if (GetRemainingMaxMoveLength(d20a, tbStatCopy, out var remainingMaxMoveLength))
            {
                if (remainingMaxMoveLength < 0.1)
                {
                    ReleasePooledPathQueryResult(ref d20aCopy.path);
                    return ActionErrorCode.AEC_TARGET_TOO_FAR;
                }

                if (remainingMaxMoveLength < pathLength)
                {
                    TrimPathToRemainingMoveLength(d20aCopy, remainingMaxMoveLength, pathQ);
                    pqResult = d20aCopy.path;
                    pathLength = remainingMaxMoveLength;
                }
            }

            /*
            this is 0 for specific move action types like 5' step, Move, Run, Withdraw;
            */
            if (nonspecificMoveType)
            {
                var chosenActionType = D20ActionType.MOVE;

                float baseMoveDist = d20aCopy.d20APerformer.Dispatch41GetMoveSpeed(out _);
                if (!d20aCopy.d20Caf.HasFlag(D20CAF.CHARGE))
                {
                    if (pathLength > tbStatCopy.surplusMoveDistance)
                    {
                        // distance greater than twice base distance . cannot do
                        if (2 * baseMoveDist + tbStatCopy.surplusMoveDistance < pathLength)
                        {
                            ReleasePooledPathQueryResult(ref d20aCopy.path);
                            return ActionErrorCode.AEC_TARGET_TOO_FAR;
                        }

                        // distance greater than base move distance . do double move
                        if (tbStatCopy.surplusMoveDistance + baseMoveDist < pathLength)
                        {
                            chosenActionType = D20ActionType.DOUBLE_MOVE;
                        }

                        // check if under 5'
                        else if (pathLength <= 5.0f)
                        {
                            if (d20a.d20ActType != D20ActionType.UNSPECIFIED_MOVE)
                            {
                                var actCost = new ActionCostPacket();
                                D20ActionDefs.GetActionDef(d20a.d20ActType).actionCost(d20a, tbStatCopy, actCost);

                                if (actCost.hourglassCost == 4 ||
                                    tbStatCopy.hourglassState == HourglassState.EMPTY)
                                {
                                    chosenActionType = D20ActionType.FIVEFOOTSTEP;
                                }
                            }
                            else
                            {
                                // added for AI to take 5' steps when it still has full round action to exploit
                                if (tbStatCopy.hourglassState == HourglassState.EMPTY ||
                                    tbStatCopy.hourglassState == HourglassState.FULL &&
                                    !GameSystems.Party.IsPlayerControlled(d20a.d20APerformer))
                                {
                                    chosenActionType = D20ActionType.FIVEFOOTSTEP;
                                    if ((tbStatCopy.tbsFlags &
                                         (TurnBasedStatusFlags.Movement | TurnBasedStatusFlags.Movement2)) != 0)
                                    {
                                        chosenActionType = D20ActionType.MOVE;
                                    }
                                }
                            }
                        }
                    }

                    d20aCopy.d20ActType = chosenActionType;
                }
                else if (2 * baseMoveDist >= pathLength)
                {
                    chosenActionType = D20ActionType.RUN;
                }
                else
                {
                    ReleasePooledPathQueryResult(ref d20aCopy.path);
                    return ActionErrorCode.AEC_TARGET_TOO_FAR;
                }

                d20aCopy.d20ActType = chosenActionType;
            }

            actSeq.performerLoc = pqResult.to;

            ProcessPathForReadiedActions(d20aCopy, out var readiedAction);
            ProcessSequenceForAoOs(actSeq, d20aCopy);
            if (readiedAction != null)
            {
                AddReadiedInterrupt(actSeq, readiedAction);
            }
            UpdateDistTraversed(actSeq);
            return 0;
        }

        [TempleDllLocation(0x1008bb40)]
        private void ProcessSequenceForAoOs(ActionSequence actSeq, D20Action action)
        {
	        float distFeet = 0.0f;
	        var pqr = action.path;
	        var d20ActionTypePostAoO = action.d20ActType;
	        bool addingAoOStatus = true;
	        if (d20ActionTypePostAoO == D20ActionType.FIVEFOOTSTEP)
	        {
		        actSeq.d20ActArray.Add(action.Copy());
		        return;
	        }
	        if (d20ActionTypePostAoO == D20ActionType.DOUBLE_MOVE)
	        {
		        distFeet = 5.0f;
		        d20ActionTypePostAoO = D20ActionType.MOVE;
	        }

	        if (!GameSystems.D20.Combat.FindAttacksOfOpportunity(action.d20APerformer, action.path, distFeet, out var attacks))
	        {
		        actSeq.d20ActArray.Add(action.Copy());
		        return;
	        }

	        var d20aAoOMovement = new D20Action(D20ActionType.AOO_MOVEMENT);
            var startDistFeet = 0.0f;
            var endDistFeet = action.path.GetPathResultLength();
            foreach (var attack in attacks) {
		        if (attack.Distance != startDistFeet)
		        {
			        d20aAoOMovement = action.Copy();
			        var aooDistFeet = attack.Distance;

                    GameSystems.PathX.GetPartialPath(pqr, out var pqrTrunc, startDistFeet, aooDistFeet);
                    d20aAoOMovement.path = pqrTrunc;

			        startDistFeet = attack.Distance;
			        d20aAoOMovement.destLoc = attack.Location;
			        d20aAoOMovement.distTraversed = pqrTrunc.GetPathResultLength();
			        if (!addingAoOStatus)
                    {
                        d20aAoOMovement.d20ActType = d20ActionTypePostAoO;
                    }
                    addingAoOStatus = false;
                    actSeq.d20ActArray.Add(d20aAoOMovement);
                }
		        d20aAoOMovement.d20APerformer = attack.Interrupter;
		        d20aAoOMovement.d20ATarget = action.d20APerformer;
		        d20aAoOMovement.destLoc = attack.Location;
		        d20aAoOMovement.d20Caf = 0;
		        d20aAoOMovement.distTraversed = 0;
		        d20aAoOMovement.path = null;
		        d20aAoOMovement.d20ActType = D20ActionType.AOO_MOVEMENT;
		        d20aAoOMovement.data1 = 1;
                actSeq.d20ActArray.Add(d20aAoOMovement.Copy());
	        }

            d20aAoOMovement = action.Copy();

            GameSystems.PathX.GetPartialPath(pqr, out var pqrLastStretch, startDistFeet, endDistFeet);
            d20aAoOMovement.path = pqrLastStretch;

	        d20aAoOMovement.destLoc = action.destLoc;
	        d20aAoOMovement.distTraversed = pqrLastStretch.GetPathResultLength();
	        if (!addingAoOStatus)
		        d20aAoOMovement.d20ActType = d20ActionTypePostAoO;
            actSeq.d20ActArray.Add(d20aAoOMovement);
            ReleasePooledPathQueryResult(ref action.path);
        }

        [TempleDllLocation(0x1008bac0)]
        private void AddReadiedInterrupt(ActionSequence actSeq, ReadiedActionPacket readied)
        {
            var readiedAction = new D20Action();
            readiedAction.d20ATarget = actSeq.performer;
            readiedAction.d20Caf = default;
            readiedAction.d20ActType = D20ActionType.READIED_INTERRUPT;
            readiedAction.data1 = 1;
            readiedAction.path = null;
            readiedAction.d20APerformer = readied.interrupter;
            actSeq.d20ActArray.Add(readiedAction);
        }

        // The size of increments in which we search for interrupts along the path
        private const float InterruptSearchIncrement = 4.0f;

        [TempleDllLocation(0x100939d0)]
        private void ProcessPathForReadiedActions(D20Action action, out ReadiedActionPacket triggeredReadiedAction)
        {
            Span<int> actionIndices = stackalloc int[_readiedActions.Count];
            FindHostileReadyVsMoveActions(action.d20APerformer, ref actionIndices);
            if (actionIndices.Length == 0)
            {
                triggeredReadiedAction = null;
                return;
            }

            var pathLength = action.path.GetPathResultLength();
            var startLocation = action.path.from;

            // Search along the path and look for changes of interrupts
            for (var dist = 0.0f; dist < pathLength + InterruptSearchIncrement / 2; dist += InterruptSearchIncrement)
            {
                var truncateLengthFeet = MathF.Min(dist, pathLength);
                GameSystems.PathX.TruncatePathToDistance(action.path, out var newLocation, truncateLengthFeet);

                // Process every approach/withdraw action along the path
                foreach (var readiedActionIdx in actionIndices)
                {
                    var readiedAction = _readiedActions[readiedActionIdx];

                    var wasInRange =
                        GameSystems.D20.Combat.CanMeleeTargetAtLocation(readiedAction.interrupter, action.d20APerformer,
                            startLocation);
                    var willBeInRange =
                        GameSystems.D20.Combat.CanMeleeTargetAtLocation(readiedAction.interrupter, action.d20APerformer,
                            newLocation);

                    if (readiedAction.readyType == ReadyVsTypeEnum.RV_Approach)
                    {
                        // This essentially checks if the action performer moves from a spot that the interrupter
                        // can attack it into a spot that it cannot attack it.
                        if (wasInRange)
                        {
                            continue;
                        }

                        if (!willBeInRange && !GameSystems.D20.Combat.IsWithinThreeFeet(readiedAction.interrupter,
                                action.d20APerformer,
                                newLocation))
                        {
                            continue;
                        }
                    }
                    else if (readiedAction.readyType == ReadyVsTypeEnum.RV_Withdrawal)
                    {
                        if (!wasInRange)
                        {
                            continue;
                        }

                        if (willBeInRange)
                        {
                            if (!GameSystems.D20.Combat.IsWithinThreeFeet(readiedAction.interrupter,
                                action.d20APerformer,
                                newLocation))
                            {
                                continue;
                            }
                        }
                    }
                    else
                    {
                        continue;
                    }

                    triggeredReadiedAction = readiedAction;

                    // Truncate the path so it ends where the interrupt happens
                    GameSystems.PathX.GetPartialPath(action.path, out var trimmedPath, 0, dist);
                    ReleasePooledPathQueryResult(ref action.path);
                    action.path = trimmedPath;
                    return;
                }
            }

            triggeredReadiedAction = null;
        }

        [TempleDllLocation(0x10091580)]
        private void FindHostileReadyVsMoveActions(GameObjectBody mover, ref Span<int> actionIndices)
        {
            int count = 0;
            for (var index = 0; index < _readiedActions.Count; index++)
            {
                var readiedAction = _readiedActions[index];
                if (readiedAction.flags == 1 && (readiedAction.readyType == ReadyVsTypeEnum.RV_Approach ||
                                                 readiedAction.readyType == ReadyVsTypeEnum.RV_Withdrawal))
                {
                    // Ignore actions that would not actually trigger against the moving critter
                    if (readiedAction.interrupter == mover ||
                        GameSystems.Critter.IsFriendly(readiedAction.interrupter, mover))
                    {
                        continue;
                    }

                    actionIndices[count++] = index;
                }
            }

            actionIndices = actionIndices.Slice(0, count);
        }

        [TempleDllLocation(0x1008ba80)]
        private void UpdateDistTraversed(ActionSequence actSeq)
        {
            foreach (var d20Action in actSeq.d20ActArray)
            {
                var path = d20Action.path;
                if (path != null)
                {
                    d20Action.distTraversed = path.GetPathResultLength();
                }
            }
        }

        [TempleDllLocation(0x10091580)]
        int ReadyVsApproachOrWithdrawalCount(GameObjectBody mover)
        {
            int result = 0;
            for (int i = 0; i < _readiedActions.Count; i++)
            {
                if (_readiedActions[i].flags == 1
                    && _readiedActions[i].interrupter != null
                    && (_readiedActions[i].readyType == ReadyVsTypeEnum.RV_Approach
                        || _readiedActions[i].readyType == ReadyVsTypeEnum.RV_Withdrawal))
                    result++;
            }

            return result;
        }

        [TempleDllLocation(0x10091500)]
        void ReadyVsRemoveForObj(GameObjectBody obj)
        {
            for (int i = 0; i < _readiedActions.Count; i++)
            {
                if (_readiedActions[i].flags == 1 && _readiedActions[i].interrupter == obj)
                {
                    _readiedActions[i].flags = 0;
                    _readiedActions[i].interrupter = null;
                }
            }
        }

        [TempleDllLocation(0x10091650)]
        private ReadiedActionPacket ReadiedActionGetNext(ReadiedActionPacket prevReadiedAction, D20Action d20a)
        {
            int i0 = 0;

            // find the prevReadiedAction in the array (kinda retarded since it's a pointer???)
            if (prevReadiedAction != null)
            {
                for (i0 = 0; i0 < _readiedActions.Count; i0++)
                {
                    if (_readiedActions[i0] == prevReadiedAction)
                    {
                        i0++;
                        break;
                    }
                }

                if (i0 >= _readiedActions.Count)
                    return null;
            }

            for (int i = i0; i < _readiedActions.Count; i++)
            {
                if ((_readiedActions[i].flags & 1) == 0)
                    continue;
                switch (_readiedActions[i].readyType)
                {
                    case ReadyVsTypeEnum.RV_Spell:
                    case ReadyVsTypeEnum.RV_Counterspell:
                        if (GameSystems.Critter.IsFriendly(d20a.d20APerformer, _readiedActions[i].interrupter))
                            continue;
                        if (d20a.d20ActType == D20ActionType.CAST_SPELL)
                            return _readiedActions[i];
                        break;
                    case ReadyVsTypeEnum.RV_Approach:
                    case ReadyVsTypeEnum.RV_Withdrawal:
                    default:
                        if (d20a.d20ActType != D20ActionType.READIED_INTERRUPT)
                            continue;
                        if (d20a.d20APerformer != _readiedActions[i].interrupter)
                            continue;
                        return _readiedActions[i];
                }
            }

            return null;
        }

        [TempleDllLocation(0x1008B9A0)]
        private void TrimPathToRemainingMoveLength(D20Action d20a, float remainingMoveLength, PathQuery pathQ)
        {
            var pathLengthTrimmed = remainingMoveLength - 0.1f;

            GameSystems.PathX.GetPartialPath(d20a.path, out var pqrTrimmed, 0.0f, pathLengthTrimmed);

            ReleasePooledPathQueryResult(ref d20a.path);
            d20a.path = pqrTrimmed;
            var destination = pqrTrimmed.to;
            if (!GameSystems.PathX.PathDestIsClear(pathQ, d20a.d20APerformer, destination))
            {
                d20a.d20Caf |= D20CAF.ALTERNATE;
            }

            d20a.d20Caf |= D20CAF.TRUNCATED;
        }

        internal bool GetRemainingMaxMoveLength(D20Action d20a, TurnBasedStatus tbStat, out float moveLen)
        {
            var surplusMoves = tbStat.surplusMoveDistance;
            var moveSpeed = d20a.d20APerformer.Dispatch41GetMoveSpeed(out _);
            if (d20a.d20ActType == D20ActionType.UNSPECIFIED_MOVE)
            {
                if (tbStat.hourglassState >= HourglassState.FULL)
                {
                    moveLen = 2 * moveSpeed + surplusMoves;
                    return true;
                }

                if (tbStat.hourglassState >= HourglassState.MOVE)
                {
                    moveLen = moveSpeed + surplusMoves;
                    return true;
                }

                moveLen = surplusMoves;
                return true;
            }

            if (d20a.d20ActType == D20ActionType.FIVEFOOTSTEP
                && (tbStat.tbsFlags & (TurnBasedStatusFlags.Movement | TurnBasedStatusFlags.Movement2)) == default)
            {
                if (surplusMoves <= 0.001f)
                {
                    moveLen = 5.0f;
                    return true;
                }

                moveLen = surplusMoves;
                return true;
            }

            if (d20a.d20ActType == D20ActionType.MOVE)
            {
                moveLen = moveSpeed + surplusMoves;
                return true;
            }

            if (d20a.d20ActType == D20ActionType.DOUBLE_MOVE)
            {
                moveLen = 2 * moveSpeed + surplusMoves;
                return true;
            }

            moveLen = 0;
            return false;
        }

        [TempleDllLocation(0x1008b850)]
        private void ReleasePooledPathQueryResult(ref PathQueryResult result)
        {
            if (result == null)
            {
                return;
            }

            result = null;
            throw new NotImplementedException(); // Will not be implemented either
        }

        [TempleDllLocation(0x1008b810)]
        public PathQueryResult GetPooledPathQueryResult()
        {
            throw new NotImplementedException(); // Will not be implemented either
        }

        [TempleDllLocation(0x10094ca0)]
        private ActionErrorCode seqCheckFuncs(TurnBasedStatus tbStatus)
        {
            var curSeq = CurrentSequence;
            var result = ActionErrorCode.AEC_OK;

            if (curSeq == null)
            {
                tbStatus.Clear();
                return 0;
            }

            tbStatus = curSeq.tbStatus;
            var seqPerfLoc = curSeq.performer.GetLocationFull();

            for (int i = 0; i < curSeq.d20ActArray.Count; i++)
            {
                var d20a = curSeq.d20ActArray[i];
                if (curSeq.d20ActArrayNum <= 0) return 0;

                var d20Def = D20ActionDefs.GetActionDef(d20a.d20ActType);
                var tgtCheckFunc = d20Def.tgtCheckFunc;
                if (tgtCheckFunc != null)
                {
                    result = (ActionErrorCode) tgtCheckFunc(d20a, tbStatus);
                    if (result != ActionErrorCode.AEC_OK)
                    {
                        break;
                    }
                }

                result = TurnBasedStatusUpdate(d20a, tbStatus);
                if (result != ActionErrorCode.AEC_OK)
                {
                    tbStatus.errCode = result;
                    break;
                }

                var actCheckFunc = d20Def.actionCheckFunc;
                if (actCheckFunc != null)
                {
                    result = actCheckFunc(d20a, tbStatus);
                    if (result != ActionErrorCode.AEC_OK)
                    {
                        break;
                    }
                }

                var locCheckFunc = d20Def.locCheckFunc;
                if (locCheckFunc != null)
                {
                    result = locCheckFunc(d20a, tbStatus, seqPerfLoc);
                    if (result != 0)
                    {
                        break;
                    }
                }

                var path = curSeq.d20ActArray[i].path;
                if (path != null)
                {
                    seqPerfLoc = path.to;
                }
            }

            if (result != ActionErrorCode.AEC_OK)
            {
                if (CurrentSequence == null)
                {
                    tbStatus.Clear();
                }
                else
                {
                    CurrentSequence.tbStatus.CopyTo(tbStatus);
                }
            }

            return result;
        }

        [TempleDllLocation(0x10093950)]
        internal ActionErrorCode TurnBasedStatusUpdate(D20Action action, TurnBasedStatus tbStatus)
        {
            var tbStatCopy = tbStatus.Copy();

            var actProcResult = ActionCostProcess(tbStatCopy, action);
            if (actProcResult == ActionErrorCode.AEC_OK)
            {
                tbStatCopy.CopyTo(tbStatus);
                return actProcResult;
            }

            var tbsCheckFunc = D20ActionDefs.GetActionDef(action.d20ActType).turnBasedStatusCheck;
            if (tbsCheckFunc != null)
            {
                if (tbsCheckFunc(action, tbStatCopy) != ActionErrorCode.AEC_OK)
                {
                    return actProcResult;
                }

                actProcResult = ActionCostProcess(tbStatCopy, action);
            }

            if (actProcResult == ActionErrorCode.AEC_OK)
            {
                tbStatCopy.CopyTo(tbStatus);
                return actProcResult;
            }

            return actProcResult;
        }

        [TempleDllLocation(0x1008b040)]
        private ActionErrorCode ActionCostProcess(TurnBasedStatus tbStat, D20Action action)
        {
            var actCost = new ActionCostPacket();

            var result = D20ActionDefs.GetActionDef(action.d20ActType).actionCost(action, tbStat, actCost);
            if (result != ActionErrorCode.AEC_OK)
            {
                return result;
            }

            // Adding action cost modification facility
            actCost.hourglassCost = action.d20APerformer.DispatchActionCostMod(actCost, tbStat, action);

            var hourglassCost = actCost.hourglassCost;
            tbStat.hourglassState = GetHourglassTransition(tbStat.hourglassState, actCost.hourglassCost);

            if (tbStat.hourglassState == HourglassState.INVALID) // this is an error state I think
            {
                if (hourglassCost <= 4)
                {
                    switch (hourglassCost)
                    {
                        case 1:
                            tbStat.errCode = ActionErrorCode.AEC_NOT_ENOUGH_TIME2;
                            return ActionErrorCode.AEC_NOT_ENOUGH_TIME2;
                        case 2:
                            tbStat.errCode = ActionErrorCode.AEC_NOT_ENOUGH_TIME1;
                            return ActionErrorCode.AEC_NOT_ENOUGH_TIME1;
                        case 4:
                            tbStat.errCode = ActionErrorCode.AEC_NOT_ENOUGH_TIME3;
                            return ActionErrorCode.AEC_NOT_ENOUGH_TIME3;
                        case 3:
                            tbStat.errCode = ActionErrorCode.AEC_NOT_ENOUGH_TIME3;
                            break;
                    }
                }
            }

            if (tbStat.surplusMoveDistance >= actCost.moveDistCost)
            {
                tbStat.surplusMoveDistance -= actCost.moveDistCost;
                if (actCost.chargeAfterPicker <= 0
                    || actCost.chargeAfterPicker + tbStat.attackModeCode <=
                    tbStat.baseAttackNumCode + tbStat.numBonusAttacks)
                {
                    if (tbStat.numBonusAttacks < actCost.chargeAfterPicker)
                        tbStat.attackModeCode += actCost.chargeAfterPicker;
                    else
                        tbStat.numBonusAttacks -= actCost.chargeAfterPicker;
                    if (tbStat.attackModeCode == tbStat.baseAttackNumCode && tbStat.numBonusAttacks == 0)
                        tbStat.tbsFlags &= ~TurnBasedStatusFlags.FullAttack;
                    result = ActionErrorCode.AEC_OK;
                }
                else
                {
                    result = ActionErrorCode.AEC_NOT_ENOUGH_TIME1;
                }
            }
            else
            {
                result = (tbStat.tbsFlags & TurnBasedStatusFlags.Movement2) != 0
                    ? ActionErrorCode.AEC_ALREADY_MOVED
                    : ActionErrorCode.AEC_TARGET_OUT_OF_RANGE;
            }

            return result;
        }

        [TempleDllLocation(0x10094c60)]
        public ActionErrorCode seqCheckAction(D20Action action, TurnBasedStatus tbStat)
        {
            ActionErrorCode errorCode = TurnBasedStatusUpdate(action, tbStat);
            if (errorCode != ActionErrorCode.AEC_OK)
            {
                tbStat.errCode = errorCode;
                return errorCode;
            }

            var actionCheckFunc = D20ActionDefs.GetActionDef(action.d20ActType).actionCheckFunc;
            if (actionCheckFunc != null)
            {
                return actionCheckFunc(action, tbStat);
            }
            return ActionErrorCode.AEC_OK;
        }

        [TempleDllLocation(0x1008c580)]
        public void FullAttackCostCalculate(D20Action d20a, TurnBasedStatus tbStatus, out int baseAttackNumCode, out int bonusAttacks, out int numAttacks, out int attackModeCode)
        {
            	GameObjectBody  performer = d20a.d20APerformer;
            	int usingOffhand = 0;
            	int _attackTypeCodeHigh = 1;
            	int _attackTypeCodeLow = 0;
            	int numAttacksBase = 0;
            	var mainWeapon = GameSystems.Item.ItemWornAt(performer, EquipSlot.WeaponPrimary);
            	var offhand = GameSystems.Item.ItemWornAt(performer, EquipSlot.WeaponSecondary);

            	if (offhand != null){
            		if (mainWeapon != null){
            			if (offhand.type != ObjectType.armor) {
            				_attackTypeCodeHigh = AttackPacket.ATTACK_CODE_OFFHAND + 1; // originally 5
            				_attackTypeCodeLow = AttackPacket.ATTACK_CODE_OFFHAND; // originally 4
            				usingOffhand = 1;
            			}
            		}
            		else {
            			mainWeapon = offhand;
            		}

            	}
            	if (mainWeapon != null && GameSystems.Item.IsRangedWeapon(mainWeapon))
                {
                    d20a.d20Caf |= D20CAF.RANGED;
            	}

            	// if unarmed check natural attacks (for monsters)
            	if (mainWeapon == null && offhand == null){
            		numAttacksBase = DispatchD20ActionCheck(d20a, tbStatus, DispatcherType.GetCritterNaturalAttacksNum);

            		if (numAttacksBase > 0)	{
            			_attackTypeCodeHigh = AttackPacket.ATTACK_CODE_NATURAL_ATTACK + 1; // originally 10
            			_attackTypeCodeLow = AttackPacket.ATTACK_CODE_NATURAL_ATTACK; // originally 9
            		}
            	}

            	if (numAttacksBase <= 0){
            		numAttacksBase = DispatchD20ActionCheck(d20a, tbStatus, DispatcherType.GetNumAttacksBase);
            	}

            	bonusAttacks = DispatchD20ActionCheck(d20a, tbStatus, DispatcherType.GetBonusAttacks);
            	numAttacks = usingOffhand + numAttacksBase + bonusAttacks;
            	attackModeCode = _attackTypeCodeLow;
            	baseAttackNumCode = numAttacksBase + _attackTypeCodeHigh - 1 + usingOffhand;
        }

        public bool UsingSecondaryWeapon(D20Action d20a)
        {
            return UsingSecondaryWeapon(d20a.d20APerformer, d20a.data1);
        }

        public bool UsingSecondaryWeapon(GameObjectBody obj, int attackCode)
        {
            if (attackCode == AttackPacket.ATTACK_CODE_OFFHAND + 2
                || attackCode == AttackPacket.ATTACK_CODE_OFFHAND + 4
                || attackCode == AttackPacket.ATTACK_CODE_OFFHAND + 6)
            {
                if (attackCode == AttackPacket.ATTACK_CODE_OFFHAND + 2)
                {
                    return true;
                }
                if (attackCode == AttackPacket.ATTACK_CODE_OFFHAND + 4)
                {
                    if (GameSystems.Feat.HasFeatCount(obj, FeatId.IMPROVED_TWO_WEAPON_FIGHTING) != 0
                        || GameSystems.Feat.HasFeatCountByClass(obj, FeatId.IMPROVED_TWO_WEAPON_FIGHTING_RANGER, (Stat)0,0) != 0)
                        return true;
                }
                else if (attackCode == AttackPacket.ATTACK_CODE_OFFHAND + 6)
                {
                    if (GameSystems.Feat.HasFeatCount(obj, FeatId.GREATER_TWO_WEAPON_FIGHTING) != 0
                        || GameSystems.Feat.HasFeatCountByClass(obj, FeatId.GREATER_TWO_WEAPON_FIGHTING_RANGER, (Stat)0, 0) != 0)
                        return true;
                }
            }
            return false;
        }

    }
}