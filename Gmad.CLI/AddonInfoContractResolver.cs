using Gmad.Shared;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Gmad.CLI
{
	internal class AddonInfoContractResolver : DefaultContractResolver
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

		protected override string ResolvePropertyName( string propertyName )
		{
			if( MemberNames.TryGetValue( propertyName , out string newPropertyName ) )
			{
				return base.ResolvePropertyName( newPropertyName );
			}
			return string.Empty;
		}
	}
}
