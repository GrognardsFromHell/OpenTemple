using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text.Json;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Ui.Styles;

namespace OpenTemple.Core.Ui.Widgets;

public delegate WidgetBase CustomWidgetFactory(string type, JsonElement definition);

internal class WidgetDocLoader
{
    private readonly string _path;

    private static readonly CustomWidgetFactory DefaultCustomFactory = (type, definition) => null;

    public WidgetDocLoader(string path)
    {
        _path = path;
    }

    public Dictionary<string, WidgetBase> Registry { get; } = new();

    public Dictionary<string, WidgetContent> ContentRegistry { get; } = new();

    public CustomWidgetFactory CustomFactory { get; set; } = DefaultCustomFactory;

    private void LoadContent(in JsonElement contentList, WidgetBase widget)
    {
        foreach (var contentJson in contentList.EnumerateArray())
        {
            var type = contentJson.GetProperty("type").GetString();

            WidgetContent content;
            switch (type)
            {
                case "image":
                {
                    var image = new WidgetImage();

                    var path = contentJson.GetStringProp("path", null);
                    if (path != null)
                    {
                        image.SetTexture(path);
                    }

                    if (contentJson.TryGetProperty("srcRect", out var srcRect))
                    {
                        image.SourceRect = new Rectangle(
                            srcRect.GetInt32Prop("x", 0),
                            srcRect.GetInt32Prop("y", 0),
                            srcRect.GetInt32Prop("width", 0),
                            srcRect.GetInt32Prop("height", 0)
                        );
                    }

                    if (contentJson.TryGetProperty("localStyles", out var localStylesNode))
                    {
                        StyleJsonDeserializer.DeserializeProperties(localStylesNode, image.LocalStyles);
                    }

                    content = image;
                    break;
                }

                case "text":
                {
                    var textContent = new WidgetText();
                    if (contentJson.TryGetProperty("style", out var styleIdNode))
                    {
                        if (styleIdNode.ValueKind != JsonValueKind.String)
                        {
                            throw new Exception("Style ID must be a string: " + styleIdNode);
                        }
                        textContent.AddStyle(styleIdNode.GetString()!);
                    }

                    if (contentJson.TryGetProperty("localStyles", out var localStylesNode))
                    {
                        StyleJsonDeserializer.DeserializeProperties(localStylesNode, textContent.LocalStyles);
                    }

                    if (contentJson.TryGetProperty("text", out var textNode))
                    {
                        if (textNode.ValueKind != JsonValueKind.String)
                        {
                            throw new Exception("text must be a string: " + textNode);
                        }
                        textContent.Text = textNode.GetString()!;
                    }

                    textContent.SetCenterVertically(contentJson.GetBoolProp("centerVertically", false));
                    content = textContent;
                    break;
                }

                case "rectangle":
                {
                    var rect = new WidgetRectangle();

                    if (contentJson.TryGetProperty("brush", out var brushJson) &&
                        brushJson.ValueKind != JsonValueKind.Null)
                    {
                        rect.Brush = brushJson.GetBrush();
                    }

                    if (contentJson.TryGetProperty("pen", out var penJson) &&
                        penJson.ValueKind != JsonValueKind.Null)
                    {
                        rect.Pen = penJson.GetColor();
                    }

                    if (contentJson.TryGetProperty("localStyles", out var localStylesNode))
                    {
                        StyleJsonDeserializer.DeserializeProperties(localStylesNode, rect.LocalStyles);
                    }

                    content = rect;
                    break;
                }

                default:
                    throw new Exception($"Unknown widget content type: '{type}'");
            }

            // Generic properties
            if (contentJson.TryGetProperty("x", out var xNode))
            {
                content.X = xNode.GetInt32();
            }

            if (contentJson.TryGetProperty("y", out var yNode))
            {
                content.Y = yNode.GetInt32();
            }

            if (contentJson.TryGetProperty("width", out var widthNode))
            {
                content.FixedWidth = widthNode.GetInt32();
            }

            if (contentJson.TryGetProperty("height", out var heightNode))
            {
                content.FixedHeight = heightNode.GetInt32();
            }

            widget.AddContent(content);

            // If the content had an ID, put it into the registry
            if (contentJson.TryGetProperty("id", out var idNode))
            {
                var id = idNode.GetString();
                if (ContentRegistry.ContainsKey(id))
                {
                    throw new Exception($"Duplicate content id: {id}");
                }

                ContentRegistry[id] = content;
            }
        }
    }

