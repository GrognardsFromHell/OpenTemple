import QtQuick 2.0
pragma Singleton

QtObject {

    function toVariantList(x) {
        let result = [];
        for (let i = 0; i < x.length; i++) {
            result.push(x[i]);
        }
        return result;
    }

}
