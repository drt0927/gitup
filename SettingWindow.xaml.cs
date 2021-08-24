using gitup.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace gitup
{
	/// <summary>
	/// Setting.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class SettingWindow : Window
	{
		private ConfigModel _config;
		public SettingWindow(ConfigModel config)
		{
			InitializeComponent();

			this._config = config;
			tbAccessToken.Text = this._config.AccessToken;
			rbRootPath.IsChecked = this._config.PathType == 1;
			rbSlnPath.IsChecked = this._config.PathType == 2;
			tbRootPath.Text = this._config.RootPath;
			tbSlnPath.Text = this._config.SlnPath;
		}

		private void PathFind_Click(object sender, RoutedEventArgs e)
		{
			FolderBrowserDialog fbd = new FolderBrowserDialog();
			if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				tbRootPath.Text = fbd.SelectedPath;
			}
		}

		private void SlnPathFind_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog fd = new OpenFileDialog();
			fd.Filter = "Solution files (*.sln)|*.sln";
			fd.Multiselect = false;
			if (fd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				tbSlnPath.Text = fd.FileName;
			}
		}

		private void Save_Click(object sender, RoutedEventArgs e)
		{
			this._config.AccessToken = tbAccessToken.Text;
			this._config.PathType = (rbRootPath.IsChecked ?? true) ? 1 : 2;
			this._config.RootPath = tbRootPath.Text;
			this._config.SlnPath = tbSlnPath.Text;
			if (this._config.RootPath != tbRootPath.Text)
			{
				this._config.GitPaths.Clear();
			}
			ConfigProvider.Write(this._config);
			this.DialogResult = true;
			this.Close();
		}

		private void PathType_Checked(object sender, RoutedEventArgs e)
		{
			var rb = sender as System.Windows.Controls.RadioButton;
			if (rb.Name == "rbRootPath")
			{
				tbRootPath.IsEnabled = true;
				btnRootPath.IsEnabled = true;
				tbSlnPath.IsEnabled = false;
				btnSlnPath.IsEnabled = false;
			}
			else if (rb.Name == "rbSlnPath")
			{
				tbRootPath.IsEnabled = false;
				btnRootPath.IsEnabled = false;
				tbSlnPath.IsEnabled = true;
				btnSlnPath.IsEnabled = true;
			}

			tbRootPath.Text = "";
			tbSlnPath.Text = "";
		}
	}
}
