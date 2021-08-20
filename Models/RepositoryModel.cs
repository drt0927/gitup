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

		#region " Private "
		private static readonly object _lock = new object();
		private string _accessToken { get; set; }
		private BranchModel _currentBranch { get; set; }
		private List<BranchModel> _branches { get; set; }
		private List<BranchModel> _originBranches { get; set; }
		private int _changesCount { get; set; }
		private bool _isFetchProgressStart { get; set; }
		#endregion

		#region " Public "
		public string RepoName { get; set; }
		public string Path { get; set; }
		public string ParentPath { get; set; }
		public bool IsHighlightRow => ChangesCount > 0 || ((_currentBranch.Ahead ?? 0) + (_currentBranch.Behind ?? 0)) > 0;
		public BranchModel CurrentBranch
		{
			get => _currentBranch;
			set
			{
				DispatcherUtil.BeginInvoke(new Action(() =>
				{
					var pre = _currentBranch;
					_currentBranch = value;

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

		public RepositoryModel()
		{
			FetchClickCommand = new DelegateCommand(Fetch);
			PullClickCommand = new DelegateCommand(Pull);
			GoForkClickCommand = new DelegateCommand(GoFork);
			OpenCommitsCommand = new DelegateCommand(OpenCommits);
			OpenChangesCommand = new DelegateCommand(OpenChanges);
		}

		public RepositoryModel(DirectoryInfo di, string accessToken) : this()
		{
			this._accessToken = accessToken;
			using (var repo = new Repository(di.FullName))
			{
				this.ChangesCount = repo.GetChangesCount();
				this.RepoName = di.Parent.Name;
				this.Path = di.FullName;
				this.ParentPath = di.Parent.FullName;
				this.Branches = new List<BranchModel>(repo.Branches.GetLocalBranchModels());
				this.OriginBranches = new List<BranchModel>(repo.Branches.GetRemoteBranchModels());
				this.CurrentBranch = this.Branches.FirstOrDefault(b => b.Name == repo.Head.FriendlyName);
				BindingOperations.EnableCollectionSynchronization(Branches, _lock);
				BindingOperations.EnableCollectionSynchronization(OriginBranches, _lock);
			}
		}

		private bool Checkout(string branchName)
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

				if (string.IsNullOrEmpty(this._accessToken))
				{
					throw new Exception("AccessToken이 없으면 Fetch할 수 없습니다.");
				}

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

						var signature = repo.Config.BuildSignature(DateTimeOffset.Now);
						var options = new PullOptions();

						options.FetchOptions = new FetchOptions();
						options.FetchOptions.CredentialsProvider = new CredentialsHandler(
							(url, usernameFromUrl, types) =>
								new UsernamePasswordCredentials()
								{
									Username = signature.Name,
									Password = this._accessToken
								});
						options.FetchOptions.TagFetchMode = TagFetchMode.All;

						// Pull
						var merge = Commands.Pull(repo, signature, options);

						this.Branches.Clear();
						this.Branches.AddRange(repo.Branches.Where(b => !b.FriendlyName.ToLower().StartsWith("origin")).Select(b => new BranchModel(b)));
						this.OriginBranches.Clear();
						this.OriginBranches.AddRange(repo.Branches.Where(b => b.FriendlyName.ToLower().StartsWith("origin") && !b.FriendlyName.ToLower().StartsWith("origin/HEAD".ToLower())).Select(b => new BranchModel(b)));
						this.CurrentBranch = this.Branches.FirstOrDefault(b => b.Name == repo.Head.FriendlyName);
						this.ChangesCount = repo.GetChangesCount();
					}
				});

				LogProvider.Instance.Info($"[{this.RepoName}] Pull이 완료 되었습니다.");
			}
			catch (Exception ex)
			{
				throw ex;
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

				if (string.IsNullOrEmpty(this._accessToken))
				{
					throw new Exception("AccessToken이 없으면 Fetch할 수 없습니다.");
				}

				await Task.Run(() =>
				{
					string logMessage = "";
					using (var repo = new Repository(this.Path))
					{
						var signature = repo.Config.BuildSignature(DateTimeOffset.Now);

						FetchOptions options = new FetchOptions();
						options.CredentialsProvider = new CredentialsHandler((url, usernameFromUrl, types) =>
							new UsernamePasswordCredentials()
							{
								Username = signature.Name,
								Password = this._accessToken
							});
						options.TagFetchMode = TagFetchMode.All;

						foreach (Remote remote in repo.Network.Remotes)
						{
							IEnumerable<string> refSpecs = remote.FetchRefSpecs.Select(x => x.Specification);
							Commands.Fetch(repo, remote.Name, refSpecs, options, logMessage);
						}

						this.Branches.Clear();
						this.Branches.AddRange(repo.Branches.Where(b => !b.FriendlyName.ToLower().StartsWith("origin")).Select(b => new BranchModel(b)));
						this.OriginBranches.Clear();
						this.OriginBranches.AddRange(repo.Branches.Where(b => b.FriendlyName.ToLower().StartsWith("origin") && !b.FriendlyName.ToLower().StartsWith("origin/HEAD".ToLower())).Select(b => new BranchModel(b)));
						this.CurrentBranch = this.Branches.FirstOrDefault(b => b.Name == repo.Head.FriendlyName);
						this.ChangesCount = repo.GetChangesCount();
					}
				});

				LogProvider.Instance.Info($"[{this.RepoName}] Fetch가 완료 되었습니다.");
			}
			catch (Exception ex)
			{
				throw ex;
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

		public event PropertyChangedEventHandler PropertyChanged;
		public void OnPropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
