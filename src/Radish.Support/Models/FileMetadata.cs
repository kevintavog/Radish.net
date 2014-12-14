using System;
using NLog;
using System.Collections.Generic;
using Radish.Utilities;
using System.Linq;
using System.IO;
using Rangic.Utilities.Image;
using ExifLib;

namespace Radish.Models
{
    public class FileMetadata : MediaMetadata
	{
		static private readonly Logger logger = LogManager.GetCurrentClassLogger();

        public override string Keywords { get { return String.Join(", ", imageDetails.Keywords); } }

        public void SetFileDateToExifDate()
		{
			if (!FileAndExifTimestampMatch)
			{
				File.SetCreationTime(FullPath, Timestamp);
				File.SetLastWriteTime(FullPath, Timestamp);
				UpdateTimestampMatch();

                ClearMetadata();
			}
		}

        private ImageDetails imageDetails;

		public FileMetadata(string fullPath)
		{
			FullPath = fullPath;
            Name = Path.GetFileName(FullPath);
            imageDetails = new ImageDetails(FullPath);
            Timestamp = imageDetails.CreatedTime;
            Location = imageDetails.Location;
			UpdateTimestampMatch();
		}

		private void UpdateTimestampMatch()
		{
			// Some filesystems, such as FAT, don't have reasonable granularity. If it's close, it's good enough
			FileAndExifTimestampMatch = Math.Abs((Timestamp - new FileInfo(FullPath).CreationTime).TotalSeconds) < 2;
		}

        override protected IList<MetadataEntry> GetAllMetadata()
		{
			try
			{
				return ReadExifMetadata.GetMetadata(FullPath);
			}
			catch (Exception e)
			{
				logger.Info("Exception getting metadata: {0} ({1})", FullPath, e);
				return new List<MetadataEntry>();
			}
		}

        override public byte[] GetData()
        {
            return File.ReadAllBytes(FullPath);
        }
	}
}
