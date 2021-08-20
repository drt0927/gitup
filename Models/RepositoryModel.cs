using gitup.Extensions;
using gitup.Providers;
using gitup.Utils;
using LibGit2Sharp;
using LibGit2Sharp.Handlers;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;

namespace gitup.Models
{
	public class RepositoryModel : INotifyPropertyChanged
	{

		#region " Private Members "
		private static readonly object _lock = new object();
		private string _accessToken { get; set; }
		private BranchModel _currentBranch { get; set; }
		private List<BranchModel> _branches { get; set; }
		private List<BranchModel> _originBranches { get; set; }
		private int _changesCount { get; set; }
		private bool _isFetchProgressStart { get; set; }
		#endregion

		#region " Public Members "
		public string RepoName { get; set; }
		public string Path { get; set; }
		public string ParentPath { get; set; }
		public bool IsHighlightRow => ChangesCount > 0 || ((_currentBranch.Ahead ?? 0) + (_currentBranch.Behind ?? 0)) > 0;
		public BranchModel CurrentBranch
		{
			get => _currentBranch;
			set
			{
				var pre = _currentBranch;
				_currentBranch = value;

				DispatcherUtil.BeginInvoke(new Action(() =>
				{
					// Branches 변경 로직
					if (!Checkout(value.Name))
					{
						_currentBranch = pre;
					}

					OnPropertyChanged(nameof(CurrentBranch));
					OnPropertyChanged(nameof(IsHighlightRow));
				}));
			}
		}
		public List<BranchModel> Branches
		{
			get => _branches;
			set
			{
				_branches = value;
				OnPropertyChanged(nameof(Branches));
				OnPropertyChanged(nameof(BranchDiffCountString));
			}
		}
		public IEnumerable<string> BranchNames => this.Branches.Select(b => b.OnlyName);
		public List<BranchModel> OriginBranches
		{
			get => _originBranches;
			set
			{
				_originBranches = value;
				OnPropertyChanged(nameof(OriginBranches));
				OnPropertyChanged(nameof(BranchDiffCountString));
			}
		}
		public IEnumerable<string> OriginBranchNames => this.OriginBranches.Select(b => b.OnlyName);
		public int LocalBranchDiffCount => this.BranchNames.Except(this.OriginBranchNames).Count();
		public int RemoteBranchDiffCount => this.OriginBranchNames.Except(this.BranchNames).Count();
		public bool IsHighlightBranchDiff => this.LocalBranchDiffCount > 0 || this.RemoteBranchDiffCount > 0;
		public string BranchDiffCountString => $"↑{this.LocalBranchDiffCount} ↓{this.RemoteBranchDiffCount}";
		public int ChangesCount
		{
			get => _changesCount;
			set
			{
				_changesCount = value;
				OnPropertyChanged(nameof(ChangesCount));
				OnPropertyChanged(nameof(IsHighlightRow));
			}
		}
		public bool IsFetchProgressStart
		{
			get => _isFetchProgressStart;
			set
			{
				_isFetchProgressStart = value;
				OnPropertyChanged(nameof(IsFetchProgressStart));
			}
		}
		#endregion

		#region " ICommand "
		public ICommand FetchClickCommand { get; set; }
		public ICommand PullClickCommand { get; set; }
		public ICommand GoForkClickCommand { get; set; }
		public ICommand OpenCommitsCommand { get; set; }
		public ICommand OpenChangesCommand { get; set; }
		#endregion

		#region " 생성자 "
		public RepositoryModel()
		{
			FetchClickCommand = new DelegateCommand(Fetch);
			PullClickCommand = new DelegateCommand(Pull);
			GoForkClickCommand = new DelegateCommand(GoFork);
			OpenCommitsCommand = new DelegateCommand(OpenCommits);
			OpenChangesCommand = new DelegateCommand(OpenChanges);
		}

		public RepositoryModel(Repository repo, string accessToken) : this()
		{
			var di = new DirectoryInfo(repo.Info.Path);
			InitMembers(di, repo, accessToken);
		}

		public RepositoryModel(DirectoryInfo di, string accessToken) : this()
		{
			using (var repo = new Repository(di.FullName))
			{
				InitMembers(di, repo, accessToken);
			}
		}
		#endregion

		#region " Private Methods "
		private void InitMembers(DirectoryInfo di, Repository repo, string accessToken)
		{
			this._accessToken = accessToken;
			this.RepoName = di.Parent.Name;
			this.Path = di.FullName;
			this.ParentPath = di.Parent.FullName;
			this.RebindMembers(repo);
			BindingOperations.EnableCollectionSynchronization(this.Branches, _lock);
			BindingOperations.EnableCollectionSynchronization(this.OriginBranches, _lock);
		}

		private void ValidAccessToken()
		{
			if (string.IsNullOrEmpty(this._accessToken))
			{
				throw new Exception("AccessToken이 없으면 Fetch할 수 없습니다.");
			}
		}

