import QtQuick 2.0

pragma Singleton

QtObject {

    function focusClear() {
    }

    function selectInRectangle(rect) {
        console.log('Selecting units in ', rect);
    }

}
