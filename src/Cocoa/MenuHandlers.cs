using System;
using Foundation;
using AppKit;
using System.IO;
using Radish.Utilities;
using Radish.Controllers;
using Radish.Models;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using Rangic.Utilities.Process;

namespace Radish
{
	public partial class MainWindowController : AppKit.NSWindowController
	{
		enum MenuTag
		{
			AlwaysEnable = 1,
			RequiresFile = 2,
		}

		[Export("validateMenuItem:")]
		public bool ValidateMenuItem(NSMenuItem menuItem)
		{
            switch ((MenuTag) (int)menuItem.Tag)
			{
				case MenuTag.AlwaysEnable:
					return true;
				case MenuTag.RequiresFile:
					return mediaListController.Count > 0;
			}

			logger.Info("ValidateMenuItem: unexpected tag {0} for menu item '{1}'", menuItem.Tag, menuItem.Title);
			return false;
		}

        [Export("autoRotate:")]
        public void AutoRotate(NSObject sender)
        {
            var fullPath = mediaListController.Current.FullPath;
            logger.Info("AutoRotate '{0}'", fullPath);
            try
            {
                NSError error;
                var fileType = NSWorkspace.SharedWorkspace.TypeOfFile(mediaListController.Current.FullPath, out error);
                if (fileType == "public.jpeg")
                {
                    var jheadInvoker = new JheadInvoker();
                    jheadInvoker.Run("-q -autorot -ft \"{0}\"", fullPath);
                    ShowFile(forceRefresh:true);
                }
            }
            catch (Exception e)
            {
                var message = String.Format("Auto rotate of '{0}' failed: {1}", fullPath, e.Message);
                var alert = NSAlert.WithMessage(message, "Close", "", "", "");
                alert.RunSheetModal(Window);
                return;
            }
        }

        [Export("firstImage:")]
        public void FirstImage(NSObject sender)
        {
            mediaListController.SelectFirst();
            ShowFile();
        }

        [Export("lastImage:")]
        public void LastImage(NSObject sender)
        {
            mediaListController.SelectLast();
            ShowFile();
        }

		[Export("nextImage:")]
		public void NextImage(NSObject sender)
		{
			mediaListController.ChangeIndex(+1);
			ShowFile();
			if (mediaListController.WrappedToStart)
			{
				ShowNotification(NotificationGraphic.WrappedToStart);
			}
		}

		[Export("previousImage:")]
		public void PreviousImage(NSObject sender)
		{
			mediaListController.ChangeIndex(-1);
			ShowFile();
			if (mediaListController.WrappedToEnd)
			{
				ShowNotification(NotificationGraphic.WrappedToEnd);
			}
		}

		[Export("openFolderOrFile:")]
		public void OpenFolderOrFile(NSObject sender)
		{
			logger.Info("OpenFolderOrFile");

			var openPanel = new NSOpenPanel
			{
                AllowsMultipleSelection = true,
				CanChooseDirectories = true,
				CanChooseFiles = true,
                Prompt = "Select",
                ReleasedWhenClosed = true,
			};

            var result = (NsButtonId)(int)openPanel.RunModal();
			if (result != NsButtonId.OK)
			{
				return;
			}

            string[] paths = openPanel.Urls.Select( u => u.Path ).ToArray();
			OpenFolderOrFiles(paths);
		}

        [Export("search:")]
        public void Search(NSObject sender)
        {
            logger.Info("Search");
            SearchController.RunModal(searchWindow);

            if (SearchController.SearchResults != null)
            {
                var searchController = mediaListController as FindAPhotoController;
                if (searchController == null)
                {
                    searchController = new FindAPhotoController(this);
                    mediaListController = searchController as MediaListController;
                }

                searchController.Set(SearchController.SearchResults);
                ShowFile();

                ThumbController.SetMediaListController(mediaListController);
            }
        }

		[Export("toggleInformation:")]
		public void ToggleInformation(NSObject sender)
		{
			if (informationPanel.IsVisible)
			{
				informationPanel.OrderOut(this);
			}
			else
			{
				informationPanel.MakeKeyAndOrderFront(this);
				InformationController.SetFile(mediaListController.Current);
			}
		}

