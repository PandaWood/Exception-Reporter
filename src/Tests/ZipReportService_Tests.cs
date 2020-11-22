using System.Collections.Generic;
using System.Linq;
using ExceptionReporting;
using ExceptionReporting.Core;
using ExceptionReporting.Mail;
using Moq;
using NUnit.Framework;

namespace Tests.ExceptionReporting
{
	public class ZipReportService_Tests
	{
		//private Mock<IAttach> _iAttach;
		private Mock<IScreenshotTaker> _screenshotTaker;
		private Mock<IFileService> _fileService;
		private Mock<IZipper> _zipper;

		[SetUp]
		public void SetUp()
		{
			//_iAttach = new Mock<IAttach>();
			_screenshotTaker = new Mock<IScreenshotTaker>();
			_fileService = new Mock<IFileService>();
			_zipper = new Mock<IZipper>();
		}

		[Test]
		public void None_Files_To_Add_To_Archive_ReturnsEmptyString()
		{
			var zipFilename = "Test.zip";
			_fileService.Setup(f => f.TempFile(zipFilename)).Returns(zipFilename);
			_zipper.Setup(z => z.Zip(zipFilename, It.IsAny<IEnumerable<string>>()));

			var filesToAttach = new List<string>();
			var config = new ExceptionReportInfo()
				{ FilesToAttach = filesToAttach.ToArray(), TakeScreenshot = false, AttachmentFilename = zipFilename };
			var zipReportService = new ZipReportService(_zipper.Object, _screenshotTaker.Object, _fileService.Object);

			var result = zipReportService.CreateZipReport(config);

			Assert.IsTrue(result == string.Empty);
			_zipper.Verify(z => z.Zip(zipFilename, It.IsAny<IEnumerable<string>>()), Times.Never);
			Mock.Verify();
		}


		[Test]
		public void Two_Files_But_None_Exists_ReturnsEmptyString()
		{
			var logFile1 = "file1.log";
			var logFile2 = "file2.log";
			var zipFilename = "Test.zip";
			_fileService.Setup(f => f.Exists(logFile1)).Returns(false);
			_fileService.Setup(f => f.Exists(logFile2)).Returns(false);
			_fileService.Setup(f => f.Exists(zipFilename)).Returns(true);
			_fileService.Setup(f => f.TempFile(zipFilename)).Returns(zipFilename);
			_zipper.Setup(z => z.Zip(zipFilename, It.IsAny<IEnumerable<string>>()));

			var filesToAttach = new List<string>() { logFile1, logFile2 };
			var config = new ExceptionReportInfo()
				{ FilesToAttach = filesToAttach.ToArray(), TakeScreenshot = false, AttachmentFilename = zipFilename };
			var zipReportService = new ZipReportService(_zipper.Object, _screenshotTaker.Object, _fileService.Object);

			var result = zipReportService.CreateZipReport(config);

			Assert.IsTrue(result == string.Empty);
			_zipper.Verify(z => z.Zip(zipFilename, It.IsAny<IEnumerable<string>>()), Times.Never);
			Mock.Verify();
		}

