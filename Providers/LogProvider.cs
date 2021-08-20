using gitup.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace gitup.Providers
{
	public sealed class LogProvider
	{
		/// <summary>
		/// 로그 추가 이벤트
		/// </summary>
		/// <param name="sender">LogProvider</param>
		/// <param name="logType">로그 종류 [ 1 - Debug | 2 - Info | 3 - Error ]</param>
		/// <param name="log">로그 내용</param>
		public delegate void LogAddedHandler(object sender, LogLevel logLevel, string log);
		public event LogAddedHandler LogAdded;

		//private 생성자
		private LogProvider() { }
		//private static 인스턴스 객체
		private static readonly Lazy<LogProvider> _instance = new Lazy<LogProvider>(() => new LogProvider(), true);
		//public static 의 객체반환 함수
		public static LogProvider Instance
		{
			get { return _instance.Value; }
		}

		public void Debug(string text)
		{
			SetLog(text, LogLevel.Debug);
		}

		public void Info(string text)
		{
			SetLog(text, LogLevel.Info);
		}

		public void Error(Exception ex, bool isErrorStackTraceShow = false)
		{
			if (isErrorStackTraceShow)
			{
				SetLog(ex.ToString(), LogLevel.Error);
			}
			else
			{
				SetLog(ex.Message, LogLevel.Error);
			}
		}

		public void Error(string text)
		{
			SetLog(text, LogLevel.Error);
		}

		private void SetLog(string text, LogLevel logLevel)
		{
			DispatcherUtil.BeginInvoke(new Action(() =>
			{
				LogAdded?.Invoke(this, logLevel, $"[[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fffff")} - {logLevel.ToString()}]] {text}");
			}));
		}
	}

	public enum LogLevel
	{
		Debug,
		Info,
		Error
	}
}
