﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CarbonitePersist
{
    public class CarboniteStorage
    {
        private static readonly NLog.Logger _log = NLog.LogManager.GetCurrentClassLogger();

        private readonly CarboniteTool _ct;

        private readonly XmlSerializer serializer = new XmlSerializer(typeof(FileStorageMetadata));

        private List<string> storageManifest
        {
            get
            {
                return new List<string>(Directory.GetFiles(_ct.storagePath, "*.bin", SearchOption.TopDirectoryOnly));
            }
        }

        private List<string> storageMetadataManifest
        {
            get
            {
                return new List<string>(Directory.GetFiles(_ct.storageMetadataPath, "*.xml", SearchOption.TopDirectoryOnly));
            }
        }

        internal CarboniteStorage(CarboniteTool ct)
        {
            _ct = ct;
        }

        private void CopyFileToStorage(object id, string source)
        {
            var metadata = new FileStorageMetadata
            {
                Id = id,
                Filename = Path.GetFileName(source)
            };
            File.Copy(source, Path.Combine(_ct.storagePath, $"{id}.bin"), true);
            _log.Info($"Copied {source} into Carbonite");
            WriteMetadataToXml(metadata);
        }

        private void WriteMetadataToXml(FileStorageMetadata metadata)
        {
            using (var writer = new StreamWriter(Path.Combine(_ct.storageMetadataPath, $"{metadata.Id}.xml")))
            {
                serializer.Serialize(writer, metadata);
                _log.Info($"Inserted {metadata} into Carbonite");
            }
        }

        private FileStorageMetadata ReadMetadataFromXml(string path)
        {
            using (var stream = new FileStream(Path.Combine(_ct.storageMetadataPath, path), FileMode.Open))
            {
                return (FileStorageMetadata)serializer.Deserialize(stream);
            }
        }

        private void RetrieveFileFromStorage(string source, string destination, bool overwrite = false)
        {
            File.Copy(source, destination, overwrite);
        }

        private string FindFileById(object id)
        {
            return storageManifest.Find(x => Path.GetFileName(x).Equals($"{id}.bin"));
        }

        private string FindMetadataFromId(object id)
        {
            return storageMetadataManifest.Find(x => Path.GetFileName(x).Equals($"{id}.xml"));
        }

        private void DeleteFileInStorageById(object id)
        {
            File.Delete(FindMetadataFromId(id));
            File.Delete(FindFileById(id));
        }

        public async Task UploadAsync(object id, string source, CancellationToken cancellationToken = default)
        {
            try
            {
                await Task.Run(() =>
                {
                    if (id == null || source == null)
                        throw new ArgumentNullException("Must provide an id and a source");

                    if (cancellationToken.IsCancellationRequested)
                        throw new OperationCanceledException();

                    CopyFileToStorage(id, source);
                }).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _log.Error(e);
                throw e;
            }
        }

        public async Task UploadAsync(List<object> ids, List<string> sources, CancellationToken cancellationToken = default)
        {
            try
            {
                if (ids == null || sources == null)
                    throw new ArgumentNullException("Must provide ids and sources");

                if (!(ids.Count == sources.Count && ids.Count > 0))
                    throw new InvalidDataException("Ids and sources must have matching count");

                await Task.Run(() =>
                {
                    for (int i = 0; i < sources.Count; i++)
                    {
                        if (cancellationToken.IsCancellationRequested)
                            throw new OperationCanceledException();

                        CopyFileToStorage(ids[i], sources[i]);
                    }
                }).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _log.Error(e);
                throw e;
            }
        }

        public async Task<List<FileStorageMetadata>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var result = new List<FileStorageMetadata>();
            try
            {
                await Task.Run(() => {
                    foreach (string file in storageMetadataManifest)
                    {
                        if (cancellationToken.IsCancellationRequested)
                            throw new OperationCanceledException();
                        
                        var metadata = ReadMetadataFromXml(file);
                        result.Add(metadata);
                    }
                }).ConfigureAwait(false);
                return result;
            }
            catch (Exception e)
            {
                _log.Error(e);
                throw e;
            }
        }

        public async Task<FileStorageMetadata> GetByIdAsync(object id, CancellationToken cancellationToken = default)
        {
            try
            {
                return await Task.Run(() => ReadMetadataFromXml(FindMetadataFromId(id))).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _log.Error(e);
                throw e;
            }
        }

        public async Task<List<FileStorageMetadata>> GetByIdsAsync(List<object> ids, CancellationToken cancellationToken = default)
        {
            try
            {
                var results = new List<FileStorageMetadata>();
                await Task.Run(() =>
                {
                    foreach (object id in ids)
                    {
                        if (cancellationToken.IsCancellationRequested)
                            throw new OperationCanceledException();

                        results.Add(ReadMetadataFromXml(FindMetadataFromId(id)));
                    }
                }).ConfigureAwait(false);
                return results;
            }
            catch (Exception e)
            {
                _log.Error(e);
                throw e;
            }
        }

        public async Task DownloadAsync(object id, string destination, bool overwrite = false)
        {
            await Task.Run(() => RetrieveFileFromStorage(FindFileById(id), destination, overwrite)).ConfigureAwait(false);
        }

        public async Task DownloadAsync(List<object> ids, List<string> destinations, bool overwrite = false)
        {
            if (ids == null || destinations == null)
                throw new ArgumentNullException("Must provide ids and destinations");

            if (!(ids.Count == destinations.Count && ids.Count > 0))
                throw new InvalidDataException("Ids and destinations must have matching count");

            await Task.Run(() => {
                for (int i = 0; i < destinations.Count; i++)
                    RetrieveFileFromStorage(FindFileById(ids[i]), destinations[i], overwrite);
                }).ConfigureAwait(false);
        }

        public async Task DeleteAsync(object id, CancellationToken cancellationToken = default)
        {
            await Task.Run(() => DeleteFileInStorageById(id)).ConfigureAwait(false);
        }

        public async Task DeleteMultipleAsync(List<object> ids, CancellationToken cancellationToken = default)
        {
            await Task.Run(() => {
                for (int i = 0; i < ids.Count; i++)
                {
                    if (cancellationToken.IsCancellationRequested)
                        throw new OperationCanceledException();

                    DeleteFileInStorageById(ids[i]);
                }
            }).ConfigureAwait(false);
        }
    }
}
