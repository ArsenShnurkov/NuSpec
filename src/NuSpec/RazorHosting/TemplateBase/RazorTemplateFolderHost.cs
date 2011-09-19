using NuSpec.RazorHosting.HostContainers;

namespace NuSpec.RazorHosting.TemplateBase
{
	/// <summary>
	/// Custom template implementation for the FolderHostContainer that supports 
	/// relative path based partial rendering.    
	/// </summary>
	public class RazorTemplateFolderHost : RazorTemplateBase
	{
		public override void InitializeTemplate(object context, object configurationData)
		{
			// Pick up configuration data and stuff into Request object
			var config = configurationData as RazorFolderHostTemplateConfiguration;

			Request.TemplatePath = config.TemplatePath;
			Request.TemplateRelativePath = config.TemplateRelativePath;
		}

		/// <summary>
		/// Render a partial view based on a Web relative path
		/// </summary>
		/// <param name="relativePath"></param>
		/// <param name="context"></param>
		/// <returns></returns>
		public string RenderPartial(string relativePath, object context)
		{
			if( HostContainer==null )
			{
				return null;
			}

			// we don't know the exact type since it can be generic so make dynamic
			// execution possible with dynamic type
			dynamic hostContainer = HostContainer;

			// now execute the child request to a string
			string output = hostContainer.RenderTemplateToString(relativePath, context);

			if( output==null )
			{
				output = "!@ Error: " + hostContainer.ErrorMessage + " @!";
			}

			return output;
		}

#if false
		/// TODO:
		/// this isn't working at this point because RenderTemplate in the host
		/// and engine will release the response. 
		/// Will need custom overloads that keep the writer open through child
		/// rendering operations.

		///// <summary>
		///// Render a partial view directly into the response object
		///// </summary>
		///// <param name="relativePath"></param>
		///// <param name="context"></param>
		//public string RenderPartialResponse(string relativePath, object context)
		//{
		//    if (this.HostContainer == null)
		//        return null;

		//    RazorFolderHostContainer hostContainer = (RazorFolderHostContainer)HostContainer; 
		//    hostContainer.RenderTemplate(relativePath, context, Response.Writer, true);

		//    return null;
		//}
#endif
	}
}