    public static void LoadWidgetBase(JsonElement jsonObj, WidgetBase widget)
    {
        if (jsonObj.TryGetProperty("__styleFiles", out var textStyleFiles))
        {
            foreach (var styleSheetName in textStyleFiles.EnumerateArray())
            {
                Globals.UiStyles.LoadStylesFile(styleSheetName.GetString());
            }
        }

        if (jsonObj.TryGetProperty("__styles", out var inlineTextStyles))
        {
            Globals.UiStyles.LoadStyles(inlineTextStyles);
        }

        if (jsonObj.TryGetProperty("__buttonStyleFiles", out var buttonStyleFiles))
        {
            foreach (var style in buttonStyleFiles.EnumerateArray())
            {
                Globals.WidgetButtonStyles.LoadStylesFile(style.GetString());
            }
        }

        if (jsonObj.TryGetProperty("__buttonStyles", out var inlineButtonStyles))
        {
            Globals.WidgetButtonStyles.LoadStyles(inlineButtonStyles);
        }

        var x = jsonObj.GetInt32Prop("x", 0);
        var y = jsonObj.GetInt32Prop("y", 0);
        widget.SetPos(x, y);

        if (jsonObj.TryGetProperty("localStyles", out var localStylesNode))
        {
            StyleJsonDeserializer.DeserializeProperties(localStylesNode, widget.LocalStyles);
        }

        var size = widget.GetSize();
        if (jsonObj.TryGetProperty("width", out var widthNode))
        {
            size.Width = widthNode.GetInt32();
            widget.SetAutoSizeWidth(false);
        }

        if (jsonObj.TryGetProperty("height", out var heightNode))
        {
            size.Height = heightNode.GetInt32();
            widget.SetAutoSizeHeight(false);
        }

        widget.SetSize(size);

        if (jsonObj.TryGetProperty("centerHorizontally", out var centerHorizontallyNode))
        {
            widget.SetCenterHorizontally(centerHorizontallyNode.GetBoolean());
        }

        if (jsonObj.TryGetProperty("centerVertically", out var centerVerticallyNode))
        {
            widget.SetCenterVertically(centerVerticallyNode.GetBoolean());
        }

        if (jsonObj.TryGetProperty("sizeToParent", out var sizeToParentNode))
        {
            widget.SetSizeToParent(sizeToParentNode.GetBoolean());
        }
    }

    private void LoadChildren(JsonElement jsonObj, WidgetContainer container)
    {
        foreach (var childJson in jsonObj.EnumerateArray())
        {
            var childWidget = LoadWidgetTree(childJson);
            childWidget.SetParent(container);
            container.Add(childWidget);
        }
    }

    private void LoadWidgetBaseWithContent(JsonElement jsonObj, WidgetContainer result)
    {
        LoadWidgetBase(jsonObj, result);

        if (jsonObj.TryGetProperty("content", out var contentNode))
        {
            LoadContent(contentNode, result);
        }

        if (jsonObj.TryGetProperty("children", out var childrenNode))
        {
            LoadChildren(childrenNode, result);
        }

        if (jsonObj.TryGetProperty("zIndex", out var zIndexNode))
        {
            result.ZIndex = zIndexNode.GetInt32();
        }
    }

    private WidgetBase LoadWidgetScrollView(JsonElement jsonObj)
    {
        var width = jsonObj.GetInt32Prop("width", 0);
        var height = jsonObj.GetInt32Prop("height", 0);

        var result = new WidgetScrollView(width, height);

        LoadWidgetBaseWithContent(jsonObj, result);

        return result;
    }

    private WidgetBase LoadWidgetScrollBox(JsonElement jsonObj)
    {
        var width = jsonObj.GetInt32Prop("width", 0);
        var height = jsonObj.GetInt32Prop("height", 0);

        var settings = new ScrollBoxSettings()
        {
            AutoLayout = true
        };
        var result = new ScrollBox(new Rectangle(0, 0,width, height), settings);

        LoadWidgetBaseWithContent(jsonObj, result);

        return result;
    }

    private WidgetBase LoadWidgetContainer(JsonElement jsonObj)
    {
        var width = jsonObj.GetInt32Prop("width", 0);
        var height = jsonObj.GetInt32Prop("height", 0);

        var result = new WidgetContainer(width, height);

        LoadWidgetBaseWithContent(jsonObj, result);

        return result;
    }

