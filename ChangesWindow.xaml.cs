using gitup.ViewModels;
using LibGit2Sharp;
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
using System.Windows.Shapes;

namespace gitup
{
	/// <summary>
	/// ChangesWindow.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class ChangesWindow : Window
	{
		ChangesViewModel _viewModel;
		public ChangesWindow()
		{
			InitializeComponent();
		}

		public ChangesWindow(Repository repo) : this()
		{
			_viewModel = new ChangesViewModel(repo);
			this.DataContext = _viewModel;
		}

		public ChangesWindow(Patch patch) : this()
		{
			_viewModel = new ChangesViewModel(patch);
			this.DataContext = _viewModel;
		}
	}
}
