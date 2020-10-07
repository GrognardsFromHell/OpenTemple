import QtQuick 2.14
import QtQuick.Controls 2.14
import Common 1.0
import QtQuick.Layouts 1.15
import OpenTemple.Options 1.0

Item {
    id: root
    property Option option

    implicitHeight: row.implicitHeight + border.height

    property Item optionItem

    // Saves the user's current selection
    function commit() {
        optionItem.commit();
    }

    onOptionChanged: {
        // Decide which type of item to create
        if (optionItem) {
            optionItem.parent = null;
            optionItem = null;
        }
        if (option instanceof CheckboxOption) {
            optionItem = checkboxItem.createObject(row, {option});
        } else if (option instanceof SliderOption) {
            if (option.isPercentage) {
                optionItem = percentageSliderItem.createObject(row, {option});
            } else {
                optionItem = sliderItem.createObject(row, {option});
            }
        }
    }

    Component {
        id: checkboxItem
        CheckBox {
            property var option

            font: Fonts.buttonFont
            Layout.alignment: Qt.AlignVCenter
            Layout.margins: 10
            Component.onCompleted: {
                checkState = option.value ? Qt.Checked : Qt.Unchecked;
            }

            function commit() {
                let value = checkState == Qt.Checked;
                if (value !== option.value) {
                    option.value = value;
                }
            }
        }
    }
    Component {
        id: sliderItem
        Slider {
            property var option

            from: option.minValue
            to: option.maxValue
            stepSize: 1
            Layout.alignment: Qt.AlignVCenter
            Layout.preferredWidth: root.width * 0.4
            Layout.margins: 10

            Component.onCompleted: {
                value = option.value;
            }

            function commit() {
                if (value !== option.value) {
                    option.value = value;
                }
            }
        }
    }
    Component {
        id: percentageSliderItem
        PercentageSlider {
            property var option

            from: option.minValue
            to: option.maxValue
            stepSize: 1
            Layout.alignment: Qt.AlignVCenter
            Layout.preferredWidth: root.width * 0.4
            Layout.margins: 10

            Component.onCompleted: {
                value = option.value;
            }

            function commit() {
                if (value !== option.value) {
                    option.value = value;
                }
            }
        }
    }

    RowLayout {
        anchors.left: root.left
        anchors.right: root.right
        id: row
        Label {
            color: "white"
            text: qsTr(root.option.label)
            font: Fonts.buttonFont

            Layout.fillWidth: true
            Layout.alignment: Qt.AlignVCenter
            Layout.margins: 10
        }
    }

    Rectangle {
        id: border
        anchors.left: parent.left
        anchors.right: parent.right
        anchors.top: row.bottom
        color: "#999999"
        height: 2
    }
}
