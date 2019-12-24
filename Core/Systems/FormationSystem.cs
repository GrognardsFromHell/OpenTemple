using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.IO.SaveGames.GameState;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.Logging;
using SpicyTemple.Core.Systems.Pathfinding;
using SpicyTemple.Core.Systems.Raycast;

namespace SpicyTemple.Core.Systems
{
    public class FormationSystem : IGameSystem, ISaveGameAwareGameSystem, IResetAwareSystem
    {
        private static readonly ILogger Logger = LoggingSystem.CreateLogger();

        /// <summary>
        /// Layout is top to bottom, left to right with 5 columns.
        /// </summary>
        private static readonly int[][] InitialFormations =
        {
            new[] {6, 8, 11, 13, 16, 18, 21, 23},
            new[] {1, 2, 3, 7, 12, 17, 22, 27},
            new[] {7, 11, 13, 15, 19, 21, 23, 27},
            new[] {5, 9, 11, 13, 21, 23, 25, 29}
        };

        [TempleDllLocation(0x109DD2A0)]
        private int[][] _formationPositions;

        [TempleDllLocation(0x109DD298)]
        private int _formationSelected;

        [TempleDllLocation(0x100437c0)]
        public FormationSystem()
        {
            Reset();
        }

        public void Dispose()
        {
        }

        [TempleDllLocation(0x10043250)]
        public void Reset()
        {
            Trace.Assert(InitialFormations.Length == 4);
            _formationPositions = InitialFormations.Select(t => t.ToArray()).ToArray();
            _formationSelected = 0;
        }

        [TempleDllLocation(0x10043270)]
        public void SaveGame(SavedGameState savedGameState)
        {
            savedGameState.FormationState = new SavedFormationState
            {
                FormationSelected = _formationSelected,
                Positions = _formationPositions.Select(t => t.ToArray()).ToArray()
            };
        }

        [TempleDllLocation(0x100432e0)]
        public void LoadGame(SavedGameState savedGameState)
        {
            var savedFormationState = savedGameState.FormationState;
            _formationPositions = savedFormationState.Positions.Select(t => t.ToArray()).ToArray();
            _formationSelected = savedFormationState.FormationSelected;
        }

        [TempleDllLocation(0x10043390)]
        private bool GetFormationOffset(GameObjectBody obj, float rotation, out float xOut, out float yOut)
        {
            // Calculate the maximum diameter of any party member
            var maxDiameter = 0.0f;
            foreach (var partyMember in GameSystems.Party.PartyMembers)
            {
                var radius = partyMember.GetRadius();
                if (radius > maxDiameter)
                {
                    maxDiameter = radius;
                }
            }

            maxDiameter *= 2;

            var partyIdx = GameSystems.Party.IndexOf(obj);
            if (partyIdx < 0 || partyIdx >= GameSystems.Party.PartySize)
            {
                xOut = 0;
                yOut = 0;
                return false;
            }
            else
            {
                var formationPosition = _formationPositions[_formationSelected][partyIdx];

                var posX = -((formationPosition % 5 - 2) * maxDiameter);
                var posY = (formationPosition / 5 - 1) * maxDiameter;

                // Rotate the formation position according to the desired angle
                xOut = MathF.Cos(rotation) * posX - MathF.Sin(rotation) * posY;
                yOut = MathF.Sin(rotation) * posX + MathF.Cos(rotation) * posY;
                return true;
            }
        }

        private bool IsBlocked(Vector2 pos, float radius)
        {
            var loc = LocAndOffsets.FromInches(pos);

            using var raycastPacket = new RaycastPacket();
            raycastPacket.origin = loc;
            raycastPacket.targetLoc = loc;
            raycastPacket.radius = radius;
            raycastPacket.flags |= RaycastFlag.StopAfterFirstFlyoverFound
                                   | RaycastFlag.StopAfterFirstBlockerFound
                                   | RaycastFlag.ExcludeItemObjects
                                   | RaycastFlag.HasSourceObj
                                   | RaycastFlag.HasRadius;

            return raycastPacket.RaycastShortRange() > 0;
        }

        private bool FindFreeTargetPos(ref LocAndOffsets loc, float radius)
        {
            var loc2d = loc.ToInches2D();
            if (!IsBlocked(loc2d, radius))
            {
                return true;
            }

            var i = 0;

            for (var j = -1; j > -18; j--)
            {
                int k;
                var yOffset = i * locXY.INCH_PER_SUBTILE;
                var altPos = loc2d;
                for (k = j; k < i; k++)
                {
                    var xOffset = k * locXY.INCH_PER_SUBTILE;

                    altPos = loc2d + new Vector2(xOffset, -yOffset);
                    if (!IsBlocked(altPos, radius))
                    {
                        break;
                    }

                    altPos = loc2d + new Vector2(-xOffset, yOffset);
                    if (!IsBlocked(altPos, radius))
                    {
                        break;
                    }

                    altPos = loc2d + new Vector2(-xOffset, -yOffset);
                    if (!IsBlocked(altPos, radius))
                    {
                        break;
                    }

                    altPos = loc2d + new Vector2(xOffset, yOffset);
                    if (!IsBlocked(altPos, radius))
                    {
                        break;
                    }
                }

                if (k != i)
                {
                    loc = LocAndOffsets.FromInches(altPos);
                    return true;
                }

                i++;
            }

            return i != 0;
        }

