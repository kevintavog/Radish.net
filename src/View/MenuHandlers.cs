using System;
using MonoMac.Foundation;
using MonoMac.AppKit;
using System.IO;

namespace Radish
{
	public partial class MainWindowController : MonoMac.AppKit.NSWindowController
	{
		enum MenuTag
		{
			AlwaysEnable = 1,
			RequiresFile = 2,
		}

		[Export("validateMenuItem:")]
		public bool ValidateMenuItem(NSMenuItem menuItem)
		{
			switch ((MenuTag) menuItem.Tag)
			{
				case MenuTag.AlwaysEnable:
					return true;
				case MenuTag.RequiresFile:
					return directoryController.Count > 0;
			}

			logger.Info("ValidateMenuItem: unexpected tag {0} for menu item '{1}'", menuItem.Tag, menuItem.Title);
			return false;
		}

		[Export("nextImage:")]
		public void NextImage(NSObject sender)
		{
			directoryController.ChangeIndex(+1);
			ShowFile();
		}

		[Export("previousImage:")]
		public void PreviousImage(NSObject sender)
		{
			directoryController.ChangeIndex(-1);
			ShowFile();
		}

		[Export("openFolderOrFile:")]
		public void OpenFolderOrFile(NSObject sender)
		{
			logger.Info("OpenFolderOrFile");

			var openPanel = new NSOpenPanel
			{
				ReleasedWhenClosed = true,
				Prompt = "Select",
				CanChooseDirectories = true,
				CanChooseFiles = true
			};

			var result = (NsButtonId)openPanel.RunModal();
			if (result != NsButtonId.OK)
			{
				return;
			}

			OpenFolderOrFile(openPanel.Url.Path);
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
				InformationController.SetFile(directoryController.Current);
			}
		}

		[Export("moveToTrash:")]
		public void MoveToTrash(NSObject sender)
		{
			var fullPath = directoryController.Current.FullPath;
			logger.Info("MoveToTrash: '{0}'", fullPath);

			int tag;
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

			if (succeeded)
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
			logger.Info("Set date of '{0}' to {1}", directoryController.Current.FullPath, directoryController.Current.Timestamp);
			directoryController.Current.SetFileDateToExifDate();
			ShowFile();
		}

		[Export("revealInFinder:")]
		public void RevealInFinder(NSObject sender)
		{
			var fullPath = directoryController.Current.FullPath;
			logger.Info("RevealInFinder '{0}'", fullPath);
			NSWorkspace.SharedWorkspace.SelectFile(fullPath, "");
		}
	}
}