    private WidgetBase LoadWidgetTabBar(JsonElement jsonObj)
    {
        var width = jsonObj.GetInt32Prop("width", 0);
        var height = jsonObj.GetInt32Prop("height", 0);

        var result = new WidgetTabBar(width, height);

        LoadWidgetBaseWithContent(jsonObj, result);

        if (jsonObj.TryGetProperty("tabs", out var jsonTabs))
        {
            if (jsonTabs.ValueKind != JsonValueKind.Array)
            {
                throw new Exception($"tabs Property for tabBars must be an array in {_path}.");
            }

            result.SetTabs(jsonTabs.EnumerateArray().Select(el => el.GetString()));
        }

        if (jsonObj.TryGetProperty("tabStyle", out var jsonStyle))
        {
            switch (jsonStyle.GetString())
            {
                case "small":
                    result.Style = WidgetTabStyle.Small;
                    break;
                case "large":
                    result.Style = WidgetTabStyle.Large;
                    break;
                default:
                    throw new Exception($"Unknown tab bar style: {jsonStyle} in {_path}");
            }
        }

        if (jsonObj.TryGetProperty("spacing", out var spacingJson))
        {
            result.Spacing = spacingJson.GetInt32();
        }

        return result;
    }

    private WidgetBase LoadWidgetButton(JsonElement jsonObj)
    {
        var result = new WidgetButton();

        LoadWidgetBase(jsonObj, result);

        if (jsonObj.TryGetProperty("text", out var textProp))
        {
            result.Text = textProp.GetString();
        }

        WidgetButtonStyle buttonStyle;
        if (jsonObj.TryGetProperty("style", out var styleNode))
        {
            buttonStyle = Globals.WidgetButtonStyles.GetStyle(styleNode.GetString()).Copy();
        }
        else
        {
            buttonStyle = new WidgetButtonStyle();
        }

        // Allow local overrides
        foreach (var jsonProperty in jsonObj.EnumerateObject())
        {
            var key = jsonProperty.Name;
            switch (key)
            {
                case "disabledImage":
                    buttonStyle.DisabledImagePath = jsonProperty.Value.GetString();
                    break;
                case "activatedImage":
                    buttonStyle.ActivatedImagePath = jsonProperty.Value.GetString();
                    break;
                case "normalImage":
                    buttonStyle.NormalImagePath = jsonProperty.Value.GetString();
                    break;
                case "hoverImage":
                    buttonStyle.HoverImagePath = jsonProperty.Value.GetString();
                    break;
                case "pressedImage":
                    buttonStyle.PressedImagePath = jsonProperty.Value.GetString();
                    break;
                case "frameImage":
                    buttonStyle.FrameImagePath = jsonProperty.Value.GetString();
                    break;
                case "textStyle":
                    buttonStyle.TextStyleId = jsonProperty.Value.GetString();
                    break;
                case "hoverTextStyle":
                    buttonStyle.HoverTextStyleId = jsonProperty.Value.GetString();
                    break;
                case "pressedTextStyle":
                    buttonStyle.PressedTextStyleId = jsonProperty.Value.GetString();
                    break;
                case "disabledTextStyle":
                    buttonStyle.DisabledTextStyleId = jsonProperty.Value.GetString();
                    break;
            }
        }

        result.SetStyle(buttonStyle);

        return result;
    }

    private WidgetBase LoadWidgetScrollBar(JsonElement jsonObj)
    {
        var result = new WidgetScrollBar();

        LoadWidgetBase(jsonObj, result);

        return result;
    }

    public WidgetBase LoadWidgetTree(JsonElement jsonObj)
    {
        var type = jsonObj.GetProperty("type").GetString();

        // Is there a factory for the type?
        WidgetBase widget;
        switch (type)
        {
            case "container":
                widget = LoadWidgetContainer(jsonObj);
                break;
            case "button":
                widget = LoadWidgetButton(jsonObj);
                break;
            case "scrollBar":
                widget = LoadWidgetScrollBar(jsonObj);
                break;
            case "scrollView":
                widget = LoadWidgetScrollView(jsonObj);
                break;
            case "scrollBox":
                widget = LoadWidgetScrollBox(jsonObj);
                break;
            case "tabBar":
                widget = LoadWidgetTabBar(jsonObj);
                break;
            case "custom":
                if (CustomFactory == null)
                {
                    throw new InvalidOperationException(
                        "Cannot use custom widgets if no custom element factory is set!");
                }

                widget = CustomFactory(jsonObj.GetStringProp("customType"), jsonObj);
                LoadWidgetBase(jsonObj, widget);
                break;
            default:
                throw new Exception($"Cannot process unknown widget type: '{type}'");
        }

        widget.SetSourceURI(_path);

        // If the widget had an ID, put it into the registry
        if (jsonObj.TryGetProperty("id", out var idNode))
        {
            var id = idNode.GetString();
            if (Registry.ContainsKey(id))
            {
                throw new Exception($"Duplicate widget id: {id}");
            }

            Registry[id] = widget;
            widget.SetId(id);
        }

        return widget;
    }
}

