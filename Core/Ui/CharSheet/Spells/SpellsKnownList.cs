using System.Drawing;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Systems;
using SpicyTemple.Core.Systems.Spells;
using SpicyTemple.Core.Ui.WidgetDocs;

namespace SpicyTemple.Core.Ui.CharSheet.Spells
{
    public class SpellsKnownList : WidgetContainer
    {
        public SpellsKnownList(Rectangle rectangle, GameObjectBody critter, int classCode) : base(rectangle)
        {
            var spellsKnown = critter.GetSpellArray(obj_f.critter_spells_known_idx);
            var domainSpells = GameSystems.Spell.IsDomainSpell(classCode);

            var currentY = 0;

            for (var level = 0; level <= 9; level++)
            {
                var headerAdded = false;

                for (var i = 0; i < spellsKnown.Count; i++)
                {
                    var spell = spellsKnown[i];
                    if (!domainSpells && GameSystems.Spell.IsDomainSpell(spell.classCode)
                        || domainSpells && spell.classCode != classCode
                        || spell.spellLevel != level)
                    {
                        continue;
                    }

                    if (!headerAdded)
                    {
                        var levelHeader = new WidgetText($"#{{char_ui_spells:4}} {level}", "char-spell-level");
                        levelHeader.SetY(currentY);
                        currentY += levelHeader.GetPreferredSize().Height;
                        AddContent(levelHeader);
                        headerAdded = true;
                    }

                    var spellLabel = new WidgetText(GameSystems.Spell.GetSpellName(spell.spellEnum), "char-spell-body");
                    spellLabel.SetY(currentY);
                    currentY += spellLabel.GetPreferredSize().Height;
                    AddContent(spellLabel);
                }
            }
        }
    }
}