using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Gmad.Shared
{
	public static partial class Addon
	{
		public static IReadOnlyList<string> AllowedDescriptionProperties { get; } = new List<string>()
		{
			"description",
			"type",
			"tags"
		};

		private static JsonSerializer JsonSerializer { get; } = new JsonSerializer()
		{
			Formatting = Formatting.Indented ,
			NullValueHandling = NullValueHandling.Ignore
		};
		private static JsonDescriptionContractResolver DescriptionContractResolver { get; } = new JsonDescriptionContractResolver();

		internal class JsonDescriptionContractResolver : DefaultContractResolver
		{
			protected override JsonProperty CreateProperty( MemberInfo member , MemberSerialization memberSerialization )
			{
				var prop = base.CreateProperty( member , memberSerialization );

				if( !AllowedDescriptionProperties.Contains( prop.PropertyName ) )
				{
					return null;
				}


				return prop;
			}
		}

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

		private static string CreateJsonDescription( AddonInfo addonInfo )
		{
			var stringOutputBuilder = new StringBuilder();
			using( var stringWriter = new StringWriter( stringOutputBuilder ) )
			using( var jsonWriter = new JsonTextWriter( stringWriter ) )
			{
				var oldContract = JsonSerializer.ContractResolver;
				JsonSerializer.ContractResolver = DescriptionContractResolver;

				JsonSerializer.Serialize( jsonWriter , addonInfo );

				JsonSerializer.ContractResolver = oldContract;
			}
			return stringOutputBuilder.ToString();
		}
	}
}
