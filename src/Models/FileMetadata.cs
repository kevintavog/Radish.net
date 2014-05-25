using System;
using ExifLib;
using NLog;
using System.Collections.Generic;
using Radish.Utilities;
using System.Linq;
using System.IO;

namespace Radish.Models
{
	public class FileMetadata
	{
		static private readonly Logger logger = LogManager.GetCurrentClassLogger();

		public string FullPath { get; private set; }
		public DateTime Timestamp { get; private set; }
		public Location Location { get; private set; }
		public bool FileAndExifTimestampMatch { get; private set; }

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

		private IList<MetadataEntry> metadata;

		public FileMetadata(string fullPath)
		{
			FullPath = fullPath;
			GetDetails();

			var fileTimestamp = new FileInfo(FullPath).CreationTime;

			// Some filesystems, such as FAT, don't have reasonable granularity. If it's close, it's good enough
			FileAndExifTimestampMatch = Math.Abs((Timestamp - fileTimestamp).TotalSeconds) < 2;
		}

		private void GetDetails()
		{
			Timestamp = new FileInfo(FullPath).CreationTime;
			try
			{
				using (var exif = new ExifLib.ExifReader(FullPath))
				{
					DateTime dt;
					exif.GetTagValue<DateTime>(ExifTags.DateTimeDigitized, out dt);
					if (dt == DateTime.MinValue)
					{
						exif.GetTagValue<DateTime>(ExifTags.DateTimeOriginal, out dt);
					}
					if (dt == DateTime.MinValue)
					{
						exif.GetTagValue<DateTime>(ExifTags.DateTime, out dt);
					}
					if (dt != DateTime.MinValue)
					{
						Timestamp = dt;
					}

					string latRef, longRef;
					double[] latitude, longitude;
					exif.GetTagValue<string>(ExifTags.GPSLatitudeRef, out latRef);
					exif.GetTagValue<string>(ExifTags.GPSLongitudeRef, out longRef);
					exif.GetTagValue<double[]>(ExifTags.GPSLatitude, out latitude);
					exif.GetTagValue<double[]>(ExifTags.GPSLongitude, out longitude);

					if (latRef != null && longRef != null)
					{
						Location = new Location(
							ConvertLocation(latRef, latitude),
							ConvertLocation(longRef, longitude));
					}
				}
			}
			catch (ExifLib.ExifLibException)
			{
				// Eat it, this file isn't supported
			}
			catch (Exception ex)
			{
				logger.Info("Exception getting location: {0}", ex);
			}
		}

		private IList<MetadataEntry> GetAllMetadata()
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

		static private double ConvertLocation(string geoRef, double[] val)
		{
			var v = val[0] + val[1] / 60 + val[2] / 3600;
			if (geoRef == "S" || geoRef == "W")
			{
				v *= -1;
			}
			return v;
		}
	}
}
