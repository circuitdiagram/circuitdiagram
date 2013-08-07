using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace TaskDialogInterop
{
	/// <summary>
	/// Defines a command that is relayed to a pre-defined delegate handler.
	/// </summary>
	internal class RelayCommand : ICommand
	{
		readonly Action _execute;
		readonly Func<bool> _canExecute;

		/// <summary>
		/// Initializes a new instance of the <see cref="RelayCommand"/> class.
		/// </summary>
		/// <param name="execute">A method to execute when the command fires.</param>
		public RelayCommand(Action execute)
			: this(execute, null)
		{
		}
		/// <summary>
		/// Initializes a new instance of the <see cref="RelayCommand"/> class.
		/// </summary>
		/// <param name="execute">A method to execute when the command fires.</param>
		/// <param name="canExecute">A method to execute to determine whether the command can execure.</param>
		public RelayCommand(Action execute, Func<bool> canExecute)
		{
			if (execute == null)
				throw new ArgumentNullException("execute");

			_execute = execute;
			_canExecute = canExecute;
		}

		/// <summary>
		/// Defines the method that determines whether the command can execute in its current state.
		/// </summary>
		/// <param name="parameter">Data used by the command.  If the command does not require data to be passed, this object can be set to null.</param>
		/// <returns>
		/// true if this command can be executed; otherwise, false.
		/// </returns>
		[System.Diagnostics.DebuggerStepThrough]
		public bool CanExecute(object parameter)
		{
			return _canExecute == null ? true : _canExecute();
		}

		/// <summary>
		/// Occurs when changes occur that affect whether or not the command should execute.
		/// </summary>
		public event EventHandler CanExecuteChanged
		{
			add { CommandManager.RequerySuggested += value; }
			remove { CommandManager.RequerySuggested -= value; }
		}

		/// <summary>
		/// Defines the method to be called when the command is invoked.
		/// </summary>
		/// <param name="parameter">Data used by the command.  If the command does not require data to be passed, this object can be set to null.</param>
		public void Execute(object parameter)
		{
			_execute();
		}
	}
	/// <summary>
	/// Defines a command that is relayed to a pre-defined delegate handler.
	/// </summary>
	internal class RelayCommand<T> : ICommand
	{
		readonly Action<T> _execute;
		readonly Predicate<T> _canExecute;

		/// <summary>
		/// Initializes a new instance of the <see cref="RelayCommand"/> class.
		/// </summary>
		/// <param name="execute">A method to execute when the command fires.</param>
		public RelayCommand(Action<T> execute)
			: this(execute, null)
		{
		}
		/// <summary>
		/// Initializes a new instance of the <see cref="RelayCommand"/> class.
		/// </summary>
		/// <param name="execute">A method to execute when the command fires.</param>
		/// <param name="canExecute">A method to execute to determine whether the command can execure.</param>
		public RelayCommand(Action<T> execute, Predicate<T> canExecute)
		{
			if (execute == null)
				throw new ArgumentNullException("execute");

			_execute = execute;
			_canExecute = canExecute;
		}

		/// <summary>
		/// Defines the method that determines whether the command can execute in its current state.
		/// </summary>
		/// <param name="parameter">Data used by the command.  If the command does not require data to be passed, this object can be set to null.</param>
		/// <returns>
		/// true if this command can be executed; otherwise, false.
		/// </returns>
		[System.Diagnostics.DebuggerStepThrough]
		public bool CanExecute(object parameter)
		{
			if (_canExecute == null)
				return true;

			if (parameter.GetType() == typeof(T))
			{
				return _canExecute((T)parameter);
			}
			else
			{
				return _canExecute(default(T));
			}
		}

		/// <summary>
		/// Occurs when changes occur that affect whether or not the command should execute.
		/// </summary>
		public event EventHandler CanExecuteChanged
		{
			add { CommandManager.RequerySuggested += value; }
			remove { CommandManager.RequerySuggested -= value; }
		}

		/// <summary>
		/// Defines the method to be called when the command is invoked.
		/// </summary>
		/// <param name="parameter">Data used by the command.  If the command does not require data to be passed, this object can be set to null.</param>
		public void Execute(object parameter)
		{
			if (parameter != null && parameter.GetType() == typeof(T))
			{
				_execute((T)parameter);
			}
			else
			{
				_execute(default(T));
			}
		}
		/// <summary>
		/// Defines the method to be called when the command is invoked.
		/// </summary>
		/// <param name="parameter">Data used by the command.  If the command does not require data to be passed, this object can be set to null.</param>
		public void Execute(T parameter)
		{
			_execute(parameter);
		}
	}
}
