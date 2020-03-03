using Gmad.Shared;
using System.Collections.Generic;
using System.Text.Json;

namespace Gmad.CLI
{
	/// <summary>
	/// This is the best way to translate the names of the AddonInfo class rather
	/// than having to stamp attributes on the class from the shared project
	/// </summary>
	internal class AddonInfoContractResolver : JsonNamingPolicy
	{
		private static Dictionary<string , string> MemberNames { get; } = new Dictionary<string , string>()
		{
			{ nameof( AddonInfo.Title ) , "title" },
			{ nameof( AddonInfo.Description ) , "description" },
			{ nameof( AddonInfo.WorkshopType ) , "type" },
			{ nameof( AddonInfo.WorkshopID ) , "workshopid" },
			{ nameof( AddonInfo.LogoImage ) , "logo" },
			{ nameof( AddonInfo.Tags ) , "tags" },
			{ nameof( AddonInfo.IgnoreWildcard ) , "ignore" },
		};

		public override string ConvertName( string propertyName ) => MemberNames.TryGetValue( propertyName , out string newPropertyName ) ? newPropertyName : propertyName;
	}
}
