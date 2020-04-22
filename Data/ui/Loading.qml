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

    property real progress : 0

    property string message

    property alias logo : logo

    Image {
        id: logo
        source: 'legal.png'
        anchors.fill: parent
        fillMode: Image.PreserveAspectFit
    }
    Text  {
        anchors.bottom: parent.bottom
        anchors.horizontalCenter: parent.horizontalCenter
        color: "white"
        text: root.message + ", " + root.progress
    }
}
