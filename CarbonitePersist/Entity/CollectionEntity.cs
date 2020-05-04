using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CarbonitePersist.Entity
{
    public class CollectionEntity
    {
        public string Id { get; private set; }

        internal string Filepath { get; private set; }

        internal CollectionEntity(string path)
        {
            Id = Path.GetFileNameWithoutExtension(path);
            Filepath = path;
        }
    }
}
