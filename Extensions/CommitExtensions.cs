using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gitup.Extensions
{
	public static class CommitExtensions
	{
		public static string ShortSha(this Commit commit) => commit.Sha.Substring(0, 7);
	}
}
