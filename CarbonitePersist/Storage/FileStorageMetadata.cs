using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace CarbonitePersist.Storage
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
        
        public DateTime UploadDate { get; set; }
        
        public List<SerializableKeyValuePair<string, string>> XMLMetadataProxy { get; set; }

        [XmlIgnore]
        public Dictionary<string, string> Metadata
        {
            get
            {
                var dictionary = new Dictionary<string, string>();
                foreach (SerializableKeyValuePair<string, string> pair in XMLMetadataProxy)
                    dictionary.Add(pair.Key, pair.Value);
                return dictionary;
            }
            set
            {
                var xmlMetadataProxy = new List<SerializableKeyValuePair<string, string>>();
                foreach (KeyValuePair<string, string> pair in value)
                    xmlMetadataProxy.Add(new SerializableKeyValuePair<string, string> { 
                        Key = pair.Key,
                        Value = pair.Value,
                    });
                XMLMetadataProxy = xmlMetadataProxy;
            }
        }
    }
}
