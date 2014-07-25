using System;
using System.Collections.Generic;

namespace Radish.Models
{
    abstract public class MediaMetadata
    {
        public string Name { get; protected set; }
        public string FullPath { get; protected set; }
        public DateTime Timestamp { get; protected set; }
        public Location Location { get; protected set; }
        public bool FileAndExifTimestampMatch { get; protected set; }

        public string ToDms()
        {
            if (Location == null)
            {
                return "";
            }

            return Location.ToDms();
        }

        private IList<MetadataEntry> metadata;
        public IList<MetadataEntry> Metadata 
        {
            get
            {
                if (metadata == null)
                {
                    metadata = GetAllMetadata();
                }
                return metadata;
            }
        }

        protected void ClearMetadata()
        {
            metadata = null;
        }

        abstract protected IList<MetadataEntry> GetAllMetadata();
        abstract public byte[] GetData();

    }
}

