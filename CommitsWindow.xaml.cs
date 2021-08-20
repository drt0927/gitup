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
	/// CommitsWindow.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class CommitsWindow : Window
	{
		private CommitsViewModel _commitsViewModel;

		public CommitsWindow(string path)
		{
			InitializeComponent();

			_commitsViewModel = new CommitsViewModel(path);
			this.DataContext = _commitsViewModel;
		}
	}
}
