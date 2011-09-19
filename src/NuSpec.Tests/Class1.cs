using System;
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Newtonsoft.Json;

namespace NuSpec.Tests
{
	[TestFixture]
	public class Class1
	{
		[Test]
		public void Test()
		{
			using(var stream = GetType().Assembly.GetManifestResourceStream("NuSpec.Tests.CodeFile.cs"))
			{
				using(var tr = new StreamReader(stream))
				{
					var codeBlock = tr.ReadToEnd();

					var nuspecRegion = Regex.Match(codeBlock, @"#region nuspec(.|\n)*?#endregion").Value.Replace("#region nuspec", "").Replace("#endregion", "").Replace("$nuspec = ", "");
					var result = Regex.Replace(nuspecRegion, @"//\s+", "");

					Console.WriteLine(result);
				}
			}
		}
	}
}