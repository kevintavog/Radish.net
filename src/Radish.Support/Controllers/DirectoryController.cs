using System;
using System.Collections.Generic;
using Radish.Models;
using System.IO;
using System.Collections;
using NLog;
using System.ComponentModel;
using Radish.Support.Utilities;

namespace Radish.Controllers
{
    public class DirectoryController : INotifyPropertyChanged
	{
		static private readonly Logger logger = LogManager.GetCurrentClassLogger();

        public event PropertyChangedEventHandler PropertyChanged;

        // Status bar helpers
        public string StatusFilename 
        {
            get
            {
                if (FileList.Count < 1)
                {
                    return "";
                }
                return Path.GetFileName(Current.FullPath); 
            }
        }

        public string StatusTimestamp
        {
            get
            {
                if (FileList.Count < 1)
                {
                    return "";
                }
                return Current.Timestamp.ToString("yyyy/MM/dd HH:mm:ss"); ;
            }
        }

        public string StatusIndex
        {
            get
            {
                if (FileList.Count < 1)
                {
                    return "No files";
                }
                return String.Format("{0} of {1}", CurrentIndex + 1, Count);
            }
        }

        public string StatusGps
        {
            get
            {
                if (FileList.Count < 1)
                {
                    return "";
                }
                return Current.ToDms();
            }
        }

        private List<FileMetadata> FileList { get; set; }


		public bool WrappedToStart { get; private set; }
		public bool WrappedToEnd { get; private set; }
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
			WrappedToEnd = WrappedToStart = false;
			if (FileList.Count < 1)
			{
				return -1;
			}

			if (index < 0)
			{
				index = FileList.Count - 1;
				WrappedToEnd = true;
			}

			if (index >= FileList.Count)
			{
				index = 0;
				WrappedToStart = true;
			}

			if (index == CurrentIndex)
			{
				WrappedToEnd = WrappedToStart = false;
				return CurrentIndex;
			}

			CurrentIndex = index;

            FirePropertyChanges();
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

            FirePropertyChanges();
		}

        private void FirePropertyChanges()
        {
            this.FirePropertyChanged<string>(PropertyChanged, () => StatusFilename);
            this.FirePropertyChanged<string>(PropertyChanged, () => StatusTimestamp);
            this.FirePropertyChanged<string>(PropertyChanged, () => StatusIndex);
            this.FirePropertyChanged<string>(PropertyChanged, () => StatusGps);
        }

		private void InternalScan(string path)
		{
			var list = new List<FileMetadata>();
			foreach (var f in Directory.EnumerateFiles(path))
			{
				if (fileViewer.IsFileSupported(f))
				{
					list.Add(new FileMetadata(f));
				}
			}

			list.Sort(new TimestampComparer());
			FileList = list;
		}

		private void OnWatcherChanged(object source, FileSystemEventArgs evt)
		{
			fileViewer.InvokeOnMainThread( ()=>
			{
				logger.Info("Watcher: '{0}' - {1}", evt.FullPath, evt.ChangeType);

				if (File.Exists(evt.FullPath) && !fileViewer.IsFileSupported(evt.FullPath))
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
