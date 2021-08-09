using System;
using System.Collections.Generic;
using System.Drawing;
using FluentAssertions;
using OpenTemple.Core.GameObject;
using OpenTemple.Core.Systems.Raycast;
using NUnit.Framework;

namespace OpenTemple.Tests
{
    public class PartialSectorObjectEnumeratorTest
    {
        private static List<GameObjectBody>[,] EmptyTiles => new List<GameObjectBody>[64, 64];

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
            var testObject = new GameObjectBody();
            tiles[63, 63] = new List<GameObjectBody> {testObject};
            var e = new PartialSectorObjectEnumerator(tiles, new Rectangle(0, 0, 64, 64));
            e.MoveNext().Should().BeTrue();
            e.Current.Should().Be(testObject);
            e.MoveNext().Should().BeFalse();
        }

        [Test]
        public void TestEnumerationOfSectorWithDiagonalObjects()
        {
            var tiles = EmptyTiles;
            var testObject1 = new GameObjectBody();
            var testObject2 = new GameObjectBody();
            var testObject3 = new GameObjectBody();
            var testObject4 = new GameObjectBody();
            tiles[0, 0] = new List<GameObjectBody> {testObject1};
            tiles[63, 31] = new List<GameObjectBody> {testObject2, testObject3};
            tiles[63, 63] = new List<GameObjectBody> {testObject4};
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
            var testObject = new GameObjectBody();
            tiles[2, 2] = new List<GameObjectBody> {testObject};
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
            var testObject = new GameObjectBody();
            for (int x = 0; x < 64; x++)
            {
                for (int y = 0; y < 64; y++)
                {
                    tiles[x, y] = new List<GameObjectBody>() {testObject};
                }
            }

            tiles[2, 2] = null;
            var e = new PartialSectorObjectEnumerator(tiles, new Rectangle(2, 2, 1, 1));
            e.MoveNext().Should().BeFalse();
        }
    }
}