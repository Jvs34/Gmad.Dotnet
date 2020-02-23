using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gmad.Shared
{
	public class AddonInfo
	{
		[JsonProperty( "title" )] public string Title { get; set; }
		[JsonProperty( "description" )] public string Description { get; set; }
		[JsonProperty( "type" )] public string WorkshopType { get; set; }

		/// <summary>
		/// To remain 0 until gmpublish gives you an id, not sure why this is saved as a float in the file
		/// </summary>
		[JsonProperty( "workshopid" )] public uint WorkshopID { get; set; }

		/// <summary>
		/// The logo image, this is a relative path
		/// </summary>
		[JsonProperty( "logo" )] public string LogoImage { get; set; }
		[JsonProperty( "tags" )] public List<string> Tags { get; set; }
		[JsonProperty( "ignore" )] public List<string> IgnoreWildcard { get; set; }
	}
}
