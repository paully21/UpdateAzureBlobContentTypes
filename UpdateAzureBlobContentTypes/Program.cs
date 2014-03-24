using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UpdateAzureBlobContentTypes
{
    class Program
    {
        static CloudStorageAccount storageAccount;
        static CloudBlobClient blobStorage;
        static string[] accessibleContainers;
        static Dictionary<string, string> contentTypes;
        static Dictionary<string, string> ContentTypes
        {
            get
            {
                if (contentTypes == null)
                {
                    contentTypes = new Dictionary<string, string>();
                    contentTypes.Add("pdf", "application/pdf");
                    contentTypes.Add("doc", "application/msword");
                    contentTypes.Add("xls", "application/vnd.ms-excel");
                    contentTypes.Add("docx", "application/msword");
                    contentTypes.Add("xlsx", "application/vnd.ms-excel");
                    contentTypes.Add("mp3", "audio/mpeg");
                    contentTypes.Add("mp4", "video/mp4");
                    contentTypes.Add("jpg", "image/jpeg");
                    contentTypes.Add("jpeg", "image/jpeg");
                    contentTypes.Add("gif", "image/gif");
                    contentTypes.Add("png", "image/png");
                    contentTypes.Add("zip", "application/zip");
                }
                return contentTypes;
            }
        }

        static void Main(string[] args)
        {
            storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["StorageConnection"].ConnectionString);
            blobStorage = storageAccount.CreateCloudBlobClient();
            accessibleContainers = ConfigurationManager.AppSettings["Containers"].Split(new char[] { '|' });
            var containers = blobStorage.ListContainers().Where(con => accessibleContainers.Contains(con.Name));
            foreach (var container in containers)
            {
                foreach (IListBlobItem blobRef in container.ListBlobs(useFlatBlobListing: true))
                {
                    CloudBlockBlob blob = container.GetBlockBlobReference(blobRef.Uri.ToString());
                    blob.FetchAttributes();
                    if (!ContentTypes.Values.Contains(blob.Properties.ContentType))
                    {
                        blob.Properties.ContentType = GetContentType(GetExtension(blobRef.Uri.ToString()));                    
                        blob.SetProperties();
                    }
                }
            }
        }

        static string GetExtension(string url)
        {
            int position = url.LastIndexOf(".");
            if (position >= 0)
                return url.Substring(position + 1, url.Length - position - 1);
            return string.Empty;
        }

        static string GetContentType(string extension)
        {
            if (ContentTypes.ContainsKey(extension))
                return ContentTypes[extension];
            return "application/octet-stream";
        }
    }
}
