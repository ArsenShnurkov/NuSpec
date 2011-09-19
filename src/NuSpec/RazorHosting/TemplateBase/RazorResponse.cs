using System.IO;
using System;

namespace NuSpec.RazorHosting.TemplateBase
{
	/// <summary>
	/// Class that handles Response output generation inside of
	/// RazorTemplateBase implementation.
	/// </summary>
	public class RazorResponse : IDisposable
	{
		/// <summary>
		/// Internal text writer - default to StringWriter()
		/// </summary>
		public TextWriter Writer = new StringWriter();

		public virtual void Write(object value)
		{
			Writer.Write(value);
		}

		public virtual void WriteLine(object value)
		{
			Write(value);
			Write("\r\n");
		}

		public virtual void WriteFormat(string format, params object[] args)
		{
			Write(string.Format(format, args));
		}

		public override string ToString()
		{
			return Writer.ToString();
		}

		public virtual void Dispose()
		{
			Writer.Close();
		}

		/// <summary>
		/// Allows overriding the TextWriter used write output to.
		/// Note: This method MUST be called before any output has
		/// been written to the Response to capture the entire response.
		/// </summary>
		/// <param name="writer"></param>
		public virtual void SetTextWriter(TextWriter writer)
		{
			// Close original writer
			if( Writer!=null )
			{
				Writer.Close();
			}

			Writer = writer;
		}
	}
}