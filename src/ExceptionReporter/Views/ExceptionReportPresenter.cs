using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using ExceptionReporting.Core;
using ExceptionReporting.Mail;
using ExceptionReporting.Network;
using ExceptionReporting.Network.Senders;
using ExceptionReporting.SystemInfo;

namespace ExceptionReporting.Views
{
	/// <summary>
	/// The Presenter in this MVP (Model-View-Presenter) implementation 
	/// </summary>
	internal class ExceptionReportPresenter
	{
		private readonly IExceptionReportView _view;
		private readonly ExceptionReportGenerator _reportGenerator;

		/// <summary>
		/// constructor accepting a view and the data/config of the report
		/// </summary>
		public ExceptionReportPresenter(IExceptionReportView view, ExceptionReportInfo info)
		{
			_view = view;
			_reportGenerator = new ExceptionReportGenerator(info);
			ReportInfo = info;
		}

		/// <summary>
		/// The application assembly - ie the main application using the exception reporter assembly
		/// </summary>
		public Assembly AppAssembly
		{
			get { return ReportInfo.AppAssembly; }
		}

		/// <summary>
		/// Report configuration and data
		/// </summary>
		public ExceptionReportInfo ReportInfo { get; }

		private ExceptionReport CreateExceptionReport()
		{
			ReportInfo.UserExplanation = _view.UserExplanation;
			return _reportGenerator.CreateExceptionReport();
		}

		/// <summary>
		/// Save the exception report to file/disk
		/// </summary>
		/// <param name="fileName">the filename to save</param>
		public void SaveReportToFile(string fileName)
		{
			if (string.IsNullOrEmpty(fileName)) return;

			var exceptionReport = CreateExceptionReport();

			try
			{
				using (var stream = File.OpenWrite(fileName))
				{
					var writer = new StreamWriter(stream);
					writer.Write(exceptionReport);
					writer.Flush();
				}
			}
			catch (Exception exception)
			{
				_view.ShowError(string.Format("Unable to save file '{0}'", fileName), exception);
			}
		}

		/// <summary>
		/// Send the exception report via email, using the configured email method/type
		/// </summary>
		public void SendReport()
		{
			if (ReportInfo.SendMethod == ReportSendMethod.WebService)
			{
				Send(new WebServiceSender(ReportInfo, _view));
			}
			else if (ReportInfo.SendMethod == ReportSendMethod.SMTP ||
			         ReportInfo.MailMethod == ExceptionReportInfo.EmailMethod.SMTP)		// backwards compatibility
			{
				Send(new SmtpMailSender(ReportInfo, _view));
			}
			else if (ReportInfo.SendMethod == ReportSendMethod.SimpleMAPI ||
			    ReportInfo.MailMethod == ExceptionReportInfo.EmailMethod.SimpleMAPI)		// backwards compatibility
			{		// this option must be last for compatibility because MailMethod.SimpleMAPI was previously 0/default
				Send(new MapiMailSender(ReportInfo, _view));
			}
		}

		/// <summary>
		/// copy the report to the clipboard
		/// </summary>
		public void CopyReportToClipboard()
		{
			var exceptionReport = CreateExceptionReport();
			WinFormsClipboard.CopyTo(exceptionReport.ToString());
			_view.ProgressMessage = string.Format("{0} copied to clipboard", ReportInfo.TitleText);
		}

		/// <summary>
		/// toggle the detail between 'simple' (just message) and showFullDetail (ie normal)
		/// </summary>
		public void ToggleDetail()
		{
			_view.ShowFullDetail = !_view.ShowFullDetail;
			_view.ToggleShowFullDetail();
		}

		private string BuildReportString()
		{
			var emailTextBuilder = new EmailTextBuilder();
			var emailIntroString = emailTextBuilder.CreateIntro(ReportInfo.TakeScreenshot);
			var entireEmailText = new StringBuilder(emailIntroString);

			var report = CreateExceptionReport();
			entireEmailText.Append(report);

			return entireEmailText.ToString();
		}

		private void Send(IReportSender sender)
		{
			_view.ProgressMessage = sender.ConnectingMessage;
			_view.EnableEmailButton = false;
			_view.ShowProgressBar = true;
			
			try
			{
				var report = BuildReportString();
				sender.Send(report);
			}
			catch (Exception exception)
			{		// most/all exceptions will be thrown in the WebServiceSender - this is just a double backup
				_view.Completed(false);
				_view.ShowError(string.Format("Unable to setup {0}", sender.Description) + Environment.NewLine + exception.Message, exception);
			}
		}

//			finally
//			{
//				_view.SetEmailCompletedState_WithMessageIfSuccess(success, string.Empty);
//			}
//		}

		/// <summary>
		/// Fetch the WMI system information
		/// </summary>
		public IEnumerable<SysInfoResult> GetSysInfoResults()
		{
			return _reportGenerator.GetOrFetchSysInfoResults();
		}

		/// <summary>
		/// Send email (using ShellExecute) to the configured contact email address
		/// </summary>
		public void SendContactEmail()
		{
			ShellExecute(string.Format("mailto:{0}", ReportInfo.ContactEmail));
		}

		/// <summary>
		/// Navigate to the website configured
		/// </summary>
		public void NavigateToWebsite()
		{
			ShellExecute(ReportInfo.WebUrl);
		}

		private void ShellExecute(string executeString)
		{
			try
			{
				var psi = new ProcessStartInfo(executeString) { UseShellExecute = true };
				Process.Start(psi);
			}
			catch (Exception exception)
			{
				_view.ShowError(string.Format("Unable to (Shell) Execute '{0}'", executeString), exception);
			}
		}

		/// <summary>
		/// The main entry point, populates the report with everything it needs
		/// </summary>
		public void PopulateReport()
		{
			try
			{
				_view.SetInProgressState();

				_view.PopulateExceptionTab(ReportInfo.Exceptions);
				_view.PopulateAssembliesTab();
				if (ExceptionReporter.NotRunningMono())
					_view.PopulateSysInfoTab();
			}
			finally
			{
				_view.SetProgressCompleteState();
			}
		}

		/// <summary>
		/// Close/cleanup
		/// </summary>
		public void Close()
		{
			_reportGenerator.Dispose();
		}
	}
}