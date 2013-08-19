using StockScanner.Scans;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace StockScanner
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window, INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;
		public List<Scan> AvailableScans { get; set; }
		private ScanController controller;
		private bool isRepeat = false;
		private bool isAuto = false;
		private readonly System.Timers.Timer refreshTimer = new System.Timers.Timer();		
		
		void OnElapsedTimer(object sender, ElapsedEventArgs e)
		{
			List<Scan> runningScans = new List<Scan>();
			runningScans = AvailableScans.Where(x => x.IsRunning).ToList();
			
			if (runningScans.Count > 0)
			{
				foreach (Scan scan in runningScans)
				{
					scan.ElapsedTime = DateTime.Now - scan.StartTime;
				}	
			}
			if (!isAuto)
				return;
			List<Scan> completedScans = new List<Scan>();
			completedScans = AvailableScans.Where(x => (!x.IsRunning) && (DateTime.Now > x.StopTime.AddSeconds(5))).ToList();
			if (completedScans.Count == AvailableScans.Count)
				Process.GetCurrentProcess().Kill();

			//if (isRepeat && restartScans.Count > 0)
			//{
			//	controller.StartScan(restartScans);
			//}							
		}

		// Create the OnPropertyChanged method to raise the event 
		protected void OnPropertyChanged(string name)
		{
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null)
			{
				handler(this, new PropertyChangedEventArgs(name));
			}
		}

		private void StartTimer()
		{
			refreshTimer.Elapsed += OnElapsedTimer;
			refreshTimer.Interval = 1000;
			refreshTimer.Start();
		}

		public MainWindow()
		{
			DataContext = this;			
			Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
			var worker = new ScanFactory();
			AvailableScans = worker.CreateScans();
			controller = new ScanController();
			StartTimer();
			
			InitializeComponent();
			var startParameter = Environment.GetCommandLineArgs();
			if (startParameter.Count() > 1)
				if (startParameter[1] == "auto")
				{
					controller.StartScan(AvailableScans);
					isAuto = true;
				}
		}		
		
		private void OnClickScanAll(object sender, RoutedEventArgs e)
		{			
			controller.StartScan(AvailableScans);
		}

		private void OnClickScanAllRepeat(object sender, RoutedEventArgs e)
		{
			isRepeat = !isRepeat;
			controller.StartScan(AvailableScans);
		}

		private void OnClickScanSingle(object sender, RoutedEventArgs e)
		{
			object clicked = (e.OriginalSource as FrameworkElement).DataContext;
			var lbi = listbox.ItemContainerGenerator.ContainerFromItem(clicked) as ListBoxItem;			
			lbi.IsSelected = true;
			var scanlist  = new List<Scan>();
			scanlist.Add((Scan)listbox.SelectedItems[0]);			
			controller.StartScan(scanlist);
		}
	}
}

