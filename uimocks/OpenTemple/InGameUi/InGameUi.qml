import QtQuick 2.0

pragma Singleton

QtObject {

    function pickObject(gameView, x, y) {

    }

    function setScrollDirection(gameView, x, y) {

    }

    function findPartyMembersInRect(x, y, width, height) {
        return [];
    }

    function setSelectionHighlights(objs) {
        console.log('Setting selection highlights to: ', objs);
    }

    function scrollBy(vector) {
        console.log('Scroll by: ', vector);
    }

}
