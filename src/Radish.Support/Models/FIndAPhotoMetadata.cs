using System;
using System.Collections.Generic;
using System.Net.Http;
using System.IO;
using System.Web;

namespace Radish.Models
{
    public class FindAPhotoMetadata : MediaMetadata
    {
        protected override IList<MetadataEntry> GetAllMetadata()
        {
            return new List<MetadataEntry>();
        }

        override public byte[] GetData()
        {
            using (var client = new HttpClient())
            {
                using (var stream = client.GetStreamAsync(new Uri(FullPath)).Result)
                {
                    using (var mem = new MemoryStream(1 * 1024 * 1024))
                    {
                        stream.CopyTo(mem);
                        return mem.ToArray();
                    }
                }
            }
        }

        public FindAPhotoMetadata(string fullPath, DateTime timestamp, Location location)
        {
            FullPath = fullPath;
            Name = Path.GetFileName("/" + HttpUtility.UrlDecode(FullPath));
            Timestamp = timestamp;
            Location = location;
            FileAndExifTimestampMatch = true;
        }
    }
}

