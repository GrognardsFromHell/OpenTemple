using System;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.GFX;
using SpicyTemple.Core.IO;
using SpicyTemple.Core.Platform;
using SpicyTemple.Core.Systems;
using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.TigSubsystems;
using SpicyTemple.Core.Ui.WidgetDocs;

namespace SpicyTemple.Core.Ui.CharSheet.Stats
{
    public class CharSheetStatsUi : IDisposable
    {
        public WidgetContainer Container { get; }

        [TempleDllLocation(0x101ccce0)]
        public CharSheetStatsUi(Rectangle mainWindowRectangle)
        {
            Stub.TODO();

            var uiParams = new StatsUiParams(
                mainWindowRectangle,
                Tig.FS.ReadMesFile("art/interface/char_ui/char_stats_ui/1_char_stats_ui.mes"),
                Tig.FS.ReadMesFile("art/interface/char_ui/char_stats_ui/1_char_stats_ui_textures.mes"),
                Tig.FS.ReadMesFile("mes/1_char_stats_ui_text.mes")
            );

            Container = new WidgetContainer(uiParams.MainWindow);
            Container.SetVisible(false);

            CreateMoneyLabels(uiParams);

            CreateXpAndLevelLabels(uiParams);

            CreateAbilityScoreLabels(uiParams);

            CreateDefensiveLabels(uiParams);

            CreateSavingThrowLabels(uiParams);

            CreatePhysicalAppearanceLabels(uiParams);

            CreateCombatLabels(uiParams);
        }

        private void CreateXpAndLevelLabels(StatsUiParams uiParams)
        {
            Container.Add(new StatsLabel(Stat.experience,
                "TAG_EXPERIENCE_AND_LEVELS",
                uiParams.XpLabel,
                StatsUiTexture.ButtonEXPClick,
                StatsUiTexture.ButtonEXPHover,
                uiParams
            ));
            Container.Add(new StatsValue(
                obj => GetStatValue(obj, Stat.experience),
                uiParams.XpValue,
                StatsUiTexture.ButtonEXPOutputClick,
                StatsUiTexture.ButtonEXPOutputHover,
                uiParams
            ));
            Container.Add(new StatsLabel(Stat.level,
                "TAG_EXPERIENCE_AND_LEVELS",
                uiParams.LevelLabel,
                StatsUiTexture.ButtonLevelClick,
                StatsUiTexture.ButtonLevelHover,
                uiParams
            ));
            Container.Add(new StatsValue(
                obj => GetStatValue(obj, Stat.level),
                uiParams.LevelValue,
                StatsUiTexture.ButtonLevelOutputClick,
                StatsUiTexture.ButtonLevelOutputHover,
                uiParams
            ));
        }

        private void CreateAbilityScoreLabels(StatsUiParams uiParams)
        {
            void AddLabel(Stat stat, string helpTopic, Rectangle rectangle)
            {
                Container.Add(new StatsLabel(stat,
                    helpTopic,
                    rectangle,
                    StatsUiTexture.ButtonSTRClick,
                    StatsUiTexture.ButtonSTRHover,
                    uiParams
                ));
            }

            void AddScore(Stat stat, Rectangle rectangle)
            {
                var widget = new StatsValue(
                    obj => GetAbilityScoreValue(obj, stat),
                    rectangle,
                    StatsUiTexture.ButtonLevelOutputClick,
                    StatsUiTexture.ButtonLevelOutputHover,
                    uiParams
                );
                widget.SetClickHandler(() => ShowAbilityScoreHelp(stat));
                Container.Add(widget);
            }

            void AddModifier(Stat stat, Rectangle rectangle)
            {
                var widget = new StatsValue(
                    obj => GetStatModifierText(obj, stat),
                    rectangle,
                    StatsUiTexture.ButtonLevelOutputClick,
                    StatsUiTexture.ButtonLevelOutputHover,
                    uiParams
                );
                widget.Tooltip = null;
                widget.SetClickHandler(() => { GameSystems.Help.ShowTopic("TAG_ABILITY_MODIFIERS"); });
                Container.Add(widget);
            }

            AddLabel(Stat.strength, "TAG_STRENGTH", uiParams.StrLabel);
            AddScore(Stat.strength, uiParams.StrValue);
            AddModifier(Stat.str_mod, uiParams.StrBonusValue);

            AddLabel(Stat.dexterity, "TAG_DEXTERITY", uiParams.DexLabel);
            AddScore(Stat.dexterity, uiParams.DexValue);
            AddModifier(Stat.dex_mod, uiParams.DexBonusValue);

            AddLabel(Stat.constitution, "TAG_CONSTITUTION", uiParams.ConLabel);
            AddScore(Stat.constitution, uiParams.ConValue);
            AddModifier(Stat.con_mod, uiParams.ConBonusValue);

            AddLabel(Stat.intelligence, "TAG_INTELLIGENCE", uiParams.IntLabel);
            AddScore(Stat.intelligence, uiParams.IntValue);
            AddModifier(Stat.int_mod, uiParams.IntBonusValue);

            AddLabel(Stat.wisdom, "TAG_WISDOM", uiParams.WisLabel);
            AddScore(Stat.wisdom, uiParams.WisValue);
            AddModifier(Stat.wis_mod, uiParams.WisBonusValue);

            AddLabel(Stat.charisma, "TAG_CHARISMA", uiParams.ChaLabel);
            AddScore(Stat.charisma, uiParams.ChaValue);
            AddModifier(Stat.cha_mod, uiParams.ChaBonusValue);
        }

