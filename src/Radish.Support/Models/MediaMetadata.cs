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

            char latNS = Location.Latitude < 0 ? 'S' : 'N';
            char longEW = Location.Longitude < 0 ? 'W' : 'E';
            return String.Format("{0} {1}, {2} {3}", ToDms(Location.Latitude), latNS, ToDms(Location.Longitude), longEW);
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


        private string ToDms(double l)
        {
            if (l < 0)
            {
                l *= -1f;
            }
            var degrees = Math.Truncate(l);
            var minutes = (l - degrees) * 60f;
            var seconds = (minutes - (int) minutes) * 60;
            minutes = Math.Truncate(minutes);
            return String.Format("{0:00}° {1:00}' {2:00}\"", degrees, minutes, seconds);
        }

    }
}

