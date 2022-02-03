using System;
using System.Collections.Generic;
using System.Drawing;
using FluentAssertions;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.Systems.Raycast;
using NUnit.Framework;

namespace OpenTemple.Tests;

public class PartialSectorObjectEnumeratorTest
{
    private static List<GameObject>[,] EmptyTiles => new List<GameObject>[64, 64];

    [Test]
    public void TestEmptyEnumeration()
    {
        var e = new PartialSectorObjectEnumerator();
        e.MoveNext().Should().BeFalse();
    }

    [Test]
    public void TestEnumerationOfEmptySector()
    {
        var e = new PartialSectorObjectEnumerator(EmptyTiles, new Rectangle(0, 0, 64, 64));
        e.MoveNext().Should().BeFalse();
    }

    [Test]
    public void TestEnumerationOfSectorWithObjectInLastTile()
    {
        var tiles = EmptyTiles;
        var testObject = new GameObject();
        tiles[63, 63] = new List<GameObject> {testObject};
        var e = new PartialSectorObjectEnumerator(tiles, new Rectangle(0, 0, 64, 64));
        e.MoveNext().Should().BeTrue();
        e.Current.Should().Be(testObject);
        e.MoveNext().Should().BeFalse();
    }

    [Test]
    public void TestEnumerationOfSectorWithDiagonalObjects()
    {
        var tiles = EmptyTiles;
        var testObject1 = new GameObject();
        var testObject2 = new GameObject();
        var testObject3 = new GameObject();
        var testObject4 = new GameObject();
        tiles[0, 0] = new List<GameObject> {testObject1};
        tiles[63, 31] = new List<GameObject> {testObject2, testObject3};
        tiles[63, 63] = new List<GameObject> {testObject4};
        var e = new PartialSectorObjectEnumerator(tiles, new Rectangle(0, 0, 64, 64));
        e.MoveNext().Should().BeTrue();
        e.Current.Should().Be(testObject1);
        e.MoveNext().Should().BeTrue();
        e.Current.Should().Be(testObject2);
        e.MoveNext().Should().BeTrue();
        e.Current.Should().Be(testObject3);
        e.MoveNext().Should().BeTrue();
        e.Current.Should().Be(testObject4);
        e.MoveNext().Should().BeFalse();
    }

    [Test]
    public void TestEnumeratePartialSectorOneTileSet()
    {
        var tiles = EmptyTiles;
        var testObject = new GameObject();
        tiles[2, 2] = new List<GameObject> {testObject};
        var e = new PartialSectorObjectEnumerator(tiles, new Rectangle(0, 0, 2, 64));
        e.MoveNext().Should().BeFalse();

        // Check to the right of the set cell
        e = new PartialSectorObjectEnumerator(tiles, new Rectangle(3, 0, 1, 64));
        e.MoveNext().Should().BeFalse();

        // Check below
        e = new PartialSectorObjectEnumerator(tiles, new Rectangle(2, 3, 62, 61));
        e.MoveNext().Should().BeFalse();

        // Check above
        e = new PartialSectorObjectEnumerator(tiles, new Rectangle(2, 1, 62, 1));
        e.MoveNext().Should().BeFalse();

        // Check tile only
        e = new PartialSectorObjectEnumerator(tiles, new Rectangle(2, 2, 1, 1));
        e.MoveNext().Should().BeTrue();
        e.Current.Should().Be(testObject);
        e.MoveNext().Should().BeFalse();
    }

    [Test]
    public void TestEnumeratePartialSectorOneTileNotSet()
    {
        var tiles = EmptyTiles;
        var testObject = new GameObject();
        for (int x = 0; x < 64; x++)
        {
            for (int y = 0; y < 64; y++)
            {
                tiles[x, y] = new List<GameObject>() {testObject};
            }
        }

        tiles[2, 2] = null;
        var e = new PartialSectorObjectEnumerator(tiles, new Rectangle(2, 2, 1, 1));
        e.MoveNext().Should().BeFalse();
    }
}