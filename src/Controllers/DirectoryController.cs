using System;
using System.Collections.Generic;
using Radish.Models;
using System.IO;
using MonoMac.ImageIO;
using MonoMac.Foundation;
using MonoMac.AppKit;
using System.Collections;

namespace Radish.Controllers
{
	public class DirectoryController
	{
		static private HashSet<string> ImageTypes = new HashSet<string>(CGImageSource.TypeIdentifiers);

		private IList<FileMetadata> FileList { get; set; }


		public int CurrentIndex { get; private set; }
		public FileSort Sort { get; set; }

		public int Count { get { return FileList.Count; } }
		public FileMetadata Current { get { return FileList[CurrentIndex]; } }

		public DirectoryController()
		{
			FileList = new List<FileMetadata>();
		}

		public bool SelectFile(string filename)
		{
			if (String.IsNullOrWhiteSpace(filename))
			{
				return false;
			}

			for (int idx = 0; idx < FileList.Count; ++idx)
			{
				if (FileList[idx].FullPath.Equals(filename, StringComparison.CurrentCultureIgnoreCase))
				{
					SetIndex(idx);
					return true;
				}
			}

			return false;
		}

		public int ChangeIndex(int diff)
		{
			return SetIndex(CurrentIndex + diff);
		}

		public int SetIndex(int index)
		{
			if (FileList.Count < 1)
			{
				return -1;
			}

			if (index < 0)
			{
				index = FileList.Count - 1;
			}

			var newIndex = index % FileList.Count;
			if (newIndex == CurrentIndex)
			{
				return CurrentIndex;
			}

			CurrentIndex = newIndex;
			return CurrentIndex;
		}

		public void Scan(string path)
		{
			var list = new List<FileMetadata>();
			foreach (var f in Directory.EnumerateFiles(path))
			{
				NSError error;
				var fileType = NSWorkspace.SharedWorkspace.TypeOfFile(f, out error);
				if (ImageTypes.Contains(fileType))
				{
					list.Add(new FileMetadata(f));
				}
			}

			list.Sort(new TimestampComparer());
			CurrentIndex = 0;
			FileList = list;
		}
	}

	class TimestampComparer : IComparer<FileMetadata>
	{
		public int Compare(FileMetadata x, FileMetadata y)
		{
			return DateTime.Compare(x.Timestamp, y.Timestamp);
		}
	}

	public enum FileSort
	{
		None,
		Timestamp,
		Name,
	}
}
