using gitup.Common;
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
		private RepositoryViewModel _viewModel;
		private ShortcutProvider _shortcutProvider;
		public MainWindow()
		{
			InitializeComponent();

			LogProvider.Instance.LogAdded += Instance_LogAdded;

			#region " Shorcut Add "
			_shortcutProvider = new ShortcutProvider(CommandBindings);
			_shortcutProvider.AddEvent(Key.F, ModifierKeys.Control, (object sender, ExecutedRoutedEventArgs e) =>
			{
				SearchTextBox.Focus();
			});

			_shortcutProvider.AddEvent(Key.Enter, ModifierKeys.Control, (object sender, ExecutedRoutedEventArgs e) =>
			{
				_viewModel.AllFetch();
			}, grd);

			_shortcutProvider.AddEvent(Key.T, ModifierKeys.Control, (object sender, ExecutedRoutedEventArgs e) =>
			{
				foreach (RepositoryModel r in grd.SelectedItems)
				{
					r.Fetch();
				}
			});

			_shortcutProvider.AddEvent(Key.G, ModifierKeys.Control, (object sender, ExecutedRoutedEventArgs e) =>
			{
				var model = grd.SelectedItem as RepositoryModel;
				if (model != null)
				{
					model.GoFork();
				}
			});

			_shortcutProvider.AddEvent(Key.F5, (object sender, ExecutedRoutedEventArgs e) =>
			{
				_viewModel.LoadRepos();
			});
			#endregion

			#region " timer "
			// 한시간에 한번씩 실행
			DispatcherTimer allFetchTimer = new DispatcherTimer();
			allFetchTimer.Interval = TimeSpan.FromHours(1);
			allFetchTimer.Tick += AllFetchTimer_Tick;
			allFetchTimer.Start();
			#endregion

			_viewModel = new RepositoryViewModel();
			this.DataContext = _viewModel;
		}

		private void Instance_LogAdded(object sender, int logType, string log)
		{
			Run r = new Run(log);
			if (logType == 1) // debug
			{
				r.Foreground = Brushes.Green;
			}
			else if (logType == 2) // info
			{
				r.Foreground = Brushes.CadetBlue;
			}
			else if (logType ==3) // error
			{
				r.Foreground = Brushes.IndianRed;
			}

			Bold b = new Bold(r);
			Paragraph p = new Paragraph();
			p.Inlines.Add(b);
			rtbLog.Document.Blocks.Add(p);
			rtbLog.ScrollToEnd();
		}

		private void AllFetchTimer_Tick(object sender, EventArgs e)
		{
			_viewModel.AllFetch();
		}

		private void btnSetting_Click(object sender, RoutedEventArgs e)
		{
			SettingWindow setting = new SettingWindow(_viewModel.Config);
			setting.Owner = this;
			setting.WindowStartupLocation = WindowStartupLocation.CenterOwner;
			if (setting.ShowDialog() ?? false)
			{
				_viewModel.ReadFolder();
			}
		}
	}
}
