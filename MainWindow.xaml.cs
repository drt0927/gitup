using gitup.Providers;
using gitup.Models;
using gitup.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace gitup
{
	/// <summary>
	/// MainWindow.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class MainWindow : Window
	{
		private MainVieModel _viewModel;
		private ShortcutProvider _shortcutProvider;
		public MainWindow()
		{
			InitializeComponent();

			LogProvider.Instance.LogAdded += Instance_LogAdded;

			#region " Shortcut Add "
			_shortcutProvider = new ShortcutProvider(CommandBindings);
			_shortcutProvider.AddEvent(Key.F, ModifierKeys.Control, (object sender, ExecutedRoutedEventArgs e) =>
			{
				SearchTextBox.Focus();
			});

			_shortcutProvider.AddEvent(Key.Enter, ModifierKeys.Control, (async (object sender, ExecutedRoutedEventArgs e) =>
			{
				await _viewModel.AllFetch();
			}), grd);

			_shortcutProvider.AddEvent(Key.T, ModifierKeys.Control, (async (object sender, ExecutedRoutedEventArgs e) =>
			{
				await SelectedFetch();
			}));

			_shortcutProvider.AddEvent(Key.G, ModifierKeys.Control, (object sender, ExecutedRoutedEventArgs e) =>
			{
				var model = grd.SelectedItem as RepositoryModel;
				if (model != null)
				{
					model.GoFork();
				}
			});

			_shortcutProvider.AddEvent(Key.F5, (async (object sender, ExecutedRoutedEventArgs e) =>
			{
				await _viewModel.LoadRepos();
			}));
			#endregion

			#region " timer "
			// 한시간에 한번씩 실행
			DispatcherTimer allFetchTimer = new DispatcherTimer();
			allFetchTimer.Interval = TimeSpan.FromHours(1);
			allFetchTimer.Tick += AllFetchTimer_Tick;
			allFetchTimer.Start();
			#endregion

			_viewModel = new MainVieModel();
			this.DataContext = _viewModel;
		}

		private void Instance_LogAdded(object sender, LogLevel logLevel, string log)
		{
			Run r = new Run(log);
			if (logLevel == LogLevel.Debug)
			{
				r.Foreground = Brushes.Green;
			}
			else if (logLevel == LogLevel.Info)
			{
				r.Foreground = Brushes.CadetBlue;
			}
			else if (logLevel == LogLevel.Error)
			{
				r.Foreground = Brushes.IndianRed;
			}

			Bold b = new Bold(r);
			Paragraph p = new Paragraph();
			p.Inlines.Add(b);
			rtbLog.Document.Blocks.Add(p);
			rtbLog.ScrollToEnd();
		}

		private async Task SelectedFetch()
		{
			var items = grd.SelectedItems;
			await Task.Run(() =>
			{
				foreach (RepositoryModel r in items)
				{
					r.Fetch();
				}
			});
		}

		private async Task selectedPull()
		{
			var items = grd.SelectedItems;
			await Task.Run(() =>
			{
				foreach (RepositoryModel r in items)
				{
					r.Pull();
				}
			});
		}

		private async void AllFetchTimer_Tick(object sender, EventArgs e)
		{
			await _viewModel.AllFetch();
		}

		private void btnSetting_Click(object sender, RoutedEventArgs e)
		{
			SettingWindow setting = new SettingWindow(_viewModel.Config);
			setting.Owner = this;
			setting.WindowStartupLocation = WindowStartupLocation.CenterOwner;
			if (setting.ShowDialog() ?? false)
			{
				rtbLog.Document.Blocks.Clear();
				if (_viewModel.Config.PathType == 1)
				{
					_viewModel.ReadFolder();
				}
				else
				{
					_viewModel.ReadSlnFile();
				}
			}
		}

		private async void SelFetch_Click(object sender, RoutedEventArgs e)
		{
			await SelectedFetch();
		}

		private async void SelPull_Click(object sender, RoutedEventArgs e)
		{
			await selectedPull();
		}

		private async void Refresh_Click(object sender, RoutedEventArgs e)
		{
			await _viewModel.LoadRepos();
		}

		private async void MenuItem_Click(object sender, RoutedEventArgs e)
		{
			BulkCreateBranch bulkCreateBranch = new BulkCreateBranch();
			bulkCreateBranch.Owner = this;
			bulkCreateBranch.WindowStartupLocation = WindowStartupLocation.CenterOwner;
			if (bulkCreateBranch.ShowDialog() ?? false)
			{
				var name = bulkCreateBranch.GetCreateBranchName();

				var items = grd.SelectedItems;
				await Task.Run(() =>
				{
					foreach (RepositoryModel r in items)
					{
						r.CreateBranch(name);
					}
				});

				await _viewModel.LoadRepos();
			}
		}
	}
}
