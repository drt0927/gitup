using gitup.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gitup.Providers
{
	public static class ConfigProvider
	{
		private static string filePath = System.IO.Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
		private static string fileName = "gitup-config.json";
		private static string fileFullPath => $@"{filePath}\{fileName}";
		public static void Write(ConfigModel config)
		{
			File.WriteAllText(fileFullPath, JsonConvert.SerializeObject(config, Formatting.Indented), Encoding.UTF8);
		}

		public static ConfigModel Read()
		{
			if (File.Exists(fileFullPath))
			{
				var config = File.ReadAllText(fileFullPath);
				var model = JsonConvert.DeserializeObject<ConfigModel>(config);
				if (model.GitPaths == null)
				{
					model.GitPaths = new List<ConfigGitPathModel>();
				}

				return model;
			}

			return new ConfigModel()
			{
				GitPaths = new List<ConfigGitPathModel>()
			};
		}
	}

	public class ConfigModel
	{
		public string AccessToken { get; set; }
		/// <summary>
		/// 경로 타입 [ 1 - Root Folder Path | 2 - Solution File Path ]
		/// </summary>
		public int PathType { get; set; }
		public string RootPath { get; set; }
		public string SlnPath { get; set; }
		public List<ConfigGitPathModel> GitPaths { get; set; }
	}

	public class ConfigGitPathModel
	{
		public string Path;
		public string Name;
		public ConfigGitPathModel()
		{

		}
		public ConfigGitPathModel(RepositoryModel model)
		{
			this.Path = model.Path;
			this.Name = model.RepoName;
		}
	}
}
