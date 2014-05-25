using System;
using MonoMac.Foundation;

namespace Radish
{
	public class MetadataEntry : NSObject
	{
		public string Category { get; private set; }
		public string Name { get; private set; }
		public string Value { get; private set; }

		public MetadataEntry(string category, string name, string value)
		{
			Category = category;
			Name = name;
			Value = value;
		}
	}
}

