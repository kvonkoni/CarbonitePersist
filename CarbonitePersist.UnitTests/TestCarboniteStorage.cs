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

        [Fact]
        public async Task TestGetAllAsync()
        {
            var path = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName + @"\CarboniteTest";
            var ct = new CarboniteTool($"Path={path}");
            var stor = ct.GetStorage();

            var files = await stor.GetAllAsync();

            Assert.Equal(3, files.Count);

            Assert.Equal(1, files[0].Id);
            Assert.Equal("This is a test file.docx", files[0].Filename);

            Assert.Equal(2, files[1].Id);
            Assert.Equal("broken.pdf", files[1].Filename);

            Assert.Equal(3, files[2].Id);
            Assert.Equal("letter.txt", files[2].Filename);
        }

        [Fact]
        public async Task TestGetByIdAsync()
        {
            var path = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName + @"\CarboniteTest";
            var ct = new CarboniteTool($"Path={path}");
            var stor = ct.GetStorage();

            var file = await stor.GetByIdAsync(1);

            Assert.Equal(1, file.Id);
            Assert.Equal("This is a test file.docx", file.Filename);
        }
    }
}
