namespace ExceptionReporting.Core
{
	internal interface IZipReportService
	{
		/// <summary>
		/// Create Zip file
		/// filepath = %TEMP% + ExceptionReportInfo.AttachmentFilename
		/// </summary>
		/// <returns>
		/// Path to zip file.
		/// Empty if not saved.
		/// </returns>
		string CreateZipReport(ExceptionReportInfo reportInfo);

		/// <summary>
		/// Create Zip file
		/// </summary>
		/// <returns>
		/// Path to zip file.
		/// Empty if not saved.
		/// </returns>
		string CreateZipReport(ExceptionReportInfo reportInfo, string filepath);
	}
}