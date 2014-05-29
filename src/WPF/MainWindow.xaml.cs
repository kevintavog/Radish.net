using NLog;
using Radish.Controllers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Radish
{
    public partial class MainWindow : Window, IFileViewer
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private DirectoryController directoryController;
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
            InitializeComponent();

            directoryController = new DirectoryController(this, FileListUpdated);
        }

        private void NextFile(object sender, ExecutedRoutedEventArgs e)
        {
            directoryController.ChangeIndex(+1);
            ShowFile();
            if (directoryController.WrappedToStart)
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
                directoryController.Scan(lastAddedPath);
                directoryController.SelectFile(filename);
                ShowFile();
            }
        }

        private void PreviousFile(object sender, ExecutedRoutedEventArgs e)
        {
            directoryController.ChangeIndex(-1);
            ShowFile();
            if (directoryController.WrappedToEnd)
            {
//                ShowNotification(NotificationGraphic.WrappedToEnd);
            }
        }

        private void ShowFile()
        {
            if (directoryController.Count < 1)
            {
                currentlyDisplayedFile = null;
                Image.Source = null;
                Title = "<No files>";
                //UpdateStatusBar();
                return;
            }

            var fi = directoryController.Current;
            if (fi.FullPath != currentlyDisplayedFile)
            {
                logger.Info("ShowFile: {0}; {1}", directoryController.CurrentIndex, fi.FullPath);
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
            directoryController.SelectFile(currentlyDisplayedFile);
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
