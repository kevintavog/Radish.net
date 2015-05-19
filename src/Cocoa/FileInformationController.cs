using System;
using MonoMac.AppKit;
using MonoMac.Foundation;
using Radish.Models;

namespace Radish
{
    [MonoMac.Foundation.Register("FileInformationController")]
	public partial class FileInformationController : NSViewController
	{
		private MediaMetadata MediaMetadata;

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

		public void SetFile(MediaMetadata mediaMetadata)
		{
			MediaMetadata = mediaMetadata;
			tableView.ReloadData();

			for (var column = 0; column < tableView.TableColumns().Length; ++column)
			{
				var biggestWidth = 0f;
				for (int row = 0; row < tableView.RowCount; ++row)
				{
					var cellWidth = tableView.GetCell(column, row).CellSize.Width;
                    biggestWidth = Math.Max(biggestWidth, cellWidth);
				}

				var c = tableView.TableColumns()[column];
				c.Width = c.MaxWidth = biggestWidth;
			}
		}

		[Export("numberOfRowsInTableView:")]
		public int NumberOfRowsInTableView(NSTableView tv)
		{
			if (MediaMetadata == null)
			{
				return 0;
			}
			return MediaMetadata.Metadata.Count;
		}

		[Export("tableView:objectValueForTableColumn:row:")]
		public string ObjectValueForTableColumn(NSTableView table, NSTableColumn column, int rowIndex)
		{
			var info = MediaMetadata.Metadata[rowIndex];
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
		public bool IsGroupRow(NSTableView table, int row)
		{
			return MediaMetadata.Metadata[row].Category != null;
		}

		[Export("tableView:willDisplayCell:forTableColumn:row:")]
		public void WillDisplayCell(NSTableView table, NSObject cell, NSTableColumn column, int row)
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
