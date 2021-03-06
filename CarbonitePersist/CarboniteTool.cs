﻿using CarbonitePersist.Connection;
using System;
using System.IO;

namespace CarbonitePersist
{
    public class CarboniteTool
    {
        private readonly CarboniteConnectionStringBuilder _carboniteConnectionStringBuilder;

        internal readonly string path;

        public string CabronitePath
        {
            get
            {
                return path;
            }
        }

        internal readonly string collectionPath;

        internal readonly string storagePath;

        public CarboniteTool(string connectionString)
        {
            _carboniteConnectionStringBuilder = new CarboniteConnectionStringBuilder(connectionString);
            
            path = _carboniteConnectionStringBuilder.Path;
            collectionPath = Path.Combine(path, @"collections");
            storagePath = Path.Combine(path, @"storage");

            DbConnectionInit();
        }

        public void DbConnectionInit()
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
        }

        public CarboniteCollection<T> GetCollection<T>()
        {
            return new CarboniteCollection<T>(this);
        }

        public CarboniteCollection<T> GetCollection<T>(string name)
        {
            return new CarboniteCollection<T>(this, name);
        }

        public CarboniteStorage GetStorage()
        {
            return new CarboniteStorage(this);
        }
    }
}
