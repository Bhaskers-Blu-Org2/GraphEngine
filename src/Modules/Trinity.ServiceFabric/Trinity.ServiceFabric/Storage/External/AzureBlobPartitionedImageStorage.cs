﻿using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Trinity.ServiceFabric.Diagnostics;

namespace Trinity.ServiceFabric.Storage.External
{
    public partial class AzureBlobPartitionedImageStorage : IPartitionedImageStorage
    {
        private string connectionString;
        private string storageContainer;
        private string storageFolder;

        public Func<Stream, ICellStreamReader> CreateCellStreamReader { get; set; } = (stream) => new CellStreamReader(stream);
        public Func<Stream, ICellStreamWriter> CreateCellStreamWriter { get; set; } = (stream) => new CellStreamWriter(stream);

        public TimeSpan RetryInterval { get; set; } = TimeSpan.FromSeconds(5);
        public int DefaultConnectionLimit { get; set; } = Environment.ProcessorCount * 8;
        public TimeSpan? DefaultRequestMaxExecutionTime { get; set; } = null;

        public AzureBlobPartitionedImageStorage(string connectionString, string storageContainer, string folder)
        {
            this.connectionString = connectionString;
            this.storageContainer = storageContainer;
            this.storageFolder = folder;
        }

        public ImagePartitionSignature LoadPartitionSignature(int partition)
        {
            var bytes = DownloadBlockBlob(Path.Combine(storageFolder, $"{partition}.sig"));
            if (bytes == null)
                return new ImagePartitionSignature { PartitionId = partition };

            var json = Encoding.UTF8.GetString(bytes);
            return JsonConvert.DeserializeObject<ImagePartitionSignature>(json);
        }

        public async Task SavePartitionSignatureAsync(ImagePartitionSignature signature)
        {
            while (true)
            {
                try
                {
                    var json = JsonConvert.SerializeObject(signature);
                    await UploadBlockBlobAsync(Path.Combine(storageFolder, $"{signature.PartitionId}.sig"), Encoding.UTF8.GetBytes(json));
                    return;
                }
                catch (Exception e)
                {
                    Log.Error($"Failed to save siganature for partition#{signature.PartitionId}. Retry {RetryInterval.TotalSeconds} seconds later: {e}");
                    await Task.Delay(RetryInterval);
                }
            }
        }

        public async Task<string> LoadImagePartitionAsync(int partition)
        {
            var blob = Container.GetBlockBlobReference(Path.Combine(storageFolder, $"{partition}.image"));

            using (var reader = CreateCellStreamReader(blob.OpenRead()))
            {
                var cell = await reader.ReadCellAsync();
                long cellId = cell.Item1;
                while (cellId != 0)
                {
                    if (Global.LocalStorage.SaveCell(cellId, cell.Item3, cell.Item2) != TrinityErrorCode.E_SUCCESS)
                        throw new Exception("Failed to SaveCell");

                    cell = await reader.ReadCellAsync();
                    cellId = cell.Item1;
                }
            }

            Log.Info($"Image partition#{partition} loaded");

            return blob.Properties.ContentMD5;
        }

        public async Task<string> SaveImagePartitionAsync(int partition)
        {
            while (true)
            {
                try
                {
                    Container.CreateIfNotExists();
                    var blob = Container.GetBlockBlobReference(Path.Combine(storageFolder, $"{partition}.image"));

                    var cellIds = Global.LocalStorage.GenericCellAccessor_Selector()
                        .Where(c => Global.CloudStorage.GetPartitionIdByCellId(c.CellID) == partition)
                        .Select(c => c.CellID).ToList();

                    using (var writer = CreateCellStreamWriter(blob.OpenWrite()))
                    {
                        foreach (var id in cellIds)
                        {
                            byte[] bytes;
                            ushort cellType;

                            Global.LocalStorage.LoadCell(id, out bytes, out cellType);
                            await writer.WriteCellAsync(id, cellType, bytes);
                        }
                    }

                    Log.Info($"Image partition#{partition} saved");

                    return blob.Properties.ContentMD5;
                }
                catch (Exception e)
                {
                    Log.Error($"Failed to save image partition#{partition}. Retry after {RetryInterval.TotalSeconds} seconds.: {e}");
                    await Task.Delay(RetryInterval);
                }
            }
        }
    }

    public partial class AzureBlobPartitionedImageStorage
    {
        private CloudBlobClient blobClient;
        private CloudBlobClient BlobClient => UseBlobClient();
        private CloudBlobContainer Container => BlobClient.GetContainerReference(storageContainer);

        private CloudBlobClient UseBlobClient()
        {
            if (blobClient == null)
            {
                var storageAccount = CloudStorageAccount.Parse(connectionString);
                blobClient = storageAccount.CreateCloudBlobClient();

                ServicePointManager.DefaultConnectionLimit = DefaultConnectionLimit;
                blobClient.DefaultRequestOptions.MaximumExecutionTime = DefaultRequestMaxExecutionTime;
            }
            return blobClient;
        }

        private byte[] DownloadBlockBlob(string blobName)
        {
            //if (!Container.Exists())
            //    return null;

            var blob = Container.GetBlockBlobReference(blobName);
            if (!blob.Exists())
                return null;

            using (var ms = new MemoryStream())
            {
                blob.DownloadToStream(ms);
                return ms.ToArray();
            }
        }

        private async Task UploadBlockBlobAsync(string blobName, byte[] bytes)
        {
            Container.CreateIfNotExists();

            var blob = Container.GetBlockBlobReference(blobName);
            await blob.UploadFromByteArrayAsync(bytes, 0, bytes.Length);
        }
    }
}
