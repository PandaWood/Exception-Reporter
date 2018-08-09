// MIT License
// Copyright (c) 2008-2018 Peter van der Woude
// https://github.com/PandaWood/ExceptionReporter.NET
//

using System;
using System.Reflection;
using System.Windows.Forms;
using ExceptionReporting.Core;
using ExceptionReporting.Views;
using static ExceptionReporting.Core.ExceptionReportInfo;

// ReSharper disable UnusedMember.Global

namespace ExceptionReporting
{
  /// <summary>
  /// The entry-point (class) to invoking an ExceptionReporter dialog
  /// eg new ExceptionReporter().Show()
  /// </summary>
  public class ExceptionReporter
  {
	private readonly ExceptionReportInfo _reportInfo;
	private IExceptionReportView _view;

	/// <summary>
	/// Initialise the ExceptionReporter
	/// <remarks>readConfig() should be called (explicitly) if you need to override default config settings</remarks>
	/// </summary>
	public ExceptionReporter()
	{
	  var callingAssembly = Assembly.GetCallingAssembly();

	  _reportInfo = new ExceptionReportInfo { AppAssembly = callingAssembly };
	}

	/// <summary>
	/// Public access to configuration
	/// </summary>
	public ExceptionReportInfo Config
	{
	  get { return _reportInfo; }
	}

	/// <summary>
	/// Show the ExceptionReport dialog
	/// </summary>
	/// <remarks>The <see cref="ExceptionReporter"/> will analyze the <see cref="Exception"/>s and 
	/// create and show the report dialog.</remarks>
	/// <param name="exceptions">The <see cref="Exception"/>s to show.</param>
	public void Show(params Exception[] exceptions)
	{
	  if (exceptions == null) return;   //TODO perhaps show a dialog that says "No exception to show" ?

	  try
	  {
		_reportInfo.SetExceptions(exceptions);
		_view = new ExceptionReportView(_reportInfo);
		_view.ShowExceptionReport();
	  }
	  catch (Exception internalException)
	  {
		MessageBox.Show(internalException.Message, "Error Reporting Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
	  }
	}

	/// <summary>
	/// Show the ExceptionReport dialog with a custom message instead of the Exception's Message property
	/// </summary>
	/// <param name="customMessage">custom (exception) message</param>
	/// <param name="exceptions">The exception/s to display in the exception report</param>
	public void Show(string customMessage, params Exception[] exceptions)
	{
	  _reportInfo.CustomMessage = customMessage;
	  Show(exceptions);
	}

	/// <summary>
	/// Send the report without showing a dialog (silent send)
	/// MailMethod must be set to SMTP or WebService, else this is ignored (silently)
	/// </summary>
	/// <param name="method">Method to send report</param>
	/// <param name="exceptions">The exception/s to display in the exception report</param>
	public void Send(SendMethod method, params Exception[] exceptions)
	{
	  _reportInfo.SetExceptions(exceptions);
	  var generator = new ExceptionReportGenerator(_reportInfo);

	  switch (method)
	  {

		case SendMethod.SMTP:
		  generator.SendReportByEmail();
		  break;

		case SendMethod.WebService:
		  generator.SendReportToWebService();
		  break;

		case SendMethod.WebPage:
		  generator.SendReportToWebPage();
		  break;

		case SendMethod.SimpleMAPI:
		  break;

	  }


		/*
		switch (_reportInfo.MailMethod)
		{
			case ExceptionReportInfo.EmailMethod.SMTP:
				generator.SendReportByEmail();
				break;

			case ExceptionReportInfo.EmailMethod.WebService:
				generator.SendReportToWebService();
				break;

			case ExceptionReportInfo.EmailMethod.None:
				break;

			case ExceptionReportInfo.EmailMethod.SimpleMAPI:
				break;
		}*/



	  }

	static readonly bool _isRunningMono = System.Type.GetType("Mono.Runtime") != null;

	/// <returns><c>true</c>, if running mono <c>false</c> otherwise.</returns>
	public static bool IsRunningMono() { return _isRunningMono; }

  }
}

// ReSharper restore UnusedMember.Global
