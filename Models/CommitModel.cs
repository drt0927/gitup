using Devcorner.NIdenticon;
using Devcorner.NIdenticon.BrushGenerators;
using gitup.Extensions;
using LibGit2Sharp;
using Prism.Commands;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace gitup.Models
{
	public class CommitModel
	{
		public string Subject { get; set; }
		public string Author { get; set; }
		public string ShortId { get; set; }
		public string Sha { get; set; }
		public string ParentShortId { get; set; }
		public DateTimeOffset Date { get; set; }
		public string Branches { get; set; }
		public ICommand OpenChangesCommand { get; set; }
		public BitmapImage Avatar { get; set; }
		private string _path { get; set; }
		public CommitModel(Commit commit, string path)
		{
			this._path = path;
			this.Subject = commit.MessageShort;
			this.Author = commit.Author.Name;
			this.ShortId = commit.ShortSha();
			this.Sha = commit.Sha;
			this.Date = commit.Author.When;
			this.ParentShortId = commit.Parents.FirstOrDefault()?.ShortSha();
			OpenChangesCommand = new DelegateCommand(OpenChangesWindow);

			using (MemoryStream ms = new MemoryStream())
			{
				// 문자열 HEX로 변환
				string resultHex = string.Empty;
				byte[] arr_byteStr = Encoding.Default.GetBytes(this.Author);

				foreach (byte byteStr in arr_byteStr)
					resultHex += string.Format("{0:X2}", byteStr);

				// HEX값 Color로 변환
				var color = System.Drawing.ColorTranslator.FromHtml($"#{(resultHex.Length > 6 ? resultHex.Substring(resultHex.Length - 6) : resultHex)}");

				// 단색 Brush 생성
				IBrushGenerator brushGenerator = new StaticColorBrushGenerator(color);

				// Avatar 생성
				var ig = new IdenticonGenerator()
					.WithSize(25, 25)
					.WithBlocks(6, 4)
					.WithAlgorithm("MD5")
					.WithBackgroundColor(Color.White)
					.WithBrushGenerator(brushGenerator);
				var bitmap = ig.Create(this.Author);
				bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);

				// Image 생성
				this.Avatar = new BitmapImage();
				this.Avatar.BeginInit();
				this.Avatar.StreamSource = ms;
				this.Avatar.CacheOption = BitmapCacheOption.OnLoad;
				this.Avatar.EndInit();
			}
		}

		public void OpenChangesWindow()
		{
			using (Repository repo = new Repository(this._path))
			{
				var commit = repo.Commits.FirstOrDefault(c => c.Sha == this.Sha);

				var commitTree = commit.Tree;
				var parentCommitTree = commit.Parents.FirstOrDefault()?.Tree;

				var patch = repo.Diff.Compare<Patch>(parentCommitTree, commitTree);

				ChangesWindow window = new ChangesWindow(patch);
				window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
				window.ShowDialog();
			}
		}
	}
}
