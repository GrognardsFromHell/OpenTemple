import QtQuick 2.0
import Common 1.0
import QtQuick.Controls 2.12

Rectangle {
    color: "#999999"

    ScrollView {
        width: 800
        height: 800
        anchors.centerIn: parent
        clip: true
        ScrollBar.horizontal.policy: ScrollBar.AlwaysOn
        ScrollBar.vertical.policy: ScrollBar.AlwaysOn

        Rectangle {
            implicitHeight: 3000
            implicitWidth: 3000
        }
    }
}