        private bool FindIntersectingPosition(IReadOnlyList<GameObjectBody> partyMembers,
            ReadOnlySpan<LocAndOffsets> positions,
            Vector2 partyMemberPos,
            float partyMemberRadius)
        {
            for (var i = 0; i < positions.Length; i++)
            {
                var otherPos = positions[i].ToInches2D();

                var otherMember = partyMembers[i];
                var otherRadius = otherMember.GetRadius();
                var dist = (otherPos - partyMemberPos).Length();
                if (dist < otherRadius + partyMemberRadius)
                {
                    // Found another one that collides with this one.
                    // The pointless part here is that it'll do this calculation
                    // for nIdx == nIdx_00!
                    return true;
                }
            }

            return false;
        }

        [TempleDllLocation(0x100437F0)]
        public int PartySelectedFormationMoveToPosition(LocAndOffsets loc, bool mayWalk)
        {
            var selectedMembers = GameSystems.Party.Selected;
            if (selectedMembers.Count == 0)
            {
                return 1;
            }

            var maxRadius = float.MinValue;
            foreach (var selectedMember in selectedMembers)
            {
                if (!GameSystems.Critter.IsDeadNullDestroyed(selectedMember))
                {
                    maxRadius = Math.Max(maxRadius, selectedMember.GetRadius());
                }
            }

            Span<LocAndOffsets> plannedPositions = stackalloc LocAndOffsets[selectedMembers.Count];

            if (!FindFreeTargetPos(ref loc, maxRadius))
            {
                return 1;
            }

            // NOTE: There seems to have been a bug in Vanilla where it wanted to
            // find the average center of the party, but never actually updated the obj
            // it took the position of. So it ended up just taking the leader's position.
            var leaderPos = selectedMembers[0].GetLocationFull().ToInches2D();
            var targetPos = loc.ToInches2D();

            // This rotation is used to rotate the formation at the target point
            var formationRotation = MathF.PI - MathF.Atan2(targetPos.X - leaderPos.X, targetPos.Y - leaderPos.Y);

            if (!GameSystems.Critter.IsDeadNullDestroyed(selectedMembers[0]))
            {
                plannedPositions[0] = loc;
            }

            for (var selectedIdx = 0; selectedIdx < selectedMembers.Count; selectedIdx++)
            {
                var obj = selectedMembers[selectedIdx];
                if (GameSystems.Critter.IsDeadNullDestroyed(obj))
                {
                    continue;
                }

                var partyMemberRadius = obj.GetRadius();

                GetFormationOffset(obj, formationRotation, out var formationOffX, out var formationOffY);
                if (selectedMembers.Count < 2)
                {
                    plannedPositions[selectedIdx] = loc;
                }
                else
                {
                    var formationOffLen = MathF.Sqrt(formationOffY * formationOffY + formationOffX * formationOffX);
                    // Remember that the length of the formation offset can be 0 in case it's dead-center
                    if (formationOffLen > locXY.INCH_PER_SUBTILE)
                    {
                        // Normalize the formation offset
                        var formationOffNormX = formationOffX / formationOffLen;
                        var formationOffNormY = formationOffY / formationOffLen;
                        var formationOffDir = new Vector2(formationOffNormX, formationOffNormY);

                        var local_90 = 0.0f;
                        for (; local_90 < formationOffLen; local_90 += locXY.INCH_PER_SUBTILE)
                        {
                            var formationShift = local_90 * formationOffDir;
                            var formationPos = targetPos + formationShift;

                            if (!IsBlocked(formationPos, maxRadius))
                            {
                                plannedPositions[selectedIdx] = LocAndOffsets.FromInches(formationPos);
                                break;
                            }
                        }

                        if (local_90 >= locXY.INCH_PER_SUBTILE)
                        {
                            // It somehow falls back to the previous location even if unblocked ???
                            var formationPos = targetPos + (local_90 - locXY.INCH_PER_SUBTILE) * formationOffDir;
                            plannedPositions[selectedIdx] = LocAndOffsets.FromInches(formationPos);
                        }
                    }
                    else
                    {
                        plannedPositions[selectedIdx] = loc;
                    }
                }

                var previousPositions = plannedPositions.Slice(0, selectedIdx);

                // We have to do conflict resolution if another party member before this one
                // chose a colliding position
                if (FindIntersectingPosition(selectedMembers, previousPositions,
                    plannedPositions[selectedIdx].ToInches2D(), partyMemberRadius))
                {
                    int upperBound;
                    for (upperBound = 1; upperBound < 18; upperBound++)
                    {
                        var local_84 = upperBound * locXY.INCH_PER_SUBTILE;

                        int local_8c;
                        var alternatePos = plannedPositions[selectedIdx].ToInches2D();
                        for (local_8c = -upperBound; local_8c < upperBound; local_8c++)
                        {
                            alternatePos = plannedPositions[selectedIdx].ToInches2D();
                            alternatePos.X += local_8c * locXY.INCH_PER_SUBTILE;
                            alternatePos.Y -= local_84;
                            if (!FindIntersectingPosition(selectedMembers, previousPositions, alternatePos,
                                    partyMemberRadius)
                                && !IsBlocked(alternatePos, partyMemberRadius))
                            {
                                break;
                            }

                            alternatePos = plannedPositions[selectedIdx].ToInches2D();
                            alternatePos.X -= local_8c * locXY.INCH_PER_SUBTILE;
                            alternatePos.Y += local_84;
                            if (!FindIntersectingPosition(selectedMembers, previousPositions, alternatePos,
                                    partyMemberRadius)
                                && !IsBlocked(alternatePos, partyMemberRadius))
                            {
                                break;
                            }

                            alternatePos = plannedPositions[selectedIdx].ToInches2D();
                            alternatePos.X -= local_8c * locXY.INCH_PER_SUBTILE;
                            alternatePos.Y -= local_84;
                            if (!FindIntersectingPosition(selectedMembers, previousPositions, alternatePos,
                                    partyMemberRadius)
                                && !IsBlocked(alternatePos, partyMemberRadius))
                            {
                                break;
                            }

                            alternatePos = plannedPositions[selectedIdx].ToInches2D();
                            alternatePos.X += local_8c * locXY.INCH_PER_SUBTILE;
                            alternatePos.Y += local_84;
                            if (!FindIntersectingPosition(selectedMembers, previousPositions, alternatePos,
                                    partyMemberRadius)
                                && !IsBlocked(alternatePos, partyMemberRadius))
                            {
                                break;
                            }
                        }

                        if (local_8c != upperBound)
                        {
                            plannedPositions[selectedIdx] = LocAndOffsets.FromInches(alternatePos);
                            break;
                        }
                    }

                    if (upperBound == 18)
                    {
                        Logger.Info("Unable to find suitable formation position after 18 iterations.");
                    }
                }
            }

            for (var i = 0; i < selectedMembers.Count; i++)
            {
                var obj = selectedMembers[i];
                if (!GameSystems.Critter.IsDeadNullDestroyed(obj))
                {
                    PartySelectedFormationMoveToPosition_FindPath(obj, selectedMembers.Count, loc,
                        plannedPositions[i], out var actualTargetPos);
                    if (!mayWalk)
                    {
                        GameSystems.Anim.PushRunToTile(obj, actualTargetPos);
                        GameSystems.Anim.TurnOnRunning();
                    }
                    else
                    {
                        GameSystems.Anim.PushMoveToTile(obj, actualTargetPos);
                    }
                }
            }

            return 1;
        }

