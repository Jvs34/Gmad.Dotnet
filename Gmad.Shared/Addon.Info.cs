using System;
using System.Collections.Generic;

namespace Gmad.Shared
{
	public static partial class Addon
	{
		public static Func<string , AddonInfo> DeserializeAddonInfoCallback { get; set; }
		public static Func<AddonInfo ,string > SerializeAddonInfoToStringCallback { get; set; }

		private static void PopulateFromDescription( AddonInfo addonInfo , string gmadAddonJson )
		{
			var newAddonInfo = DeserializeAddonInfoCallback( gmadAddonJson );

			//populate tags description and type only
			addonInfo.Description = newAddonInfo.Description;
			addonInfo.WorkshopType = newAddonInfo.WorkshopType;
			if( addonInfo.Tags != null )
			{
				addonInfo.Tags.UnionWith( newAddonInfo.Tags );
			}
			else
			{
				addonInfo.Tags = new HashSet<string>( newAddonInfo.Tags );
			}
		}

		private static string CreateJsonDescription( AddonInfo addonInfo )
		{
			return SerializeAddonInfoToStringCallback( addonInfo );
		}
	}
}
