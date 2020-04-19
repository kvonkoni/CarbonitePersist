using System;
using System.Data.Common;

namespace CarbonitePersist
{
    public class CarboniteConnectionStringBuilder
    {
        private string path = null;

        public string Path { get => path; }

        public string ConnectionString {
            private get
            {
                return ConnectionString;
            }
            set
            {
                var builder = new DbConnectionStringBuilder();
                builder.ConnectionString = value;

                RetrieveValue(builder, "Path", ref path);

                if (path == null)
                    throw new ArgumentNullException("Storage path required");
            }
        }

        private void RetrieveValue<T>(DbConnectionStringBuilder builder, string key, ref T value)
        {
            try
            {
                if (builder.TryGetValue(key, out object objectValue))
                {
                    Type t = Nullable.GetUnderlyingType(typeof(T));
                    if (t == null)
                        value = (T)Convert.ChangeType(objectValue, typeof(T));
                    else
                    {
                        value = (T)Convert.ChangeType(objectValue, t);
                    }
                }
            }
            catch (ArgumentNullException) { }
        }

        public CarboniteConnectionStringBuilder(string connectionString)
        {
            ConnectionString = connectionString;
        }
    }
}
