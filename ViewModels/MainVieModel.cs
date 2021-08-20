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
			LoadRepos();

			RepoCollectionViewSource = new CollectionViewSource();
			RepoCollectionViewSource.Source = this.Repos;
			RepoCollectionViewSource.Filter += RepoCollectionViewSource_Filter;

			AllFetchClickCommand = new DelegateCommand(AllFetch);
			AllPullClickCommand = new DelegateCommand(AllPull);
			LoadClickCommand = new DelegateCommand(LoadRepos);
		}

		private void RepoCollectionViewSource_Filter(object sender, FilterEventArgs e)
		{
			RepositoryModel svm = (RepositoryModel)e.Item;

			if (string.IsNullOrWhiteSpace(this.Filter) || this.Filter.Length == 0)
			{
				var a = 1;
				e.Accepted = true;
			}
			else
			{
				e.Accepted = svm.RepoName.ToLower().Contains(Filter.ToLower()) || svm.CurrentBranch.Name.ToLower().Contains(Filter.ToLower());
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

		private IEnumerable<DirectoryInfo> GetDirs(string root)
		{
			var rootDir = new DirectoryInfo(root);

			foreach (var di in rootDir.GetDirectories(".git", SearchOption.AllDirectories))
			{
				yield return di;
			}
			yield break;
		}

		public async void ReadFolder()
		{
			if (string.IsNullOrEmpty(this._config.VsPath))
			{
				return;
			}

			this.IsEnable = false;
			this.Repos.Clear();
			this._config.GitPaths.Clear();

			await Task.Run(() =>
			{
				foreach(var d in GetDirs(this._config.VsPath))
				{
					using (var repo = new Repository(d.FullName))
					{
						var remote = repo.Network.Remotes.FirstOrDefault();
						if (remote?.Url.ToLower().Contains("amazon") ?? false)
						{
							continue;
						}

						var r = new RepositoryModel(d, this._config.AccessToken);
						this.Repos.Add(r);
						_config.GitPaths.Add(new ConfigGitPathModel()
						{
							Name = r.RepoName,
							Path = r.Path
						});
					}
				}

				this.Repos.OrderBy(x => x.RepoName);
				_config.GitPaths.OrderBy(x => x.Name);
			});

			ConfigProvider.Write(_config);

			this.IsEnable = true;
		}

		public async void LoadRepos()
		{
			if (string.IsNullOrEmpty(this._config.VsPath))
			{
				return;
			}

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

		public async void AllFetch()
		{
			if (string.IsNullOrEmpty(this._config.AccessToken))
			{
				System.Windows.MessageBox.Show("AccessToken이 없으면 Fetch할 수 없습니다.");
				return;
			}

			await Task.Run(() =>
			{
				this.Repos.AsParallel().ForAll(r => r.Fetch());
			});

			RepoCollectionViewSource.View.Refresh();
		}

		public async void AllPull()
		{
			if (string.IsNullOrEmpty(this._config.AccessToken))
			{
				System.Windows.MessageBox.Show("AccessToken이 없으면 Fetch할 수 없습니다.");
				return;
			}

			await Task.Run(() =>
			{
				this.Repos.AsParallel().ForAll(r => r.Pull());
			});

			RepoCollectionViewSource.View.Refresh();
		}
	}
}
