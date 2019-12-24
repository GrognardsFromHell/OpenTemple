using System.Text;
using OpenTemple.Core.IO.TabFiles;
using Xunit;

namespace OpenTemple.Tests
{
    public class TabFileTest
    {
        [Fact]
        public void TestLineParsing()
        {
            var line = "0\tobj_t_portal\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t0";

            var recordsFound = 0;

            TabFile.ParseSpan(Encoding.Default.GetBytes(line), record =>
            {
                recordsFound++;

                Assert.Equal(0, record[0].GetInt());
                Assert.Equal("obj_t_portal", record[1].AsString());
                for (int i = 2; i < 23; i++)
                {
                    Assert.True(record[i].IsEmpty);
                }
                Assert.Equal(0, record[23].GetInt());
            });

            Assert.Equal(1, recordsFound);
        }
    }
}