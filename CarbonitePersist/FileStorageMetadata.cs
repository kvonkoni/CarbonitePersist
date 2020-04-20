using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CarbonitePersist
{
    public class FileStorageMetadata
    {
        public object Id { get; set; }
        public string Filename { get; set; }
        public string FileType
        {
            get
            {
                return Path.GetExtension(Filename);
            }
        }
        public string Description { get; set; }
    }
}
