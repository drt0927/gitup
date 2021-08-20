using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace gitup.Utils
{
	public static class DispatcherUtil
	{
		public static DispatcherOperation BeginInvoke(Action action)
		{
			return Application.Current.Dispatcher.BeginInvoke(action, DispatcherPriority.Normal, null);
		}
	}
}
