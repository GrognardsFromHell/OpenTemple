using System;
using System.Drawing;
using OpenTemple.Core.GameObject;
using OpenTemple.Core.GFX;
using OpenTemple.Core.Logging;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Systems.D20.Actions;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui.Combat
{
    /// <summary>
    /// Renders the bar on the left hand side of the screen that shows how much of the action is still available.
    /// </summary>
    public class ActionBarUi
    {
        private static readonly ILogger Logger = LoggingSystem.CreateLogger();

        [TempleDllLocation(0x10c0408c)]
        private WidgetContainer _window;

        [TempleDllLocation(0x10c040b0)]
        private bool uiCombat_10C040B0;

        [TempleDllLocation(0x10c040c0)]
        private GameObjectBody actionBarActor;

        [TempleDllLocation(0x10c040b8)]
        private readonly ActionBar _actionBar = GameSystems.Vagrant.AllocateActionBar();

        [TempleDllLocation(0x10C040B4)]
        private readonly ActionBar _pulseAnimation = GameSystems.Vagrant.AllocateActionBar();

        [TempleDllLocation(0x10c040c8)]
        private float actionBarEndingMoveDist;

        [TempleDllLocation(0x10c040ac)]
        private ResourceRef<ITexture> _combatBarFill;

        private ResourceRef<ITexture> _combatBarHighlight;
        private ResourceRef<ITexture> _combatBarHighlight2;
        private ResourceRef<ITexture> _combatBarGrey;
        private ResourceRef<ITexture> _combatBarFillInvalid;

        [TempleDllLocation(0x101734b0)]
        public ActionBarUi()
        {
            _pulseAnimation = GameSystems.Vagrant.AllocateActionBar();
            // These values were configurable in a MES file before
            GameSystems.Vagrant.ActionBarSetPulseValues(_pulseAnimation, 64, 255, 0.125f);

            _combatBarFill = Tig.Textures.Resolve("art/interface/COMBAT_UI/CombatBar_Fill.tga", false);
            _combatBarHighlight = Tig.Textures.Resolve("art/interface/COMBAT_UI/CombatBar_Highlight.tga", false);
            _combatBarHighlight2 = Tig.Textures.Resolve("art/interface/COMBAT_UI/CombatBar_Highlight2.tga", false);
            _combatBarGrey = Tig.Textures.Resolve("art/interface/COMBAT_UI/CombatBar_GREY.tga", false);
            _combatBarFillInvalid = Tig.Textures.Resolve("art/interface/COMBAT_UI/CombatBar_Fill_INVALID.tga", false);

            _window = new WidgetContainer(8, 117, 50, 260);
            _window.Name = "combat_ui_debug_output_window";

            // Hide or show the entire action bar based on combat status
            _window.Visible = false;
            GameSystems.Combat.OnCombatStatusChanged += combatStatus => { _window.Visible = combatStatus; };

            var actionBarImage = new WidgetImage("art/interface/COMBAT_UI/combatbar.img");
            _window.AddContent(actionBarImage);

            var actionBar = new WidgetContainer(new Rectangle(13, 15, 22, 170));
            _window.Add(actionBar);
            actionBar.Name = "action bar";
            actionBar.OnBeforeRender += () => UiCombatActionBarRender(actionBar);

            // Add a full sized button on top of the action bar to handle tooltips and help requests
            var actionBarButton = new WidgetButton();
            actionBarButton.SetStyle(new WidgetButtonStyle());
            actionBarButton.SetSizeToParent(true);
            actionBarButton.SetClickHandler(OnActionBarButtonClick);
            actionBarButton.TooltipStyle = "action-bar-tooltip";
            actionBarButton.OnBeforeRender += () =>
            {
                // Use the time update message (one per frame) to update the tooltip text
                actionBarButton.TooltipText = GetActionBarTooltipText();
            };
            actionBar.Add(actionBarButton);

            var nextTurn = new WidgetButton(new Rectangle(0, 194, 50, 50));
            nextTurn.SetStyle(new WidgetButtonStyle
            {
                NormalImagePath = "art/interface/COMBAT_UI/Action-End-Turn.tga",
                HoverImagePath = "art/interface/COMBAT_UI/Action-End-Turn-Hover.tga",
                PressedImagePath = "art/interface/COMBAT_UI/Action-End-Turn-Click.tga",
                DisabledImagePath = "art/interface/COMBAT_UI/Action-End-Turn-Disabled.tga",
            });
            nextTurn.Name = "next_turn";
            nextTurn.OnBeforeRender += () => OnBeforeRenderNextTurn(nextTurn);
            nextTurn.SetClickHandler(OnNextTurnClick);
            _window.Add(nextTurn);
        }

        [TempleDllLocation(0x10172b80)]
        private string GetActionBarTooltipText()
        {
            if (!GameSystems.Combat.IsCombatActive())
            {
                return null;
            }

            var tbStatus = GameSystems.D20.Actions.curSeqGetTurnBasedStatus();
            if (!GameSystems.Party.IsPlayerControlled(GameSystems.D20.Initiative.CurrentActor))
            {
                return null;
            }

            if (uiCombat_10C040B0)
            {
                return null;
            }

            var tooltip = GameSystems.D20.Actions.GetRemainingTimeDescription(tbStatus.hourglassState);

            if (tbStatus.surplusMoveDistance > 0.0f)
            {
                var suffix = GameSystems.D20.Combat.GetCombatMesLine(163);
                tooltip += $"\n{(int) tbStatus.surplusMoveDistance} {suffix}";
            }

            var attackNumber = tbStatus.baseAttackNumCode + tbStatus.numBonusAttacks - tbStatus.attackModeCode;
            if (attackNumber > 0)
            {
                var suffix = GameSystems.D20.Combat.GetCombatMesLine(164);
                tooltip += $"\n{attackNumber} {suffix}";
            }

            return tooltip;
        }

        [TempleDllLocation(0x10172a80)]
        private void OnActionBarButtonClick()
        {
            if (UiSystems.HelpManager.IsSelectingHelpTarget)
            {
                UiSystems.HelpManager.ShowPredefinedTopic(17);
            }
        }

        [TempleDllLocation(0x10172eb0)]
        private void OnBeforeRenderNextTurn(WidgetButton nextTurn)
        {
            var disabled = !GameSystems.Party.IsPlayerControlled(GameSystems.D20.Initiative.CurrentActor) ||
                           uiCombat_10C040B0;
            nextTurn.SetDisabled(disabled);
        }

        [TempleDllLocation(0x10172ac0)]
        private void OnNextTurnClick()
        {
            // Don't allow ending turns of non party controlled players
            var currentActor = GameSystems.D20.Initiative.CurrentActor;
            if (!GameSystems.Combat.IsCombatActive() || UiSystems.InGameSelect.IsPicking ||
                !GameSystems.Party.IsPlayerControlled(currentActor))
            {
                return;
            }

            if (UiSystems.HelpManager.IsSelectingHelpTarget)
            {
                UiSystems.HelpManager.ShowPredefinedTopic(17);
            }
            else if (GameSystems.D20.Actions.IsCurrentlyPerforming(currentActor))
            {
                // Delay ending the turn, TODO: This is badly placed inside a render function...
                uiCombat_10C040B0 = true;
            }

            Logger.Info("Combat UI for {0} ending turn (button)...", currentActor);
            GameSystems.Combat.AdvanceTurn(currentActor);
        }


        [TempleDllLocation(0x10173070)]
        private void UiCombatActionBarRender(WidgetContainer container)
        {
            // Get the on-screen content rect
            var contentRect = container.GetContentArea();

            var v21 = true;
            var tbStatus = GameSystems.D20.Actions.curSeqGetTurnBasedStatus()?.Copy();
            if (GameSystems.Combat.IsCombatActive() && tbStatus != null)
            {
                var actor = GameSystems.D20.Initiative.CurrentActor;
                if (uiCombat_10C040B0)
                {
                    if (!GameSystems.D20.Actions.IsCurrentlyPerforming(actor))
                    {
                        uiCombat_10C040B0 = false;
                        if (GameSystems.Party.IsPlayerControlled(actor))
                        {
                            Logger.Info("Combat UI for {0} ending turn (button)...", actor);
                            GameSystems.Combat.AdvanceTurn(actor);
                        }
                    }
                }

                // TODO ui_render_img_file/*0x101e8460*/(dword_10C04088/*0x10c04088*/, uiCombatMainWndX/*0x10c04040*/, uiCombatMainWndY/*0x10c04044*/);
                if (GameSystems.Combat.IsCombatActive() &&
                    GameSystems.Party.IsPlayerControlled(GameSystems.D20.Initiative.CurrentActor) && !uiCombat_10C040B0)
                {
                    var maxFullRoundMoveDist = UiCombatActionBarGetMaximumMoveDistance();
                    if (maxFullRoundMoveDist <= 0.0f)
                    {
                        maxFullRoundMoveDist = 30.0f;
                    }

                    if (actor != actionBarActor)
                    {
                        GameSystems.Vagrant.ActionbarUnsetFlag1(_actionBar);
                        actionBarEndingMoveDist = 0;
                    }

                    float actualRemainingMoveDist;
                    if (GameSystems.Vagrant.ActionBarIsFlag1Set(_actionBar))
                    {
                        actualRemainingMoveDist = GameSystems.Vagrant.ActionBarGetPulseMinVal(_actionBar);
                        v21 = false;
                    }
                    else if (GameSystems.D20.Actions.IsCurrentlyPerforming(actor))
                    {
                        actualRemainingMoveDist = actionBarEndingMoveDist;
                        v21 = false;
                    }
                    else
                    {
                        actualRemainingMoveDist = UiCombatActionBarGetRemainingMoveDistance(tbStatus);
                    }

                    var factor = Math.Clamp(actualRemainingMoveDist / maxFullRoundMoveDist, 0.0f, 1.0f);

                    var v13 = (int) (contentRect.Height * factor);
                    int v14 = v13;
                    if (v13 > 0)
                    {
                        var a1 = new Render2dArgs();
                        var v22 = new Rectangle(0,
                            contentRect.Height - v13,
                            contentRect.Width,
                            v13);

                        var v23 = new Rectangle(
                            contentRect.X,
                            contentRect.Y + contentRect.Height - v13,
                            v22.Width,
                            v22.Height
                        );
                        a1.customTexture = _combatBarFill.Resource;
                        a1.srcRect = v22;
                        a1.destRect = v23;
                        a1.flags = Render2dFlag.BUFFERTEXTURE;
                        Tig.ShapeRenderer2d.DrawRectangle(ref a1);
                    }

                    if (UiIntgameActionbarShouldUpdate() && v21)
                    {
                        if (GameSystems.D20.Actions.seqCheckFuncs(out tbStatus) != ActionErrorCode.AEC_OK)
                        {
                            UiCombatActionBarDrawButton(contentRect, _combatBarFillInvalid.Resource);
                        }
                        else
                        {
                            var v16 = Math.Clamp(
                                UiCombatActionBarGetRemainingMoveDistance(tbStatus) / maxFullRoundMoveDist, 0.0f, 1.0f);
                            int v17 = v14 - (int) (contentRect.Height * v16);
                            if (v17 >= 1)
                            {
                                Render2dArgs a1 = new Render2dArgs();
                                var srcRect = new Rectangle(
                                    0,
                                    contentRect.Height - v14,
                                    contentRect.Width,
                                    v17
                                );
                                var destRect = new Rectangle(
                                    contentRect.X,
                                    srcRect.Y + contentRect.Y,
                                    srcRect.Width,
                                    srcRect.Height
                                );
                                a1.flags = Render2dFlag.BUFFERTEXTURE | Render2dFlag.VERTEXCOLORS |
                                           Render2dFlag.VERTEXALPHA;
                                a1.srcRect = srcRect;
                                a1.destRect = destRect;
                                a1.customTexture = _combatBarHighlight2.Resource;

                                var alpha = GameSystems.Vagrant.ActionBarGetPulseMinVal(_pulseAnimation);
                                var color = new PackedLinearColorA(255, 255, 255, (byte) alpha);
                                a1.vertexColors = new[]
                                {
                                    color, color, color, color
                                };

                                if (v17 > 0)
                                {
                                    Tig.ShapeRenderer2d.DrawRectangle(ref a1);
                                }
                            }
                        }
                    }
                }
                else
                {
                    UiCombatActionBarDrawButton(contentRect, _combatBarGrey.Resource);
                }
            }
        }

        [TempleDllLocation(0x10172a00)]
        private void UiCombatActionBarDrawButton(Rectangle rect, ITexture texture)
        {
            var args = new Render2dArgs();
            args.srcRect = new Rectangle(0, 0, rect.Width, rect.Height);
            args.destRect = rect;
            args.flags = Render2dFlag.BUFFERTEXTURE;
            args.customTexture = texture;
            Tig.ShapeRenderer2d.DrawRectangle(ref args);
        }

        [TempleDllLocation(0x10173ad0)]
        private bool UiIntgameActionbarShouldUpdate()
        {
            return GameSystems.Combat.IsCombatActive() && UiSystems.TurnBased.uiIntgameWidgetEnteredForRender;
        }

        [TempleDllLocation(0x10172fb0)]
        private float UiCombatActionBarGetRemainingMoveDistance(TurnBasedStatus tbStat)
        {
            var moveSpeed = GameSystems.D20.Initiative.CurrentActor.Dispatch41GetMoveSpeed(out _);
            if (moveSpeed < 0.001f)
            {
                moveSpeed = 30.0f;
            }

            if (GameSystems.D20.Actions.GetHourglassTransition(tbStat.hourglassState, ActionCostType.FullRound) != HourglassState.INVALID)
            {
                // A full action can still be taken, return double move distance (PS: This is not great)
                return moveSpeed + moveSpeed;
            }

            float newMoveActDistance;
            if (GameSystems.D20.Actions.GetHourglassTransition(tbStat.hourglassState, ActionCostType.Standard) == HourglassState.INVALID
                && GameSystems.D20.Actions.GetHourglassTransition(tbStat.hourglassState, ActionCostType.Move) == HourglassState.INVALID)
            {
                // No new move actions can be taken
                newMoveActDistance = 0.0f;
            }
            else
            {
                // Full move actions can be taken, so take the max dist for one (PS: This is not great)
                newMoveActDistance = moveSpeed;
            }

            // Add the remaining distance of an already performed moved action that wasn't fully used up
            var remainDist = newMoveActDistance + tbStat.surplusMoveDistance;
            if (GameSystems.D20.Actions.GetHourglassTransition(tbStat.hourglassState, ActionCostType.Move) != HourglassState.INVALID
                || (tbStat.tbsFlags & TurnBasedStatusFlags.Moved) != 0)
            {
                return remainDist;
            }
            else
            {
                // Five foot step is still possible
                return remainDist + 5.0f;
            }
        }

        [TempleDllLocation(0x101729d0)]
        private float UiCombatActionBarGetMaximumMoveDistance()
        {
            var moveSpeed = GameSystems.D20.Initiative.CurrentActor.Dispatch41GetMoveSpeed(out _);
            if (moveSpeed < 0.001f)
            {
                moveSpeed = 30.0f;
            }

            return 2 * moveSpeed;
        }

        [TempleDllLocation(0x10173440)]
        public void StartMovement()
        {
            var currenTbStatus = GameSystems.D20.Actions.curSeqGetTurnBasedStatus();
            var startDist = UiCombatActionBarGetRemainingMoveDistance(currenTbStatus);

            GameSystems.D20.Actions.seqCheckFuncs(out var statusAfterAction);
            var endDist = UiCombatActionBarGetRemainingMoveDistance(statusAfterAction);

            GameSystems.Vagrant.ActionBarSetMovementValues(_actionBar, startDist, endDist,
                20.0f /* This was configurable in a MES file before */);
            actionBarActor = GameSystems.D20.Initiative.CurrentActor;
            actionBarEndingMoveDist = endDist;
        }
    }
}