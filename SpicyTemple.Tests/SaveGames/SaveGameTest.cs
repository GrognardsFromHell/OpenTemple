using System;
using System.IO;
using System.Reflection;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SpicyTemple.Core.IO.SaveGames;
using Xunit;

namespace SpicyTemple.Tests.SaveGames
{
    public class SaveGameTest
    {
        [Fact]
        public void CanLoadSaveGame()
        {
            var savePath = TestData.GetPath("SaveGames/TestData/slot0007");
            using var tempDir = new TempDirectory();

            var saveFile = SaveGameFile.Load(savePath, tempDir.Path);
        }
    }
}