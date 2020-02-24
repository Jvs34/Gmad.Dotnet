using Newtonsoft.Json;
using System.IO;

namespace Gmad.Shared
{
	public static partial class Addon
	{
		private static JsonSerializer JsonSerializer { get; } = new JsonSerializer()
		{
			Formatting = Formatting.Indented,
			NullValueHandling = NullValueHandling.Ignore
		};

		public static AddonInfo LoadAddonInfo( Stream inputStream )
		{
			AddonInfo addonInfo = new AddonInfo();

			using( var reader = new StreamReader( inputStream ) )
			using( var jsonReader = new JsonTextReader( reader ) )
			{
				addonInfo = JsonSerializer.Deserialize<AddonInfo>( jsonReader );
			}

			return addonInfo;
		}

		public static void SaveAddonInfo( AddonInfo addonInfo , TextWriter outputStream )
		{
			using( var writer = new JsonTextWriter( outputStream ) )
			{
				JsonSerializer.Serialize( writer , addonInfo );
			}
		}

		private static void PopulateFromDescription( AddonInfo addonInfo , string gmadAddonJson )
		{
			using( var stringReader = new StringReader( gmadAddonJson ) )
			using( var jsonReader = new JsonTextReader( stringReader ) )
			{
				JsonSerializer.Populate( jsonReader , addonInfo );
			}
		}
	}
}
