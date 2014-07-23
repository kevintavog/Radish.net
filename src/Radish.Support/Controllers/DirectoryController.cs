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
    public class DirectoryController : MediaListController
	{
		static private readonly Logger logger = LogManager.GetCurrentClassLogger();


		private IComparer<MediaMetadata> comparer;
		private FileSystemWatcher watcher;
		private Action listUpdated;

		public DirectoryController(IFileViewer fileViewer, Action listUpdated)
            : base(fileViewer)
		{
			this.listUpdated = listUpdated;

			comparer = new TimestampComparer();
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

		private void InternalScan(string path)
		{
			var list = new List<MediaMetadata>();
			foreach (var f in Directory.EnumerateFiles(path))
			{
				if (fileViewer.IsFileSupported(f))
				{
					list.Add(new FileMetadata(f));
				}
			}

			list.Sort(new TimestampComparer());
            SetList(list);
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
					int newIndex = MediaList.BinarySearch(fm, comparer);
					if (newIndex < 0)
					{
						MediaList.Insert(~newIndex, fm);
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
                            MediaList.RemoveAt(index);
							break;

						case WatcherChangeTypes.Renamed:
                            MediaList[index] = new FileMetadata(evt.FullPath);
							break;

						case WatcherChangeTypes.Changed:
                            MediaList[index] = new FileMetadata(evt.FullPath);
							break;
					}
				}

				listUpdated();
			});
		}
    }
}
