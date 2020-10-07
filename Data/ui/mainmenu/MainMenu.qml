import QtQuick 2.7
import QtGraphicalEffects 1.14
import OpenTemple 1.0
import QtQuick.Controls 2.14
import QtQuick.Layouts 1.3
import "../options"

Item {
    id: root
    anchors.fill: parent

    enum Action {
        NewGameNormal,
        NewGameIronman,
        LoadGame,
        StartTutorial,
        Quit
    }

    property int page: 0
    signal action(int action)

    GameView {
        id: gameView
        anchors.fill: parent
    }

    Image {
        anchors.top: parent.top
        anchors.topMargin: 20
        anchors.horizontalCenter: parent.horizontalCenter

        source: 'logo.png'
    }

    ButtonColumn {
        visible: page == 0
        MainMenuButton {
            text: qsTr("New Game", "#{main_menu:0}")
            onClicked: page = 1
        }
        MainMenuButton {
            text: qsTr("Load Game", "#{main_menu:1}")
            onClicked: root.action(MainMenu.Action.LoadGame)
        }
        MainMenuButton {
            text: qsTr("Tutorial", "#{main_menu:2}")
            onClicked: root.action(MainMenu.Action.StartTutorial)
        }
        MainMenuButton {
            text: qsTr("Options", "#{main_menu:3}")
            onClicked: page = 4
        }
        MainMenuButton {
            text: qsTr("Quit Game", "#{main_menu:4}")
            onClicked: root.action(MainMenu.Action.Quit)
        }
    }

    // Difficulty selection
    ButtonColumn {
        visible: page == 1

        MainMenuButton {
            text: qsTr("Normal", "#{main_menu:10}")
            onClicked: root.action(MainMenu.Action.NewGameNormal)
        }
        MainMenuButton {
            text: qsTr("Ironman", "#{main_menu:11}")
            onClicked: root.action(MainMenu.Action.NewGameIronman)
        }
        MainMenuButton {
            text: qsTr("Exit", "#{main_menu:12}")
            onClicked: page = 0
        }
    }

    // Ingame menu, normal difficulty
    ButtonColumn {
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
    ButtonColumn {
        visible: page == 3

        MainMenuButton {
            text: qsTr("#{main_menu:30}")
        }
        MainMenuButton {
            text: qsTr("#{main_menu:31}")
        }
    }

    // Options
    ButtonColumn {
        visible: page == 4

        MainMenuButton {
            text: qsTr("#{main_menu:40}")
            onClicked: optionsDialog.open()
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

    OptionsDialog {
        id: optionsDialog
    }
}
