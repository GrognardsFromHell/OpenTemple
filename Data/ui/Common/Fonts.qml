pragma Singleton

import QtQuick 2.14

QtObject {
    property FontLoader headerTextFontLoader: FontLoader {
        id: headerTextFontLoader
        source: "../fonts/LiberationSans/LiberationSans-Bold.ttf"
    }

    readonly property font headerFont: Qt.font({
                                                   "family": headerTextFontLoader.name,
                                                   "bold": true,
                                                   "pointSize": 24
                                               })

    property FontLoader bodyTextFontLoader: FontLoader {
        id: bodyTextFontLoader
        source: "../fonts/LibreBaskerville-Regular.ttf"
    }
    readonly property alias bodyText: bodyTextFontLoader.name

    property FontLoader mainMenuButtonFontLoader: FontLoader {
        id: mainMenuButtonFontLoader
        source: "../fonts/UncialAntiqua/UncialAntiqua-Regular.ttf"
    }
    readonly property alias mainMenuButton: mainMenuButtonFontLoader.name

    readonly property font buttonFont: Qt.font({
                                                   "family": headerTextFontLoader.name,
                                                   "bold": true,
                                                   "pointSize": 18
                                               })
}