        private void CreateDefensiveLabels(StatsUiParams uiParams)
        {
            Container.Add(new StatsLabel(Stat.hp_current,
                "TAG_HIT_POINTS",
                uiParams.HpLabel,
                StatsUiTexture.ButtonHPClick,
                StatsUiTexture.ButtonHPHover,
                uiParams
            ));
            var hpValue = new StatsValue(
                GetHitPointsText,
                uiParams.HpValue,
                StatsUiTexture.ButtonHPOutputClick,
                StatsUiTexture.ButtonHPOutputHover,
                uiParams
            );
            hpValue.SetClickHandler(() => GameSystems.Help.ShowTopic("TAG_HIT_POINTS"));
            hpValue.Tooltip = null;
            Container.Add(hpValue);

            Container.Add(new StatsLabel(Stat.ac,
                "TAG_ARMOR_CLASS",
                uiParams.AcLabel,
                StatsUiTexture.ButtonHPClick,
                StatsUiTexture.ButtonHPHover,
                uiParams
            ));
            var acValue = new StatsValue(
                obj => GetStatValue(obj, Stat.ac),
                uiParams.AcValue,
                StatsUiTexture.ButtonHPOutputClick,
                StatsUiTexture.ButtonHPOutputHover,
                uiParams
            );
            acValue.SetClickHandler(ShowArmorClassHelp);
            Container.Add(acValue);
        }

        private void CreateSavingThrowLabels(StatsUiParams uiParams)
        {
            void AddWidgets(Stat stat, string helpTopic, Rectangle labelRect, Rectangle valueRect)
            {
                Container.Add(new StatsLabel(stat,
                    helpTopic,
                    labelRect,
                    StatsUiTexture.ButtonFORTClick,
                    StatsUiTexture.ButtonFORTHover,
                    uiParams
                ));
                var valueWidget = new StatsValue(
                    obj => GetStatModifierText(obj, stat),
                    valueRect,
                    StatsUiTexture.ButtonFORTOutputClick,
                    StatsUiTexture.ButtonFORTOutputHover,
                    uiParams
                );
                valueWidget.SetClickHandler(() => ShowSavingThrowHelp(stat));
                Container.Add(valueWidget);
            }

            AddWidgets(Stat.save_fortitude, "TAG_FORTITUDE", uiParams.FortLabel, uiParams.FortValue);
            AddWidgets(Stat.save_reflexes, "TAG_REFLEX", uiParams.RefLabel, uiParams.RefValue);
            AddWidgets(Stat.save_willpower, "TAG_WILL", uiParams.WillLabel, uiParams.WillValue);
        }

        private void CreatePhysicalAppearanceLabels(StatsUiParams uiParams)
        {
            Container.Add(new StatsLabel(Stat.height,
                null,
                uiParams.HeightLabel,
                StatsUiTexture.ButtonHTClick,
                StatsUiTexture.ButtonHTHover,
                uiParams
            ));
            var heightValue = new StatsValue(
                GetHeightText,
                uiParams.HeightValue,
                StatsUiTexture.ButtonHTOutputClick,
                StatsUiTexture.ButtonHTOutputHover,
                uiParams
            );
            heightValue.Tooltip = null;
            Container.Add(heightValue);

            Container.Add(new StatsLabel(Stat.weight,
                null,
                uiParams.WeightLabel,
                StatsUiTexture.ButtonHTClick,
                StatsUiTexture.ButtonHTHover,
                uiParams
            ));
            var weightValue = new StatsValue(
                obj => GetStatValue(obj, Stat.weight),
                uiParams.WeightValue,
                StatsUiTexture.ButtonHTOutputClick,
                StatsUiTexture.ButtonHTOutputHover,
                uiParams
            );
            weightValue.Tooltip = null;
            Container.Add(weightValue);
        }

