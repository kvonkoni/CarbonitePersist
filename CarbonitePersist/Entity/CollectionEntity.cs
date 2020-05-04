using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CarbonitePersist.Entity
{
    public class CollectionEntity
    {
        public object Id { get; private set; }

        public string Filepath { get; private set; }

        public CollectionEntity(string path)
        {
            Id = Path.GetFileNameWithoutExtension(path);
            Filepath = path;
        }
    }
}
