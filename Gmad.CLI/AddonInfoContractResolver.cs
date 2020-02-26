using Gmad.Shared;
using System.Collections.Generic;
using System.Text.Json;

namespace Gmad.CLI
{
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

		public override string ConvertName( string propertyName )
		{
			if( MemberNames.TryGetValue( propertyName , out string newPropertyName ) )
			{
				propertyName = newPropertyName;
			}

			return propertyName;
		}
	}
}
