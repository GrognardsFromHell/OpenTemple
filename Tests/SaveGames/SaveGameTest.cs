using System;
using System.IO;
using System.Reflection;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using OpenTemple.Core.IO.SaveGames;
using Xunit;

namespace OpenTemple.Tests.SaveGames
{
    public class SaveGameTest
    {
        [Fact]
        public void CanLoadCo8SaveGame()
        {
            var savePath = TestData.GetPath("SaveGames/TestData/slot0007");
            using var tempDir = new TempDirectory();

            var saveFile = SaveGameFile.Load(savePath, tempDir.Path);
            Console.WriteLine();
        }

        [Fact]
        public void CanLoadVanillaPatch2SaveGame()
        {
            var savePath = TestData.GetPath("SaveGames/TestData/slot0014");
            using var tempDir = new TempDirectory();

            var saveFile = SaveGameFile.Load(savePath, tempDir.Path);
            Console.WriteLine();
        }
    }
}