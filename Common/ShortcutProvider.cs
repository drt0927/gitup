using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace gitup.Common
{
	public class ShortcutProvider
	{
		private CommandBindingCollection _commandBindings;

		public ShortcutProvider(CommandBindingCollection commandBinding)
		{
			this._commandBindings = commandBinding;
		}

		public void AddEvent(Key key, ExecutedRoutedEventHandler action)
		{
			var command = new RoutedCommand();
			command.InputGestures.Add(new KeyGesture(key));
			this._commandBindings.Add(new CommandBinding(command, action));
		}

		public void AddEvent(Key key, ModifierKeys modifierKeys, ExecutedRoutedEventHandler action)
		{
			var command = new RoutedCommand();
			command.InputGestures.Add(new KeyGesture(key, modifierKeys));
			this._commandBindings.Add(new CommandBinding(command, action));
		}

		public void AddEvent(Key key, ModifierKeys modifierKeys, ExecutedRoutedEventHandler action, DataGrid grd)
		{
			var command = new RoutedCommand();
			command.InputGestures.Add(new KeyGesture(key, modifierKeys));
			this._commandBindings.Add(new CommandBinding(command, action));
			grd.CommandBindings.Add(new CommandBinding(command, action));
		}
	}
}
