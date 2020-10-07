

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

T.ScrollBar {
    id: control

    implicitWidth: orientation
                   == Qt.Horizontal ? Math.max(
                                          implicitBackgroundWidth + leftInset + rightInset,
                                          implicitContentWidth + leftPadding
                                          + rightPadding) : minusButton.implicitWidth
    implicitHeight: orientation
                    == Qt.Horizontal ? minusButton.implicitHeight : Math.max(
                                           implicitBackgroundHeight + topInset + bottomInset,
                                           implicitContentHeight + topPadding + bottomPadding)

    visible: control.policy !== T.ScrollBar.AlwaysOff
    minimumSize: orientation == Qt.Horizontal ? height / width : width / height

    leftPadding: orientation == Qt.Horizontal ? minusButton.implicitWidth : 0
    rightPadding: orientation == Qt.Horizontal ? plusButton.implicitWidth : 0
    topPadding: orientation == Qt.Horizontal ? 0 : minusButton.implicitHeight
    bottomPadding: orientation == Qt.Horizontal ? 0 : plusButton.implicitHeight

    background: Item {
        id: background
        ScrollBarButton {
            id: minusButton
            direction: orientation === Qt.Horizontal ? ScrollBarButton.Direction.Left : ScrollBarButton.Direction.Up
            onClicked: control.decrease()
        }

        ScrollBarButton {
            id: plusButton

            anchors.right: orientation === Qt.Horizontal ? background.right : undefined
            anchors.bottom: orientation === Qt.Vertical ? background.bottom : undefined

            direction: orientation === Qt.Horizontal ? ScrollBarButton.Direction.Right : ScrollBarButton.Direction.Down
            onClicked: control.increase()
        }

        Image {
            id: track
            anchors.left: orientation == Qt.Horizontal ? minusButton.right : minusButton.left
            anchors.top: orientation == Qt.Horizontal ? minusButton.top : minusButton.bottom
            anchors.bottom: orientation == Qt.Horizontal ? minusButton.bottom : plusButton.top
            anchors.right: orientation == Qt.Horizontal ? plusButton.left : minusButton.right
            source: orientation == Qt.Horizontal ? "ScrollBar_Hor_Empty.png" : "ScrollBar_Empty.png"
        }
    }

    contentItem: BorderImage {
        // color: control.pressed ? control.palette.dark : control.palette.mid
        source: orientation == Qt.Horizontal ? "ScrollBar_Thumb_Hor.sci" : "ScrollBar_Thumb_Ver.sci"
        visible: control.policy === T.ScrollBar.AlwaysOn
                 || (control.active && control.size < 1.0) ? 1.0 : 0.0
    }
}
