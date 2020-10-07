import QtQuick 2.15

Item {
    id: root

    property Item currentItem

    function showItem(item: Item) {
        if (currentItem) {
            currentItem.parent = null;
        }

        item.parent = root;
        item.anchors.fill = root;
        currentItem = item;
    }

    function clear() {
        if (currentItem) {
            currentItem.parent = null;
            currentItem = null;
        }
    }

}