		private void RebindMembers(Repository repo)
		{
			if (this.Branches == null)
			{
				this.Branches = new List<BranchModel>();
			}
			if (this.OriginBranches == null)
			{
				this.OriginBranches = new List<BranchModel>();
			}
			this.Branches.Clear();
			this.Branches.AddRange(repo.Branches.Where(b => !b.FriendlyName.ToLower().StartsWith("origin")).Select(b => new BranchModel(b)));
			this.OriginBranches.Clear();
			this.OriginBranches.AddRange(repo.Branches.Where(b => b.FriendlyName.ToLower().StartsWith("origin") && !b.FriendlyName.ToLower().StartsWith("origin/HEAD".ToLower())).Select(b => new BranchModel(b)));
			this._currentBranch = this.Branches.FirstOrDefault(b => b.Name == repo.Head.FriendlyName);
			this.ChangesCount = repo.GetChangesCount();
		}
		#endregion

		#region " Public Methods "
		public bool Checkout(string branchName)
		{
			bool result = false;

			try
			{
				using (var repo = new Repository(this.Path))
				{
					var branch = repo.Branches[branchName];

					if (branch == null)
					{
						throw new Exception("Branch is not found");
					}

					var newBranch = Commands.Checkout(repo, branch);
				}

				LogProvider.Instance.Info($"[{this.RepoName} : {branchName}]으로 체크아웃 되었습니다.");

				result = true;
			}
			catch (Exception ex)
			{
				LogProvider.Instance.Error(new Exception($"[{this.RepoName} : {branchName}] 체크아웃 도중 에러가 발생하였습니다.\r\n{ex.Message}"));
			}

			return result;
		}

		public void Push()
		{
			using (var repo = new Repository(this.Path))
			{

			}
		}

		public void Commit()
		{
			using (var repo = new Repository(this.Path))
			{

			}
		}

		public async void Pull()
		{
			try
			{
				IsFetchProgressStart = true;

				ValidAccessToken();

				await Task.Run(() =>
				{
					using (var repo = new Repository(this.Path))
					{
						if (string.IsNullOrEmpty(repo.Head.RemoteName))
						{
							IsFetchProgressStart = false;
							LogProvider.Instance.Error(new Exception($"[{this.RepoName} : {repo.Head.FriendlyName}] Branch는 Remote에 Push되지 않은 Branch입니다."));
							return;
						}

						var fetchOptionsAndSignature = repo.GetFetchOptionsAndSignature(this._accessToken);
						var options = new PullOptions();
						options.FetchOptions = fetchOptionsAndSignature.FetchOptions;

						// Pull
						var merge = Commands.Pull(repo, fetchOptionsAndSignature.Signature, options);

						this.RebindMembers(repo);
					}
				});

				LogProvider.Instance.Info($"[{this.RepoName}] Pull이 완료 되었습니다.");
			}
			catch (Exception ex)
			{
				LogProvider.Instance.Error(new Exception($"[{this.RepoName}] Pull 도중 에러가 발생하였습니다.\r\n{ex.Message}"));
			}
			finally
			{
				IsFetchProgressStart = false;
			}
		}

		public async void Fetch()
		{
			try
			{
				IsFetchProgressStart = true;

				ValidAccessToken();

				await Task.Run(() =>
				{
					string logMessage = "";
					using (var repo = new Repository(this.Path))
					{
						FetchOptions options = repo.GetFetchOptionsAndSignature(this._accessToken).FetchOptions;

						foreach (Remote remote in repo.Network.Remotes)
						{
							IEnumerable<string> refSpecs = remote.FetchRefSpecs.Select(x => x.Specification);
							Commands.Fetch(repo, remote.Name, refSpecs, options, logMessage);
						}

						this.RebindMembers(repo);
					}
				});

				LogProvider.Instance.Info($"[{this.RepoName}] Fetch가 완료 되었습니다.");
			}
			catch (Exception ex)
			{
				LogProvider.Instance.Error(new Exception($"[{this.RepoName}] Fetch 도중 에러가 발생하였습니다.\r\n{ex.Message}"));
			}
			finally
			{
				IsFetchProgressStart = false;
			}
		}

		public void GoFork()
		{
			string strCmdText = $@"/c %localappdata%\fork\fork.exe {this.ParentPath}";
			System.Diagnostics.Process process = new System.Diagnostics.Process();
			System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
			startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
			startInfo.CreateNoWindow = true;
			startInfo.FileName = "cmd.exe";
			startInfo.Arguments = strCmdText;
			process.StartInfo = startInfo;
			process.Start();
		}

		public void OpenCommits()
		{
			CommitsWindow window = new CommitsWindow(this.Path);
			window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
			window.ShowDialog();
		}

		public void OpenChanges()
		{
			using (var repo = new Repository(this.Path))
			{
				ChangesWindow window = new ChangesWindow(repo);
				window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
				window.ShowDialog();
			}
		}
		#endregion

		#region " PropertyChanged "
		public event PropertyChangedEventHandler PropertyChanged;
		public void OnPropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
		#endregion
	}
}
