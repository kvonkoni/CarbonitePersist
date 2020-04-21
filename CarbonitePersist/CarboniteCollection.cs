using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CarbonitePersist
{
    public class CarboniteCollection<T>
    {
        private readonly CarboniteTool _ct;

        private readonly string _collectionPath;

        private readonly XmlSerializer serializer = new XmlSerializer(typeof(TEntity<T>));

        private List<string> CollectionManifest
        {
            get
            {
                return new List<string>(Directory.GetFiles(_collectionPath, "*.xml", SearchOption.TopDirectoryOnly));
            }
        }

        internal CarboniteCollection(CarboniteTool ct)
        {
            _ct = ct;
            _collectionPath = Path.Combine(_ct.collectionPath, typeof(T).Name);

            if (!Directory.Exists(_collectionPath))
            {
                Directory.CreateDirectory(_collectionPath);
            }
        }

        internal CarboniteCollection(CarboniteTool ct, string name)
        {
            _ct = ct;
            _collectionPath = Path.Combine(_ct.collectionPath, name);

            if (!Directory.Exists(_collectionPath))
            {
                Directory.CreateDirectory(_collectionPath);
            }
        }

        private void WriteToXml(T input)
        {
            var entity = ObjectHandler.ConvertToEntity<T>(input);
            entity.CreationDate = DateTime.Now;

            using var writer = new StreamWriter(Path.Combine(_collectionPath, $"{entity.Id}.xml"));
            serializer.Serialize(writer, entity);
        }

        private string FindFileById(object id)
        {
            return CollectionManifest.Find(x => Path.GetFileName(x).Equals($"{id}.xml"));
        }

        private void DeleteFileById(object id)
        {
            File.Delete(FindFileById(id));
        }

        private T ReadFromXml(string path)
        {
            using var stream = new FileStream(Path.Combine(_collectionPath, path), FileMode.Open);
            var entity = (TEntity<T>)serializer.Deserialize(stream);
            return entity.Entity;
        }
        
        public async Task InsertAsync(T entity, CancellationToken cancellationToken = default)
        {
            await Task.Run(() =>
            {
                if (entity == null)
                    throw new ArgumentNullException();

                if (cancellationToken.IsCancellationRequested)
                    throw new OperationCanceledException();

                WriteToXml(entity);
            }).ConfigureAwait(false);
        }

        public async Task InsertAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
        {
            if (entities == null)
                throw new ArgumentNullException();

            await Task.Run(() =>
            {
                foreach (T entity in entities)
                {
                    if (cancellationToken.IsCancellationRequested)
                        throw new OperationCanceledException();
                        
                    WriteToXml(entity);
                }
            }).ConfigureAwait(false);
        }

        public async Task<T[]> GetAllAsync(CancellationToken cancellationToken = default)
        {
        var result = new List<T>();
            await Task.Run(() => {
                foreach (string file in CollectionManifest)
                {
                    if (cancellationToken.IsCancellationRequested)
                        throw new OperationCanceledException();

                    var entity = ReadFromXml(file);
                    result.Add(entity);
                }
            }).ConfigureAwait(false);
            return result.ToArray();
        }

        public async Task<T> GetByIdAsync(object id, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
                throw new OperationCanceledException();

            return await Task.Run(() =>ReadFromXml(FindFileById(id))).ConfigureAwait(false);
        }

        public async Task<T[]> GetByIdsAsync(IEnumerable<object> ids, CancellationToken cancellationToken = default)
        {
            var results = new List<T>();
            await Task.Run(() =>
            {
                foreach (object id in ids)
                {
                    if (cancellationToken.IsCancellationRequested)
                        throw new OperationCanceledException();

                    results.Add(ReadFromXml(FindFileById(id)));
                }
            }).ConfigureAwait(false);
            return results.ToArray();
        }

        public async Task DeleteAsync(object id, CancellationToken cancellationToken = default)
        {
            await Task.Run(() =>
            {
                if (cancellationToken.IsCancellationRequested)
                    throw new OperationCanceledException();

                DeleteFileById(id);
            }).ConfigureAwait(false);
        }

        public async Task DeleteMultipleAsync(IEnumerable<object> ids, CancellationToken cancellationToken = default)
        {
            await Task.Run(() =>
            {
                foreach (object id in ids)
                {
                    if (cancellationToken.IsCancellationRequested)
                        throw new OperationCanceledException();

                    DeleteFileById(id);
                }
            }).ConfigureAwait(false);
        }

        public async Task DeleteAllAsync(CancellationToken cancellationToken = default)
        {
            await Task.Run(() =>
            {
                foreach (string file in CollectionManifest)
                {
                    if (cancellationToken.IsCancellationRequested)
                        throw new OperationCanceledException();

                    File.Delete(file);
                }
            }).ConfigureAwait(false);
        }
    }
}
