using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace gitup.Models
{
	public class ChangeModel
	{
		public ChangeKind? Status { get; set; }
		public string StatusString
		{
			get
			{
				switch (Status)
				{
					case ChangeKind.Deleted:
						return "Delete";
					case ChangeKind.Renamed:
					case ChangeKind.TypeChanged:
					case ChangeKind.Modified:
						return "Modify";
					case ChangeKind.Added:
						return "Add";
					default:
						return Status.ToString();
				}
			}
		}
		public Brush StatusColor
		{
			get
			{
				switch (Status)
				{
					case ChangeKind.Deleted:
						return (Brush)(new BrushConverter().ConvertFrom("#D73A49"));
					case ChangeKind.Renamed:
					case ChangeKind.TypeChanged:
					case ChangeKind.Modified:
						return (Brush)(new BrushConverter().ConvertFrom("#DBAB09"));
					case ChangeKind.Added:
						return (Brush)(new BrushConverter().ConvertFrom("#29A045"));
					default:
						return Brushes.White;
				}
			}
		}
		public FileStatus? State { get; set; }
		public string StateString
		{
			get
			{
				switch (State)
				{
					case FileStatus.DeletedFromIndex:
					case FileStatus.DeletedFromWorkdir:
						return "Delete";
					case FileStatus.RenamedInIndex:
					case FileStatus.RenamedInWorkdir:
					case FileStatus.TypeChangeInIndex:
					case FileStatus.TypeChangeInWorkdir:
					case FileStatus.ModifiedInIndex:
					case FileStatus.ModifiedInWorkdir:
						return "Modify";
					case FileStatus.NewInIndex:
					case FileStatus.NewInWorkdir:
						return "Add";
					default:
						return State.ToString();
				}
			}
		}
		public Brush StateColor
		{
			get
			{
				switch (State)
				{
					case FileStatus.DeletedFromIndex:
					case FileStatus.DeletedFromWorkdir:
						return (Brush)(new BrushConverter().ConvertFrom("#D73A49"));
					case FileStatus.RenamedInIndex:
					case FileStatus.RenamedInWorkdir:
					case FileStatus.TypeChangeInIndex:
					case FileStatus.TypeChangeInWorkdir:
					case FileStatus.ModifiedInIndex:
					case FileStatus.ModifiedInWorkdir:
						return (Brush)(new BrushConverter().ConvertFrom("#DBAB09"));
					case FileStatus.NewInIndex:
					case FileStatus.NewInWorkdir:
						return (Brush)(new BrushConverter().ConvertFrom("#29A045"));
					default:
						return Brushes.White;
				}
			}
		}

		public string Path { get; set; }
		public string Name { get; set; }

		public ChangeModel(StatusEntry status)
		{
			this.State = status.State;
			this.Path = status.FilePath;
			this.Name = System.IO.Path.GetFileName(status.FilePath);
		}

		public ChangeModel(PatchEntryChanges patch)
		{
			this.Status = patch.Status;
			this.Path = patch.Path;
			this.Name = System.IO.Path.GetFileName(patch.Path);
		}
	}
}
