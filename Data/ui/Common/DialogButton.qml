import QtQuick 2.0
import QtQuick.Controls 2.12
import Common 1.0

Button {
    id: control

    text: "Hello World"

    contentItem.layer.enabled: true
    contentItem.layer.effect: HardShadow {}

    background: BorderImage {
        source: "button_frame.sci"
        Image {
            source: control.DialogButtonBox.buttonRole
                    === DialogButtonBox.AcceptRole ? "Accept_Normal.png" : "Decline_Normal.png"
            anchors.fill: parent
            anchors.leftMargin: 10
            anchors.topMargin: 10
            anchors.bottomMargin: 10
            anchors.rightMargin: 10
        }
        Image {
            source: control.DialogButtonBox.buttonRole
                    === DialogButtonBox.AcceptRole ? "Accept_Hover.png" : "Decline_Hover.png"
            anchors.fill: parent
            anchors.leftMargin: 10
            anchors.topMargin: 10
            anchors.bottomMargin: 10
            anchors.rightMargin: 10
            visible: control.hovered
        }
        Image {
            source: control.DialogButtonBox.buttonRole
                    === DialogButtonBox.AcceptRole ? "Accept_Pressed.png" : "Decline_Pressed.png"
            anchors.fill: parent
            anchors.leftMargin: 10
            anchors.topMargin: 10
            anchors.bottomMargin: 10
            anchors.rightMargin: 10
            visible: control.pressed
        }
    }
}
