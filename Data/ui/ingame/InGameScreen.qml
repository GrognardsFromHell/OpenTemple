import QtQuick 2.15
import OpenTemple 1.0

Item {
    id: root
    GameView {
        id: gameViewItem
        anchors.fill: parent
    }
    GameViewController {
        gameViewItem: gameViewItem
    }
}
