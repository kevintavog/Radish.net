using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Rangic.Utilities.Process;

namespace Radish.Utilities
{
	public class ReadExifMetadata
	{
		private const string PropertyName = "Properties";
		private static HashSet<string> IncludeInProperties = new HashSet<string> 
		{
			"File.FileSize",
			"File.FileModifyDate",
            "File.Comment",
			"EXIF.Make",
			"EXIF.Model",
			"EXIF.ExposureTime",
			"EXIF.FNumber",
			"EXIF.CreateDate",
			"EXIF.ShutterSpeedValue",
			"EXIF.ApertureValue",
			"EXIF.ISO",
			"EXIF.FocalLength",
			"EXIF.LensModel",
			"XMP.Subject",
			"IPTC.Keywords",
			"Composite.GPSPosition",
			"Composite.ImageSize",
		};
		private static HashSet<string> ExcludedCategories = new HashSet<string> 
		{
			"ExifTool",
			"File"
		};
		private static HashSet<string> ExcludedElementsPrefix = new HashSet<string>
		{
			"ICC_Profile.ProfileDescriptionML",
		};



		static public IList<MetadataEntry> GetMetadata(string file)
		{
            var exifInvoker = new ExifToolInvoker();
			exifInvoker.Run("-a -j -g \"{0}\"", file);
			var info = JArray.Parse(exifInvoker.OutputString);

			var list = new List<MetadataEntry>();

			if (info.Count == 1)
			{
				foreach (var child in info[0].Children())
				{
					var prop = child as JProperty;
					if (prop != null)
					{
						foreach (var grandchild in child.Children())
						{
							var obj = grandchild as JObject;
							if (obj != null)
							{
								bool addedCategory = false;
								foreach (var kv in obj)
								{
									var val = ToString(kv.Value);
									if (val.StartsWith("(Binary data "))
									{
										continue;
									}

									var path = string.Format("{0}.{1}", prop.Name, kv.Key);
									if (IncludeInProperties.Contains(path))
									{
										AddProperty(list, kv);
									}

									if (ExcludedCategories.Contains(prop.Name))
									{
										continue;
									}

									bool isExcluded = false;
									foreach (var prefix in ExcludedElementsPrefix)
									{
										if (path.StartsWith(prefix, System.StringComparison.InvariantCultureIgnoreCase))
										{
											isExcluded = true;
											break;
										}
									}

									if (isExcluded)
									{
										continue;
									}

									if (!addedCategory)
									{
										list.Add(new MetadataEntry(prop.Name, null, null));
										addedCategory = true;
									}

									list.Add(new MetadataEntry(null, kv.Key, val));
								}
							}
						}
					}
				}
			}

			return list;
		}

		static private string ToString(JToken j)
		{
			if (j.Type == JTokenType.Array)
			{
				var values = new List<string>();
				foreach (var c in j.Children())
				{
					values.Add(ToString(c));
				}
				return string.Join(", ", values);
			}
			return j.ToString();
		}

		static private void AddProperty(List<MetadataEntry> list, KeyValuePair<string,JToken> kv)
		{
			if (list.Count < 1)
			{
				list.Add(new MetadataEntry(PropertyName, null, null));
			}
			else
			if (list[0].Category != PropertyName)
			{
				list.Insert(0, new MetadataEntry(PropertyName, null, null));
			}

			if (list.Count < 2)
			{
				list.Add(new MetadataEntry(null, kv.Key, ToString(kv.Value)));
			}
			else
			{
				list.Insert(1, new MetadataEntry(null, kv.Key, ToString(kv.Value)));
			}
		}
	}
}
