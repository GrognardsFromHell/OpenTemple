using System.IO;
using System.Text;
using System.Text.Json;
using Scriban;
using Scriban.Runtime;

namespace Core.StyleSystemGenerator
{
    public class Program
    {
        public static void Main(string[] args)
        {
            new Program().Run();
        }

        private void Run()
        {
            var ctx = new TemplateContext();
            ctx.TemplateLoader = new TemplateLoader();
            var rootObj = new ScriptObject();
            rootObj.Import(new RootModel());
            rootObj.Import(typeof(ScriptFunctions));
            ctx.PushGlobal(rootObj);

            var generatedCode = RenderTemplate(ctx, "StyleSystem.generated.txt");
            File.WriteAllText("Core/Ui/Styles/StyleSystem.generated.cs", generatedCode, Encoding.UTF8);

            var generatedSchema = RenderTemplate(ctx, "JsonSchema.txt");
            WriteFormattedJson("Data/schemas/styles.json", generatedSchema);
        }

        private static void WriteFormattedJson(string path, string jsonContent)
        {
            using var value = JsonDocument.Parse(jsonContent, new JsonDocumentOptions()
            {
                AllowTrailingCommas = true
            });

            using var stream = new FileStream(path, FileMode.Create);
            using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions()
            {
                Indented = true
            });
            value.WriteTo(writer);
        }

        private static string RenderTemplate(TemplateContext ctx, string templateName)
        {
            var templatePath = ctx.TemplateLoader.GetPath(ctx, default, templateName);
            var template = Template.Parse(File.ReadAllText(templatePath));
            return template.Render(ctx);
        }
    }
}