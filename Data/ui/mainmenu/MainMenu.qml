import QtQuick 2.7
import QtGraphicalEffects 1.14
//import OpenTemple 1.0
import QtQuick.Controls 2.14
import QtQuick.Layouts 1.3

Item {
    id: root
    anchors.fill: parent

    property int page: 0
    signal action(string name)

    /*GameView {
        id: gameView
        anchors.fill: parent
    }*/

    Image {
        anchors.top: parent.top
        anchors.topMargin: 20
        anchors.horizontalCenter: parent.horizontalCenter

        source: 'logo.png'
    }

    ColumnLayout {
        anchors.horizontalCenter: parent.horizontalCenter
        anchors.bottom: parent.bottom
        anchors.bottomMargin: 20

        visible: page == 0
        MainMenuButton {
            text: qsTr("New Game", "#{main_menu:0}")
            onClicked: page = 1
        }
        MainMenuButton {
            text: qsTr("Load Game", "#{main_menu:1}")
            onClicked: root.action("load_game")
        }
        MainMenuButton {
            text: qsTr("Tutorial", "#{main_menu:2}")
            onClicked: root.action("tutorial")
        }
        MainMenuButton {
            text: qsTr("Options", "#{main_menu:3}")
            onClicked: page = 4
        }
        MainMenuButton {
            text: qsTr("Quit Game", "#{main_menu:4}")
            onClicked: root.action("quit_game")
        }
    }

    // Difficulty selection
    ColumnLayout {
        anchors.horizontalCenter: parent.horizontalCenter
        anchors.bottom: parent.bottom
        anchors.bottomMargin: 20
        visible: page == 1

        MainMenuButton {
            text: qsTr("Normal", "#{main_menu:10}")
            onClicked: root.action("start_game_normal")
        }
        MainMenuButton {
            text: qsTr("Ironman", "#{main_menu:11}")
            onClicked: root.action("start_game_ironman")
        }
        MainMenuButton {
            text: qsTr("Exit", "#{main_menu:12}")
            onClicked: page = 0
        }
    }

    // Ingame menu, normal difficulty
    ColumnLayout {
        anchors.horizontalCenter: parent.horizontalCenter
        anchors.bottom: parent.bottom
        anchors.bottomMargin: 20
        visible: page == 2

        MainMenuButton {
            text: qsTr("#{main_menu:20}")
        }
        MainMenuButton {
            text: qsTr("#{main_menu:21}")
        }
        MainMenuButton {
            text: qsTr("#{main_menu:22}")
        }
        MainMenuButton {
            text: qsTr("#{main_menu:23}")
        }
    }

    // Ingame menu, ironman difficulty
    ColumnLayout {
        anchors.horizontalCenter: parent.horizontalCenter
        anchors.bottom: parent.bottom
        anchors.bottomMargin: 20
        visible: page == 3

        MainMenuButton {
            text: qsTr("#{main_menu:30}")
        }
        MainMenuButton {
            text: qsTr("#{main_menu:31}")
        }
    }

    // Options
    ColumnLayout {
        anchors.horizontalCenter: parent.horizontalCenter
        anchors.bottom: parent.bottom
        anchors.bottomMargin: 20
        visible: page == 4

        MainMenuButton {
            text: qsTr("#{main_menu:40}")
        }
        MainMenuButton {
            text: qsTr("#{main_menu:41}")
        }
        MainMenuButton {
            text: qsTr("#{main_menu:42}")
        }
        MainMenuButton {
            text: qsTr("#{main_menu:43}")
            onClicked: page = 0
        }
    }
}

/*##^##
Designer {
    D{i:0;autoSize:true;height:480;width:640}
}
##^##*/