        private void CreateCombatLabels(StatsUiParams uiParams)
        {
            Container.Add(new StatsLabel(Stat.initiative_bonus,
                "TAG_INITIATIVE",
                uiParams.InitLabel,
                StatsUiTexture.ButtonInitiativeClick,
                StatsUiTexture.ButtonInitiativeHover,
                uiParams
            ));
            var initValue = new StatsValue(
                obj => GetStatModifierText(obj, Stat.initiative_bonus),
                uiParams.InitValue,
                StatsUiTexture.ButtonInitiativeOutputClick,
                StatsUiTexture.ButtonInitiativeOutputHover,
                uiParams
            );
            initValue.SetClickHandler(ShowInitiativeBonusHelp);
            Container.Add(initValue);

            Container.Add(new StatsLabel(Stat.movement_speed,
                "TAG_MOVEMENT_RATE",
                uiParams.SpeedLabel,
                StatsUiTexture.ButtonInitiativeClick,
                StatsUiTexture.ButtonInitiativeHover,
                uiParams
            ));
            var speedValue = new StatsValue(
                GetMovementSpeedText,
                uiParams.SpeedValue,
                StatsUiTexture.ButtonInitiativeOutputClick,
                StatsUiTexture.ButtonInitiativeOutputHover,
                uiParams
            );
            speedValue.SetClickHandler(() => GameSystems.Help.ShowTopic("TAG_MOVEMENT_RATE"));
            speedValue.Tooltip = null;
            Container.Add(speedValue);


            // Primary Weapon Attack Bonus
            Container.Add(new StatsLabel(Stat.melee_attack_bonus,
                "TAG_COMBAT",
                uiParams.PrimaryAtkLabel,
                StatsUiTexture.ButtonInitiativeClick,
                StatsUiTexture.ButtonInitiativeHover,
                uiParams
            ));
            var primaryAtkValue = new StatsValue(
                GetPrimaryWeaponAttackBonus,
                uiParams.PrimaryAtkValue,
                StatsUiTexture.ButtonInitiativeOutputClick,
                StatsUiTexture.ButtonInitiativeOutputHover,
                uiParams
            );
            primaryAtkValue.SetClickHandler(ShowPrimaryWeaponAttackBonusHelp);
            Container.Add(primaryAtkValue);

            // Secondary Weapon Attack Bonus
            Container.Add(new StatsLabel(Stat.ranged_attack_bonus,
                "TAG_COMBAT",
                uiParams.SecondaryAtkLabel,
                StatsUiTexture.ButtonInitiativeClick,
                StatsUiTexture.ButtonInitiativeHover,
                uiParams
            ));
            var secondaryAtkValue = new StatsValue(
                GetSecondaryWeaponAttackBonus,
                uiParams.SecondaryAtkValue,
                StatsUiTexture.ButtonInitiativeOutputClick,
                StatsUiTexture.ButtonInitiativeOutputHover,
                uiParams
            );
            secondaryAtkValue.SetClickHandler(ShowSecondaryWeaponAttackBonusHelp);
            Container.Add(secondaryAtkValue);
        }

        private string GetMovementSpeedText(GameObjectBody obj)
        {
            var movementSpeed = GameSystems.Stat.Dispatch41GetMoveSpeed(obj, out _);
            return ((int) movementSpeed).ToString();
        }

