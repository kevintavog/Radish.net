using System;
using AppKit;
using Foundation;
using NLog;
using ImageKit;
using System.Collections.Generic;
using System.IO;
using Radish.Controllers;
using Radish.Models;

namespace Radish
{
    [Foundation.Register("ThumbController")]
    public partial class ThumbController : NSWindowController
    {
        static private readonly Logger logger = LogManager.GetCurrentClassLogger();
        private List<ImageViewItem> imageViewItems = new List<ImageViewItem>();
        private MediaListController mediaListController;


        public ThumbController(IntPtr handle) : base(handle)
        {
            Initialize();
        }

        [Export ("initWithCoder:")]
        public ThumbController(NSCoder coder) : base(coder)
        {
            Initialize();
        }

        void Initialize()
        {
        }

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();

            imageView.SetValueForKey(NSColor.DarkGray, IKImageBrowserView.BackgroundColorKey);

            var oldAttrs = imageView.ValueForKey(IKImageBrowserView.CellsTitleAttributesKey);
            var newAttrs = oldAttrs.MutableCopy();
            newAttrs.SetValueForKey(NSColor.White, NSAttributedString.ForegroundColorAttributeName);
            imageView.SetValueForKey(newAttrs, IKImageBrowserView.CellsTitleAttributesKey);

            UpdateThumbSize(null);
        }

        partial void UpdateThumbSize (Foundation.NSObject sender)
        {
            imageView.ZoomValue = imageSizeSlider.FloatValue;
        }

        public void SetMediaListController(MediaListController mlc)
        {
            mediaListController = mlc;
            imageViewItems.Clear();

            mediaListController.VisitAll( (m) =>
            {
                imageViewItems.Add(new ImageViewItem(m));
            });

            imageView.ReloadData();
        }

        [Export("numberOfItemsInImageBrowser:")]
        public int ImageKitNumberOfItems(IKImageBrowserView view)
        {
            logger.Info("num items: {0}", imageViewItems.Count);
            return imageViewItems.Count;
        }

        [Export("imageBrowser:itemAtIndex:")]
        public NSObject ImageKitItemAtIndex(IKImageBrowserView view, uint index)
        {
            return imageViewItems[(int) index];
        }

        [Export("openFolderOrFile:")]
        public void OpenFolderOrFile(NSObject sender)
        {
            logger.Info("open folder or file");
        }

        [Export("autoRotate:")]
        public void AutoRotate(NSObject sender)
        {
            logger.Info("auto rotate thumbs");
        }
    }

    class ImageViewItem : IKImageBrowserItem, IComparable
    {
        private NSUrl ItemUrl { get; set; }
        private string Name { get; set; }

        public ImageViewItem(MediaMetadata mm)
        {
            Name = mm.Name;
            var uri = new Uri(mm.FullPath);
            if (uri.IsAbsoluteUri && uri.Scheme != Uri.UriSchemeFile)
            {
                ItemUrl = new NSUrl(mm.ThumbnailPath);
                _imageRepresentationType = IKImageBrowserItem.NSURLRepresentationType;
            }
            else
            {
                ItemUrl = new NSUrl(mm.FullPath, false);
            }
            CreatedTimestamp = mm.Timestamp;
        }

        private NSString _imageRepresentationType = IKImageBrowserItem.PathRepresentationType;

        public override string ImageUID { get { return ItemUrl.Path; } }
        public override NSString ImageRepresentationType { get { return _imageRepresentationType; } }
        public override NSObject ImageRepresentation { get { return ItemUrl; } }
        public override string ImageTitle { get { return Name; } }
        public override string ImageSubtitle { get { return "< sub title >"; } }

        public DateTime CreatedTimestamp { get; set; }

        public int CompareTo(object obj)
        {
            ImageViewItem other = obj as ImageViewItem;
            if (other == null)
            {
                return -1;
            }

            int dateCompare = DateTime.Compare(CreatedTimestamp, other.CreatedTimestamp);
            if (dateCompare != 0)
            {
                return dateCompare;
            }

            return String.Compare(Name, Name);
        }

    }
}
