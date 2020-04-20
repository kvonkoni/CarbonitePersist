﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CarbonitePersist
{
    public class CarboniteCollection<T>
    {
        private static readonly NLog.Logger _log = NLog.LogManager.GetCurrentClassLogger();

        private readonly CarboniteTool _ct;

        private readonly string _collectionPath;

        private XmlSerializer serializer = new XmlSerializer(typeof(TEntity<T>));

        private List<string> collectionManifest
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

            using (var writer = new StreamWriter(Path.Combine(_collectionPath, $"{entity.Id}.xml")))
            {
                serializer.Serialize(writer, entity);
                _log.Info($"Inserted {entity} into Carbonite");
            }
        }

        private string FindFileById(object id)
        {
            return collectionManifest.Find(x => Path.GetFileName(x).Equals($"{id}.xml"));
        }

        private void DeleteFileById(object id)
        {
            File.Delete(FindFileById(id));
        }

        private T ReadFromXml(string path)
        {
            using (var stream = new FileStream(Path.Combine(_collectionPath, path), FileMode.Open))
            {
                var entity = (TEntity<T>)serializer.Deserialize(stream);
                return entity.Entity;
            }
        }
        
        public async Task InsertAsync(T entity, CancellationToken cancellationToken = default)
        {
            try
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
            catch (Exception e)
            {
                _log.Error(e);
                throw e;
            }
        }

        public async Task InsertAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
        {
            try
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
            catch (Exception e)
            {
                _log.Error(e);
                throw e;
            }
        }

        public async Task<T[]> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var result = new List<T>();
            try
            {
                await Task.Run(() => {
                    foreach (string file in collectionManifest)
                    {
                        if (cancellationToken.IsCancellationRequested)
                            throw new OperationCanceledException();

                        var entity = ReadFromXml(file);
                        result.Add(entity);
                    }
                }).ConfigureAwait(false);
                return result.ToArray();
            }
            catch (Exception e)
            {
                _log.Error(e);
                throw e;
            }
        }

        public async Task<T> GetByIdAsync(object id, CancellationToken cancellationToken = default)
        {
            try
            {
                return await Task.Run(() =>ReadFromXml(FindFileById(id))).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _log.Error(e);
                throw e;
            }
        }

        public async Task<T[]> GetByIdsAsync(IEnumerable<object> ids, CancellationToken cancellationToken = default)
        {
            try
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
            catch (Exception e)
            {
                _log.Error(e);
                throw e;
            }
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
                foreach (string file in collectionManifest)
                {
                    if (cancellationToken.IsCancellationRequested)
                        throw new OperationCanceledException();

                    File.Delete(file);
                }
            }).ConfigureAwait(false);
        }
    }
}
