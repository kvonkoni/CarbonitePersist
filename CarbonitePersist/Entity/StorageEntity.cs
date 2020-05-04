using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CarbonitePersist.Entity
{
    public class StorageEntity
    {
        public string Id { get; private set; }

        internal string Filepath { get; private set; }

        internal string Metapath { get; private set; }

        internal StorageEntity(string path)
        {
            Id = Path.GetFileNameWithoutExtension(path);
            Filepath = path;
            var metadataFilepath = Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path) + ".xml");
            if (File.Exists(metadataFilepath))
                Metapath = metadataFilepath;
            else
                Metapath = null;
        }
    }
}
