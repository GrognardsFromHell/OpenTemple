using OpenTemple.Core.GameObject;
using OpenTemple.Core.Location;
using OpenTemple.Core.Platform;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.Raycast;

namespace OpenTemple.Core.Ui.InGameSelect.Pickers
{
    internal class AreaTargetBehavior : PickerBehavior
    {
        public override UiPickerType Type => UiPickerType.Area;

        public override string Name => "SP_M_AREA";

        [TempleDllLocation(0x10138a20)]
        public AreaTargetBehavior(PickerState pickerState) : base(pickerState)
        {
            UiSystems.Party.SetTargetCallbacks(TargetPartyMember, SetPartyMemberTarget, ResetPartyMemberTarget);
        }

        [TempleDllLocation(0x10138740)]
        private bool TargetPartyMember(GameObjectBody partyMember)
        {
            SetPartyMemberTarget(partyMember);
            return FinalizePicker();
        }

        [TempleDllLocation(0x10136c80)]
        private void SetPartyMemberTarget(GameObjectBody target)
        {
            PickerState.Target = target;
            ClearResults();

            if (!Picker.modeTarget.HasFlag(UiPickerType.AreaOrObj)
                || !Picker.CheckTargetVsIncFlags(target)
                || !Picker.TargetValid(target))
            {
                // Only use the target object's location, but not the object itself
                Picker.SetAreaTargets(target.GetLocationFull());
                return;
            }

            Picker.SetAreaTargets(target.GetLocationFull());
            Result.flags |= PickerResultFlags.PRF_HAS_SELECTED_OBJECT;

            // Sort the primary focus of selection to the beginning of the list,
            // But don't insert it if it isn't in the list already
            if (Result.objList.Remove(target))
            {
                Result.objList.Insert(0, target);
            }
        }

        [TempleDllLocation(0x10136c40)]
        private void ResetPartyMemberTarget(GameObjectBody obj)
        {
            PickerState.Target = null;
            ClearResults();
        }

        [TempleDllLocation(0x10138070)]
        internal override bool LeftMouseButtonReleased(IGameViewport viewport, MessageMouseArgs args)
        {
            MouseMoved(viewport, args);
            return FinalizePicker();
        }

        [TempleDllLocation(0x10137dd0)]
        internal override bool MouseMoved(IGameViewport viewport, MessageMouseArgs args)
        {
            PickerStatusFlags &= ~PickerStatusFlags.Invalid;

            if (HandleClickInUnexploredArea(args.X, args.Y))
            {
                return false;
            }

            // The picker may allow picking an object directly (which will be the basis of the area effect)
            if (Picker.modeTarget.HasFlag(UiPickerType.AreaOrObj))
            {
                var flags = PickerState.GetFlagsFromExclusions();
                if (GameSystems.Raycast.PickObjectOnScreen(viewport, args.X, args.Y, out var target, flags))
                {
                    if ((Picker.flagsTarget.HasFlag(UiPickerFlagsTarget.LosNotRequired) || !Picker.LosBlocked(target))
                        && Picker.CheckTargetVsIncFlags(target)
                        && Picker.TargetValid(target))
                    {
                        ClearResults();
                        Picker.SetAreaTargets(target.GetLocationFull());
                        Result.flags |= PickerResultFlags.PRF_HAS_SELECTED_OBJECT;

                        // Sort the primary focus of selection to the beginning of the list,
                        // But don't insert it if it isn't in the list already
                        if (Result.objList.Remove(target))
                        {
                            Result.objList.Insert(0, target);
                        }

                        return false;
                    }
                }
            }

            var targetLoc = GameViews.Primary.ScreenToTile(args.X, args.Y);

            // Even when the picked object above is not valid, targeting the location underneath is a valid alternative
            if (!Picker.flagsTarget.HasFlag(UiPickerFlagsTarget.LosNotRequired)
                && !HasLineOfSight(Picker.caster, targetLoc))
            {
                ClearResults();
                PickerStatusFlags |= PickerStatusFlags.Invalid;
                return false;
            }

            Picker.SetAreaTargets(targetLoc);
            return false;
        }

        [TempleDllLocation(0x10137430)]
        private static bool HasLineOfSight(GameObjectBody source, LocAndOffsets target)
        {
            var objIterator = new RaycastPacket();
            objIterator.flags |= RaycastFlag.StopAfterFirstBlockerFound | RaycastFlag.ExcludeItemObjects;
            objIterator.origin = source.GetLocationFull();
            objIterator.targetLoc = target;
            return objIterator.TestLineOfSight();
        }
    }
}