        [TempleDllLocation(0x101c8c10)]
        private void ShowPrimaryWeaponAttackBonusHelp()
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x101c8d30)]
        private void ShowSecondaryWeaponAttackBonusHelp()
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x101c4740)]
        private string GetSecondaryWeaponAttackBonus(GameObjectBody critter)
        {
            var secondaryWeapon = GameSystems.Item.ItemWornAt(critter, EquipSlot.WeaponSecondary);
            if (secondaryWeapon != null && secondaryWeapon.type != ObjectType.armor)
            {
                var attackBonus = DispIoAttackBonus.Default;
                attackBonus.attackPacket.weaponUsed = secondaryWeapon;
                attackBonus.attackPacket.d20ActnType = D20ActionType.FULL_ATTACK;
                attackBonus.attackPacket.dispKey = 6;
                attackBonus.attackPacket.flags = D20CAF.SECONDARY_WEAPON;
                attackBonus.attackPacket.attacker = critter;

                var overallBonus = GameSystems.Stat.Dispatch16GetToHitBonus(critter, attackBonus);
                return $"{overallBonus:+#;-#;0}";
            }
            else
            {
                return "--"; // Indicates no secondary weapon equipped
            }
        }

        [TempleDllLocation(0x101c4420)]
        private string GetPrimaryWeaponAttackBonus(GameObjectBody critter)
        {
            var primaryWeapon = GameSystems.Item.ItemWornAt(critter, EquipSlot.WeaponPrimary);
            var secondaryWeapon = GameSystems.Item.ItemWornAt(critter, EquipSlot.WeaponSecondary);

            var attackBonus = DispIoAttackBonus.Default;
            attackBonus.attackPacket.d20ActnType = D20ActionType.FULL_ATTACK;
            if (secondaryWeapon != null && secondaryWeapon.type != ObjectType.armor)
            {
                attackBonus.attackPacket.dispKey = 5;
            }
            else
            {
                attackBonus.attackPacket.dispKey = 1;
            }

            attackBonus.attackPacket.weaponUsed = primaryWeapon;
            attackBonus.attackPacket.flags = 0;
            if (primaryWeapon != null)
            {
                var v13 = primaryWeapon.WeaponFlags;
                if (v13.HasFlag(WeaponFlag.RANGED_WEAPON))
                {
                    attackBonus.attackPacket.flags |= D20CAF.RANGED;
                }
            }

            attackBonus.attackPacket.attacker = critter;
            attackBonus.attackPacket.ammoItem = GameSystems.Item.ItemWornAt(critter, EquipSlot.Ammo);

            var overallBonus = GameSystems.Stat.Dispatch16GetToHitBonus(critter, attackBonus);
            return $"{overallBonus:+#;-#;0}";
        }

        [TempleDllLocation(0x101c8b50)]
        private void ShowInitiativeBonusHelp()
        {
            var critter = UiSystems.CharSheet.CurrentCritter;
            throw new NotImplementedException();
        }

        private static string GetHeightText(GameObjectBody obj)
        {
            var height = obj.GetStat(Stat.height);
            var inches = height % 12;
            var feet = height / 12;
            return $"{inches}'{feet}\"";
        }

        private static string GetHitPointsText(GameObjectBody obj)
        {
            var currentHp = obj.GetStat(Stat.hp_current);
            var maxHp = obj.GetStat(Stat.hp_max);

            if (currentHp < maxHp)
            {
                // Display the current hp in red
                return $"@3{currentHp}@0/{maxHp}";
            }
            else
            {
                return $"{currentHp}/{maxHp}";
            }
        }

        [TempleDllLocation(0x101c8a70)]
        private void ShowAbilityScoreHelp(Stat stat)
        {
            var critter = UiSystems.CharSheet.CurrentCritter;
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x101c8af0)]
        private void ShowArmorClassHelp()
        {
            var critter = UiSystems.CharSheet.CurrentCritter;
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x101c8bb0)]
        private void ShowSavingThrowHelp(Stat stat)
        {
            var critter = UiSystems.CharSheet.CurrentCritter;
            throw new NotImplementedException();
        }

        private static string GetStatValue(GameObjectBody obj, Stat stat)
        {
            return obj.GetStat(stat).ToString(CultureInfo.InvariantCulture);
        }

        private static string GetAbilityScoreValue(GameObjectBody obj, Stat stat)
        {
            var baseScore = obj.GetBaseStat(stat);
            var score = obj.GetStat(stat);

            // Select a color code indicating if there's a malus or bonus being applied
            var colorCode = 0;
            if (score > baseScore)
            {
                colorCode = 1;
            }
            else if (score < baseScore)
            {
                colorCode = 2;
            }

            return "@" + colorCode + score;
        }

        private static string GetStatModifierText(GameObjectBody obj, Stat stat)
        {
            var score = obj.GetStat(stat);

            return $"{score:+#;-#;0}";
        }

        private void CreateMoneyLabels(StatsUiParams uiParams)
        {
            Container.Add(new StatsCurrencyLabel(uiParams, MoneyType.Platinum));
            Container.Add(new StatsCurrencyLabel(uiParams, MoneyType.Gold));
            Container.Add(new StatsCurrencyLabel(uiParams, MoneyType.Silver));
            Container.Add(new StatsCurrencyLabel(uiParams, MoneyType.Copper));
        }

        [TempleDllLocation(0x101c8dd0)]
        public void Dispose()
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x101be430)]
        public void Show()
        {
            Container.SetVisible(true);
            Stub.TODO();
        }

        [TempleDllLocation(0x101be460)]
        public void Hide()
        {
            Stub.TODO();
        }

        public void Reset()
        {
        }
    }
}