using NLog;
using Radish.Controllers;
using Radish.Support.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Radish
{
    public partial class MainWindow : Window, IFileViewer, INotifyPropertyChanged
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public event PropertyChangedEventHandler PropertyChanged;
        public DirectoryController DirectoryController { get; private set; }
        private string lastAddedPath;
        private string currentlyDisplayedFile;
        private HashSet<string> supportedExtensions = new HashSet<string>()
        {
            ".JPEG",
            ".JPG",
            ".PNG",
        };

        public MainWindow()
        {
            DirectoryController = new DirectoryController(this, FileListUpdated);

            InitializeComponent();

            this.FirePropertyChanged(PropertyChanged, () => DirectoryController);
        }

        private void NextFile(object sender, ExecutedRoutedEventArgs e)
        {
            DirectoryController.ChangeIndex(+1);
            ShowFile();
            if (DirectoryController.WrappedToStart)
            {
//                ShowNotification(NotificationGraphic.WrappedToStart);
            }
        }

        private void OpenFolderOrFile(object sender, ExecutedRoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog { CheckFileExists = true, CheckPathExists = true, Multiselect = false };

            if (lastAddedPath != null)
            {
                dialog.FileName = lastAddedPath;
            }

            var ret = dialog.ShowDialog(this);
            if (ret.HasValue && ret.Value)
            {
                lastAddedPath = dialog.FileName;

                string filename = null;
                if (File.Exists(lastAddedPath))
                {
                    filename = lastAddedPath;
                    lastAddedPath = Path.GetDirectoryName(lastAddedPath);
                }
                DirectoryController.Scan(lastAddedPath);
                DirectoryController.SelectFile(filename);
                ShowFile();
            }
        }

        private void PreviousFile(object sender, ExecutedRoutedEventArgs e)
        {
            DirectoryController.ChangeIndex(-1);
            ShowFile();
            if (DirectoryController.WrappedToEnd)
            {
//                ShowNotification(NotificationGraphic.WrappedToEnd);
            }
        }

        private void ShowFile()
        {
            if (DirectoryController.Count < 1)
            {
                currentlyDisplayedFile = null;
                Image.Source = null;
                Title = "<No files>";
                //UpdateStatusBar();
                return;
            }

            var fi = DirectoryController.Current;
            if (fi.FullPath != currentlyDisplayedFile)
            {
                logger.Info("ShowFile: {0}; {1}", DirectoryController.CurrentIndex, fi.FullPath);
                currentlyDisplayedFile = fi.FullPath;

                var source = new BitmapImage();
                source.BeginInit();
                source.UriSource = new Uri(fi.FullPath);
                source.CacheOption = BitmapCacheOption.OnLoad;
                source.EndInit();
                // To allow the image to be used by the UI thread...
                source.Freeze();
                Image.Source = source;
            }

            Title = Path.GetFileName(fi.FullPath);
            //UpdateStatusBar();

            //if (informationPanel.IsVisible)
            //{
            //    InformationController.SetFile(fi);
            //}
        }



        private void FileListUpdated()
        {
            DirectoryController.SelectFile(currentlyDisplayedFile);
            ShowFile();
        }


        public void InvokeOnMainThread(System.Action action)
        {
            Dispatcher.BeginInvoke(action);
        }

        public bool IsFileSupported(string filePath)
        {
            var extension = Path.GetExtension(filePath).ToUpper();
            return supportedExtensions.Contains(extension);
        }
    }
}
