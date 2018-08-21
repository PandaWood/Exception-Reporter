﻿using System.IO;
using System.Reflection;
using System.Text;
using HandlebarsDotNet;

// ReSharper disable UnusedMember.Global
#pragma warning disable 1591

namespace ExceptionReporting.Templates
{
	internal class TemplateRenderer
	{	
		private readonly object _model;		// model object is kept generic but we force a kind of typing via constructors
		private readonly string _name;

		public TemplateRenderer(EmailIntroModel model)
		{
			_model = model;
			_name = "EmailIntroTemplate";
		}

		public TemplateRenderer(ReportModel model)
		{
			_model = model;
			_name = "ReportTemplate";
		}

		public string Render(TemplateFormat format = TemplateFormat.Text)
		{
			var template = GetTemplate(format);
			var compiledTemplate = Handlebars.Compile(template);
			var report = compiledTemplate(_model);
			return report;
		}

		private string GetTemplate(TemplateFormat format)
		{
			var resource = string.Format("{0}.{1}.{2}", this.GetType().Namespace, _name, format.ToString().ToLower());
			var assembly = Assembly.GetExecutingAssembly();

			using (var stream = assembly.GetManifestResourceStream(resource))
			{
				using (var reader = new StreamReader(stream, Encoding.UTF8))
				{
					var template = reader.ReadToEnd();
					return template;
				}
			}
		}
	}
}