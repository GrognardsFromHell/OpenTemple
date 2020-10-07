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

    property var direction: SliderButton.Direction.Up

    padding: 0
    spacing: 0
    bottomInset: 0
    leftInset: 0
    topInset: 0
    rightInset: 0

    // Cannot focus on slider buttons
    focusPolicy: Qt.NoFocus

    hoverEnabled: true

    property string normalImage: {
        switch (direction) {
        default:

        case SliderButton.Direction.Up:
            return "slider_ver_plus_normal.png"
        case SliderButton.Direction.Right:
            return "slider_hor_plus_normal.png"
        case SliderButton.Direction.Down:
            return "slider_ver_minus_normal.png"
        case SliderButton.Direction.Left:
            return "slider_hor_minus_normal.png"
        }
    }

    property string hoverImage: {
        switch (direction) {
        default:

        case SliderButton.Direction.Up:
            return "slider_ver_plus_hover.png"
        case SliderButton.Direction.Right:
            return "slider_hor_plus_hover.png"
        case SliderButton.Direction.Down:
            return "slider_ver_minus_hover.png"
        case SliderButton.Direction.Left:
            return "slider_hor_minus_hover.png"
        }
    }

    property string pressedImage: {
        switch (direction) {
        default:

        case SliderButton.Direction.Up:
            return "slider_ver_plus_clicked.png"
        case SliderButton.Direction.Right:
            return "slider_hor_plus_clicked.png"
        case SliderButton.Direction.Down:
            return "slider_ver_minus_clicked.png"
        case SliderButton.Direction.Left:
            return "slider_hor_minus_clicked.png"
        }
    }

    background: null
    Image {
        id: image
        source: control.pressed ? pressedImage : (control.hovered ? hoverImage : normalImage)
    }
    contentItem: null
    autoRepeat: true
    implicitWidth: image.width
    implicitHeight: image.height
}
