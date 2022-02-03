using System.Text;
using OpenTemple.Core.IO.TabFiles;
using NUnit.Framework;

namespace OpenTemple.Tests;

public class TabFileTest
{
    [Test]
    public void TestLineParsing()
    {
        var line = "0\tobj_t_portal\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t0";

        var recordsFound = 0;

        TabFile.ParseSpan(Encoding.Default.GetBytes(line), (in TabFileRecord record) =>
        {
            recordsFound++;

            Assert.AreEqual(0, record[0].GetInt());
            Assert.AreEqual("obj_t_portal", record[1].AsString());
            for (int i = 2; i < 23; i++)
            {
                Assert.True(record[i].IsEmpty);
            }
            Assert.AreEqual(0, record[23].GetInt());
        });

        Assert.AreEqual(1, recordsFound);
    }
}