using CarbonitePersist.Entity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CarbonitePersist.Storage
{
    public class CarboniteStorage
    {
        private readonly CarboniteTool _ct;

        private readonly XmlSerializer serializer = new XmlSerializer(typeof(FileStorageMetadata));

        private List<StorageEntity> StorageManifest
        {
            get
            {
                var storageEntityList = new List<StorageEntity>();
                foreach (string file in Directory.GetFiles(_ct.storagePath, "*.bin", SearchOption.TopDirectoryOnly))
                {
                    storageEntityList.Add(new StorageEntity(file));
                }
                return storageEntityList;
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
                Filename = Path.GetFileName(source),
                UploadDate = DateTime.Now,
            };
            File.Copy(source, Path.Combine(_ct.storagePath, $"{id}.bin"), true);
            WriteMetadataToXml(metadata);
        }

        private FileStream OpenFileStreamInStorage(object id, string filename)
        {
            var metadata = new FileStorageMetadata
            {
                Id = id,
                Filename = filename,
                UploadDate = DateTime.Now,
            };
            WriteMetadataToXml(metadata);
            return new FileStream(Path.Combine(_ct.storagePath, $"{id}.bin"), FileMode.Create, FileAccess.Write);
        }

        private void WriteMetadataToXml(FileStorageMetadata metadata)
        {
            using var writer = new StreamWriter(Path.Combine(_ct.storagePath, $"{metadata.Id}.xml"));
            serializer.Serialize(writer, metadata);
        }

        private FileStorageMetadata ReadMetadataFromXml(StorageEntity file)
        {
            using var stream = new FileStream(file.Metapath, FileMode.Open, FileAccess.Read, FileShare.Read);
            return (FileStorageMetadata)serializer.Deserialize(stream);
        }

        private void RetrieveFileFromStorage(StorageEntity file, string destination, bool overwrite = false)
        {
            File.Copy(file.Filepath, destination, overwrite);
        }

        public FileStream RetrieveFileStreamFromStorage(StorageEntity file)
        {
            return File.Open(file.Filepath, FileMode.Open, FileAccess.Read, FileShare.Read);
        }

        private StorageEntity FindFileById(object id)
        {
            return StorageManifest.Find(x => x.Id.Equals(id.ToString()));
        }

        private IReadOnlyList<StorageEntity> FindAllByPredicate(Predicate<StorageEntity> predicate)
        {
            return StorageManifest.FindAll(predicate);
        }

        private void DeleteFileInStorageById(object id)
        {
            var file = FindFileById(id);
            File.Delete(file.Filepath);
            if (file.Metapath != null)
                File.Delete(file.Metapath);
        }

        public async Task UploadAsync(object id, string source, CancellationToken cancellationToken = default)
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

        public async Task UploadAsync(IReadOnlyList<object> ids, IReadOnlyList<string> sources, CancellationToken cancellationToken = default)
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

        public async Task UploadAsync(object id, string filename, Stream stream, CancellationToken cancellationToken = default)
        {
            using (FileStream destinationStream = OpenFileStreamInStorage(id, filename))
            {
                await stream.CopyToAsync(destinationStream, cancellationToken).ConfigureAwait(false);
            }
        }

        public async Task UploadAsync(IReadOnlyList<object> ids, IReadOnlyList<string> filenames, IReadOnlyList<Stream> streams, CancellationToken cancellationToken = default)
        {
            var list = new List<Task>();
            for (int i = 0; i < ids.Count; i++)
            {
                list.Add(UploadAsync(ids[i], filenames[i], streams[i], cancellationToken));
            }
            await Task.WhenAll(list).ConfigureAwait(false);
        }

        public async Task<FileStream> OpenWriteStream(object id, string filename, CancellationToken cancellationToken = default)
        {
            return await Task.Run(() => OpenFileStreamInStorage(id, filename), cancellationToken).ConfigureAwait(false);
        }

        public async Task<IReadOnlyList<FileStream>> OpenWriteStreams(IReadOnlyList<object> ids, IReadOnlyList<string> filenames, CancellationToken cancellationToken = default)
        {
            var list = new List<Task<FileStream>>();
            for (int i = 0; i < ids.Count; i++)
                list.Add(OpenWriteStream(ids[i], filenames[i], cancellationToken));
            return await Task.WhenAll(list).ConfigureAwait(false);
        }

        public async Task<FileStorageMetadata[]> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var result = new List<FileStorageMetadata>();
            await Task.Run(() => {
                foreach (StorageEntity file in StorageManifest)
                {
                    if (cancellationToken.IsCancellationRequested)
                        throw new OperationCanceledException();
                        
                    var metadata = ReadMetadataFromXml(file);
                    result.Add(metadata);
                }
            }).ConfigureAwait(false);
            return result.ToArray();
        }

        public async Task<FileStorageMetadata[]> FindAllAsync(Predicate<StorageEntity> predicate, CancellationToken cancellationToken = default)
        {
            var result = new List<FileStorageMetadata>();
            await Task.Run(() => {
                foreach (StorageEntity file in FindAllByPredicate(predicate))
                {
                    if (cancellationToken.IsCancellationRequested)
                        throw new OperationCanceledException();

                    var metadata = ReadMetadataFromXml(file);
                    result.Add(metadata);
                }
            }).ConfigureAwait(false);
            return result.ToArray();
        }

        public async Task<FileStorageMetadata> GetByIdAsync(object id, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
                throw new OperationCanceledException();

            return await Task.Run(() => ReadMetadataFromXml(FindFileById(id))).ConfigureAwait(false);
        }

        public async Task<FileStorageMetadata[]> GetByIdsAsync(IReadOnlyList<object> ids, CancellationToken cancellationToken = default)
        {
            var results = new List<FileStorageMetadata>();
            await Task.Run(() =>
            {
                foreach (object id in ids)
                {
                    if (cancellationToken.IsCancellationRequested)
                        throw new OperationCanceledException();

                    results.Add(ReadMetadataFromXml(FindFileById(id)));
                }
            }).ConfigureAwait(false);
            return results.ToArray();
        }

        public async Task DownloadAsync(object id, string destination, bool overwrite, CancellationToken cancellationToken = default)
        {
            await Task.Run(() => {
                if (cancellationToken.IsCancellationRequested)
                    throw new OperationCanceledException();

                RetrieveFileFromStorage(FindFileById(id), destination, overwrite);
            }).ConfigureAwait(false);
        }

        public async Task DownloadAsync(object id, string destination, CancellationToken cancellationToken = default)
        {
            await DownloadAsync(id, destination, false, cancellationToken).ConfigureAwait(false);
        }

        public async Task DownloadAsync(IReadOnlyList<object> ids, IReadOnlyList<string> destinations, bool overwrite, CancellationToken cancellationToken = default)
        {
            if (ids == null || destinations == null)
                throw new ArgumentNullException("Must provide ids and destinations");

            if (!(ids.Count == destinations.Count && ids.Count > 0))
                throw new InvalidDataException("Ids and destinations must have matching count");

            await Task.Run(() => {
                if (cancellationToken.IsCancellationRequested)
                    throw new OperationCanceledException();

                for (int i = 0; i < destinations.Count; i++)
                    RetrieveFileFromStorage(FindFileById(ids[i]), destinations[i], overwrite);
                }).ConfigureAwait(false);
        }

        public async Task DownloadAsync(IReadOnlyList<object> ids, IReadOnlyList<string> destinations, CancellationToken cancellationToken = default)
        {
            await DownloadAsync(ids, destinations, false, cancellationToken).ConfigureAwait(false);
        }

        public async Task CopyFileToStreamAsync(object id, Stream stream, CancellationToken cancellationToken = default)
        {
            using (FileStream sourceStream = RetrieveFileStreamFromStorage(FindFileById(id)))
            {
                await sourceStream.CopyToAsync(stream, cancellationToken).ConfigureAwait(false);
            }
        }

        public async Task CopyFileToStreamAsync(IReadOnlyList<object> ids, IReadOnlyList<Stream> streams, CancellationToken cancellationToken = default)
        {
            var list = new List<Task>();
            for (int i = 0; i < ids.Count; i++)
                list.Add(CopyFileToStreamAsync(ids[i], streams[i], cancellationToken));
            await Task.WhenAll(list).ConfigureAwait(false);
        }

        public async Task<FileStream> OpenReadStreamAsync(object id, CancellationToken cancellationToken = default)
        {
            return await Task.Run(() => RetrieveFileStreamFromStorage(FindFileById(id)), cancellationToken).ConfigureAwait(false);
        }

        public async Task<IReadOnlyList<FileStream>> OpenReadStreamsAsync(IReadOnlyList<object> ids, CancellationToken cancellationToken = default)
        {
            var list = new List<Task<FileStream>>();
            foreach (object id in ids)
                list.Add(OpenReadStreamAsync(id, cancellationToken));
            return await Task.WhenAll(list).ConfigureAwait(false);
        }

        public async Task SetFilename(object id, string filename, CancellationToken cancellationToken = default)
        {
            await Task.Run(() =>
            {
                if (cancellationToken.IsCancellationRequested)
                    throw new OperationCanceledException();

                var file = FindFileById(id);
                if (file != null)
                {
                    var meta = ReadMetadataFromXml(file);
                    meta.Filename = filename;
                    WriteMetadataToXml(meta);
                }
            }).ConfigureAwait(false);
        }

        public async Task SetMetadata(object id, Dictionary<string, string> metadata, CancellationToken cancellationToken = default)
        {
            await Task.Run(() =>
            {
                if (cancellationToken.IsCancellationRequested)
                    throw new OperationCanceledException();

                var file = FindFileById(id);
                if (file != null)
                {
                    var metafile = ReadMetadataFromXml(file);
                    metafile.Metadata = metadata;
                    WriteMetadataToXml(metafile);
                }
            }).ConfigureAwait(false);
        }

        public async Task DeleteAsync(object id, CancellationToken cancellationToken = default)
        {
            await Task.Run(() => {
                if (cancellationToken.IsCancellationRequested)
                    throw new OperationCanceledException();

                DeleteFileInStorageById(id);
            }).ConfigureAwait(false);
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

        public async Task DeleteAllAsync(CancellationToken cancellationToken = default)
        {
            await Task.Run(() => {
                foreach (StorageEntity file in StorageManifest)
                {
                    if (cancellationToken.IsCancellationRequested)
                        throw new OperationCanceledException();
                    
                    DeleteFileInStorageById(file.Id);
                }
            }).ConfigureAwait(false);
        }
    }
}
