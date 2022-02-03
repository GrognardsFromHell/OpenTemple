using System;

namespace OpenTemple.Core.Ui.Styles;

public class StyleParsingException : Exception
{
    public StyleParsingException(string message) : base(message)
    {
    }
}