        [TempleDllLocation(0x100435c0)]
        private void PartySelectedFormationMoveToPosition_FindPath(GameObjectBody partyMember, int count,
            LocAndOffsets from, LocAndOffsets plannedPos, out LocAndOffsets actualPos)
        {
            actualPos = plannedPos;

            if (IsBlocked(plannedPos.ToInches2D(), partyMember.GetRadius()))
            {
                var pq = new PathQuery();
                pq.flags2 = 0x2000;
                pq.flags = PathQueryFlags.PQF_FORCED_STRAIGHT_LINE | PathQueryFlags.PQF_HAS_CRITTER;
                pq.critter = partyMember;
                pq.from = from;
                pq.to = plannedPos;
                pq.flags = PathQueryFlags.PQF_FORCED_STRAIGHT_LINE | PathQueryFlags.PQF_HAS_CRITTER |
                           PathQueryFlags.PQF_TO_EXACT | PathQueryFlags.PQF_800;
                pq.distanceToTargetMin = count * locXY.INCH_PER_TILE;
                if (!GameSystems.Combat.IsCombatActive())
                {
                    pq.flags |= PathQueryFlags.PQF_IGNORE_CRITTERS;
                }

                if (GameSystems.PathX.FindPath(pq, out var pathResult))
                {
                    actualPos = pathResult.nodes[pathResult.nodeCount - 1];
                }
            }
        }
    }
}