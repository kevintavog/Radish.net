using System;
using MonoMac.AppKit;
using MonoMac.Foundation;
using Radish.Models;
using NLog;
using System.Drawing;

namespace Radish
{
	[MonoMac.Foundation.Register("FileInformationController")]
	public partial class FileInformationController : NSViewController
	{
//		static private readonly Logger logger = LogManager.GetCurrentClassLogger();
		private FileMetadata FileMetadata;

		public FileInformationController(IntPtr handle) : base(handle)
		{
			Initialize();
		}

		[Export ("initWithCoder:")]
		public FileInformationController(NSCoder coder) : base(coder)
		{
			Initialize();
		}

		void Initialize()
		{
		}

		public override void AwakeFromNib()
		{
			base.AwakeFromNib();
			tableView.BackgroundColor = NSColor.Clear;
		}

		public void SetFile(FileMetadata fileMetadata)
		{
			FileMetadata = fileMetadata;
			tableView.ReloadData();
		}

		[Export("numberOfRowsInTableView:")]
		public int numberOfRowsInTableView(NSTableView tv)
		{
			if (FileMetadata == null)
			{
				return 0;
			}
			return FileMetadata.Metadata.Count;
		}

		[Export("tableView:objectValueForTableColumn:row:")]
		public string objectValueForTableColumn(NSTableView table, NSTableColumn column, int rowIndex)
		{
			var info = FileMetadata.Metadata[rowIndex];
			switch (column.DataCell.Tag)
			{
				case 1:
					return info.Category;
				case 2:
					return info.Name;
				case 3:
					return info.Value;
			}

			return "<Unknown>";
		}

		[Export("tableView:isGroupRow:")]
		public bool isGroupRow(NSTableView table, int row)
		{
			return FileMetadata.Metadata[row].Category != null;
		}

		[Export("tableView:willDisplayCell:forTableColumn:row:")]
		public void willDisplayCell(NSTableView table, NSObject cell, NSTableColumn column, int row)
		{
			var textCell = cell as NSTextFieldCell;
			if (textCell != null)
			{
				textCell.TextColor = NSColor.White;
				textCell.DrawsBackground = false;
				textCell.StringValue = textCell.StringValue;
			}
		}
	}
}
