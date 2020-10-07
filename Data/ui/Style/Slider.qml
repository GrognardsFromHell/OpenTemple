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

import QtQuick 2.12
import QtQuick.Controls 2.12
import QtQuick.Controls.impl 2.12
import QtQuick.Templates 2.12 as T

T.Slider {
    id: control

    implicitWidth: Math.max(implicitBackgroundWidth + leftInset + rightInset,
                            implicitHandleWidth + leftPadding + rightPadding)
    implicitHeight: Math.max(implicitBackgroundHeight + topInset + bottomInset,
                             implicitHandleHeight + topPadding + bottomPadding)

    padding: 6

    handle: Image {
        source: control.horizontal ? "slider_handle.png" : "slider_handle_ver.png"
        x: control.leftPadding + (control.horizontal ? control.visualPosition * (control.availableWidth - width) : (control.availableWidth - width) / 2)
        y: control.topPadding + (control.horizontal ? (control.availableHeight - height) / 2 : control.visualPosition * (control.availableHeight - height))
        //implicitWidth: 28
        //implicitHeight: 28
        //radius: width / 2
        //color: control.pressed ? control.palette.light : control.palette.window
        //border.width: control.visualFocus ? 2 : 1
        //border.color: control.visualFocus ? control.palette.highlight : control.enabled ? control.palette.mid : control.palette.midlight
    }

    background: BorderImage {
        id: bgimage
        source: control.horizontal ? "slider_bg.sci" : "slider_bg_ver.sci"

        x: control.leftPadding + (control.horizontal ? 0 : (control.availableWidth - width) / 2)
        y: control.topPadding + (control.horizontal ? (control.availableHeight - height) / 2 : 0)
        width: control.horizontal ? control.availableWidth : implicitWidth
        height: control.horizontal ? implicitHeight : control.availableHeight
        // scale: control.horizontal && control.mirrored ? -1 : 1
    }

    topPadding: control.horizontal ? 6 : plusButton.height
    bottomPadding: control.horizontal ? 6 : minusButton.height

    leftPadding: control.horizontal ? minusButton.width : 6
    rightPadding: control.horizontal ? plusButton.width : 6

    SliderButton {
        id: plusButton
        anchors.bottom: control.horizontal ? undefined : bgimage.top
        anchors.horizontalCenter: control.horizontal ? undefined : bgimage.horizontalCenter
        anchors.left: control.horizontal ? bgimage.right : undefined
        anchors.verticalCenter: control.horizontal ? bgimage.verticalCenter : undefined
        direction: control.horizontal ? SliderButton.Direction.Right : SliderButton.Direction.Up
        onClicked: control.increase()
    }
    SliderButton {
        id: minusButton
        anchors.top: control.horizontal ? undefined : bgimage.bottom
        anchors.horizontalCenter: control.horizontal ? undefined : bgimage.horizontalCenter
        anchors.right: control.horizontal ? bgimage.left : undefined
        anchors.verticalCenter: control.horizontal ? bgimage.verticalCenter : undefined
        direction: control.horizontal ? SliderButton.Direction.Left : SliderButton.Direction.Down
        onClicked: control.decrease()
    }

}
