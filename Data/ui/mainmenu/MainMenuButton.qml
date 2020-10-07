import QtQuick 2.14
import QtQuick.Controls 2.14
import QtQuick.Layouts 1.3
import QtGraphicalEffects 1.0
import Common 1.0

Button {
    id: control
    Layout.alignment: Qt.AlignHCenter
    background: null

    font.family: Fonts.mainMenuButton
    font.pixelSize: 48
    hoverEnabled: true

    property color topColor: control.hovered ? "#01ffff" : "#0064a4"
    property color bottomColor: control.hovered ? "#01d0ff" : "#01415d"

    Behavior on topColor {
        ColorAnimation {
            duration: 100
            easing.type: Easing.OutQuad
        }
    }
    Behavior on bottomColor {
        ColorAnimation {
            duration: 100
            easing.type: Easing.OutQuad
        }
    }

    contentItem: Text {
        text: control.text
        font: control.font
        opacity: enabled ? 1.0 : 0.3
        horizontalAlignment: Text.AlignHCenter
        verticalAlignment: Text.AlignVCenter
        elide: Text.ElideRight
        visible: false
    }

    LinearGradient {
        id: effect1
        anchors.fill: contentItem
        source: contentItem
        visible: false
        gradient: Gradient {
            GradientStop {
                id: gradientColorTop
                position: 0.1
                color: control.topColor
            }
            GradientStop {
                id: gradientColorBottom
                position: 1.0
                color: control.bottomColor
            }
        }
        layer.enabled: true
        layer.effect: DropShadow {
            horizontalOffset: 3
            verticalOffset: 3
            radius: 8.0
            samples: 17
            color: "#000000"
        }
    }
    InnerShadow {
        anchors.fill: contentItem
        source: effect1

        horizontalOffset: 0
        verticalOffset: 0
        radius: 4
        samples: 4
        color: "#90ffffff"
    }
}
