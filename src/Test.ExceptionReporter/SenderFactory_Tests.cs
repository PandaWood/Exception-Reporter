﻿using System;
using ExceptionReporting.Network;
using ExceptionReporting.Network.Events;
using ExceptionReporting.Network.Senders;
using Moq;
using NUnit.Framework;

namespace ExceptionReporting.Tests
{
	public class SenderFactory_Tests
	{
		[TestCase(ReportSendMethod.None,       ExpectedResult = typeof(GhostSender))]
		[TestCase(ReportSendMethod.SimpleMAPI, ExpectedResult = typeof(MapiMailSender))]
		[TestCase(ReportSendMethod.SMTP,       ExpectedResult = typeof(SmtpMailSender))]
		[TestCase(ReportSendMethod.WebService, ExpectedResult = typeof(WebServiceSender))]
		public Type Can_Determine_Sender(ReportSendMethod method)
		{
			var factory = new SenderFactory(new ReportBag(new ErrorDetail(), new ReportConfig 
			{ 
				SendMethod = method
			}), new Mock<IReportSendEvent>().Object);

			return factory.Get().GetType();
		}
	}
}