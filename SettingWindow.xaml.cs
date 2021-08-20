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
			tbPath.Text = this._config.VsPath;
		}

		private void PathFind_Click(object sender, RoutedEventArgs e)
		{
			FolderBrowserDialog fbd = new FolderBrowserDialog();
			if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				tbPath.Text = fbd.SelectedPath;
			}
		}

		private void Save_Click(object sender, RoutedEventArgs e)
		{
			this._config.AccessToken = tbAccessToken.Text;
			this._config.VsPath = tbPath.Text;
			if (this._config.VsPath != tbPath.Text)
			{
				this._config.GitPaths.Clear();
			}
			ConfigProvider.Write(this._config);
			this.DialogResult = true;
			this.Close();
		}
	}
}
