using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace CarbonitePersist.UnitTest
{
    public class TestCarboniteDatabase
    {
        [Fact]
        public async Task TestInsertAsync()
        {
            var database = Guid.NewGuid().ToString();
            var path = Path.Combine(Path.GetTempPath(), database);
            var db = new CarboniteTool($"Database={path}");
            var col = db.GetCollection<MyEntity>();

            var entity = new MyEntity
            {
                Id = 6,
                Name = "First",
                IsCreated = true,
            };

            await col.InsertAsync(entity);

            Directory.Delete(path, true);
        }
    }
}