        [Export("toggleThumbnails:")]
        public void ToggleThumbnails(NSObject sender)
        {
            if (thumbnailWindow.IsVisible)
            {
                thumbnailWindow.OrderOut(sender);
            }
            else
            {
                thumbnailWindow.MakeKeyAndOrderFront(sender);
            }
        }

		[Export("toggleMap:")]
		public void ToggleMap(NSObject sender)
		{
			var fm = mediaListController.Current;
			var url = new NSUrl(
				String.Format("http://maps.google.com/maps?q={0},{1}", fm.Location.Latitude, fm.Location.Longitude));
			NSWorkspace.SharedWorkspace.OpenUrl(url);

		}

		[Export("moveToTrash:")]
		public void MoveToTrash(NSObject sender)
		{
            if (mediaListController.Current as FileMetadata == null)
            {
                return;
            }

			var fullPath = mediaListController.Current.FullPath;
			logger.Info("MoveToTrash: '{0}'", fullPath);

			nint tag;
			var succeeded = NSWorkspace.SharedWorkspace.PerformFileOperation(
				NSWorkspace.OperationRecycle,
				Path.GetDirectoryName(fullPath),
				"",
				new string[] { Path.GetFileName(fullPath) },
				out tag);

			if (tag != 0)
			{
				logger.Info("PerformFileOperation {0}; tag={1}", succeeded, tag);
			}

			if (!succeeded)
			{
				var message = String.Format("Failed moving '{0}' to trash.", fullPath);
				var alert = NSAlert.WithMessage(message, "Close", "", "", "");
				alert.RunSheetModal(Window);
				return;
			}

			new NSSound(TrashSoundPath, false).Play();
		}

		[Export("setFileDateFromExifDate:")]
		public void SetFileDateFromExifDate(NSObject sender)
		{
			logger.Info("Set date of '{0}' to {1}", mediaListController.Current.FullPath, mediaListController.Current.Timestamp);
            var fm = mediaListController.Current as FileMetadata;
            if (fm != null)
            {
                fm.SetFileDateToExifDate();
                ShowFile();
            }
		}

        [Export("zoomIn:")]
        public void ZoomIn(NSObject sender)
        {
            zoomView.ZoomIn();
        }

        [Export("zoomOut:")]
        public void ZoomOut(NSObject sender)
        {
            zoomView.ZoomOut();
        }

        [Export("zoomToFit:")]
        public void ZoomToFit(NSObject sender)
        {
            zoomView.ZoomToFit();
        }

        [Export("zoomToActualSize:")]
        public void ZoomToActualSize(NSObject sender)
        {
            var rect = imageView.Frame;
            rect.Size = imageView.Image.Size;
            zoomView.ZoomViewToFitRect(rect);
        }

        [Export("revealInFinder:")]
		public void RevealInFinder(NSObject sender)
		{
			var fullPath = mediaListController.Current.FullPath;
			logger.Info("RevealInFinder '{0}'", fullPath);
			NSWorkspace.SharedWorkspace.SelectFile(fullPath, "");
		}

		uint AllModifiers = (uint) (NSEventModifierMask.CommandKeyMask 
			| NSEventModifierMask.ControlKeyMask 
			| NSEventModifierMask.AlternateKeyMask 
			| NSEventModifierMask.ShiftKeyMask);

		public override void KeyDown(NSEvent evt)
		{
			if (((uint)evt.ModifierFlags & AllModifiers) == 0)
			{
				if (evt.CharactersIgnoringModifiers.Length == 1)
				{
					NSKey key = (NSKey) evt.CharactersIgnoringModifiers[0];
					switch (evt.CharactersIgnoringModifiers[0])
					{
						case (char) NSKey.DownArrow:
						case ' ':
							NextImage(null);
							return;
						case (char) NSKey.UpArrow:
							PreviousImage(null);
							return;

                        default:
                            logger.Info("char [{0}], key: {1:X} ({2})", evt.CharactersIgnoringModifiers[0], key, evt.Window.Title);
                            break;
					}
				}
			}
			base.KeyDown(evt);
		}

	}
}
