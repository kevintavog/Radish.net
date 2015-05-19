using System;
using System.Collections.Generic;
using System.Net.Http;
using System.IO;
using System.Web;
using Rangic.Utilities.Geo;

namespace Radish.Models
{
    public class FindAPhotoMetadata : MediaMetadata
    {
        protected override IList<MetadataEntry> GetAllMetadata()
        {
            var path = HttpUtility.UrlDecode(new Uri(FullPath).PathAndQuery);
            var parentFolder = Path.GetDirectoryName(LastIndexOf(path, "/", 4));

            var list = new List<MetadataEntry>
            {
                new MetadataEntry("Properties", null, null),
                new MetadataEntry(null, "Name", Name),
                new MetadataEntry(null, "Create Date", Timestamp.ToString()),
                new MetadataEntry(null, "Parent Folder", parentFolder),
            };

            if (!String.IsNullOrWhiteSpace(Keywords))
            {
                list.Add(new MetadataEntry(null, "Keywords", Keywords));
            }
            if (Location != null)
            {
                list.Add(new MetadataEntry(null, "Location", Location.ToDms()));
            }
            return list;
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

        public FindAPhotoMetadata(string fullPath, DateTime timestamp, Location location, string[] keywords, string thumbUrl)
        {
            FullPath = fullPath;
            Name = Path.GetFileName("/" + HttpUtility.UrlDecode(FullPath));
            Timestamp = timestamp;
            Location = location;
            FileAndExifTimestampMatch = true;
            HasThumbnailPath = true;
            ThumbnailPath = thumbUrl;

            Keywords = String.Join(", ", keywords);
        }

        private string LastIndexOf(string str, string search, int count)
        {
            int pos = str.Length;
            while (count > 0)
            {
                var newPos = str.LastIndexOf(search, pos - 1);
                if (newPos < 0)
                {
                    break;
                }

                pos = newPos;
                --count;
            }

            return str.Substring(pos + 1);
        }
    }
}
