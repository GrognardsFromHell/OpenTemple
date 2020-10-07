import QtQuick 2.14
import QtQuick.Controls 2.14
import Common 1.0
import QtQuick.Layouts 1.15
import OpenTemple 1.0
import OpenTemple.Options 1.0

Dialog {
    id: dialog
    title: "Options"

    modal: true
    parent: Overlay.overlay
    anchors.centerIn: parent

    property var pages : Net.toVariantList(Options.pages)

    signal commitOptions()

    // Save stuff
    onAccepted: {
        commitOptions();
        Options.save();
    }

    ColumnLayout {
        anchors.fill: parent

        TabBar {
            id: tabs
            z: 1 // Put on top of panel sibling
            Repeater {
                model: pages
                TabButton {
                    text: qsTr(modelData.name)
                    /* Size tab buttons to desired text width */
                    width: implicitWidth
                }
            }
            Layout.leftMargin: 15
            Layout.bottomMargin: -15
        }
        Panel {
            Layout.fillHeight: true
            Layout.fillWidth: true

            StackLayout {
                anchors.fill: parent
                currentIndex: tabs.currentIndex

                // Pre-create the pages so they keep their state
                // when tabbing between them
                Repeater {
                    model: pages

                    ScrollView {
                        id: scrollView
                        ScrollBar.vertical.policy: ScrollBar.AlwaysOn
                        ScrollBar.horizontal.policy: ScrollBar.AlwaysOff
                        clip: true
                        Layout.fillHeight: true
                        Layout.fillWidth: true

                        ColumnLayout {
                            width: scrollView.availableWidth

                            Repeater {
                                model: Net.toVariantList(modelData.options)
                                OptionsRow {
                                    id: optionsRow
                                    option: modelData
                                    Layout.fillWidth: true

                                    Connections {
                                        target: dialog
                                        function onCommitOptions() {
                                            optionsRow.commit();
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    standardButtons: DialogButtonBox.Ok | DialogButtonBox.Cancel
}
