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


        private List<FileSystemWatcher> watchers = new List<FileSystemWatcher>();
		private Action listUpdated;

		public DirectoryController(IFileViewer fileViewer, Action listUpdated)
            : base(fileViewer)
		{
			this.listUpdated = listUpdated;
		}


		public void Scan(string[] paths)
		{
            foreach (var w in watchers)
			{
                w.Dispose();
			}
            watchers.Clear();

            var list = new List<MediaMetadata>();
            foreach (var p in paths)
            {
                if (Directory.Exists(p))
                {
                    var w = new FileSystemWatcher
        			{
        				Path = p,
        				NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size
        			};
        			w.Changed += OnWatcherChanged;
        			w.Created += OnWatcherChanged;
        			w.Deleted += OnWatcherChanged;
        			w.Renamed += OnWatcherChanged;
        			w.EnableRaisingEvents = true;
                    watchers.Add(w);
                }

    			InternalScan(p, list);
            }

            SetList(list);
			CurrentIndex = 0;

            FirePropertyChanges();
		}

        private void InternalScan(string path, List<MediaMetadata> list)
		{
            if (Directory.Exists(path))
            {
    			foreach (var f in Directory.EnumerateFiles(path))
    			{
    				if (fileViewer.IsFileSupported(f))
    					list.Add(new FileMetadata(f));
    			}
            }
            else
            {
                if (fileViewer.IsFileSupported(path))
                    list.Add(new FileMetadata(path));
            }
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
