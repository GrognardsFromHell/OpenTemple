import QtQuick 2.7

Item {
    enum FillMode {
        Stretch = 1,
        PreserveAspectFit = 2,
        PreserveAspectCrop = 3
    }

    property var fillMode;

    signal ended();

    function open(path: string) {
    }

    function stop() {
    }

    implicitWidth: 800
    implicitHeight: 600
    Rectangle {
        color: "green"
        anchors.fill: parent
    }
}
