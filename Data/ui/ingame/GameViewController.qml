import QtQuick 2.15
import OpenTemple 1.0
import OpenTemple.InGameUi 1.0
import OpenTemple.GameSystems 1.0

/*
 * Provides mouse control for a gameview.
 */
MouseArea {
    id: root
    anchors.fill: gameViewItem
    acceptedButtons: Qt.LeftButton | Qt.MiddleButton | Qt.RightButton

    property GameView gameViewItem;

    function getGameView() {
        return gameViewItem.gameViewHandle;
    }

    property int scrollMarginH : 7;
    property int scrollMarginV : 7;

    hoverEnabled: true

    property vector2d mouseDownPos
    property bool rectangleSelect : false
    // The current rectangle in screen coords for rectangleSelect
    property rect rectangleSelectRect

    property var hoveredObject

    function updateRectangleSelectRect() {
        const x = Math.min(root.mouseDownPos.x, root.mouseX);
        const y = Math.min(root.mouseDownPos.y, root.mouseY);
        const width = Math.max(root.mouseDownPos.x, root.mouseX) - x;
        const height = Math.max(root.mouseDownPos.y, root.mouseY) - y;
        rectangleSelectRect = Qt.rect(x, y, width, height);
    }

    onHoveredObjectChanged: {
        console.log("Hover: ", hoveredObject);
        Party.forceHovered = null;
        InGameSelect.focus = null;

        if (rectangleSelect) {
            return; // Don't show extra focus while performing a rectangle select
        }

        if (InGameSelect.shouldShowFocusRing(hoveredObject)) {
            InGameSelect.focus = hoveredObject;
        }

        if (Party.isInParty(hoveredObject))
        {
            Party.forceHovered = hoveredObject;
        }
    }

    onPressed: {
        // Remember where the mouse was held
        if (mouse.buttons == Qt.LeftButton) {
            mouseDownPos = Qt.vector2d(mouse.x, mouse.y);
        } else if (mouse.buttons == Qt.MiddleButton) {
            mouseDownPos = Qt.vector2d(mouse.x, mouse.y);
        }
    }

    onPositionChanged: {
        if (!mouse.buttons) {
            const objectUnderMouse = InGameUi.pickObject(root.getGameView(), mouse.x, mouse.y);
            if (objectUnderMouse !== hoveredObject) {
                console.log("Hover: ", hoveredObject, objectUnderMouse);
                hoveredObject = objectUnderMouse;
            }

            return;
        }

        if (mouse.buttons == Qt.MiddleButton) {
            const mouseLoc = Qt.vector2d(mouse.x, mouse.y);
            const delta = mouseLoc.minus(root.mouseDownPos);
            if (delta.length() >= 1) {
                InGameUi.scrollBy(delta);
                root.mouseDownPos = mouseLoc;
            }
        }

        // Determine if rectangle selection should be started
        if (mouse.buttons == Qt.LeftButton) {
            if (!rectangleSelect) {
                const currentPos = Qt.vector2d(mouse.x, mouse.y);
                if ((currentPos.minus(mouseDownPos)).length() > 4) {
                    rectangleSelect = true;
                }
            }
            if (rectangleSelect) {
                updateRectangleSelectRect();

                // TODO: This highlights objects within the rectangle
                const partyMembersInRect = InGameUi.findPartyMembersInRect(rectangleSelectRect.x, rectangleSelectRect.y,
                                                rectangleSelectRect.width, rectangleSelectRect.height);
                InGameUi.setSelectionHighlights(partyMembersInRect);
            }
        }
    }

    onDoubleClicked: {
        Scroll.centerOnScreenSmooth(mouse.x, mouse.y);
    }

    onReleased: {
        InGameSelect.focusClear();

        if (rectangleSelect) {
            rectangleSelect = false;
            updateRectangleSelectRect();

            if (!(mouse.modifiers & Qt.ShiftModifier))
            {
                Party.clearSelection();
            }

            InGameSelect.selectInRectangle(rectangleSelectRect);
        } else {
            let obj = InGameUi.pickObject(root.getGameView(), mouse.x, mouse.y);
            if (obj) {
                // Holding shift while clicking allows adding to the selection
                if (!(mouse.modifiers & Qt.ShiftModifier))
                {
                    Party.clearSelection();
                }
                else
                {
                    if (Party.isSelected(obj)) {
                        Party.removeFromSelection(obj);
                        return;
                    }
                }

                Party.addToSelection(obj);
            } else {

                if (!(mouse.modifiers & Qt.ShiftModifier))
                {
                    InGameUi.moveSelectedPartyToPosition(root.getGameView(), mouse.x, mouse.y);
                }
            }
        }
    }

    /* Used to show the selection rectangle when doing an area selection */
    Rectangle {
        visible: root.rectangleSelect
        x: root.rectangleSelectRect.x
        y: root.rectangleSelectRect.y
        width: root.rectangleSelectRect.width
        height: root.rectangleSelectRect.height
        color: "transparent"
        border.color: "teal"
        border.width: 2
    }

    // Timer for refreshing the hovered object,
    // this is needed because objects in the scene can move or animate
    // without the mouse being moved. This should still update the
    // tooltip and other highlighting to what is currently under the mouse.
    Timer {
        running: root.containsMouse
        interval: 10
        repeat: true

        onTriggered: {
            const {mouseX, mouseY} = root;
            const objUnderMouse = InGameUi.pickObject(root.getGameView(), mouseX, mouseY);
            if (objUnderMouse !== root.hoveredObject) {
                root.hoveredObject = objUnderMouse;
            }
        }
    }

    // Timer for edge scrolling
    Timer {
        running: root.containsMouse && !root.pressed
        interval: 10
        repeat: true

        onTriggered: {
            // Check if x,y are in the border scrolling zone

            // TODO This should be the size of the game view
            const {mouseX, mouseY, width, height, scrollMarginH, scrollMarginV} = root;

            let scrollX = 0;
            if (mouseX <= scrollMarginH) // scroll left
            {
                scrollX = -1;
            }
            else if (mouseX >= width - scrollMarginH) // scroll right
            {
                scrollX = 1;
            }

            let scrollY = 0;
            if (mouseY <= scrollMarginV) // scroll up
                scrollY = -1;
            else if (mouseY >= height - scrollMarginV) // scroll down
                scrollY = 1;

            if (scrollX || scrollY)
            {
                InGameUi.setScrollDirection(root.getGameView(), scrollX, scrollY);
            }
        }
    }

}
