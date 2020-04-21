using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;

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
        
        public DateTime UploadDate { get; set; }
        
        public List<SerializableKeyValuePair<string, string>> XMLMetadataProxy
        {
            get
            {
                var proxy = new List<SerializableKeyValuePair<string, string>>();
                if (Metadata != null)
                {
                    foreach (KeyValuePair<string, string> pair in Metadata)
                    {
                        proxy.Add(new SerializableKeyValuePair<string, string>
                        {
                            Key = pair.Key,
                            Value = pair.Value,
                        });
                    }
                }
                return proxy;
            }
            set
            {
                Metadata = new Dictionary<string, string>();
                foreach (SerializableKeyValuePair<string, string> pair in value)
                    Metadata.Add(pair.Key, pair.Value);
                XMLMetadataProxy = value;
            }
        }

        [XmlIgnore]
        public Dictionary<string, string> Metadata { get; set; }
    }
}
