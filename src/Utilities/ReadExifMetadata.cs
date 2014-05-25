using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using NLog;

namespace Radish.Utilities
{
	public class ReadExifMetadata
	{
		static public IList<MetadataEntry> GetMetadata(string file)
		{
			var exifOutput = ExifToolInvoker.Run("-a -j -g \"{0}\"", file);
			var info = JArray.Parse(exifOutput.OutputString);

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
									if (!addedCategory)
									{
										list.Add(new MetadataEntry(prop.Name, null, null));
										addedCategory = true;
									}

									list.Add(new MetadataEntry(null, kv.Key, kv.Value.ToString()));
								}
							}
						}
					}
				}
			}

			return list;
		}
	}
}