		[Test]
		public void One_File_To_Add_To_Archive_ReturnsArchiveWithOneFile()
		{
			var logFile = "file1.log";
			var zipFilename = "Test.zip";
			_fileService.Setup(f => f.Exists(logFile)).Returns(true);
			_fileService.Setup(f => f.Exists(zipFilename)).Returns(true);
			_fileService.Setup(f => f.TempFile(zipFilename)).Returns(zipFilename);

			var filesToAttach = new List<string>() {logFile};
			var config = new ExceptionReportInfo()
				{FilesToAttach = filesToAttach.ToArray(), TakeScreenshot = false, AttachmentFilename = zipFilename};
			var zipReportService = new ZipReportService(_zipper.Object, _screenshotTaker.Object, _fileService.Object);

			var result = zipReportService.CreateZipReport(config);

			Assert.IsTrue(result == zipFilename);
			_zipper.Verify(z => z.Zip(zipFilename, It.Is<IEnumerable<string>>(en => en.Count() == 1)), Times.AtLeastOnce);
			Mock.Verify();
		}
		[Test]
		public void Only_Screenshot_To_Add_To_Archive_ReturnsArchiveWithOneFile()
		{
			var zipFilename = "Test.zip";
			var screenshotFilename = "Screenshot.jpg";
			_screenshotTaker.Setup(s => s.TakeScreenShot()).Returns(screenshotFilename);
			_fileService.Setup(f => f.Exists(screenshotFilename)).Returns(true);
			_fileService.Setup(f => f.Exists(zipFilename)).Returns(true);
			_fileService.Setup(f => f.TempFile(zipFilename)).Returns(zipFilename);
			_zipper.Setup(z => z.Zip(zipFilename, It.Is<IEnumerable<string>>(en => en.Count() == 1)));

			var filesToAttach = new List<string>();// { logFile };
			var config = new ExceptionReportInfo()
				{ FilesToAttach = filesToAttach.ToArray(), TakeScreenshot = true, AttachmentFilename = zipFilename };
			var zipReportService = new ZipReportService(_zipper.Object, _screenshotTaker.Object, _fileService.Object);

			var result = zipReportService.CreateZipReport(config);

			Assert.IsTrue(result == zipFilename);
			_zipper.Verify(z => z.Zip(zipFilename, It.Is<IEnumerable<string>>(en => en.Count() == 1)), Times.AtLeastOnce);
			Mock.Verify();
		}

	[Test]
	public void Two_Files_To_Add_To_Archive_ReturnsArchiveWithTwoFiles()
	{
		var logFile1 = "file1.log";
		var logFile2 = "file2.log";
		var zipFilename = "Test.zip";
		_fileService.Setup(f => f.Exists(logFile1)).Returns(true);
		_fileService.Setup(f => f.Exists(logFile2)).Returns(true);
		_fileService.Setup(f => f.Exists(zipFilename)).Returns(true);
		_fileService.Setup(f => f.TempFile(zipFilename)).Returns(zipFilename);

		var filesToAttach = new List<string>() { logFile1, logFile2 };
		var config = new ExceptionReportInfo()
			{ FilesToAttach = filesToAttach.ToArray(), TakeScreenshot = false, AttachmentFilename = zipFilename };
		var zipReportService = new ZipReportService(_zipper.Object, _screenshotTaker.Object, _fileService.Object);

		var result = zipReportService.CreateZipReport(config);

		Assert.IsTrue(result == zipFilename);
		_zipper.Verify(z => z.Zip(zipFilename, It.Is<IEnumerable<string>>(en => en.Count() == 2)), Times.AtLeastOnce);
		Mock.Verify();
		}

	[Test]
	public void Take_Screenshot_True_MakesScreenshot()
	{
		var zipFilename = "Test.zip";
		var screenshotFilename = "Screenshot.jpg";
		_screenshotTaker.Setup(s => s.TakeScreenShot()).Returns(screenshotFilename);
		_fileService.Setup(f => f.Exists(screenshotFilename)).Returns(true);
		_fileService.Setup(f => f.TempFile(zipFilename)).Returns(zipFilename);

		var config = new ExceptionReportInfo()
			{TakeScreenshot = true, AttachmentFilename = zipFilename};
		var zipReportService = new ZipReportService(_zipper.Object, _screenshotTaker.Object, _fileService.Object);
		zipReportService.CreateZipReport(config);

		_screenshotTaker.Verify(s => s.TakeScreenShot(), Times.Once());
		Mock.Verify();
	}

	[Test]
	public void Take_Screenshot_False_DoesNotMakeScreenshot()
	{
		var zipFilename = "Test.zip";
		var screenshotFilename = "Screenshot.jpg";
		_screenshotTaker.Setup(s => s.TakeScreenShot()).Returns(screenshotFilename);
		_fileService.Setup(f => f.Exists(screenshotFilename)).Returns(true);
		_fileService.Setup(f => f.TempFile(zipFilename)).Returns(zipFilename);

		var config = new ExceptionReportInfo()
			{ TakeScreenshot = false, AttachmentFilename = zipFilename };
		var zipReportService = new ZipReportService(_zipper.Object, _screenshotTaker.Object, _fileService.Object);
		zipReportService.CreateZipReport(config);

		_screenshotTaker.Verify(s => s.TakeScreenShot(), Times.Never());
		Mock.Verify();
	}
  }
}
