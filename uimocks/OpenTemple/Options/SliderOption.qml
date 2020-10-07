import QtQuick 2.0

Option {

    required property int minValue

    required property int maxValue

    property bool isPercentage : false

    property int value

    function onChange(value: int) {}
}
