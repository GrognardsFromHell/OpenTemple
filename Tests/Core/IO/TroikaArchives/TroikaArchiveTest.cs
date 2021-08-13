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
    }
}