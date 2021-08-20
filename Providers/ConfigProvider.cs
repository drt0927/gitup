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
		static string filePath = System.IO.Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
		static string fileName = "gitup-config.json";
		public static void Write(ConfigModel config)
		{
			File.WriteAllText(filePath + @"\" + fileName, JsonConvert.SerializeObject(config, Formatting.Indented), Encoding.UTF8);
		}

		public static ConfigModel Read()
		{
			if (File.Exists(filePath + @"\" + fileName))
			{
				var config = File.ReadAllText(filePath + @"\" + fileName);
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
		public string VsPath { get; set; }
		public List<ConfigGitPathModel> GitPaths { get; set; }
	}

	public class ConfigGitPathModel
	{
		public string Path;
		public string Name;
	}
}
