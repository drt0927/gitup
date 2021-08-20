using gitup.Extensions;
using LibGit2Sharp;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace gitup.Models
{
	public class CommitModel
	{
		public string Subject { get; set; }
		public string Author { get; set; }
		public string ShortId { get; set; }
		public string Sha { get; set; }
		public string ParentShortId { get; set; }
		public DateTimeOffset Date { get; set; }
		public string Branches { get; set; }
		public ICommand OpenChangesCommand { get; set; }
		private string _path { get; set; }
		public CommitModel(Commit commit, string path)
		{
			this._path = path;
			this.Subject = commit.MessageShort;
			this.Author = commit.Author.Name;
			this.ShortId = commit.ShortSha();
			this.Sha = commit.Sha;
			this.Date = commit.Author.When;
			this.ParentShortId = commit.Parents.FirstOrDefault()?.ShortSha();
			OpenChangesCommand = new DelegateCommand(OpenChangesWindow);
		}

		public void OpenChangesWindow()
		{
			using (Repository repo = new Repository(this._path))
			{
				var commit = repo.Commits.FirstOrDefault(c => c.Sha == this.Sha);

				var commitTree = commit.Tree;
				var parentCommitTree = commit.Parents.FirstOrDefault()?.Tree;

				var patch = repo.Diff.Compare<Patch>(parentCommitTree, commitTree);

				ChangesWindow window = new ChangesWindow(patch);
				window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
				window.ShowDialog();
			}
		}
	}
}
