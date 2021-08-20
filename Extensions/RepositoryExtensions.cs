using gitup.Models;
using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gitup.Extensions
{
	public static class RepositoryExtensions
	{
		public static IEnumerable<BranchModel> GetLocalBranchModels(this BranchCollection branches)
		{
			return branches.Where(b => !b.FriendlyName.ToLower().StartsWith("origin")).Select(b => new BranchModel(b));
		}

		public static IEnumerable<BranchModel> GetRemoteBranchModels(this BranchCollection branches)
		{
			return branches.Where(b => b.FriendlyName.ToLower().StartsWith("origin") && !b.FriendlyName.ToLower().StartsWith("origin/HEAD".ToLower())).Select(b => new BranchModel(b));
		}

		public static int GetChangesCount(this Repository repository)
		{
			var status = repository.RetrieveStatus();
			return status.Count() - status.Ignored.Count();
		}
	}
}
