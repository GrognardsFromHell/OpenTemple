using System;
using NUnit.Framework;
using OpenTemple.Core;
using OpenTemple.Core.GFX;
using OpenTemple.Core.GFX.TextRendering;
using OpenTemple.Core.Ui.FlowModel;
using OpenTemple.Core.Ui.Styles;
using OpenTemple.Tests.TestUtils;

namespace OpenTemple.Tests.Core.GFX.TextRendering
{
    public class TextEngineTest : RenderingTest
    {
        [Test]
        public void TestRichTextRendering()
        {
            var paragraph = new Paragraph();
            paragraph.LocalStyles.FontSize = 18;
            paragraph.AppendContent("Lorem ipsum dolor sit amet consectetur adipiscing elit ultricies ");
            paragraph.AppendContent("quis dis", new StyleDefinition()
            {
                FontWeight = FontWeight.Bold
            });
            paragraph.AppendContent(", nisl ");
            paragraph.AppendContent("dapibus", new StyleDefinition()
            {
                FontStyle = FontStyle.Italic
            });
            paragraph.AppendContent(" per lobortis diam cubilia ");
            paragraph.AppendContent("odio", new StyleDefinition()
            {
                Color = new PackedLinearColorA(200, 0, 0, 255)
            });
            paragraph.AppendContent(
                " dignissim rutrum orci, egestas curabitur suscipit augue nulla lectus potenti est leo.");

            using var layout = Device.TextEngine.CreateTextLayout(paragraph, 400, 400);
            RenderAndCompare(layout, "Core/GFX/TextRendering/RichTextLayout.png");
        }

        [Test]
        public void TestNonRichTextRendering()
        {
            var styles = Globals.UiStyles.StyleResolver.Resolve(new[]
            {
                new StyleDefinition()
                {
                    Color = PackedLinearColorA.FromHex("#AAAAAA"),
                    FontSize = 20,
                    Underline = true
                }
            });

            using var layout = Device.TextEngine.CreateTextLayout(styles, "Hello World", 250, 250);
            RenderAndCompare(layout, "Core/GFX/TextRendering/NonRichTextLayout.png");
        }

        [Test]
        public void TestBorderAndBackground()
        {
            var backgroundAndBorder = Style(new StyleDefinition()
            {
                BackgroundColor = PackedLinearColorA.FromHex("#666666"),
                BorderColor = PackedLinearColorA.FromHex("#AAAAAA"),
                BorderWidth = 2
            });
            var backgroundOnly = Style(new StyleDefinition()
            {
                BackgroundColor = PackedLinearColorA.FromHex("#666666"),
                BorderColor = PackedLinearColorA.FromHex("#AAAAAA")
            });
            var borderOnly = Style(new StyleDefinition()
            {
                BorderColor = PackedLinearColorA.FromHex("#AAAAAA"),
                BorderWidth = 1
            });
            var thickBorder = Style(new StyleDefinition()
            {
                BorderColor = PackedLinearColorA.FromHex("#AAAAAA"),
                BorderWidth = 4
            });

            Device.BeginDraw();
            Device.TextEngine.RenderBackgroundAndBorder(25, 75, 100, 50, backgroundAndBorder);
            Device.TextEngine.RenderBackgroundAndBorder(25, 275, 100, 50, borderOnly);
            Device.TextEngine.RenderBackgroundAndBorder(225, 275, 100, 50, thickBorder);
            Device.TextEngine.RenderBackgroundAndBorder(225, 75, 100, 50, backgroundOnly);
            Device.EndDraw();

            var screenshot = TakeScreenshot();
            ImageComparison.AssertImagesEqual(screenshot, "Core/GFX/TextRendering/BackgroundAndBorder.png");
        }

        private void RenderAndCompare(TextLayout layout, string refName)
        {
            Device.BeginDraw();
            Device.TextEngine.RenderTextLayout(200, 200, layout);
            Device.EndDraw();

            var screenshot = TakeScreenshot();
            ImageComparison.AssertImagesEqual(screenshot, refName);
        }

        private static ComputedStyles Style(StyleDefinition definition)
        {
            return Globals.UiStyles.StyleResolver.Resolve(new[]
            {
                definition
            });
        }
    }
}