using System;
using System.Text;
using System.IO;

namespace NuSpec.RazorHosting.Core
{
	/// <summary>
	/// Configuration for the Host class. These settings determine some of the
	/// operational parameters of the RazorHost class that can be changed at
	/// runtime.
	/// </summary>        
	public class RazorEngineConfiguration : MarshalByRefObject
	{
		private string _tempAssemblyPath;

		public RazorEngineConfiguration()
		{
			StreamBufferSize = 2048;
			OutputEncoding = Encoding.UTF8;
			CompileToMemory = true;
		}

		/// <summary>
		/// Determines if assemblies are compiled to disk or to memory.
		/// If compiling to disk generated assemblies are not cleaned up
		/// </summary>
		public bool CompileToMemory { get; set; }

		/// <summary>
		/// When compiling to disk use this Path to hold generated assemblies
		/// </summary>
		public string TempAssemblyPath
		{
			get
			{
				if( !string.IsNullOrEmpty(_tempAssemblyPath) )
				{
					return _tempAssemblyPath;
				}

				return Path.GetTempPath();
			}
			set
			{
				_tempAssemblyPath = value;
			}
		}

		/// <summary>
		/// Encoding to be used when generating output to file
		/// </summary>
		public Encoding OutputEncoding { get; set; }

		/// <summary>
		/// Buffer size for streamed template output when using filenames
		/// </summary>
		public int StreamBufferSize { get; set; }
	}
}