using System.IO;
using System.Threading.Tasks;
using Scriban;
using Scriban.Parsing;
using Scriban.Runtime;

namespace Core.WidgetEventGenerator;

public class TemplateLoader : ITemplateLoader
{
    public string GetPath(TemplateContext context, SourceSpan callerSpan, string templateName)
    {
        var basePath = Path.GetDirectoryName(typeof(TemplateLoader).Assembly.Location);
        return Path.Join(basePath, "Templates", templateName);
    }

    public string Load(TemplateContext context, SourceSpan callerSpan, string templatePath)
    {
        return File.ReadAllText(templatePath);
    }

    public ValueTask<string> LoadAsync(TemplateContext context, SourceSpan callerSpan, string templatePath)
    {
        return new(Load(context, callerSpan, templatePath));
    }
}
