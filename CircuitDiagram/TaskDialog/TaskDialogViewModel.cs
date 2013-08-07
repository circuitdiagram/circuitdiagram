using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace TaskDialogInterop
{
	/// <summary>
	/// Provides commands and properties to the emulated TaskDialog view.
	/// </summary>
	public class TaskDialogViewModel : IActiveTaskDialog, INotifyPropertyChanged
	{
		private static readonly TimeSpan CallbackTimerInterval = new TimeSpan(0, 0, 0, 0, 200);

		private static bool? _isInDesignMode;
		[System.Diagnostics.CodeAnalysis.SuppressMessage(
			"Microsoft.Security",
			"CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands",
			Justification = "The security risk here is neglectible.")]
		internal static bool IsInDesignMode
		{
			get
			{
				if (!_isInDesignMode.HasValue)
				{
					var prop = DesignerProperties.IsInDesignModeProperty;
					_isInDesignMode
						= (bool)DependencyPropertyDescriptor
						.FromProperty(prop, typeof(System.Windows.FrameworkElement))
						.Metadata.DefaultValue;

					// Just to be sure
					if (!_isInDesignMode.Value
						&& System.Diagnostics.Process.GetCurrentProcess().ProcessName.StartsWith
						("devenv", StringComparison.Ordinal))
					{
						_isInDesignMode = true;
					}
				}

				return _isInDesignMode.Value;
			}
		}

		private TaskDialogOptions options;
		private List<TaskDialogButtonData> _normalButtons;
		private List<TaskDialogButtonData> _commandLinks;
		private List<TaskDialogButtonData> _radioButtons;
		private int _dialogResult = -1;
		private int _radioResult = -1;
		private bool _expandedInfoVisible;
		private bool _verificationChecked;
		private bool _preventClose;
		private bool _progressBarMarqueeEnabled;
		private double _progressBarMin;
		private double _progressBarMax;
		private double _progressBarValue;
		private System.Windows.Threading.DispatcherTimer _callbackTimer;
		private DateTime _callbackTimerStart;

		private ICommand _commandNormalButton;
		private ICommand _commandCommandLink;
		private ICommand _commandRadioButton;
		private ICommand _commandHyperlink;

		/// <summary>
		/// Initializes a new instance of the <see cref="TaskDialogViewModel"/> class.
		/// </summary>
		public TaskDialogViewModel()
		{
			_progressBarMin = 0d;
			_progressBarMax = 100d;
		}
		/// <summary>
		/// Initializes a new instance of the <see cref="TaskDialogViewModel"/> class.
		/// </summary>
		/// <param name="options">Options to use.</param>
		public TaskDialogViewModel(TaskDialogOptions options)
			: this()
		{
			this.options = options;

			_expandedInfoVisible = options.ExpandedByDefault;
			_verificationChecked = options.VerificationByDefault;

			if (options.EnableCallbackTimer)
			{
				// By default it will run on the default dispatcher and with Background priority
				_callbackTimer = new System.Windows.Threading.DispatcherTimer();

				_callbackTimer.Interval = CallbackTimerInterval;
				_callbackTimer.Tick += new EventHandler(CallbackTimer_Tick);
			}

			FixAllButtonLabelAccessKeys();

			// If radio buttons are defined, set the radio result to the default selected radio
			if (RadioButtons.Count > 0)
			{
				_radioResult = RadioButtons[DefaultButtonIndex].ID;
			}
		}

		/// <summary>
		/// Gets the window start position.
		/// </summary>
		public System.Windows.WindowStartupLocation StartPosition
		{
			get
			{
				return (options.Owner == null) ? System.Windows.WindowStartupLocation.CenterScreen : System.Windows.WindowStartupLocation.CenterOwner;
			}
		}
		/// <summary>
		/// Gets the window caption.
		/// </summary>
		public string Title
		{
			get
			{
				return String.IsNullOrEmpty(options.Title) ? System.AppDomain.CurrentDomain.FriendlyName : options.Title;
			}
			private set
			{
				options.Title = value;

				RaisePropertyChangedEvent("Title");
			}
		}
		/// <summary>
		/// Gets the principal text for the dialog.
		/// </summary>
		public string MainInstruction
		{
			get
			{
				return options.MainInstruction;
			}
			private set
			{
				options.MainInstruction = value;

				RaisePropertyChangedEvent("MainInstruction");
			}
		}
		/// <summary>
		/// Gets the supplemental text for the dialog.
		/// </summary>
		public string Content
		{
			get
			{
				return options.Content;
			}
			private set
			{
				options.Content = value;

				RaisePropertyChangedEvent("Content");
			}
		}
		/// <summary>
		/// Gets the expanded info text for the dialog's content area.
		/// </summary>
		public string ContentExpandedInfo
		{
			get
			{
				return options.ExpandToFooter ? null : options.ExpandedInfo;
			}
		}
		/// <summary>
		/// Gets the expanded info text for the dialog's footer area.
		/// </summary>
		public string FooterExpandedInfo
		{
			get
			{
				return options.ExpandToFooter ? options.ExpandedInfo : null;
			}
		}
		/// <summary>
		/// Gets a value indicating whether or not any expanded info text has
		/// been set.
		/// </summary>
		public bool HasExpandedInfo
		{
			get
			{
				return !String.IsNullOrEmpty(options.ExpandedInfo);
			}
		}
		/// <summary>
		/// Gets or sets a value indicating whether expanded info is visible.
		/// </summary>
		public bool ExpandedInfoVisible
		{
			get
			{
				return _expandedInfoVisible;
			}
			set
			{
				if (_expandedInfoVisible == value)
					return;

				_expandedInfoVisible = value;

				RaisePropertyChangedEvent("ExpandedInfoVisible");
				RaisePropertyChangedEvent("ContentExpandedInfoVisible");
				RaisePropertyChangedEvent("FooterExpandedInfoVisible");

				var args = new VistaTaskDialogNotificationArgs();

				args.Config = this.options;
				args.Notification = VistaTaskDialogNotification.ExpandoButtonClicked;
				args.Expanded = _expandedInfoVisible;

				OnCallback(args);
			}
		}
		/// <summary>
		/// Gets or sets a value indicating whether content area expanded info is visible.
		/// </summary>
		public bool ContentExpandedInfoVisible
		{
			get
			{
				return !options.ExpandToFooter && _expandedInfoVisible;
			}
		}
		/// <summary>
		/// Gets or sets a value indicating whether footer area expanded info is visible.
		/// </summary>
		public bool FooterExpandedInfoVisible
		{
			get
			{
				return options.ExpandToFooter && _expandedInfoVisible;
			}
		}
		/// <summary>
		/// Gets the verification text.
		/// </summary>
		public string VerificationText
		{
			get
			{
				return options.VerificationText;
			}
			private set
			{
				options.VerificationText = value;

				RaisePropertyChangedEvent("VerificationText");
			}
		}
		/// <summary>
		/// Gets or sets whether the verification checkbox was checked.
		/// </summary>
		public bool VerificationChecked
		{
			get
			{
				return _verificationChecked;
			}
			set
			{
				if (_verificationChecked == value)
					return;

				_verificationChecked = value;

				RaisePropertyChangedEvent("VerificationChecked");

				var args = new VistaTaskDialogNotificationArgs();

				args.Config = this.options;
				args.Notification = VistaTaskDialogNotification.VerificationClicked;
				args.VerificationFlagChecked = _verificationChecked;

				OnCallback(args);
			}
		}
		/// <summary>
		/// Gets the footer text.
		/// </summary>
		public string FooterText
		{
			get
			{
				return options.FooterText;
			}
			private set
			{
				options.FooterText = value;

				RaisePropertyChangedEvent("FooterText");
			}
		}
		/// <summary>
		/// Gets the type of the main icon.
		/// </summary>
		public VistaTaskDialogIcon MainIconType
		{
			get
			{
				return options.MainIcon;
			}
			private set
			{
				options.MainIcon = value;

				RaisePropertyChangedEvent("MainIconType");
			}
		}
		/// <summary>
		/// Gets the main icon.
		/// </summary>
		public System.Windows.Media.ImageSource MainIcon
		{
			get
			{
				return ConvertIconToImageSource(options.MainIcon, options.CustomMainIcon, true);
			}
		}
		/// <summary>
		/// Gets the footer icon.
		/// </summary>
		public System.Windows.Media.ImageSource FooterIcon
		{
			get
			{
				return ConvertIconToImageSource(options.FooterIcon, options.CustomFooterIcon, false);
			}
		}
		/// <summary>
		/// Gets the default button index.
		/// </summary>
		public int DefaultButtonIndex
		{
			get
			{
				return options.DefaultButtonIndex ?? 0;
			}
		}
		/// <summary>
		/// Gets a value indicating whether or not Alt-F4, Esc, and the red X
		/// close button should work.
		/// </summary>
		public bool AllowDialogCancellation
		{
			get
			{
				// Alt-F4 should only work if there is a close button or some other
				//normal button marked as IsCancel or its been overridden
				return options.AllowDialogCancellation
					|| NormalButtons.Any(button => button.IsCancel)
					|| ((options.CommandButtons == null || options.CommandButtons.Length == 0)
						&& (options.RadioButtons == null || options.RadioButtons.Length == 0)
						&& (options.CustomButtons == null || options.CustomButtons.Length == 0));
			}
		}
		/// <summary>
		/// Gets a value indicating whether to show a progress bar.
		/// </summary>
		public bool ShowProgressBar
		{
			get
			{
				return options.ShowProgressBar || options.ShowMarqueeProgressBar;
			}
		}
		/// <summary>
		/// Gets a value indicating whether to show an indeterminate progress bar or a regular one.
		/// </summary>
		public bool ProgressBarIndeterminate
		{
			get
			{
				return options.ShowMarqueeProgressBar && _progressBarMarqueeEnabled;
			}
			private set
			{
				options.ShowMarqueeProgressBar = value;

				RaisePropertyChangedEvent("ShowProgressBar");
				RaisePropertyChangedEvent("ProgressBarIndeterminate");
			}
		}
		/// <summary>
		/// Gets or sets the progress bar's minimum value.
		/// </summary>
		public double ProgressBarMinimum
		{
			get
			{
				return _progressBarMin;
			}
			private set
			{
				_progressBarMin = value;

				RaisePropertyChangedEvent("ProgressBarMinimum");
			}
		}
		/// <summary>
		/// Gets or sets the progress bar's maximum value.
		/// </summary>
		public double ProgressBarMaximum
		{
			get
			{
				return _progressBarMax;
			}
			private set
			{
				_progressBarMax = value;

				RaisePropertyChangedEvent("ProgressBarMaximum");
			}
		}
		/// <summary>
		/// Gets or sets the progress bar's current value.
		/// </summary>
		public double ProgressBarValue
		{
			get
			{
				return _progressBarValue;
			}
			private set
			{
				_progressBarValue = value;

				RaisePropertyChangedEvent("ProgressBarValue");
			}
		}
		/// <summary>
		/// Gets the button labels.
		/// </summary>
		public List<TaskDialogButtonData> NormalButtons
		{
			get
			{
				if (_normalButtons == null)
				{
					// Even if no buttons are specified, show a Close button at minimum
					if (CommandLinks.Count == 0
						&& RadioButtons.Count == 0
						&& (options.CustomButtons == null || options.CustomButtons.Length == 0)
						&& options.CommonButtons == TaskDialogCommonButtons.None)
					{
						_normalButtons = new List<TaskDialogButtonData>();
						_normalButtons.Add(new TaskDialogButtonData(
							(int)VistaTaskDialogCommonButtons.Close,
							VistaTaskDialogCommonButtons.Close.ToString(),
							NormalButtonCommand,
							true, true));
					}
					else if (RadioButtons.Count > 0)
					{
						_normalButtons = new List<TaskDialogButtonData>();
						_normalButtons.Add(new TaskDialogButtonData(
							(int)VistaTaskDialogCommonButtons.OK,
							VistaTaskDialogCommonButtons.OK.ToString(),
							NormalButtonCommand,
							true, false));
					}
					else if (options.CustomButtons != null)
					{
						int i = 0;
						_normalButtons =
							(from button in options.CustomButtons
							 select new TaskDialogButtonData(
								TaskDialog.CustomButtonIDOffset + i,
								button,
								NormalButtonCommand,
								DefaultButtonIndex == i++,
								button.Contains(VistaTaskDialogCommonButtons.Cancel.ToString()) || button.Contains(VistaTaskDialogCommonButtons.Close.ToString())))
							.ToList();
					}
					else if (options.CommonButtons != TaskDialogCommonButtons.None)
					{
						int i = 0;
						VistaTaskDialogCommonButtons comBtns = TaskDialog.ConvertCommonButtons(options.CommonButtons);
						_normalButtons =
							(from button in Enum.GetValues(typeof(VistaTaskDialogCommonButtons)).Cast<int>()
							 where button != (int)VistaTaskDialogCommonButtons.None
								&& comBtns.HasFlag((VistaTaskDialogCommonButtons)button)
							 select TaskDialog.ConvertCommonButton(
								(VistaTaskDialogCommonButtons)button,
								NormalButtonCommand,
								DefaultButtonIndex == i++,
								(VistaTaskDialogCommonButtons)button == VistaTaskDialogCommonButtons.Cancel || (VistaTaskDialogCommonButtons)button == VistaTaskDialogCommonButtons.Close))
							.ToList();
					}
					else
					{
						_normalButtons = new List<TaskDialogButtonData>();
					}
				}

				return _normalButtons;
			}
		}
		/// <summary>
		/// Gets the command link labels.
		/// </summary>
		public List<TaskDialogButtonData> CommandLinks
		{
			get
			{
				if (_commandLinks == null)
				{
					if (options.CommandButtons == null || options.CommandButtons.Length == 0)
					{
						_commandLinks = new List<TaskDialogButtonData>();
					}
					else
					{
						int i = 0;
						_commandLinks = (from button in options.CommandButtons
										   select new TaskDialogButtonData(
											   TaskDialog.CommandButtonIDOffset + i,
											   button,
											   CommandLinkCommand,
											   DefaultButtonIndex == i++,
											   false))
										  .ToList();
					}
				}

				return _commandLinks;
			}
		}
		/// <summary>
		/// Gets the radio button labels.
		/// </summary>
		public List<TaskDialogButtonData> RadioButtons
		{
			get
			{
				if (_radioButtons == null)
				{
					// If command buttons are defined, ignore any radio buttons (unless design mode)
					if ((!IsInDesignMode && CommandLinks.Count > 0)
						|| options.RadioButtons == null || options.RadioButtons.Length == 0)
					{
						_radioButtons = new List<TaskDialogButtonData>();
					}
					else
					{
						int i = 0;
						_radioButtons = (from button in options.RadioButtons
										 select new TaskDialogButtonData(
											 TaskDialog.RadioButtonIDOffset + i,
											 button,
											 RadioButtonCommand,
											 DefaultButtonIndex == i++,
											 false))
										.ToList();
					}
				}

				return _radioButtons;
			}
		}
		/// <summary>
		/// Gets the value of the button or command that was ultimately chosen.
		/// </summary>
		public int DialogResult
		{
			get
			{
				return _dialogResult;
			}
		}
		/// <summary>
		/// Gets the value of the chosen radio option.
		/// </summary>
		public int RadioResult
		{
			get
			{
				return _radioResult;
			}
		}

		/// <summary>
		/// Gets the command associated with custom and common buttons.
		/// </summary>
		public ICommand NormalButtonCommand
		{
			get
			{
				if (_commandNormalButton == null)
				{
					_commandNormalButton = new RelayCommand<int>((i) =>
						{
							_dialogResult = i;

							var args = new VistaTaskDialogNotificationArgs();

							args.Config = this.options;
							args.Notification = VistaTaskDialogNotification.ButtonClicked;
							args.ButtonId = i;

							OnCallback(args);

							RaiseRequestCloseEvent();
						});
				}

				return _commandNormalButton;
			}
		}
		/// <summary>
		/// Gets the command associated with command links.
		/// </summary>
		public ICommand CommandLinkCommand
		{
			get
			{
				if (_commandCommandLink == null)
				{
					_commandCommandLink = new RelayCommand<int>((i) =>
						{
							_dialogResult = i;

							RaiseRequestCloseEvent();
						});
				}

				return _commandCommandLink;
			}
		}
		/// <summary>
		/// Gets the command associated with radio buttons.
		/// </summary>
		public ICommand RadioButtonCommand
		{
			get
			{
				if (_commandRadioButton == null)
				{
					_commandRadioButton = new RelayCommand<int>((i) =>
						{
							_radioResult = i;

							var args = new VistaTaskDialogNotificationArgs();

							args.Config = this.options;
							args.Notification = VistaTaskDialogNotification.RadioButtonClicked;
							args.ButtonId = i;

							OnCallback(args);
						});
				}

				return _commandRadioButton;
			}
		}
		/// <summary>
		/// Gets the command associated with hyperlinks.
		/// </summary>
		public ICommand HyperlinkCommand
		{
			get
			{
				if (_commandHyperlink == null)
				{
					_commandHyperlink = new RelayCommand<string>((uri) =>
						{
							var args = new VistaTaskDialogNotificationArgs();

							args.Config = this.options;
							args.Notification = VistaTaskDialogNotification.HyperlinkClicked;
							args.Hyperlink = uri;

							OnCallback(args);
						});
				}

				return _commandHyperlink;
			}
		}

		/// <summary>
		/// Occurs when a property value changes.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;
		/// <summary>
		/// Occurs when a close call should be performed.
		/// </summary>
		public event EventHandler RequestClose;

		/// <summary>
		/// Returns a value indicating whether or not the dialog should cancel a closing event.
		/// </summary>
		/// <returns><c>true</c> if dialog closing should be canceled; otherwise, <c>false</c></returns>
		public bool ShouldCancelClosing()
		{
			return _preventClose;
		}
		/// <summary>
		/// Notifies any callback handlers that the dialog has been constructed but not yet shown.
		/// </summary>
		public void NotifyConstructed()
		{
			var args = new VistaTaskDialogNotificationArgs();

			args.Config = this.options;
			args.Notification = VistaTaskDialogNotification.DialogConstructed;

			OnCallback(args);
		}
		/// <summary>
		/// Notifies any callback handlers that the dialog has been created but not yet shown.
		/// </summary>
		public void NotifyCreated()
		{
			var args = new VistaTaskDialogNotificationArgs();

			args.Config = this.options;
			args.Notification = VistaTaskDialogNotification.Created;

			OnCallback(args);
		}
		/// <summary>
		/// Notifies any callback handlers periodically if a callback timer has been set.
		/// </summary>
		public void NotifyShown()
		{
			if (options.EnableCallbackTimer)
			{
				_callbackTimerStart = DateTime.Now;
				_callbackTimer.Start();
			}
		}
		/// <summary>
		/// Notifies any callback handlers that the dialog is destroyed.
		/// </summary>
		public void NotifyClosed()
		{
			var args = new VistaTaskDialogNotificationArgs();

			args.Config = this.options;
			args.Notification = VistaTaskDialogNotification.Destroyed;

			OnCallback(args);
		}

		/// <summary>
		/// Raises the <see cref="E:PropertyChanged"/> event for the given property.
		/// </summary>
		/// <param name="propertyName">Name of the property.</param>
		protected void RaisePropertyChangedEvent(string propertyName)
		{
			OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
		}
		/// <summary>
		/// Raises the <see cref="E:RequestClose"/> event.
		/// </summary>
		protected void RaiseRequestCloseEvent()
		{
			OnRequestClose(EventArgs.Empty);
		}
		/// <summary>
		/// Raises the <see cref="E:PropertyChanged"/> event.
		/// </summary>
		/// <param name="e">The <see cref="System.ComponentModel.PropertyChangedEventArgs"/> instance containing the event data.</param>
		protected void OnPropertyChanged(PropertyChangedEventArgs e)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, e);
			}
		}
		/// <summary>
		/// Raises the <see cref="E:RequestClose"/> event.
		/// </summary>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		protected void OnRequestClose(EventArgs e)
		{
			if (RequestClose != null)
			{
				RequestClose(this, e);
			}
		}
		/// <summary>
		/// Raises a callback.
		/// </summary>
		/// <param name="e">The <see cref="VistaTaskDialogNotificationArgs"/> instance containing the event data.</param>
		protected void OnCallback(VistaTaskDialogNotificationArgs e)
		{
			if (options.Callback != null)
			{
				HandleCallbackReturn(e, options.Callback(this, e, options.CallbackData));
			}
		}

		private void HandleCallbackReturn(VistaTaskDialogNotificationArgs e, bool returnValue)
		{
			switch (e.Notification)
			{
				default: // all others
					// Return value ignored according to MSDN
					break;
				case VistaTaskDialogNotification.ButtonClicked:
					// TRUE : prevent dialog from closing
					_preventClose = returnValue;
					break;
				case VistaTaskDialogNotification.Timer:
					// TRUE : reset tickcount
					if (returnValue)
						_callbackTimerStart = DateTime.Now;
					break;
			}
		}
		private void CallbackTimer_Tick(object sender, EventArgs e)
		{
			var args = new VistaTaskDialogNotificationArgs();

			args.Config = this.options;
			args.Notification = VistaTaskDialogNotification.Timer;
			args.TimerTickCount = Convert.ToUInt32(Math.Round(DateTime.Now.Subtract(_callbackTimerStart).TotalMilliseconds, 0));

			OnCallback(args);
		}
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
		private System.Windows.Media.ImageSource ConvertIconToImageSource(VistaTaskDialogIcon icon, Icon customIcon, bool isLarge)
		{
			System.Windows.Media.ImageSource iconSource = null;
			System.Drawing.Icon sysIcon = null;
			System.Drawing.Bitmap altBmp = null;

			try
			{
				switch (icon)
				{
					default:
					case VistaTaskDialogIcon.None:
						break;
					case VistaTaskDialogIcon.Information:
						sysIcon = SystemIcons.Information;
						break;
					case VistaTaskDialogIcon.Warning:
						sysIcon = SystemIcons.Warning;
						break;
					case VistaTaskDialogIcon.Error:
						sysIcon = SystemIcons.Error;
						break;
					case VistaTaskDialogIcon.Shield:
						if (isLarge)
						{
							altBmp = Properties.Resources.shield_32;
						}
						else
						{
							altBmp = Properties.Resources.shield_16;
						}
						break;
				}

				// Custom Icons always take priority
				if (customIcon != null)
				{
					iconSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
						customIcon.Handle,
						System.Windows.Int32Rect.Empty,
						System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
				}
				else if (sysIcon != null)
				{
					iconSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
						sysIcon.Handle,
						System.Windows.Int32Rect.Empty,
						System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
				}
				else if (altBmp != null)
				{
					iconSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
						altBmp.GetHbitmap(),
						IntPtr.Zero,
						System.Windows.Int32Rect.Empty,
						System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
				}
			}
			finally
			{
				// Not responsible for disposing of custom icons

				if (sysIcon != null)
					sysIcon.Dispose();
				if (altBmp != null)
					altBmp.Dispose();
			}

			return iconSource;
		}
		private void FixAllButtonLabelAccessKeys()
		{
			options.CommandButtons = FixLabelAccessKeys(options.CommandButtons);
			options.RadioButtons = FixLabelAccessKeys(options.RadioButtons);
			options.CustomButtons = FixLabelAccessKeys(options.CustomButtons);
		}
		private string[] FixLabelAccessKeys(string[] labels)
		{
			if (labels == null || labels.Length == 0)
				return labels;

			string[] fixedLabels = new string[labels.Length];

			for (int i = 0; i < labels.Length; i++)
			{
				// WPF uses underscores for denoting access keys, whereas TaskDialog
				//expects ampersands
				// First, we escape any existing underscores by doubling them, so that WPF
				//will render them normally
				// Last, we replace any ampersands with underscores
				fixedLabels[i] = labels[i].Replace("_", "__").Replace("&", "_");
			}

			return fixedLabels;
		}

		bool IActiveTaskDialog.SetMarqueeProgressBar(bool marquee)
		{
			//options.ShowProgressBar = false; // do we need this? does setting marquee to true override in the native implementation?
			
			ProgressBarIndeterminate = marquee;

			return true;
		}
		bool IActiveTaskDialog.SetProgressBarState(VistaProgressBarState newState)
		{
			// TODO Support progress bar state colors on the emulated form
			// Might be able to do it with some triggers... binding directly to the
			//Foreground property will overwrite default OS progress bar color
			return false;
		}
		bool IActiveTaskDialog.SetProgressBarRange(short minRange, short maxRange)
		{
			ProgressBarMinimum = Convert.ToDouble(minRange);
			ProgressBarMaximum = Convert.ToDouble(maxRange);

			return true;
		}
		int IActiveTaskDialog.SetProgressBarPosition(int newPosition)
		{
			int prevValue = Convert.ToInt32(ProgressBarValue);

			ProgressBarValue = Convert.ToDouble(newPosition);

			return prevValue;
		}
		void IActiveTaskDialog.SetProgressBarMarquee(bool startMarquee, uint speed)
		{
			// speed setting is ignored

			_progressBarMarqueeEnabled = startMarquee;

			RaisePropertyChangedEvent("ProgressBarIndeterminate");
		}
		bool IActiveTaskDialog.SetWindowTitle(string title)
		{
			Title = title;

			return true;
		}
		bool IActiveTaskDialog.SetContent(string content)
		{
			Content = content;

			return true;
		}
		bool IActiveTaskDialog.SetExpandedInformation(string expandedInformation)
		{
			options.ExpandedInfo = expandedInformation;

			RaisePropertyChangedEvent("ContentExpandedInfo");
			RaisePropertyChangedEvent("FooterExpandedInfo");

			return true;
		}
		bool IActiveTaskDialog.SetFooter(string footer)
		{
			FooterText = footer;

			return true;
		}
		bool IActiveTaskDialog.SetMainInstruction(string mainInstruction)
		{
			MainInstruction = mainInstruction;

			return true;
		}
		void IActiveTaskDialog.UpdateMainIcon(VistaTaskDialogIcon icon)
		{
			options.MainIcon = icon;

			RaisePropertyChangedEvent("MainIconType");
			RaisePropertyChangedEvent("MainIcon");
		}
		void IActiveTaskDialog.UpdateMainIcon(Icon icon)
		{
			options.CustomMainIcon = icon;

			RaisePropertyChangedEvent("MainIcon");
		}
		void IActiveTaskDialog.UpdateFooterIcon(VistaTaskDialogIcon icon)
		{
			options.FooterIcon = icon;

			RaisePropertyChangedEvent("FooterIcon");
		}
		void IActiveTaskDialog.UpdateFooterIcon(Icon icon)
		{
			options.CustomFooterIcon = icon;

			RaisePropertyChangedEvent("FooterIcon");
		}
	}
}
