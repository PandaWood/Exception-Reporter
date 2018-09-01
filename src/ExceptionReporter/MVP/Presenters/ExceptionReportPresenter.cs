using System;
using System.Collections.Generic;
using System.Linq;
using ExceptionReporting.Core;
using ExceptionReporting.MVP.Views;
using ExceptionReporting.Network;
using ExceptionReporting.Report;
using ExceptionReporting.SystemInfo;
using ExceptionReporting.Templates;

namespace ExceptionReporting.MVP.Presenters
{
	/// <summary>
	/// The Presenter in this MVP (Model-View-Presenter) implementation 
	/// </summary>
	internal class ExceptionReportPresenter
	{
		private readonly IFileService _fileService;
		private readonly ReportGenerator _reportGenerator;

		/// <summary>
		/// constructor accepting a view and the data/config of the report
		/// </summary>
		public ExceptionReportPresenter(IExceptionReportView view, ReportConfig config, ErrorData error)
		{
			_reportGenerator = new ReportGenerator(config, error);
			_fileService = new FileService();
			View = view;
			Config = config;
			Error = error;
		}

		/// <summary> Report configuration and data  </summary>
		public ReportConfig Config { get; }
		private ErrorData Error { get; }

		/// <summary> The main dialog/view  </summary>
		private IExceptionReportView View { get; }

		private string CreateReport()
		{
			Config.UserExplanation = View.UserExplanation;
			return _reportGenerator.Generate();
		}

		/// <summary>
		/// Save the exception report to file/disk
		/// </summary>
		/// <param name="fileName">the filename to save to</param>
		public void SaveReportToFile(string fileName)
		{
			if (string.IsNullOrEmpty(fileName)) return;

			var report = CreateReport();
			var result = _fileService.Write(fileName, report);
			
			if (!result.Saved)
			{
				View.ShowError(string.Format("Unable to save file '{0}'", fileName), result.Exception);
			}
		}

		/// <summary>
		/// Send the exception report using the configured send method
		/// </summary>
		public void SendReport()
		{
			View.EnableEmailButton = false;
			View.ShowProgressBar = true;
			
			var sender = new SenderFactory(Config, Error, View).Get();
			View.ProgressMessage = sender.ConnectingMessage;
			
			try
			{
				var report = Config.IsSimpleMAPI() ? CreateEmailReport() : CreateReport();
				sender.Send(report);
			}
			catch (Exception exception)
			{		// most exceptions will be thrown in the Sender - this is just a backup
				View.Completed(false);
				View.ShowError(string.Format("Unable to setup {0}", sender.Description) + 
				                Environment.NewLine + exception.Message, exception);
			}
			finally
			{
				if (Config.IsSimpleMAPI())
				{
					View.MapiSendCompleted();
				}
			}
		}

		/// <summary>
		/// copy the report to the clipboard
		/// </summary>
		public void CopyReportToClipboard()
		{
			var report = CreateReport();
			WinFormsClipboard.CopyTo(report);
			View.ProgressMessage = "Copied to clipboard";
		}

		/// <summary>
		/// toggle the detail between 'simple' (just message) and showFullDetail (ie normal)
		/// </summary>
		public void ToggleDetail()
		{
			View.ShowFullDetail = !View.ShowFullDetail;
			View.ToggleShowFullDetail();
		}

		private string CreateEmailReport()
		{
			var template = new TemplateRenderer(new EmailIntroModel
			{
				ScreenshotTaken = Config.TakeScreenshot
			});
			var emailIntro = template.RenderPreset();
			var report = CreateReport();

			return emailIntro + report;
		}

		/// <summary>
		/// Fetch the WMI system information
		/// </summary>
		public IEnumerable<SysInfoResult> GetSysInfoResults()
		{
			return _reportGenerator.GetOrFetchSysInfoResults();
		}

		/// <summary>
		/// The main entry point, populates the report with everything it needs
		/// </summary>
		public void PopulateReport()
		{
			try
			{
				View.SetInProgressState();

				View.PopulateExceptionTab(Error.Exceptions);
				View.PopulateAssembliesTab();
				if (ExceptionReporter.NotRunningMono())
					View.PopulateSysInfoTab();
			}
			finally
			{
				View.SetProgressCompleteState();
			}
		}

		public List<AssemblyRef> GetReferencedAssemblies()
		{
			return new AssemblyDigger(Error.AppAssembly).GetAssemblyRefs().ToList();
		}
	}
}