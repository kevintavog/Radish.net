﻿using System;
using System.Collections.Generic;
using System.Linq;
using MonoMac.Foundation;
using MonoMac.AppKit;
using NLog;
using Radish.Models;
using Radish.Controllers;
using System.IO;
using System.Drawing;
using MonoMac.CoreText;
using MonoMac.CoreGraphics;

namespace Radish
{
	public partial class MainWindowController : MonoMac.AppKit.NSWindowController
	{
		static private readonly Logger logger = LogManager.GetCurrentClassLogger();


		private DirectoryController		directoryController = new DirectoryController();


#region Constructors

		// Called when created from unmanaged code
		public MainWindowController(IntPtr handle) : base(handle)
		{
			Initialize();
		}
		// Called when created directly from a XIB file
		[Export("initWithCoder:")]
		public MainWindowController(NSCoder coder) : base(coder)
		{
			Initialize();
		}
		// Call to load from the XIB/NIB file
		public MainWindowController() : base("MainWindow")
		{
			Initialize();
		}
		// Shared initialization code
		void Initialize()
		{
		}

#endregion

		//strongly typed window accessor
		public new MainWindow Window { get { return (MainWindow)base.Window; } }
		public FileInformationController InformationController { get { return (FileInformationController)fileInformationController; } }

		public override void AwakeFromNib()
		{
			base.AwakeFromNib();

			Window.BackgroundColor = NSColor.DarkGray;
			imageView.ImageScaling = NSImageScale.ProportionallyUpOrDown;
		}

		private void ShowFile()
		{
			if (directoryController.Count < 1)
			{
				return;
			}

			var fi = directoryController.Current;
			logger.Info("ShowFile: {0}; {1}", directoryController.CurrentIndex, fi.FullPath);

			using (var image = new NSImage(fi.FullPath))
			{
				imageView.Image = image;
				image.Release();
			}

			Window.Title = Path.GetFileName(fi.FullPath);
			UpdateStatusBar();

			if (informationPanel.IsVisible)
			{
				InformationController.SetFile(fi);
			}
		}

		private void UpdateStatusBar()
		{
			var fi = directoryController.Current;
			statusFilename.StringValue = Path.GetFileName(fi.FullPath);

			var timestamp = fi.Timestamp.ToString("yyyy/MM/dd HH:mm:ss");
			if (fi.FileAndExifTimestampMatch)
			{
				statusTimestamp.StringValue = timestamp;
			}
			else
			{
				var str = new NSMutableAttributedString(timestamp);
				str.AddAttribute(
					NSAttributedString.ForegroundColorAttributeName, 
					NSColor.Yellow,
					new NSRange(0, timestamp.Length));
				str.AddAttribute(
					NSAttributedString.UnderlineStyleAttributeName, 
					new NSNumber((int) NSUnderlineStyle.Single), 
					new NSRange(0, timestamp.Length));
				statusTimestamp.AttributedStringValue = str;
			}
			statusIndex.StringValue = String.Format(
				"{0} of {1}", 
				directoryController.CurrentIndex + 1, 
				directoryController.Count);

			statusGps.StringValue = fi.ToDms();
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
					logger.Info("char [{0}], key: {1:X}", evt.CharactersIgnoringModifiers[0], key);
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
			if (directoryController.Count < 1)
			{
				return;
			}

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

		public bool OpenFolderOrFile(string path)
		{
			string filename = null;
			if (File.Exists(path))
			{
				filename = path;
				path = Path.GetDirectoryName(path);
			}

			logger.Info("Open {0}", path);
			directoryController.Scan(path);
			directoryController.SelectFile(filename);
			ShowFile();
			return true;
		}
	}

	public enum NsButtonId
	{
		Cancel = 0,
		OK = 1
	}
}
