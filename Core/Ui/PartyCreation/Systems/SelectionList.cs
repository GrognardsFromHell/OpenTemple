using System;
using System.Collections.Generic;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui.PartyCreation.Systems
{
    public class SelectionList<T>
    {
        public WidgetContainer Container { get; }

        private readonly List<(string, T)> _items = new List<(string, T)>();

        private readonly int _itemsPerPage;

        private readonly WidgetButton _nextPageButton;

        private readonly WidgetButton _prevPageButton;

        private readonly List<WidgetButton> _selectionButtons;

        private int _pages;

        private int _currentPage;

        private T _selectedItem;

        public T SelectedItem
        {
            get => _selectedItem;
            set
            {
                _selectedItem = value;
                UpdateOptions();
            }
        }

        public event Action OnSelectedItemChanged;

        public event Action<T> OnItemHovered;

        private readonly IEqualityComparer<T> _equality;

        public SelectionList() : this(EqualityComparer<T>.Default)
        {
        }

        public SelectionList(IEqualityComparer<T> equality)
        {
            _equality = equality;

            var doc = WidgetDoc.Load("ui/pc_creation/selection_list_ui.json");
            Container = doc.TakeRootContainer();

            _nextPageButton = doc.GetButton("nextPage");
            _nextPageButton.SetClickHandler(() =>
            {
                _currentPage++;
                UpdateOptions();
            });
            _prevPageButton = doc.GetButton("prevPage");
            _prevPageButton.SetClickHandler(() =>
            {
                _currentPage--;
                UpdateOptions();
            });

            _selectionButtons = new List<WidgetButton>();
            for (var i = 0; i < 50; i++)
            {
                var index = i; // Copy to prevent modified closure issues
                var id = "item" + i;
                if (!doc.HasWidget(id))
                {
                    break;
                }

                var button = doc.GetButton(id);
                button.SetClickHandler(() =>
                {
                    var value = GetValueForIndexOnPage(index);
                    if (!_equality.Equals(_selectedItem, value))
                    {
                        _selectedItem = value;
                        OnSelectedItemChanged?.Invoke();
                        UpdateOptions();
                    }
                });
                // Allows the caller to display tooltips in the help-box if items are hovered
                button.OnMouseEnter += msg => OnItemHovered?.Invoke(GetValueForIndexOnPage(index));
                button.OnMouseExit += msg => OnItemHovered?.Invoke(default);
                _selectionButtons.Add(button);
            }

            _itemsPerPage = _selectionButtons.Count;
        }

        private T GetValueForIndexOnPage(int index) => _items[PageStart + index].Item2;

        public void AddItem(string label, T value)
        {
            _items.Add((label, value));
            UpdateOptions();
        }

        private int PageStart => _currentPage * _itemsPerPage;
        private int CurrentPageSize => Math.Min(_itemsPerPage, _items.Count - PageStart);

        private void UpdateOptions()
        {
            // Round up
            _pages = (_items.Count + _itemsPerPage - 1) / _itemsPerPage;
            _currentPage = Math.Clamp(_currentPage, 0, _pages - 1);

            _nextPageButton.Visible = _pages > 1;
            _prevPageButton.Visible = _pages > 1;
            _nextPageButton.SetDisabled(_currentPage + 1 == _pages);
            _prevPageButton.SetDisabled(_currentPage == 0);

            // Update the selection buttons
            var itemsOnPage = CurrentPageSize;
            for (var i = 0; i < _selectionButtons.Count; i++)
            {
                _selectionButtons[i].Visible = i < itemsOnPage;
                if (i < itemsOnPage)
                {
                    var (label, value) = _items[PageStart + i];
                    _selectionButtons[i].SetText(label);
                    _selectionButtons[i].SetActive(_equality.Equals(value, _selectedItem));
                }
            }
        }

        public void Reset()
        {
            _selectedItem = default;
            _currentPage = 0;
            UpdateOptions();
        }
    }
}