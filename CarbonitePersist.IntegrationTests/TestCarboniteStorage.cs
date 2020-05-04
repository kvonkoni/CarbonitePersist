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
        public async Task TestUploadAsync()
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
        public async Task TestUploadOverloadAsync()
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
        public async Task TestUploadStreamingAsync()
        {
            var database = Guid.NewGuid().ToString();
            var path = Path.Combine(Path.GetTempPath(), database);
            var ct = new CarboniteTool($"Path={path}");
            var stor = ct.GetStorage();

            var fileSource = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName + @"\filesource";

            using (var sourceStream = new FileStream($"{Path.Combine(fileSource, "This is a test file.docx")}", FileMode.Open))
                await stor.UploadAsync(1, "This is an uploaded test file.docx", sourceStream);

            var file = await stor.GetByIdAsync(1);

            Assert.Equal("This is an uploaded test file.docx", file.Filename);

            Directory.Delete(path, true);
        }

        [Fact]
        public async Task TestUploadStreamingOverloadAsync()
        {
            var database = Guid.NewGuid().ToString();
            var path = Path.Combine(Path.GetTempPath(), database);
            var ct = new CarboniteTool($"Path={path}");
            var stor = ct.GetStorage();

            var fileSource = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName + @"\filesource";

            using (var sourceStream1 = new FileStream($"{Path.Combine(fileSource, "This is a test file.docx")}", FileMode.Open))
            using (var sourceStream2 = new FileStream($"{Path.Combine(fileSource, "broken.pdf")}", FileMode.Open))
                await stor.UploadAsync(new List<object> { 1, 2 }, new List<string> { "This is an uploaded test file.docx", "broken.pdf" }, new List<Stream> { sourceStream1, sourceStream2 });

            var file = await stor.GetByIdAsync(1);

            Assert.Equal("This is an uploaded test file.docx", file.Filename);

            Directory.Delete(path, true);
        }

        [Fact]
        public async Task TestGetAllAsync()
        {
            var path = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName + @"\CarboniteTest";
            var ct = new CarboniteTool($"Path={path}");
            var stor = ct.GetStorage();

            var files = await stor.GetAllAsync();

            Assert.Equal(3, files.Length);

            Assert.Equal(1, files[0].Id);
            Assert.Equal("This is a test file.docx", files[0].Filename);

            Assert.Equal(2, files[1].Id);
            Assert.Equal("broken.pdf", files[1].Filename);

            Assert.Equal(3, files[2].Id);
            Assert.Equal("letter.txt", files[2].Filename);
        }

        [Fact]
        public async Task TestFindAllAsync()
        {
            var path = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName + @"\CarboniteTest";
            var ct = new CarboniteTool($"Path={path}");
            var stor = ct.GetStorage();

            var file = await stor.FindAllAsync(x => x.Id == "1");

            Assert.Equal(1, file[0].Id);
            Assert.Equal("This is a test file.docx", file[0].Filename);
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

        [Fact]
        public async Task TestGetByIdsAsync()
        {
            var path = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName + @"\CarboniteTest";
            var ct = new CarboniteTool($"Path={path}");
            var stor = ct.GetStorage();

            var files = await stor.GetByIdsAsync(new List<object> { 1, 2 });

            Assert.Equal(2, files.Length);

            Assert.Equal(1, files[0].Id);
            Assert.Equal("This is a test file.docx", files[0].Filename);

            Assert.Equal(2, files[1].Id);
            Assert.Equal("broken.pdf", files[1].Filename);
        }

        [Fact]
        public async Task TestDownloadAsync()
        {
            var path = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName + @"\CarboniteTest";
            var ct = new CarboniteTool($"Path={path}");
            var stor = ct.GetStorage();

            var randomName = Guid.NewGuid().ToString();
            var destination = Path.Combine(Path.GetTempPath(), randomName);

            await stor.DownloadAsync(1, destination);

            File.Delete(destination);
        }

        [Fact]
        public async Task TestDownloadOverloadAsync()
        {
            var path = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName + @"\CarboniteTest";
            var ct = new CarboniteTool($"Path={path}");
            var stor = ct.GetStorage();

            var randomName1 = Guid.NewGuid().ToString();
            var randomName2 = Guid.NewGuid().ToString();
            var destination1 = Path.Combine(Path.GetTempPath(), randomName1);
            var destination2 = Path.Combine(Path.GetTempPath(), randomName2);

            await stor.DownloadAsync(new List<object> { 1, 2 }, new List<string> { destination1, destination2 });

            File.Delete(destination1);
            File.Delete(destination2);
        }

        [Fact]
        public async Task TestCopyToStreamAsync()
        {
            var path = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName + @"\CarboniteTest";
            var ct = new CarboniteTool($"Path={path}");
            var stor = ct.GetStorage();

            var randomName = Guid.NewGuid().ToString();
            var destination = Path.Combine(Path.GetTempPath(), randomName);

            using (var destinationStream = new FileStream(destination, FileMode.Create))
                await stor.CopyFileToStreamAsync(1, destinationStream);

            File.Delete(destination);
        }

        [Fact]
        public async Task TestCopyToStreamOverloadAsync()
        {
            var path = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName + @"\CarboniteTest";
            var ct = new CarboniteTool($"Path={path}");
            var stor = ct.GetStorage();

            var randomName1 = Guid.NewGuid().ToString();
            var randomName2 = Guid.NewGuid().ToString();
            var destination1 = Path.Combine(Path.GetTempPath(), randomName1);
            var destination2 = Path.Combine(Path.GetTempPath(), randomName2);

            using (var destinationStream1 = new FileStream(destination1, FileMode.Create))
            using (var destinationStream2 = new FileStream(destination2, FileMode.Create))
                await stor.CopyFileToStreamAsync(new List<object> { 1, 2 }, new List<Stream> { destinationStream1, destinationStream2 });

            File.Delete(destination1);
            File.Delete(destination2);
        }

        [Fact]
        public async Task TestSetFilenameAsync()
        {
            var database = Guid.NewGuid().ToString();
            var path = Path.Combine(Path.GetTempPath(), database);
            var ct = new CarboniteTool($"Path={path}");
            var stor = ct.GetStorage();

            var fileSource = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName + @"\filesource";

            await stor.UploadAsync(1, $"{Path.Combine(fileSource, "This is a test file.docx")}");

            var oldmeta = await stor.GetByIdAsync(1);

            Assert.Equal("This is a test file.docx", oldmeta.Filename);

            await stor.SetFilename(1, "NewFilename.docx");

            var newmeta = await stor.GetByIdAsync(1);

            Assert.Equal("NewFilename.docx", newmeta.Filename);

            Directory.Delete(path, true);
        }

        [Fact]
        public async Task TestSetMetadataAsync()
        {
            var database = Guid.NewGuid().ToString();
            var path = Path.Combine(Path.GetTempPath(), database);
            var ct = new CarboniteTool($"Path={path}");
            var stor = ct.GetStorage();

            var fileSource = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName + @"\filesource";

            await stor.UploadAsync(1, $"{Path.Combine(fileSource, "This is a test file.docx")}");

            var oldmeta = await stor.GetByIdAsync(1);

            Assert.Empty(oldmeta.Metadata);

            var metadata = new Dictionary<string, string>
            {
                {"key1" , "value1" },
                {"key2" , "value2" },
                {"key3" , "value3" },
                {"key4" , "value4" },
            };

            await stor.SetMetadata(1, metadata);

            var newmeta = await stor.GetByIdAsync(1);

            Assert.Equal(metadata, newmeta.Metadata);

            Directory.Delete(path, true);
        }

        [Fact]
        public async Task TestDeleteAsync()
        {
            var database = Guid.NewGuid().ToString();
            var path = Path.Combine(Path.GetTempPath(), database);
            var ct = new CarboniteTool($"Path={path}");
            var stor = ct.GetStorage();

            var fileSource = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName + @"\filesource";

            await stor.UploadAsync(new List<object> { 1, 2 }, new List<string> { $"{Path.Combine(fileSource, "This is a test file.docx")}", $"{Path.Combine(fileSource, "broken.pdf")}" });

            await stor.DeleteAsync(1);

            var files = await stor.GetAllAsync();

            Assert.Single(files);
            Assert.Equal(2, files[0].Id);

            Directory.Delete(path, true);
        }

        [Fact]
        public async Task TestDeleteMultipleAsync()
        {
            var database = Guid.NewGuid().ToString();
            var path = Path.Combine(Path.GetTempPath(), database);
            var ct = new CarboniteTool($"Path={path}");
            var stor = ct.GetStorage();

            var fileSource = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName + @"\filesource";

            await stor.UploadAsync(new List<object> { 1, 2 }, new List<string> { $"{Path.Combine(fileSource, "This is a test file.docx")}", $"{Path.Combine(fileSource, "broken.pdf")}" });

            await stor.DeleteMultipleAsync(new List<object> { 1, 2 });

            var files = await stor.GetAllAsync();

            Assert.Empty(files);

            Directory.Delete(path, true);
        }

        [Fact]
        public async Task TestDeleteAllAsync()
        {
            var database = Guid.NewGuid().ToString();
            var path = Path.Combine(Path.GetTempPath(), database);
            var ct = new CarboniteTool($"Path={path}");
            var stor = ct.GetStorage();

            var fileSource = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName + @"\filesource";

            await stor.UploadAsync(new List<object> { 1, 2 }, new List<string> { $"{Path.Combine(fileSource, "This is a test file.docx")}", $"{Path.Combine(fileSource, "broken.pdf")}" });

            await stor.DeleteAllAsync();

            var files = await stor.GetAllAsync();

            Assert.Empty(files);

            Directory.Delete(path, true);
        }
    }
}
