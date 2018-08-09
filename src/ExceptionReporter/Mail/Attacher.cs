﻿// MIT License
// Copyright (c) 2008-2018 Peter van der Woude
// https://github.com/PandaWood/ExceptionReporter.NET
//

using System.Collections.Generic;
using System.Linq;
using ExceptionReporting.Core;

namespace ExceptionReporting.Mail
{
	internal class Attacher
	{
		private const string ZIP = ".zip";
		public IFileService File { private get; set; } = new FileService();
		public IZipper Zipper { private get; set; } = new Zipper();
		private readonly ExceptionReportInfo _config;

		public Attacher(ExceptionReportInfo config)
		{
			_config = config;
		}

		public void AttachFiles(IAttach attacher)
		{
			var files = new List<string>();
			if (_config.FilesToAttach.Length > 0)
			{
				files.AddRange(_config.FilesToAttach);
			}
			if (_config.ScreenshotAvailable)
			{
				files.Add(ScreenshotTaker.GetImageAsFile(_config.ScreenshotImage));
			}

			var filesThatExist = files.Where(f => File.Exists(f)).ToList();

			// attach external zip files separately - admittedly weak detection using just file extension
			filesThatExist.Where(f => f.EndsWith(ZIP)).ToList().ForEach(attacher.Attach);

			// now zip & attach all specified files (ie config FilesToAttach) plus screenshot, if taken
			var filesToZip = filesThatExist.Where(f => !f.EndsWith(ZIP)).ToList();
			if (filesToZip.Any())
			{
				var zipFile = File.TempFile(_config.AttachmentFilename);
				Zipper.Zip(zipFile, filesToZip);
				attacher.Attach(zipFile);
			}
		}
	}
}
