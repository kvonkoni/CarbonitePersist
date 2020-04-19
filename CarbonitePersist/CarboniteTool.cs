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

        internal readonly string storagePath;

        internal readonly string entityCollectionPath;

        internal readonly string documentCollectionPath;

        public CarboniteTool(string connectionString)
        {
            _carboniteConnectionStringBuilder = new CarboniteConnectionStringBuilder(connectionString);
            
            storagePath = _carboniteConnectionStringBuilder.Path;
            entityCollectionPath = Path.Combine(storagePath, @"xmls");
            documentCollectionPath = Path.Combine(storagePath, @"documents");

            DbConnectionInit();
        }

        public void DbConnectionInit()
        {
            try
            {
                if (!Directory.Exists(storagePath))
                {
                    Directory.CreateDirectory(storagePath);
                }
                if (!Directory.Exists(entityCollectionPath))
                {
                    Directory.CreateDirectory(entityCollectionPath);
                }
                if (!Directory.Exists(documentCollectionPath))
                {
                    Directory.CreateDirectory(documentCollectionPath);
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
