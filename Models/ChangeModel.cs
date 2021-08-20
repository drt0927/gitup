using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gitup.Models
{
	public class ChangeModel
	{
		public ChangeKind? Status { get; set; }
		public FileStatus? State { get; set; }
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
