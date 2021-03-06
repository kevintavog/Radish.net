﻿using System;
using System.ComponentModel;
using Radish.Support.Utilities;
using System.Collections.Generic;
using Radish.Models;

namespace Radish.Controllers
{
    abstract public class MediaListController: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected List<MediaMetadata> MediaList { get; set; }

        public bool WrappedToStart { get; private set; }
        public bool WrappedToEnd { get; private set; }
        public int CurrentIndex { get; protected set; }
        public FileSort Sort { get; set; }

        public int Count { get { return MediaList.Count; } }
        public MediaMetadata Current { get { return MediaList[CurrentIndex]; } }


        protected IComparer<MediaMetadata> comparer;
        protected IFileViewer FileViewer;
        protected MediaListController(IFileViewer fileViewer)
        {
            FileViewer = fileViewer;
            comparer = new TimestampComparer();
            MediaList = new List<MediaMetadata>();
        }

        public void VisitAll(Action<MediaMetadata> visitAction)
        {
            foreach (var m in MediaList)
            {
                visitAction(m);
            }
        }

        // Status bar helpers
        public string StatusFilename
        {
            get
            {
                if (MediaList.Count < 1)
                {
                    return "";
                }
                return Current.FullPath;
            }
        }

        public string StatusTimestamp
        {
            get
            {
                if (MediaList.Count < 1)
                {
                    return "";
                }
                return Current.Timestamp.ToString("yyyy/MM/dd HH:mm:ss");
            }
        }

        public string StatusIndex
        {
            get
            {
                if (MediaList.Count < 1)
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
                if (MediaList.Count < 1)
                {
                    return "";
                }
                return Current.ToDms();
            }
        }

        public string StatusKeywords
        {
            get
            {
                if (MediaList.Count < 1)
                {
                    return "";
                }
                return Current.Keywords;
            }
        }

        public bool SelectFile(string filename)
        {
            var index = FindFile(filename);
            if (index >= 0)
            {
                SetIndex(index);
                return true;
            }

            // Do the best we can, ensure a valid file is selected
            if (CurrentIndex >= Count)
            {
                SetIndex(Count - 1);
            }
            return false;
        }

        public void SelectFirst()
        {
            SetIndex(0);
        }

        public void SelectLast()
        {
            SetIndex(Count - 1);
        }

        public int FindFile(string filename)
        {
            if (String.IsNullOrWhiteSpace(filename))
            {
                return -1;
            }

            for (int idx = 0; idx < MediaList.Count; ++idx)
            {
                if (MediaList[idx].FullPath.Equals(filename, StringComparison.CurrentCultureIgnoreCase))
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
            if (MediaList.Count < 1)
            {
                return -1;
            }

            if (index < 0)
            {
                index = MediaList.Count - 1;
                WrappedToEnd = true;
            }

            if (index >= MediaList.Count)
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

        protected void FirePropertyChanges()
        {
            this.FirePropertyChanged<string>(PropertyChanged, () => StatusFilename);
            this.FirePropertyChanged<string>(PropertyChanged, () => StatusTimestamp);
            this.FirePropertyChanged<string>(PropertyChanged, () => StatusIndex);
            this.FirePropertyChanged<string>(PropertyChanged, () => StatusGps);
            this.FirePropertyChanged<string>(PropertyChanged, () => StatusKeywords);
        }

        protected void SetList(IList<MediaMetadata> list)
        {
            MediaList = list as List<MediaMetadata>;
            MediaList.Sort(new TimestampComparer());
            SetIndex(0);
        }
    }

    class TimestampComparer : IComparer<MediaMetadata>
    {
        public int Compare(MediaMetadata x, MediaMetadata y)
        {
            if (x == y)
            {
                return 0;
            }

            var diff = DateTime.Compare(x.Timestamp, y.Timestamp);
            if (diff == 0)
            {
                diff = String.Compare(x.Name, y.Name);
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

