using Gmad.Shared;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Gmad.CLI
{
	internal static class AddonHandling
	{
		private static JsonSerializer AddonJsonSerializer { get; } = new JsonSerializer()
		{
			Formatting = Formatting.Indented ,
			NullValueHandling = NullValueHandling.Ignore ,
			ContractResolver = new AddonInfoContractResolver() ,
		};

		internal static async Task<int> CreateAddonFile( DirectoryInfo folder , FileInfo fileOutput , bool warninvalid = false )
		{
			if( fileOutput is null )
			{
				fileOutput = new FileInfo( folder.FullName + ".gma" );
			}

			var jsonFileInfo = new FileInfo( Path.Combine( folder.FullName , "addon.json" ) );

			AddonInfo addonInfo = OpenAddonInfo( jsonFileInfo );

			//open every file in the folder, then feed it as a string:stream dictionary
			Dictionary<string , Stream> files = new Dictionary<string , Stream>();

			foreach( var fileInput in folder.EnumerateFiles( "*" , SearchOption.AllDirectories ) )
			{
				//turn the file paths into relatives and also lowercase
				string relativeFilePath = Path.GetRelativePath( folder.FullName , fileInput.FullName ).ToLower();

				relativeFilePath = relativeFilePath.Replace( "\\" , "/" );


				//this could PROBABLY be streamlined in a Addon.IsIgnoreMatching function but for now I just want this to work

				if( Addon.IsWildcardMatching( relativeFilePath , Addon.DefaultIgnores ) )
				{
					continue;
				}

				if( addonInfo.IgnoreWildcard != null && Addon.IsWildcardMatching( relativeFilePath , addonInfo.IgnoreWildcard ) )
				{
					continue;
				}

				//if it's not ignored and still not allowed, throw out an error

				if( !Addon.IsPathAllowed( relativeFilePath ) )
				{
					if( warninvalid )
					{
						Console.Error.WriteLine( $"{relativeFilePath} \t\t[Not allowed by whitelist]" );
					}
					return 1;
				}

				var fileInputStream = fileInput.OpenRead();

				files.Add( relativeFilePath , fileInputStream );
			}

			if( files.Count == 0 )
			{
				Console.Error.WriteLine( "No files found, can't continue!" );
				return 1;
			}

			//now open the stream for the output
			bool success;

			using( var outputStream = fileOutput.OpenWrite() )
			{
				success = await Addon.Create( files , outputStream , addonInfo );
			}

			foreach( var kv in files )
			{
				kv.Value?.Dispose();
			}

			if( success )
			{
				SaveAddonInfo( jsonFileInfo , addonInfo );
				Console.WriteLine( $"Successfully saved to {fileOutput.FullName}" );
			}

			return Convert.ToInt32( !success );
		}

		internal static async Task<int> ExtractAddonFile( FileInfo file , DirectoryInfo folderOutput , bool warninvalid = false )
		{
			if( folderOutput == null )
			{
				folderOutput = new DirectoryInfo( Path.GetFileNameWithoutExtension( file.FullName ) );
			}

			if( !folderOutput.Exists )
			{
				folderOutput.Create();
			}

			using var gmadFileStream = file.OpenRead();

			Dictionary<string , Stream> files = new Dictionary<string , Stream>();

			var jsonFileInfo = new FileInfo( Path.Combine( folderOutput.FullName , "addon.json" ) );

			//in case of re-extraction, we don't want to overwrite a manually written json for whatever reason
			AddonInfo addonInfo = OpenAddonInfo( jsonFileInfo ) ?? new AddonInfo();

			bool success = await Addon.Extract( gmadFileStream , ( filePath ) =>
			{
				var outputFileInfo = new FileInfo( Path.Combine( folderOutput.FullName , filePath ) );

				//create the subfolder first

				outputFileInfo.Directory.Create();

				if( !outputFileInfo.FullName.StartsWith( folderOutput.FullName ) )
				{
					throw new IOException( $"Addon extraction somehow ended up outside main folder {outputFileInfo.FullName}" );
				}

				var fileStream = outputFileInfo.OpenWrite();

				files.Add( filePath , fileStream );

				return fileStream;
			} , addonInfo );

			foreach( var kv in files )
			{
				kv.Value.Dispose();
			}

			if( success )
			{
				SaveAddonInfo( jsonFileInfo , addonInfo );
			}

			return Convert.ToInt32( !success );
		}

		private static AddonInfo OpenAddonInfo( FileInfo jsonFile )
		{
			if( !jsonFile.Exists )
			{
				return null;
			}

			using var fileStream = jsonFile.OpenRead();

			AddonInfo addonInfo = new AddonInfo();

			using( var reader = new StreamReader( fileStream ) )
			using( var jsonReader = new JsonTextReader( reader ) )
			{
				addonInfo = AddonJsonSerializer.Deserialize<AddonInfo>( jsonReader );
			}

			return addonInfo;
		}

		internal static void SaveAddonInfo( FileInfo jsonFile , AddonInfo addonInfo )
		{
			using var fileStream = jsonFile.CreateText();

			using( var writer = new JsonTextWriter( fileStream ) )
			{
				AddonJsonSerializer.Serialize( writer , addonInfo );
			}
		}

		internal static string SerializeAddonInfoToString( AddonInfo addonInfo )
		{
			var stringOutputBuilder = new StringBuilder();
			using( var stringWriter = new StringWriter( stringOutputBuilder ) )
			using( var jsonWriter = new JsonTextWriter( stringWriter ) )
			{
				AddonJsonSerializer.Serialize( jsonWriter , addonInfo );
			}
			return stringOutputBuilder.ToString();
		}

		internal static AddonInfo DeserializeAddonInfo( string jsonStr )
		{
			using var reader = new StringReader( jsonStr );
			using var jsonReader = new JsonTextReader( reader );
			return AddonJsonSerializer.Deserialize<AddonInfo>( jsonReader );
		}
	}
}
