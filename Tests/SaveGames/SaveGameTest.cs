using System;
using System.IO;
using System.Reflection;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using OpenTemple.Core.IO.SaveGames;
using OpenTemple.Tests.TestUtils;
using NUnit.Framework;

namespace OpenTemple.Tests.SaveGames
{
    public class SaveGameTest
    {
        [Test]
        public void CanLoadCo8SaveGame()
        {
            var savePath = TestData.GetPath("SaveGames/TestData/slot0007");
            using var tempDir = new TempDirectory();

            var saveFile = SaveGameFile.Load(savePath, tempDir.Path);
            Console.WriteLine();
        }

        [Test]
        public void CanLoadVanillaPatch2SaveGame()
        {
            var savePath = TestData.GetPath("SaveGames/TestData/slot0014");
            using var tempDir = new TempDirectory();

            var saveFile = SaveGameFile.Load(savePath, tempDir.Path);
            Console.WriteLine();
        }
    }
}