using System.IO;
using System.Text;
using Core.WidgetEventGenerator;
using Scriban;
using Scriban.Runtime;

var ctx = new TemplateContext
{
    TemplateLoader = new TemplateLoader()
};

var rootObj = new ScriptObject();
rootObj.Import(new RootModel());
rootObj.Import(typeof(ScriptFunctions));
ctx.PushGlobal(rootObj);

var generatedCode = RenderTemplate(ctx, "WidgetBase.generated.txt");
File.WriteAllText("Core/Ui/Widgets/WidgetBase.generated.cs", generatedCode, Encoding.UTF8);

static string RenderTemplate(TemplateContext ctx, string templateName)
{
    var templatePath = ctx.TemplateLoader.GetPath(ctx, default, templateName);
    var template = Template.Parse(File.ReadAllText(templatePath));
    return template.Render(ctx);
}
