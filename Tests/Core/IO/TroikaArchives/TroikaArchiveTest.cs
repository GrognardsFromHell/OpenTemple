using System;
using System.Linq;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using OpenTemple.Core.IO.TroikaArchives;
using OpenTemple.Tests.TestUtils;

namespace OpenTemple.Tests.Core.IO.TroikaArchives
{
    public class TroikaArchiveTest
    {
        [Test]
        public void CanReadEmptyArchive()
        {
            using var archive = new TroikaArchive(TestData.GetPath("Core/IO/TroikaArchives/empty.dat"));
            archive.Entries.Length.Should().Be(0);
        }

        [Test]
        public void FindEntryInEmptyArchiveWorks()
        {
            using var archive = new TroikaArchive(TestData.GetPath("Core/IO/TroikaArchives/empty.dat"));
            archive.FileExists("art/some-file.txt").Should().BeFalse();
            archive.DirectoryExists("art/some-dir").Should().BeFalse();
        }

        [Test]
        public void CanReadArchive()
        {
            using var archive = new TroikaArchive(TestData.GetPath("Core/IO/TroikaArchives/test.dat"));
            archive.Entries.ToArray().Select(e => archive.GetFullPath(e)).Should().BeEquivalentTo(
                "root_test.txt", "subfolder", "subfolder/TEST.txt"
            );
            archive.ArchiveGuid.Should().Be(Guid.Parse("411d678d-c5ac-5f61-543a-64d531544144"));
        }

        [Test]
        public void CanListAndReadFileInSubfolder()
        {
            using var archive = new TroikaArchive(TestData.GetPath("Core/IO/TroikaArchives/test.dat"));

            archive.FileExists("subfolder/test.txt").Should().BeTrue();
            archive.ListDirectory("subfolder").Should().BeEquivalentTo("TEST.txt");
            ReadAscii(archive, "subfolder/test.txt").Should().Be("random test data in subfolder");
        }

        [TestCase("subfolder/test.txt")]
        [TestCase("subfolder/TEST.txt")]
        [TestCase("subfolder\\TEST.txt")]
        public void CanDeleteFileInSubfolder(string pathToDelete)
        {
            using var archive = new TroikaArchive(TestData.GetPath("Core/IO/TroikaArchives/test.dat"));

            // Now mark the file as deleted
            archive.SetDeleted(pathToDelete);

            archive.FileExists("subfolder/TEST.txt").Should().BeFalse();
            archive.ListDirectory("subfolder").Should().BeEmpty();
            ReadAscii(archive, "subfolder/TEST.txt").Should().BeNull();
        }

        [Test]
        public void CanDeleteSubfolder()
        {
            using var archive = new TroikaArchive(TestData.GetPath("Core/IO/TroikaArchives/test.dat"));

            // Now mark the entire subfolder as deleted
            archive.SetDeleted("subfolder");

            archive.DirectoryExists("subfolder").Should().BeFalse();
            archive.FileExists("subfolder/TEST.txt").Should().BeFalse();
            archive.ListDirectory("subfolder").Should().BeEmpty();
            ReadAscii(archive, "subfolder/TEST.txt").Should().BeNull();
        }

        private static string ReadAscii(TroikaArchive archive, string path)
        {
            using var memory = archive.ReadFile(path);
            return memory != null ? Encoding.ASCII.GetString(memory.Memory.Span) : null;
        }
    }
}