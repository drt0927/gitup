using gitup.Models;
using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gitup.ViewModels
{
	public class CommitsViewModel
	{
		public IEnumerable<CommitModel> Commits { get; set; }

		public CommitsViewModel(string path)
		{
			using (Repository repo = new Repository(path))
			{
				this.Commits = new List<CommitModel>();

				var filter = new CommitFilter()
				{
					SortBy = CommitSortStrategies.Time
				};

				this.Commits = repo.Commits.QueryBy(filter).Select(c => new CommitModel(c, path)).ToList();
			}
		}
	}
}
