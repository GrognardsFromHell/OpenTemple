import QtQuick 2.0
import OpenTemple 1.0

Item {
    id: root

    signal ended()

    Rectangle {
        anchors.fill: root
        color: 'black'
    }

    BinkVideoItem {
        id: videoItem
        onEnded: root.ended()
        anchors.fill: root
        focus: true
        Keys.onPressed: {
            if (event.key === Qt.Key_Space) {
                console.log('Skipping movie');
                videoItem.stop();
                event.accepted = true;
            }
        }
    }

    function open(path: string) {
        videoItem.open(path);
    }

}
