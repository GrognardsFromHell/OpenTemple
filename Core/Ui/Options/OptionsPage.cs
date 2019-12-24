using System.Collections.Generic;

namespace OpenTemple.Core.Ui.Options
{
    public class OptionsPage
    {
        public string Name { get; }

        public IList<Option> Options { get; }

        public OptionsPage(string name, params Option[] options)
        {
            Name = name;
            Options = options;
        }
    }
}