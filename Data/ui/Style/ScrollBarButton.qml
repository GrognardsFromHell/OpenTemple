import QtQuick 2.12
import QtQuick.Controls 2.12
import QtQuick.Controls.impl 2.12
import QtQuick.Templates 2.12 as T
import Common 1.0

T.Button {
    id: control

    enum Direction {
        Up,
        Right,
        Down,
        Left
    }

    property var direction: ScrollBarButton.Direction.Up

    padding: 0
    spacing: 0
    bottomInset: 0
    leftInset: 0
    topInset: 0
    rightInset: 0

    // Cannot focus on scrollbar buttons
    focusPolicy: Qt.NoFocus

    property string normalImage: {
        switch (direction) {
        default:

        case ScrollBarButton.Direction.Up:
            return "ScrollBar_Arrow_Top.png"
        case ScrollBarButton.Direction.Right:
            return "ScrollBar_Arrow_Right.png"
        case ScrollBarButton.Direction.Down:
            return "ScrollBar_Arrow_Bottom.png"
        case ScrollBarButton.Direction.Left:
            return "ScrollBar_Arrow_Left.png"
        }
    }

    property string pressedImage: {
        switch (direction) {
        default:

        case ScrollBarButton.Direction.Up:
            return "ScrollBar_Arrow_Top_Click.png"
        case ScrollBarButton.Direction.Right:
            return "ScrollBar_Arrow_Right_Click.png"
        case ScrollBarButton.Direction.Down:
            return "ScrollBar_Arrow_Bottom_Click.png"
        case ScrollBarButton.Direction.Left:
            return "ScrollBar_Arrow_Left_Click.png"
        }
    }

    background: null
    Image {
        id: image
        source: control.pressed ? pressedImage : normalImage
    }
    contentItem: null
    autoRepeat: true
    implicitWidth: image.width
    implicitHeight: image.height
}
