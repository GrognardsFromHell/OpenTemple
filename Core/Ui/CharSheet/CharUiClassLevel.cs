using System;
using System.Text;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.GFX;
using OpenTemple.Core.IO;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.D20.Classes;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Ui.Styles;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui.CharSheet;

public class CharUiClassLevel : WidgetButtonBase
{
    private readonly WidgetText _text;
    private readonly string _textSeparator;
    private readonly string _textLevel;
    private readonly string _textNpc;

    public CharUiClassLevel()
    {
        AddContent(_text = new WidgetText());

        var translations = Tig.FS.ReadMesFile("mes/0_char_ui_text.mes");
        _textSeparator = ' ' + translations[1600] + ' ';
        _textLevel = translations[1590];
        _textNpc = '(' + translations[1610] + ')';

        AddStyle("char-ui-dialog-title");
    }

    [TempleDllLocation(0x10144b40)]
    public override void Render(UiRenderContext context)
    {
        var currentCritter = UiSystems.CharSheet.CurrentCritter;
        if (currentCritter == null)
        {
            return;
        }

        _text.Text = BuildClassText(currentCritter, false, false);

        // Switch to class-name shorthands if the text doesn't fit (i.e. "Brd Level 1" instead of "Bard Level 1")
        if (_text.IsTrimmed)
        {
            // Use the current text as the tooltip if we're going to shorten it
            TooltipText = _text.Text;
            _text.Text = BuildClassText(currentCritter, true, false);

            // If the text still doesn't fit, omit "Level" (i.e. "Brd 1" instead of "Bard Level 1")
            if (_text.IsTrimmed)
            {
                _text.Text = BuildClassText(currentCritter, true, true);
            }
        }
        else
        {
            TooltipText = null;
        }

        base.Render(context);
    }

    private string BuildClassText(GameObject currentCritter, bool shortClassNames, bool omitLevelText)
    {
        var textBuilder = new StringBuilder();

        if (currentCritter.IsPC() || Globals.Config.ShowNpcStats)
        {
            // cycle through classes
            bool isFirst = true;
            foreach (var classCode in D20ClassSystem.AllClasses)
            {
                var classLvl = GameSystems.Stat.StatLevelGet(currentCritter, classCode);
                if (classLvl <= 0)
                {
                    continue;
                }

                if (!isFirst)
                {
                    // add a "/" separator
                    textBuilder.Append(_textSeparator);
                }
                else
                {
                    isFirst = false;
                }

                string className;
                if (!shortClassNames)
                {
                    className = GameSystems.Stat.GetStatName(classCode);
                }
                else
                {
                    className = GameSystems.Stat.GetStatShortName(classCode);
                }

                textBuilder.Append(omitLevelText
                    ? $"{className} {classLvl}"
                    : $"{className} {_textLevel} {classLvl}");
            }
        }
        else
        {
            textBuilder.Append(_textNpc);
        }

        return textBuilder.ToString();
    }
}