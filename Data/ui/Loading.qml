import QtQuick 2.7
import "./mainmenu"

Rectangle {
    id: root
    color: "black"

    enum TextType {
        Normal,
        Heading
    }

    property MainMenu mm

    property int textType : Loading.TextType.Normal

    property real progress : 0.5

    property string message

    property alias logo : logo

    Image {
        id: logo
        source: 'legal.png'
        anchors.fill: parent
        fillMode: Image.PreserveAspectFit
    }
    Text {
        y: 416

        anchors.bottom: rectangle.top
        anchors.horizontalCenter: parent.horizontalCenter

        text: root.message + ", " + root.progress
        anchors.horizontalCenterOffset: 1
        anchors.bottomMargin: 6
        color: "white"
    }

    Rectangle {
        id: rectangle
        y: 447
        height: 25
        color: "#535353"
        border.color: "#bfbfbf"
        anchors.right: parent.right
        anchors.rightMargin: 8
        anchors.left: parent.left
        anchors.leftMargin: 8
        anchors.bottom: parent.bottom
        anchors.bottomMargin: 8

        Rectangle {
            id: fill
            anchors.left: parent.left
            anchors.top: parent.top
            anchors.bottom: parent.bottom
            anchors.margins: 1
            width: progress * parent.width
            color: "#0d18b2"
        }
    }
}

/*##^##
Designer {
    D{i:0;autoSize:true;formeditorZoom:2;height:480;width:640}D{i:3;anchors_width:624;anchors_x:8}
}
##^##*/
