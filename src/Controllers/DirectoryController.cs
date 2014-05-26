using System;
using System.Collections.Generic;
using Radish.Models;
using System.IO;
using MonoMac.ImageIO;
using MonoMac.Foundation;
using MonoMac.AppKit;
using System.Collections;
using NLog;

namespace Radish.Controllers
{
	public class DirectoryController
	{
		static private readonly Logger logger = LogManager.GetCurrentClassLogger();
		static private HashSet<string> ImageTypes = new HashSet<string>(CGImageSource.TypeIdentifiers);

		private List<FileMetadata> FileList { get; set; }


		public int CurrentIndex { get; private set; }
		public FileSort Sort { get; set; }

		public int Count { get { return FileList.Count; } }
		public FileMetadata Current { get { return FileList[CurrentIndex]; } }

		private IComparer<FileMetadata> comparer;
		private FileSystemWatcher watcher;
		private IFileViewer fileViewer;
		private Action listUpdated;

		public DirectoryController(IFileViewer fileViewer, Action listUpdated)
		{
			this.fileViewer = fileViewer;
			this.listUpdated = listUpdated;

			comparer = new TimestampComparer();
			FileList = new List<FileMetadata>();
		}

		public bool SelectFile(string filename)
		{
			var index = FindFile(filename);
			if (index >= 0)
			{
				SetIndex(index);
				return true;
			}

			return false;
		}

		public int FindFile(string filename)
		{
			if (String.IsNullOrWhiteSpace(filename))
			{
				return -1;
			}

			for (int idx = 0; idx < FileList.Count; ++idx)
			{
				if (FileList[idx].FullPath.Equals(filename, StringComparison.CurrentCultureIgnoreCase))
				{
					return idx;
				}
			}

			return -1;
		}

		public int ChangeIndex(int diff)
		{
			return SetIndex(CurrentIndex + diff);
		}

		private int SetIndex(int index)
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
			if (watcher != null)
			{
				watcher.Dispose();
			}

			watcher = new FileSystemWatcher
			{
				Path = path,
				NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size
			};
			watcher.Changed += OnWatcherChanged;
			watcher.Created += OnWatcherChanged;
			watcher.Deleted += OnWatcherChanged;
			watcher.Renamed += OnWatcherChanged;
			watcher.EnableRaisingEvents = true;

			InternalScan(path);
			CurrentIndex = 0;
		}

		private void InternalScan(string path)
		{
			var list = new List<FileMetadata>();
			foreach (var f in Directory.EnumerateFiles(path))
			{
				if (IsSupportedFileType(f))
				{
					list.Add(new FileMetadata(f));
				}
			}

			list.Sort(new TimestampComparer());
			FileList = list;
		}

		private bool IsSupportedFileType(string filePath)
		{
			NSError error;
			var fileType = NSWorkspace.SharedWorkspace.TypeOfFile(filePath, out error);
			return ImageTypes.Contains(fileType);
		}

		private void OnWatcherChanged(object source, FileSystemEventArgs evt)
		{
			fileViewer.InvokeOnMainThread( ()=>
			{
				logger.Info("Watcher: '{0}' - {1}", evt.FullPath, evt.ChangeType);

				if (File.Exists(evt.FullPath) && !IsSupportedFileType(evt.FullPath))
				{
					return;
				}

				if (WatcherChangeTypes.Created == evt.ChangeType)
				{
					// Add a new item to the list (sorted order)
					var fm = new FileMetadata(evt.FullPath);
					int newIndex = FileList.BinarySearch(fm, comparer);
					if (newIndex < 0)
					{
						FileList.Insert(~newIndex, fm);
					}
					else
					{
						logger.Warn("Item already in list: '{0}'", fm.FullPath);
					}
				}
				else
				{
					var index = FindFile(evt.FullPath);
					if (index < 0)
					{
						logger.Info("Unable to find file in list: '{0}'", evt.FullPath);
						return;
					}

					switch (evt.ChangeType)
					{
						case WatcherChangeTypes.Deleted:
							FileList.RemoveAt(index);
							break;

						case WatcherChangeTypes.Renamed:
							FileList[index] = new FileMetadata(evt.FullPath);
							break;

						case WatcherChangeTypes.Changed:
							FileList[index] = new FileMetadata(evt.FullPath);
							break;
					}
				}

				listUpdated();
			});
		}
	}

	class TimestampComparer : IComparer<FileMetadata>
	{
		public int Compare(FileMetadata x, FileMetadata y)
		{
			if (x == y)
			{
				return 0;
			}

			var diff = DateTime.Compare(x.Timestamp, y.Timestamp);
			if (diff == 0)
			{
				diff = String.Compare(Path.GetFileName(x.FullPath), Path.GetFileName(y.FullPath));
			}
			return diff;
		}
	}

	public enum FileSort
	{
		None,
		Timestamp,
		Name,
	}
}
