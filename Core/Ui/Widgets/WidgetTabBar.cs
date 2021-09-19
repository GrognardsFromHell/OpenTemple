using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;
using OpenTemple.Core.GFX;
using OpenTemple.Core.TigSubsystems;

namespace OpenTemple.Core.Ui.Widgets
{
    public enum WidgetTabStyle
    {
        Large,
        Small
    }

    public class WidgetTabBar : WidgetContainer
    {
        private readonly List<string> _labels = new List<string>();

        private readonly List<WidgetTabButton> _tabs = new List<WidgetTabButton>();

        private WidgetTabStyle _style = WidgetTabStyle.Small;

        private int _spacing;

        public int Spacing
        {
            get => _spacing;
            set
            {
                if (_spacing != value)
                {
                    _spacing = value;
                    RecreateTabs();
                }
            }
        }

        public WidgetTabStyle Style
        {
            get => _style;
            set
            {
                if (value != _style)
                {
                    _style = value;
                    RecreateTabs();
                }
            }
        }

        public int ActiveTabIndex
        {
            get
            {
                for (var i = 0; i < _tabs.Count; i++)
                {
                    if (_tabs[i].Active)
                    {
                        return i;
                    }
                }

                return -1;
            }
            set
            {
                var notifyChange = false;
                for (var i = 0; i < _tabs.Count; i++)
                {
                    // Notify listeners if an active tab was deselected or the active tab changed
                    if (i == value && !_tabs[i].Active || _tabs[i].Active && i != value)
                    {
                        notifyChange = true;
                    }

                    _tabs[i].Active = i == value;
                }

                if (notifyChange)
                {
                    OnActiveTabIndexChanged?.Invoke(value);
                }
            }
        }

        public event Action<int> OnActiveTabIndexChanged;

        public WidgetTabBar(Size size, [CallerFilePath]
            string filePath = null, [CallerLineNumber]
            int lineNumber = -1) : base(size, filePath, lineNumber)
        {
        }

        public WidgetTabBar(Rectangle rectangle, [CallerFilePath]
            string filePath = null, [CallerLineNumber]
            int lineNumber = -1) : base(rectangle, filePath, lineNumber)
        {
        }

        public WidgetTabBar(int width, int height, [CallerFilePath]
            string filePath = null, [CallerLineNumber]
            int lineNumber = -1) : base(width, height, filePath, lineNumber)
        {
        }

        public WidgetTabBar(int x, int y, int width, int height, [CallerFilePath]
            string filePath = null, [CallerLineNumber]
            int lineNumber = -1) : base(x, y, width, height, filePath, lineNumber)
        {
        }

        public void SetTabs(IEnumerable<string> labels)
        {
            _labels.Clear();
            _labels.AddRange(labels);

            RecreateTabs();

            ActiveTabIndex = -1;
        }

        private void RecreateTabs()
        {
            foreach (var button in _tabs)
            {
                Remove(button);
            }

            _tabs.Clear();

            var x = 0;
            foreach (var label in _labels)
            {
                var button = new WidgetTabButton(label, _style);
                _tabs.Add(button);
                button.X = x;
                Add(button);

                var index = _tabs.Count - 1;
                button.SetClickHandler(() => ActiveTabIndex = index);

                x += button.Width + _spacing;
            }
        }
    }

    public class WidgetTabButton : WidgetButtonBase
    {
        private readonly WidgetImage _normalLeft;
        private readonly WidgetImage _normalBg;
        private readonly WidgetImage _normalRight;

        private readonly WidgetImage _selectedLeft;
        private readonly WidgetImage _selectedBg;
        private readonly WidgetImage _selectedRight;

        private readonly WidgetText _label;

        private readonly WidgetTabStyle _style;

        public bool Active { get; set; }

        private const string LargeLabelStyle = "tab-large";
        private const string SmallLabelStyle = "tab-small";

        public WidgetTabButton(string label, WidgetTabStyle style)
        {
            _style = style;
            sndClick = WidgetButtonStyle.DefaultUpSound;
            sndDown = WidgetButtonStyle.DefaultDownSound;

            if (style == WidgetTabStyle.Large)
            {
                _normalLeft = new WidgetImage("art/interface/logbook_ui/Tab-Left.tga");
                _normalBg = new WidgetImage("art/interface/logbook_ui/Tab-Tiled.tga");
                _normalRight = new WidgetImage("art/interface/logbook_ui/Tab-Right.tga");
                _selectedLeft = new WidgetImage("art/interface/logbook_ui/Tab-Left-Selected.tga");
                _selectedBg = new WidgetImage("art/interface/logbook_ui/Tab-Tiled-Selected.tga");
                _selectedRight = new WidgetImage("art/interface/logbook_ui/Tab-Right-Selected.tga");

                _label = new WidgetText(label, LargeLabelStyle);
            }
            else
            {
                _normalLeft = new WidgetImage("ui/tabs/tab-small-left.tga");
                _normalBg = new WidgetImage("ui/tabs/tab-small-middle.tga");
                _normalRight = new WidgetImage("ui/tabs/tab-small-right.tga");
                _selectedLeft = new WidgetImage("ui/tabs/tab-small-active-left.tga");
                _selectedBg = new WidgetImage("ui/tabs/tab-small-active-middle.tga");
                _selectedRight = new WidgetImage("ui/tabs/tab-small-active-right.tga");

                _label = new WidgetText(label, SmallLabelStyle);
            }

            // Layout the content items
            LayoutContent(_selectedLeft, _selectedBg, _selectedRight, _label);
            var size = LayoutContent(_normalLeft, _normalBg, _normalRight, _label);
            SetSize(size);

            UpdateSelectedState();
        }

        public override void Render()
        {
            UpdateSelectedState();
            base.Render();
        }

        private Size LayoutContent(WidgetContent left, WidgetContent bg, WidgetContent right,
            WidgetContent label)
        {
            left.FixedSize = left.GetPreferredSize();
            bg.FixedSize = new Size(label.GetPreferredSize().Width, bg.GetPreferredSize().Height);
            bg.X = left.FixedSize.Width;
            // The label offset was hardcoded before too
            if (_style == WidgetTabStyle.Large)
            {
                label.X = bg.X - 5;
                label.Y = 5;
            }
            else
            {
                label.X = bg.X;
                label.Y = 1;
            }

            right.FixedSize = right.GetPreferredSize();
            right.X = bg.X + bg.FixedSize.Width;

            return new Size(
                right.X + right.FixedSize.Width,
                right.FixedSize.Height
            );
        }

        private void UpdateSelectedState()
        {
            _content.Clear();
            if (Active)
            {
                AddContent(_selectedLeft);
                AddContent(_selectedBg);
                AddContent(_selectedRight);
            }
            else
            {
                AddContent(_normalLeft);
                AddContent(_normalBg);
                AddContent(_normalRight);
            }

            AddContent(_label);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _normalLeft.Dispose();
                _normalBg.Dispose();
                _normalRight.Dispose();

                _selectedLeft.Dispose();
                _selectedBg.Dispose();
                _selectedRight.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}