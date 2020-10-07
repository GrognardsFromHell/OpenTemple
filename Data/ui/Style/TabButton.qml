/****************************************************************************
**
** Copyright (C) 2017 The Qt Company Ltd.
** Contact: http://www.qt.io/licensing/
**
** This file is part of the Qt Quick Controls 2 module of the Qt Toolkit.
**
** $QT_BEGIN_LICENSE:LGPL3$
** Commercial License Usage
** Licensees holding valid commercial Qt licenses may use this file in
** accordance with the commercial license agreement provided with the
** Software or, alternatively, in accordance with the terms contained in
** a written agreement between you and The Qt Company. For licensing terms
** and conditions see http://www.qt.io/terms-conditions. For further
** information use the contact form at http://www.qt.io/contact-us.
**
** GNU Lesser General Public License Usage
** Alternatively, this file may be used under the terms of the GNU Lesser
** General Public License version 3 as published by the Free Software
** Foundation and appearing in the file LICENSE.LGPLv3 included in the
** packaging of this file. Please review the following information to
** ensure the GNU Lesser General Public License version 3 requirements
** will be met: https://www.gnu.org/licenses/lgpl.html.
**
** GNU General Public License Usage
** Alternatively, this file may be used under the terms of the GNU
** General Public License version 2.0 or later as published by the Free
** Software Foundation and appearing in the file LICENSE.GPL included in
** the packaging of this file. Please review the following information to
** ensure the GNU General Public License version 2.0 requirements will be
** met: http://www.gnu.org/licenses/gpl-2.0.html.
**
** $QT_END_LICENSE$
**
****************************************************************************/

import Common 1.0
import QtQuick 2.12
import QtQuick.Controls 2.12
import QtQuick.Controls.impl 2.12
import QtQuick.Templates 2.12 as T
import QtQuick.Shapes 1.15

T.TabButton {
    id: control

    implicitWidth: Math.max(implicitBackgroundWidth + leftInset + rightInset,
                            implicitContentWidth + leftPadding + rightPadding)
    implicitHeight: Math.max(implicitBackgroundHeight + topInset + bottomInset,
                             implicitContentHeight + topPadding + bottomPadding)

    padding: 6
    spacing: 6

    icon.width: 24
    icon.height: 24
    icon.color: checked ? control.palette.windowText : control.palette.brightText

    font: Fonts.buttonFont

    leftPadding: shape.sideWidth + 5
    rightPadding: shape.sideWidth + 5

    contentItem: IconLabel {
        spacing: control.spacing
        mirrored: control.mirrored
        display: control.display

        icon: control.icon
        text: control.text
        font: control.font
        color: "#ffffff"
    }

    background: Shape {
        id: shape
        implicitHeight: 25

        layer.enabled: true
        layer.samples: 4

        readonly property int sideWidth : 10

        ShapePath {
            strokeColor: "transparent"
            fillColor: control.checked ? "#1ac3ff" : "#41586f"
            startX: 0
            startY: shape.height
            PathCubic {
                x: shape.sideWidth; y: 0
                control1X: shape.sideWidth; control1Y: shape.height
                control2X: 0.5 * shape.sideWidth; control2Y: 0
            }
            PathLine {
                x: shape.width - shape.sideWidth; y: 0
            }
            PathCubic {
                x: shape.width; y: shape.height
                control2X: shape.width - shape.sideWidth; control2Y: shape.height
                control1X: shape.width - 0.5 * shape.sideWidth; control1Y: 0
            }
        }
    }

}
