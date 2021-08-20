using gitup.Models;
using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gitup.ViewModels
{
	public class ChangesViewModel
	{
		public IEnumerable<ChangeModel> Changes { get; set; }

		public ChangesViewModel(Repository repo)
		{
			Changes = new List<ChangeModel>();
			var status = repo.RetrieveStatus();

			Changes = status.Where(x => x.State != FileStatus.Ignored).Select(s => new ChangeModel(s));
		}

		public ChangesViewModel(Patch patch)
		{
			Changes = new List<ChangeModel>();

			Changes = patch.Select(p => new ChangeModel(p));
		}
	}
}