/// <summary>
/// Contains a definition for a grabbag of widgets.
/// </summary>
public class WidgetDoc
{
    private readonly string _path;
    private readonly WidgetBase _rootWidget;
    private readonly Dictionary<string, WidgetBase> _widgetsById;
    private readonly Dictionary<string, WidgetContent> _contentById;

    private WidgetDoc(string path,
        WidgetBase root,
        Dictionary<string, WidgetBase> registry,
        Dictionary<string, WidgetContent> contentRegistry)
    {
        _path = path;
        _rootWidget = root;
        _widgetsById = registry;
        _contentById = contentRegistry;
    }

    public static WidgetDoc Load(string path, CustomWidgetFactory? customFactory = null)
    {
        var json = Tig.FS.ReadBinaryFile(path);
        using var root = JsonDocument.Parse(json);

        try
        {
            var loader = new WidgetDocLoader(path);
            if (customFactory != null)
            {
                loader.CustomFactory = customFactory;
            }

            var rootWidget = loader.LoadWidgetTree(root.RootElement);

            return new WidgetDoc(path, rootWidget, loader.Registry, loader.ContentRegistry);
        }
        catch (Exception e)
        {
            throw new Exception($"Unable to load widget doc '{path}'.", e);
        }
    }

    /**
         * Returns the root widget defined in the widget doc. The caller takes ownership of the widget.
         * This function can only be called once per widget doc instance!
         */
    public WidgetBase TakeRootWidget()
    {
        Trace.Assert(_rootWidget != null);
        return _rootWidget;
    }

    /**
          * Returns the root widget defined in the widget doc, assuming it is a container widget.
         * If the root widget is NOT a container, this method will throw an exception.
         * The caller takes ownership of the widget.
         * This function can only be called once per widget doc instance!
         */
    public WidgetContainer GetRootContainer()
    {
        Trace.Assert(_rootWidget != null);
        if (!_rootWidget.IsContainer())
        {
            throw new Exception($"Expected root widget in '{_path}' to be a container.");
        }

        return (WidgetContainer) _rootWidget;
    }

    public bool HasWidget(string id) => _widgetsById.ContainsKey(id);

    public WidgetBase GetWidget(string id)
    {
        if (!_widgetsById.TryGetValue(id, out var widget))
        {
            throw new Exception($"Couldn't find required widget id '{id}' in widget doc '{_path}'");
        }

        return widget;
    }

    public WidgetContainer GetContainer(string id)
    {
        var widget = GetWidget(id);
        if (!widget.IsContainer())
        {
            throw new Exception($"Expected widget with id '{id}' in doc '{_path}' to be a container!");
        }

        return (WidgetContainer) widget;
    }

    public WidgetButton GetButton(string id)
    {
        var widget = GetWidget(id);
        if (!widget.IsButton())
        {
            throw new Exception($"Expected widget with id '{id}' in doc '{_path}' to be a button!");
        }

        return (WidgetButton) widget;
    }

    public WidgetTabBar GetTabBar(string id)
    {
        var widget = GetWidget(id);
        if (!(widget is WidgetTabBar tabBar))
        {
            throw new Exception($"Expected widget with id '{id}' in doc '{_path}' to be a tab bar!");
        }

        return tabBar;
    }

    public WidgetScrollView GetScrollView(string id)
    {
        var widget = GetWidget(id);
        if (!widget.IsScrollView())
        {
            throw new Exception($"Expected widget with id '{id}' in doc '{_path}' to be a scroll view!");
        }

        return (WidgetScrollView) widget;
    }

    public ScrollBox GetScrollBox(string id)
    {
        var widget = GetWidget(id);
        if (widget is not ScrollBox scrollBox)
        {
            throw new Exception($"Expected widget with id '{id}' in doc '{_path}' to be a scroll box!");
        }

        return scrollBox;
    }

    public WidgetScrollBar GetScrollBar(string id)
    {
        var widget = GetWidget(id);
        return (WidgetScrollBar) widget;
    }

    private T GetContent<T>(string id) where T : WidgetContent
    {
        if (!_contentById.TryGetValue(id, out var content))
        {
            throw new Exception($"Couldn't find widget content with id '{id}'");
        }

        if (!(content is T t))
        {
            throw new Exception($"Expected widget content with id '{id}' to be of type {typeof(T)}, but " +
                                $"was {content.GetType()}");
        }

        return t;
    }

    public WidgetContent GetContent(string id) => GetContent<WidgetContent>(id);

    public WidgetText GetTextContent(string id) => GetContent<WidgetText>(id);

    public WidgetImage GetImageContent(string id) => GetContent<WidgetImage>(id);
}