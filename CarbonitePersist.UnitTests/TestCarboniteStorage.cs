using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace CarbonitePersist.UnitTests
{
    public class TestCarboniteStorage
    {
        [Fact]
        public async Task TestInsertAsync()
        {
            var database = Guid.NewGuid().ToString();
            var path = Path.Combine(Path.GetTempPath(), database);
            var ct = new CarboniteTool($"Path={path}");
            var stor = ct.GetStorage();

            var fileSource = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName + @"\filesource";

            await stor.UploadAsync(1, $"{Path.Combine(fileSource, "This is a test file.docx")}");
            await stor.UploadAsync(2, $"{Path.Combine(fileSource, "broken.pdf")}");

            Directory.Delete(path, true);
        }

        [Fact]
        public async Task TestInsertOverloadAsync()
        {
            var database = Guid.NewGuid().ToString();
            var path = Path.Combine(Path.GetTempPath(), database);
            var ct = new CarboniteTool($"Path={path}");
            var stor = ct.GetStorage();

            var fileSource = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName + @"\filesource";

            await stor.UploadAsync(new List<object> { 1, 2 }, new List<string> { $"{Path.Combine(fileSource, "This is a test file.docx")}", $"{Path.Combine(fileSource, "broken.pdf")}" });

            Directory.Delete(path, true);
        }
    }
}
