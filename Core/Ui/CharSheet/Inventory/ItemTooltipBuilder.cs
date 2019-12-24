using System.Text;
using OpenTemple.Core.GameObject;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.D20;

namespace OpenTemple.Core.Ui.CharSheet.Inventory
{
    public static class ItemTooltipBuilder
    {
        private static void StartNewBlock(StringBuilder builder)
        {
            if (builder.Length > 0)
            {
                builder.Append("\n\n");
            }
        }

        public static string BuildItemTooltip(GameObjectBody observer, GameObjectBody item)
        {
            var tooltipBuilder = new StringBuilder();

            if (GameSystems.Item.IsMagical(item))
            {
                if (GameSystems.D20.D20Query(observer, D20DispatcherKey.QUE_Critter_Can_Detect_Magic) )
                {
                    var magical = UiSystems.Tooltip.GetString(132);
                    tooltipBuilder.Append(magical);
                }
            }

            if (item.type == ObjectType.armor)
            {
                if (GameSystems.Item.IsIncompatibleWithDruid(item, observer))
                {
                    StartNewBlock(tooltipBuilder);
                    tooltipBuilder.Append(GameSystems.Item.GetItemErrorString(ItemErrorCode.Prohibited_Due_To_Class));
                }

                if (item.GetItemWearFlags().HasFlag(ItemWearFlag.ARMOR))
                {
                    if (item.GetArmorFlags().GetArmorType() != ArmorFlag.TYPE_NONE)
                    {
                        if (!GameSystems.Feat.IsProficientWithArmor(observer, item))
                        {
                            StartNewBlock(tooltipBuilder);
                            tooltipBuilder.Append(UiSystems.Tooltip.GetString(136));
                        }
                    }
                }
            }
            else if (item.type == ObjectType.weapon)
            {
                if (GameSystems.Item.GetWieldType(observer, item) == 3)
                {
                    StartNewBlock(tooltipBuilder);
                    tooltipBuilder.Append(GameSystems.Item.GetItemErrorString(ItemErrorCode.Item_Too_Large));
                }

                if (!GameSystems.Feat.IsProficientWithWeapon(observer, item))
                {
                    StartNewBlock(tooltipBuilder);
                    tooltipBuilder.Append(UiSystems.Tooltip.GetString(136));
                }
            }

            if (UiSystems.CharSheet.State == CharInventoryState.Bartering)
            {
                StartNewBlock(tooltipBuilder);
                if (UiSystems.CharSheet.Looting.IsIdentifying)
                {
                    if (GameSystems.Item.IsIdentified(item))
                    {
                        var alreadyIdentified = UiSystems.Tooltip.GetString(130);
                        tooltipBuilder.Append(alreadyIdentified);
                    }
                    else
                    {
                        if (GameSystems.Party.GetPartyMoney() <= 10000)
                        {
                            // TODO: Hardcoded cost of identify is bad

                            // Not enough money
                            tooltipBuilder.Append(UiSystems.Tooltip.GetString(123));
                            tooltipBuilder.Append("\n\n");
                        }

                        var identifyName = GameSystems.Spell.GetSpellName(238); // TODO Common spell id for identify
                        tooltipBuilder.Append(identifyName);
                        tooltipBuilder.Append(": 100 ");
                        tooltipBuilder.Append(GameSystems.Stat.GetStatShortName(Stat.money_gp));
                    }
                }
                else
                {
                    tooltipBuilder.Append(UiSystems.Tooltip.GetItemDescriptionBarter(observer, item));
                }
            }

            if (GameSystems.MapObject.HasLongDescription(item))
            {
                StartNewBlock(tooltipBuilder);
                tooltipBuilder.Append(UiSystems.Tooltip.GetString(6048));
            }

            if (tooltipBuilder.Length > 0)
            {
                return tooltipBuilder.ToString();
            }
            else
            {
                return null;
            }
        }
    }
}