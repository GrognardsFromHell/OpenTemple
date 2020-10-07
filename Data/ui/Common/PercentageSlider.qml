import QtQuick 2.0
import QtQuick.Controls 2.12
import Common 1.0
import QtQuick.Layouts 1.15

RowLayout {

    property alias from: slider.from
    property alias to: slider.to
    property alias stepSize: slider.stepSize
    property alias value: slider.value

    Label {
        text: "0%"
        font: Fonts.buttonFont
        color: "white"
        Layout.rightMargin: 5
    }

    Slider {
        id: slider
        stepSize: 1
        Layout.fillWidth: true
    }

    Label {
        text: "100%"
        font: Fonts.buttonFont
        color: "white"
        Layout.leftMargin: 5
    }

    Label {
        text: slider.value + "%"
        font: Fonts.buttonFont
        color: "white"
        Layout.leftMargin: 5
    }
}
