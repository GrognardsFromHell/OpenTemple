import QtQuick 2.7
import QtGraphicalEffects 1.14
import OpenTemple 1.0

Item {
    anchors.fill: parent

    GameView {
        id: gameView
        anchors.fill: parent
    }

    Image {
        source: 'MainMenu_Title.png'
        anchors.centerIn: parent
    }

}

/*##^##
Designer {
    D{i:0;autoSize:true;height:480;width:640}
}
##^##*/
