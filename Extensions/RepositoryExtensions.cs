using gitup.Models;
using LibGit2Sharp;
using LibGit2Sharp.Handlers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gitup.Extensions
{
	public static class RepositoryExtensions
	{
		public static IEnumerable<BranchModel> GetLocalBranchModels(this BranchCollection branches)
		{
			return branches.Where(b => !b.IsRemote).Select(b => new BranchModel(b));
		}

		public static IEnumerable<BranchModel> GetRemoteBranchModels(this BranchCollection branches)
		{
			return branches.Where(b => b.IsRemote).Select(b => new BranchModel(b));
		}

		public static int GetChangesCount(this Repository repository)
		{
			var status = repository.RetrieveStatus();
			return status.Count() - status.Ignored.Count();
		}

		public static RepositoryModel GetRepositoryModel(this Repository repository, string accessToken)
		{
			var remote = repository.Network.Remotes.FirstOrDefault();
			if (remote?.Url.ToLower().Contains("amazon") ?? false)
			{
				return null;
			}

			return new RepositoryModel(repository, accessToken);
		}

		public static FetchOptionsAndSignature GetFetchOptionsAndSignature(this Repository repository, string accessToken)
		{
			var signature = repository.Config.BuildSignature(DateTimeOffset.Now);
			var options = new FetchOptions();
			options.CredentialsProvider = new CredentialsHandler(
				(url, usernameFromUrl, types) =>
					new UsernamePasswordCredentials()
					{
						Username = signature.Name,
						Password = accessToken
					});
			options.TagFetchMode = TagFetchMode.All;

			return new FetchOptionsAndSignature(options, signature);
		}
	}

	public class FetchOptionsAndSignature
	{
		public FetchOptions FetchOptions { get; set; }
		public Signature Signature { get; set; }

		public FetchOptionsAndSignature(FetchOptions fetchOptions, Signature signature)
		{
			this.FetchOptions = fetchOptions;
			this.Signature = signature;
		}
	}
}
