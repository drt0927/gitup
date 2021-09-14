using gitup.Providers;
using gitup.Models;
using LibGit2Sharp;
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
using gitup.Extensions;
using System.Text.RegularExpressions;

namespace gitup.ViewModels
{
	public class MainVieModel : INotifyPropertyChanged
	{
		private readonly object _lock = new object();
		private ConfigModel _config;
		public ConfigModel Config => _config;

		private bool _isEnable = true;
		public bool IsEnable
		{
			get => _isEnable;
			set
			{
				_isEnable = value;
				OnPropertyChanged(nameof(IsEnable));
			}
		}

		private string _filter = string.Empty;
		public string Filter
		{
			get => _filter;
			set
			{
				_filter = value;
				OnFilterChanged();
			}
		}
		private ObservableCollection<RepositoryModel> Repos { get; set; }
		private CollectionViewSource RepoCollectionViewSource { get; set; }

		public ICollectionView RepoCollection
		{
			get
			{
				return RepoCollectionViewSource.View;
			}
		}

		public ICommand AllFetchClickCommand { get; set; }
		public ICommand AllPullClickCommand { get; set; }
		public ICommand LoadClickCommand { get; set; }
		public MainVieModel()
		{
			_config = ConfigProvider.Read();

			Repos = new ObservableCollection<RepositoryModel>();
			BindingOperations.EnableCollectionSynchronization(Repos, _lock);

			RepoCollectionViewSource = new CollectionViewSource();
			RepoCollectionViewSource.Source = this.Repos;
			RepoCollectionViewSource.Filter += RepoCollectionViewSource_Filter;

			LoadRepos();

			AllFetchClickCommand = new DelegateCommand(async () => await AllFetch());
			AllPullClickCommand = new DelegateCommand(async () => await AllPull());
			LoadClickCommand = new DelegateCommand(async () => await LoadRepos());
		}

		private void RepoCollectionViewSource_Filter(object sender, FilterEventArgs e)
		{
			RepositoryModel svm = (RepositoryModel)e.Item;

			if (string.IsNullOrWhiteSpace(this.Filter) || this.Filter.Length == 0)
			{
				e.Accepted = true;
			}
			else
			{
				e.Accepted = svm.RepoName.IndexOf(Filter, StringComparison.OrdinalIgnoreCase) >= 0 || svm.CurrentBranch.Name.IndexOf(Filter, StringComparison.OrdinalIgnoreCase) >= 0;
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;
		public void OnPropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		private void OnFilterChanged()
		{
			RepoCollectionViewSource.View.Refresh();
		}

		private IEnumerable<string> GetDirs(string rootPath, string searchPattern)
		{
			foreach (var path in Directory.EnumerateDirectories(rootPath, searchPattern, SearchOption.AllDirectories))
			{
				yield return path;
			}
			yield break;
		}

		public async void ReadFolder()
		{
			if (this._config.PathType == 1 && string.IsNullOrEmpty(this._config.RootPath))
			{
				return;
			}

			this.IsEnable = false;
			this.Repos.Clear();
			this._config.GitPaths.Clear();

			await Task.Run(() =>
			{
				foreach (var path in GetDirs(this._config.RootPath, ".git"))
				{
					var di = new DirectoryInfo(path);
					using (var repo = new Repository(di.FullName))
					{
						var r = repo.GetRepositoryModel(this._config.AccessToken);
						if (r == null)
						{
							continue;
						}

						this.Repos.Add(r);
						_config.GitPaths.Add(new ConfigGitPathModel(r));
					}
				}

				this.Repos.OrderBy(x => x.RepoName);
				_config.GitPaths.OrderBy(x => x.Name);
				ConfigProvider.Write(_config);
			});

			this.IsEnable = true;
		}

		public async void ReadSlnFile()
		{
			if (this._config.PathType == 2 && string.IsNullOrEmpty(this._config.SlnPath))
			{
				return;
			}

			this.IsEnable = false;
			this.Repos.Clear();
			this._config.GitPaths.Clear();

			await Task.Run(() =>
			{
				string slnContents = File.ReadAllText(this._config.SlnPath);
				var pattern = @"Project\(""({.*?})""\)\s*=\s*""(.*?)""\s*,\s*""(.*?)""\s*,\s*""(.*?)""\s*EndProject";
				var matches = Regex.Matches(slnContents, pattern);

				var slnInfo = new FileInfo(this._config.SlnPath);
				foreach (Match m in matches)
				{
					var projectPath = m.Groups[3].ToString();
					var filePath = System.IO.Path.Combine(slnInfo.DirectoryName, projectPath);
					var fileInfo = new FileInfo(filePath);
					var gitfolder = fileInfo.Directory.GetDirectories(".git", SearchOption.TopDirectoryOnly).FirstOrDefault();

					if (gitfolder == null)
					{
						continue;
					}

					using (var repo = new Repository(fileInfo.Directory.FullName))
					{
						var r = repo.GetRepositoryModel(this._config.AccessToken);
						if (r == null)
						{
							continue;
						}

						this.Repos.Add(r);
						_config.GitPaths.Add(new ConfigGitPathModel(r));
					}
				}

				//foreach (var path in GetDirs(this._config.RootPath, ".git"))
				//{
				//	var di = new DirectoryInfo(path);
				//	using (var repo = new Repository(di.FullName))
				//	{
				//		var r = repo.GetRepositoryModel(this._config.AccessToken);
				//		if (r == null)
				//		{
				//			continue;
				//		}

				//		this.Repos.Add(r);
				//		_config.GitPaths.Add(new ConfigGitPathModel(r));
				//	}
				//}

				this.Repos.OrderBy(x => x.RepoName);
				_config.GitPaths.OrderBy(x => x.Name);
				ConfigProvider.Write(_config);
			});

			this.IsEnable = true;
		}

		public async Task LoadRepos()
		{
			this.IsEnable = false;

			this.Repos.Clear();
			await Task.Run(() =>
			{
				_config.GitPaths.ForEach(p =>
				{
					DirectoryInfo di = new DirectoryInfo(p.Path);
					this.Repos.Add(new RepositoryModel(di, this._config.AccessToken));
				});
			});

			this.IsEnable = true;
		}

		public async Task AllFetch()
		{
			if (!this.ValidAccessToken())
			{
				return;
			}

			await Task.Run(() =>
			{
				this.Repos.AsParallel().ForAll(r => r.Fetch());
			});

			RepoCollectionViewSource.View.Refresh();
		}

		public async Task AllPull()
		{
			if (!this.ValidAccessToken())
			{
				return;
			}

			await Task.Run(() =>
			{
				this.Repos.AsParallel().ForAll(r => r.Pull());
			});

			RepoCollectionViewSource.View.Refresh();
		}

		private bool ValidAccessToken()
		{
			if (string.IsNullOrEmpty(this._config.AccessToken))
			{
				System.Windows.MessageBox.Show("AccessToken이 없으면 Fetch할 수 없습니다.");
				return false;
			}

			return true;
		}
	}
}
