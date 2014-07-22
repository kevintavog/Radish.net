using System;
using MonoMac.Foundation;
using MonoMac.AppKit;
using System.IO;
using Radish.Utilities;

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

        [Export("autoRotate:")]
        public void AutoRotate(NSObject sender)
        {
            var fullPath = directoryController.Current.FullPath;
            logger.Info("AutoRotate '{0}'", fullPath);
            try
            {
                NSError error;
                var fileType = NSWorkspace.SharedWorkspace.TypeOfFile(directoryController.Current.FullPath, out error);
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

		[Export("nextImage:")]
		public void NextImage(NSObject sender)
		{
			directoryController.ChangeIndex(+1);
			ShowFile();
			if (directoryController.WrappedToStart)
			{
				ShowNotification(NotificationGraphic.WrappedToStart);
			}
		}

		[Export("previousImage:")]
		public void PreviousImage(NSObject sender)
		{
			directoryController.ChangeIndex(-1);
			ShowFile();
			if (directoryController.WrappedToEnd)
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

		[Export("toggleMap:")]
		public void ToggleMap(NSObject sender)
		{
			var fm = directoryController.Current;
			var url = new NSUrl(
				String.Format("http://maps.google.com/maps?q={0},{1}", fm.Location.Latitude, fm.Location.Longitude));
			NSWorkspace.SharedWorkspace.OpenUrl(url);

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
					logger.Info("char [{0}], key: {1:X} ({2})", evt.CharactersIgnoringModifiers[0], key, evt.Window.Title);
					switch (evt.CharactersIgnoringModifiers[0])
					{
						case (char) NSKey.DownArrow:
						case ' ':
							NextImage(null);
							return;
						case (char) NSKey.UpArrow:
							PreviousImage(null);
							return;
					}
				}
			}
			base.KeyDown(evt);
		}

	}
}
