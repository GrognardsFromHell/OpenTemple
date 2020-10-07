import QtQuick 2.0
pragma Singleton

QtObject {

    function save() {
    }

    property list<OptionsPage> pages : [
        OptionsPage {
            name: "#{options:0}"
            options: [
                CheckboxOption {
                    label: "Windowed"
                },
                SliderOption {
                    label: "#{options:104}"
                },
                CheckboxOption {
                    label: "#{options:110}"
                }
            ]
        },
        OptionsPage {
            name: "#{options:1}"
            options: [
                SliderOption {
                    label: "Master Volume"
                },
                CheckboxOption {
                    label: "Sound Effects"
                }
            ]
        },
        OptionsPage {
            id: manyManyPage
            name: "Many Many Many"
            Component.onCompleted: {
                const component = Qt.createComponent("CheckboxOption.qml");
                for (let i = 0; i < 50; i++) {
                    const option = component.createObject(null, {
                                                              label: "Option " + i
                                                          });
                    manyManyPage.options.push(option)
                }
            }
        }
    ]
}
