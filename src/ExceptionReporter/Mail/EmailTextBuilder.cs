// MIT License
// Copyright (c) 2008-2018 Peter van der Woude
// https://github.com/PandaWood/ExceptionReporter.NET
//

using System.Text;

#pragma warning disable 1591

namespace ExceptionReporting.Mail
{
	/// <summary>
	/// textual content for email
	/// </summary>
	public class EmailTextBuilder
	{
		public string CreateIntro(bool takeScreenshot)
		{
			var s = new StringBuilder()
				.AppendLine("The email is ready to be sent.")
				.AppendLine("Information relating to the error is included. Please feel free to add any relevant information or attach any files.");

			if (takeScreenshot)
			{
				s.AppendLine("A screenshot, taken at the time of the exception, is attached.")
				 .AppendLine("You may delete the attachment before sending if you prefer.")
				 .AppendLine();
			}

			return s.ToString();
		}
	}
}
#pragma warning restore 1591
