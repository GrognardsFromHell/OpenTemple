import QtQuick 2.0
import OpenTemple.Options 1.0

Rectangle {
    color: "#333333"
    Column {
        anchors.centerIn: parent
        width: 800
        OptionsRow {
            anchors.left: parent.left
            anchors.right: parent.right
            option: CheckboxOption {
                label: "Hello World"
            }
        }

        OptionsRow {
            anchors.left: parent.left
            anchors.right: parent.right
            option: SliderOption {
                label: "Hello World"
                minValue: 1
                maxValue: 5
            }
        }

        OptionsRow {
            anchors.left: parent.left
            anchors.right: parent.right
            option: SliderOption {
                label: "Percentage"
                isPercentage: true
                minValue: 0
                maxValue: 100
            }
        }
    }
}
