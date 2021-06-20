using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.StyleSystemGenerator
{
    public class RootModel
    {
        public string Tool { get; }
        public string Version { get; }
        public StyleSystemModel StyleModel { get; } = new();
        public List<StylePropertyGroup> PropertyGroups => StyleModel.PropertyGroups;
        public List<EnumPropertyType> EnumTypes => PropertyTypes.Types.OfType<EnumPropertyType>().ToList();
        public List<StyleProperty> Properties { get; }

        public RootModel()
        {
            // Used for the GeneratedCode attribute
            var assemblyName = typeof(Program).Assembly.GetName();
            Tool = assemblyName.Name;
            Version = DateTime.UtcNow.ToString("O");
            Properties = StyleModel.PropertyGroups.SelectMany(pg => pg.Properties).ToList();
        }
    }
}