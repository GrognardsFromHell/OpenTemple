import QtQuick 2.7

Rectangle {
    color: "#0f0f0f"
    Image {
        anchors.fill: parent
        source: "gameviewbg.png"
        fillMode: Image.PreserveAspectCrop
    }

    Text {
        anchors.centerIn: parent
        color: "white"
        font.pixelSize: 100
        text: "GAME VIEW"
    }
}
