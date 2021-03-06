﻿using System;
using System.Collections.Generic;
using Rangic.Utilities.Geo;

namespace Radish.Models
{
    abstract public class MediaMetadata
    {
        public string Name { get; protected set; }
        public string FullPath { get; protected set; }
        public DateTime Timestamp { get; protected set; }
        public Location Location { get; protected set; }
        public bool FileAndExifTimestampMatch { get; protected set; }
        public virtual string Keywords { get; protected set; }
        public virtual string ThumbnailPath { get; protected set; }

        public virtual bool HasThumbnailPath { get; protected set; }

        public bool HasPlaceName { get { return placeName != null; } }
        private string placeName;
        public string ToPlaceName()
        {
            if (placeName == null)
            {
                placeName = Location.PlaceName(Location.PlaceNameFilter.Standard);
            }

            return placeName;
        }

        public string ToDms()
        {
            return Location == null ? "" : Location.ToDms();
        }

        private IList<MetadataEntry> metadata;
        public IList<MetadataEntry> Metadata 
        {
            get
            {
                if (metadata == null)
                {
                    metadata = GetAllMetadata();

                    if (Location != null)
                    {
                        int index = 0;
                        metadata.Insert(index++, new MetadataEntry("PlaceName", null, null));
                        var pnc = Location.PlaceNameComponents;
                        foreach (var key in pnc.Keys)
                        {
                            metadata.Insert(index++, new MetadataEntry(null, (string) key, (string) pnc[key]));
                        }
                    }
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
