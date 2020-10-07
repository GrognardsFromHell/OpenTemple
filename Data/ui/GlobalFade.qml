import QtQuick 2.0
import OpenTemple.GlobalFade 1.0

Rectangle {
    z: 100000
    color: GlobalFade.color
    visible: GlobalFade.isOverlayEnabled
}
