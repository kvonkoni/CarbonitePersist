using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CarbonitePersist
{
    public class CarboniteTool
    {
        private static readonly NLog.Logger _log = NLog.LogManager.GetCurrentClassLogger();

        private readonly CarboniteConnectionStringBuilder _carboniteConnectionStringBuilder;

        internal readonly string path;

        internal readonly string collectionPath;

        internal readonly string storagePath;

        internal readonly string storageMetadataPath;

        public CarboniteTool(string connectionString)
        {
            _carboniteConnectionStringBuilder = new CarboniteConnectionStringBuilder(connectionString);
            
            path = _carboniteConnectionStringBuilder.Path;
            collectionPath = Path.Combine(path, @"collections");
            storagePath = Path.Combine(path, @"storage");
            storageMetadataPath = Path.Combine(path, @"storage-metadata");

            DbConnectionInit();
        }

        public void DbConnectionInit()
        {
            try
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                if (!Directory.Exists(collectionPath))
                {
                    Directory.CreateDirectory(collectionPath);
                }
                if (!Directory.Exists(storagePath))
                {
                    Directory.CreateDirectory(storagePath);
                }
                if (!Directory.Exists(storageMetadataPath))
                {
                    Directory.CreateDirectory(storageMetadataPath);
                }
            }
            catch (Exception e)
            {
                _log.Error(e);
            }
        }

        public CarboniteCollection<T> GetCollection<T>()
        {
            return new CarboniteCollection<T>(this);
        }

        public CarboniteCollection<T> GetCollection<T>(string name)
        {
            return new CarboniteCollection<T>(this, name);
        }
    }
}
