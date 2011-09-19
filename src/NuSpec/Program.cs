using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using NuSpec.RazorHosting.Core;
using NuSpec.RazorHosting.TemplateBase;

namespace NuSpec
{
	internal class Program
	{
		private const string BuildRootPath = @"D:\Projects\Magnetix\CodeBase\trunk\build\NuGet";
		private const string SourceRootPath = @"D:\Projects\Magnetix\CodeBase\trunk\src";

		private static void Main()
		{
			var codeFiles = Directory.GetFiles(SourceRootPath, "*.cs", SearchOption.AllDirectories);

			foreach(var codeFile in codeFiles)
			{
				var r = File.OpenRead(codeFile);
				var buffer = new byte[1024];
				r.Read(buffer, 0, 1024);

				var codeBlock = System.Text.Encoding.UTF8.GetString(buffer);
				var nuspecRegion = Regex.Match(codeBlock, @"#region nuspec(.|\n)*?#endregion").Value.Replace("#region nuspec", "").Replace("#endregion", "").Replace("$nuspec = ", "");
				if( !string.IsNullOrEmpty(nuspecRegion) )
				{
					var result = Regex.Replace(nuspecRegion, @"//\s+", "");
					using(var stringReader = new StringReader(result))
					{
						using(var reader = new JsonTextReader(stringReader))
						{
							var serializer = new JsonSerializer();
							var packageConfig = serializer.Deserialize<PackageConfig>(reader);

							var packageId = packageConfig.Id;
							if( packageId.Equals("$id$", StringComparison.InvariantCultureIgnoreCase) )
							{
								packageId = codeFile.Replace(SourceRootPath, "").TrimStart('\\').Replace(".cs", "").Replace(@"\", ".");
							}

							Console.WriteLine("Parsing package with id: " + packageId);

							var packagePath = Path.Combine(BuildRootPath, packageId);
							ensurePath(packagePath);

							var packageDescription = new PackageDescription
							{
								Id = packageId,
								Version = "1.0",
								Authors = packageConfig.Authors,
								Owners = packageConfig.Owners,
								LicenseUrl = packageConfig.LicenseUrl,
								ProjectUrl = packageConfig.ProjectUrl,
								IconUrl = packageConfig.IconUrl,
								Description = packageConfig.Description,
								RequireLicenseAcceptance = packageConfig.RequireLicenseAcceptance,
								Tags = packageConfig.Tags,
							};

							var contentPath = Path.Combine(packagePath, "content");
							ensurePath(contentPath);

							var codeFileName = codeFile.Replace(SourceRootPath, "").TrimStart('\\');
							foreach(var file in new[] {codeFileName}.Concat(packageConfig.Dependencies.Include))
							{
								var sourcePath = Path.Combine(SourceRootPath, file);
								var fileExists = File.Exists(sourcePath);

								color(fileExists ? ConsoleColor.DarkGreen : ConsoleColor.DarkRed, "\t" + file);

								if( fileExists==false )
								{
									color(ConsoleColor.Yellow, "\tSkipping: " + packageId);
									continue;
								}

								var destinationPath = Path.Combine(contentPath, Path.GetDirectoryName(file.Replace(@"Magnetix\", "")));
								ensurePath(destinationPath);
								destinationPath = Path.Combine(destinationPath, Path.GetFileName(sourcePath) + ".pp");
								transform(sourcePath, destinationPath);

								if( packageConfig.Dependencies!=null )
								{
									var dependencies = new List<Dependency>();

									foreach(var dependency in packageConfig.Dependencies.Require)
									{
										color(ConsoleColor.DarkMagenta, () =>
										{
											Console.WriteLine("\tuses: " + dependency.Id);
											dependencies.Add(new Dependency {Id = dependency.Id, Version = dependency.Version});
										});
									}

									packageDescription.Dependencies = dependencies.ToArray();
								}

								var template = buildNuGetSpecFile(packageDescription);
								File.WriteAllText(Path.Combine(packagePath, packageId + ".nuspec"), template);
							}
						}
					}
				}
			}

			return;
		}

		public class ColoredOutput : IDisposable
		{
			private readonly ConsoleColor _oldColor;

			public ColoredOutput(ConsoleColor color)
			{
				_oldColor = Console.ForegroundColor;
				Console.ForegroundColor = color;
			}

			public void Dispose()
			{
				Console.ForegroundColor = _oldColor;
			}
		}

		private static void color(ConsoleColor color, string s)
		{
			using(new ColoredOutput(color))
			{
				Console.WriteLine(s);
			}
		}

		private static void color(ConsoleColor color, Action operation)
		{
			using(new ColoredOutput(color))
			{
				operation();
			}
		}

		private static void transform(string sourcePath, string destinationPath)
		{
			var contents = File.ReadAllText(sourcePath);
			contents = Regex.Replace(contents, @"\s*#region nuspec(.|\n)*?#endregion\s*", "");
			contents = contents.Replace("namespace Magnetix", "namespace $rootnamespace$");
			contents = contents.Replace("using Magnetix", "using $rootnamespace$");
			File.WriteAllText(destinationPath, contents);
		}

		private static void ensurePath(string packagePath)
		{
			if( !Directory.Exists(packagePath) )
			{
				Directory.CreateDirectory(packagePath);
			}
		}

		private static string buildNuGetSpecFile(PackageDescription context)
		{
			var engine = new RazorEngine<RazorTemplateBase>();

			using(var reader = new StreamReader("Package.cshtml", true))
			{
				var output = engine.RenderTemplate(reader, new[] {"System.Windows.Forms.dll"}, context);
				if( output==null )
				{
					throw new InvalidOperationException("*** ERROR:\r\n" + engine.ErrorMessage);
				}
				return output;
			}
		}
	}

	[JsonObject(MemberSerialization.OptOut)]
	public class PackageConfig
	{
		[JsonProperty("rootNamespace")]
		public string RootNamespace { get; set; }

		[JsonProperty("id")]
		public string Id { get; set; }

		[JsonProperty("Version")]
		public string Version { get; set; }

		[JsonProperty("Authors")]
		public string[] Authors { get; set; }

		[JsonProperty("Owners")]
		public string[] Owners { get; set; }

		[JsonProperty("licenseUrl")]
		public string LicenseUrl { get; set; }

		[JsonProperty("projectUrl")]
		public string ProjectUrl { get; set; }

		[JsonProperty("iconUrl")]
		public string IconUrl { get; set; }

		[JsonProperty("description")]
		public string Description { get; set; }

		[JsonProperty("requireLicenseAcceptance")]
		public string RequireLicenseAcceptance { get; set; }

		[JsonProperty("Tags")]
		public string[] Tags { get; set; }

		[JsonProperty("dependencies")]
		public DependencyList Dependencies { get; set; }
	}

	[JsonObject(MemberSerialization.OptOut)]
	public class DependencyList
	{
		[JsonProperty("include")]
		public string[] Include { get; set; }

		[JsonProperty("exclude")]
		public string[] Exclude { get; set; }

		[JsonProperty("require")]
		public RequiredDependency[] Require { get; set; }
	}

	[JsonObject(MemberSerialization.OptOut)]
	public class RequiredDependency
	{
		[JsonProperty("id")]
		public string Id { get; set; }

		[JsonProperty("version")]
		public string Version { get; set; }
	}

	public class PackageDescription
	{
		public string Id { get; set; }
		public string Version { get; set; }
		public string[] Authors { get; set; }
		public string[] Owners { get; set; }
		public string LicenseUrl { get; set; }
		public string ProjectUrl { get; set; }
		public string IconUrl { get; set; }
		public string RequireLicenseAcceptance { get; set; }
		public string Description { get; set; }
		public string[] Tags { get; set; }
		public Dependency[] Dependencies { get; set; }
	}

	public class Dependency
	{
		public string Id { get; set; }
		public string Version { get; set; }
	}
}