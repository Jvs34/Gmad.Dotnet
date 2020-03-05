using Epoch.net;
using Gmad.Shared.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Gmad.Shared
{
	public static partial class Addon
	{
		#region STATICDEFINES
		public static string Ident { get; } = "GMAD";
		public static int Version { get; } = 3;

		public static IReadOnlyList<string> TypeTags { get; } = new List<string>()
		{
			"gamemode",
			"map",
			"weapon",
			"vehicle",
			"npc",
			"entity",
			"tool",
			"effects",
			"model",
			"servercontent"
		};

		public static IReadOnlyList<string> MiscTags { get; } = new List<string>()
		{
			"fun",
			"roleplay",
			"scenic",
			"movie",
			"realism",
			"cartoon",
			"water",
			"comic",
			"build"
		};

		/// <summary>
		/// Check this before the addon's ignore list, because gmad's code does it in a hardcoded way
		/// </summary>
		public static IReadOnlyList<string> DefaultIgnores { get; } = new List<string>()
		{
			"addon.json",
			"*thumbs.db",
			"*desktop.ini",
			".DS_Store",
			"*/.DS_Store"
		};

		public static IReadOnlyList<string> Wildcards { get; } = new List<string>()
		{
			"lua/*.lua",
			"scenes/*.vcd",
			"particles/*.pcf",
			"resource/fonts/*.ttf",
			"scripts/vehicles/*.txt",
			"resource/localization/*/*.properties",
			"maps/*.bsp",
			"maps/*.nav",
			"maps/*.ain",
			"maps/thumb/*.png",
			"sound/*.wav",
			"sound/*.mp3",
			"sound/*.ogg",
			"materials/*.vmt",
			"materials/*.vtf",
			"materials/*.png",
			"materials/*.jpg",
			"materials/*.jpeg",
			"models/*.mdl",
			"models/*.vtx",
			"models/*.phy",
			"models/*.ani",
			"models/*.vvd",
			"gamemodes/*/*.txt",
			"gamemodes/*/*.fgd",
			"gamemodes/*/logo.png",
			"gamemodes/*/icon24.png",
			"gamemodes/*/gamemode/*.lua",
			"gamemodes/*/entities/effects/*.lua",
			"gamemodes/*/entities/weapons/*.lua",
			"gamemodes/*/entities/entities/*.lua",
			"gamemodes/*/backgrounds/*.png",
			"gamemodes/*/backgrounds/*.jpg",
			"gamemodes/*/backgrounds/*.jpeg",
			"gamemodes/*/content/models/*.mdl",
			"gamemodes/*/content/models/*.vtx",
			"gamemodes/*/content/models/*.phy",
			"gamemodes/*/content/models/*.ani",
			"gamemodes/*/content/models/*.vvd",
			"gamemodes/*/content/materials/*.vmt",
			"gamemodes/*/content/materials/*.vtf",
			"gamemodes/*/content/materials/*.png",
			"gamemodes/*/content/materials/*.jpg",
			"gamemodes/*/content/materials/*.jpeg",
			"gamemodes/*/content/scenes/*.vcd",
			"gamemodes/*/content/particles/*.pcf",
			"gamemodes/*/content/resource/fonts/*.ttf",
			"gamemodes/*/content/scripts/vehicles/*.txt",
			"gamemodes/*/content/resource/localization/*/*.properties",
			"gamemodes/*/content/maps/*.bsp",
			"gamemodes/*/content/maps/*.nav",
			"gamemodes/*/content/maps/*.ain",
			"gamemodes/*/content/maps/thumb/*.png",
			"gamemodes/*/content/sound/*.wav",
			"gamemodes/*/content/sound/*.mp3",
			"gamemodes/*/content/sound/*.ogg",
		};

		public static Func<string , AddonInfo> DeserializeAddonInfoCallback { get; set; }
		public static Func<AddonInfo , string> SerializeAddonInfoToStringCallback { get; set; }
		#endregion

		/// <summary>
		/// Calls <see cref="IsWildcardMatching"/> with Addon.Wildcards to check if the path is allowed
		/// </summary>
		/// <param name="relativePath">the filesystem path relative to the addon folder </param>
		/// <returns></returns>
		public static bool IsPathAllowed( string relativePath )
		{
			return IsWildcardMatching( relativePath , Wildcards );
		}

		public static bool IsWildcardMatching( string relativePath , IEnumerable<string> wildcardList )
		{
			return wildcardList?.Any( wildcard => IsWildcardMatching( relativePath , wildcard ) ) == true;
		}

		/// <summary>
		/// Uses the same code behind GMAD's c++ implementation (a third party function called globber)
		/// </summary>
		/// <param name="str">preferably a relative path</param>
		/// <param name="wildcard">the wildcard to match to, can contain * and ?</param>
		/// <returns></returns>
		public static bool IsWildcardMatching( string str , string wildcard )
		{
			str = str.ToLower();
			wildcard = wildcard.ToLower();

			int strIndex = 0;
			int wildIndex = 0;

			while( strIndex < str.Length && wildcard[wildIndex] != '*' )
			{
				if( wildcard[wildIndex] != str[strIndex] && wildcard[wildIndex] != '?' )
				{
					return false;
				}
				wildIndex++;
				strIndex++;
			}

			int cp = 0;
			int mp = 0;

			while( strIndex < str.Length )
			{
				if( wildcard[wildIndex] == '*' )
				{
					if( ++wildIndex >= wildcard.Length )
					{
						return true;
					}
					mp = wildIndex;
					cp = strIndex + 1;
				}
				else if( wildcard[wildIndex] == str[strIndex] || wildcard[wildIndex] == '?' )
				{
					wildIndex++;
					strIndex++;
				}
				else
				{
					wildIndex = mp;
					strIndex = cp++;
				}
			}

			while( wildIndex < wildcard.Length && wildcard[wildIndex] == '*' )
			{
				wildIndex++;
			}

			return wildIndex >= wildcard.Length;
		}

		private static void PopulateOldAddonInfo( AddonInfo addonInfo , AddonInfo newAddonInfo )
		{
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
			return SerializeAddonInfoToStringCallback( new AddonInfo()
			{
				Description = addonInfo.Description ,
				Tags = addonInfo.Tags ,
				WorkshopType = addonInfo.WorkshopType
			} );
		}

		#region BINARY
		internal static void WriteHeader( BinaryWriter writer )
		{
			writer.Write( Ident.ToCharArray() );
			writer.Write( ( char ) Version );
		}

		internal static (char[], char) ReadHeader( BinaryReader reader )
		{
			return (reader.ReadChars( Ident.Length ), reader.ReadChar());
		}

		internal static void WriteAddonInfo( BinaryWriter writer , AddonInfo addonInfo , DateTime timestamp )
		{
			writer.Write( ( ulong ) 0 ); //unused steamid
			writer.Write( ( ulong ) timestamp.ToEpochTimestamp() );
			writer.Write( ( char ) 0 ); //required content, this hasn't been worked on and I doubt it will
			writer.WriteBootilString( addonInfo.Title );
			writer.WriteBootilString( CreateJsonDescription( addonInfo ) );
			writer.WriteBootilString( "Author Name" ); //unused author name
			writer.Write( 1 ); //unused addon version
		}

		internal static AddonInfo ReadAddonInfo( BinaryReader reader , char gmadFormatVersion )
		{
			ulong steamid = reader.ReadUInt64(); //steamid
			ulong timestamp = reader.ReadUInt64(); //timestamp

			var timeUpdated = new EpochTime( ( int ) timestamp ).DateTime;

			if( gmadFormatVersion > 1 )
			{
				string content = reader.ReadBootilString();
				while( !string.IsNullOrEmpty( content ) )
				{
					content = reader.ReadBootilString();
				}
			}

			string gmadTitle = reader.ReadBootilString();
			string gmadAddonJson = reader.ReadBootilString(); //gmad reads this first then parses it later to get the actual description
			string gmadAuthor = reader.ReadBootilString();

			int gmadAddonVersion = reader.ReadInt32();

			AddonInfo addonInfo = DeserializeAddonInfoCallback( gmadAddonJson );
			addonInfo.Title = gmadTitle;

			return addonInfo;
		}

		internal static void WriteFileList( BinaryWriter writer , IEnumerable<KeyValuePair<string , Stream>> fileList )
		{
			uint fileNum = 0;
			foreach( var filesToAdd in fileList )
			{
				//Willox mentioned that CRC is never used even in gmod itself nor in any other tool, if you're using it in yours, I'm sorry

				uint crc = 0;
				long size = filesToAdd.Value.Length;
				fileNum++;
				writer.Write( fileNum );
				writer.WriteBootilString( filesToAdd.Key );
				writer.Write( size );
				writer.Write( crc );
			}

			writer.Write( 0 ); //end of the filelist
		}

		internal static long ReadFileList( BinaryReader reader , out List<FileEntry> gmadFileEntries )
		{
			gmadFileEntries = new List<FileEntry>();

			uint fileNumber = 1;
			long offset = 0;

			while( reader.ReadUInt32() != 0 )
			{
				FileEntry entry = new FileEntry()
				{
					Name = reader.ReadBootilString() ,
					Size = reader.ReadInt64() ,
					CRC = reader.ReadUInt32() ,
					Offset = offset ,
					FileNumber = fileNumber ,
				};

				gmadFileEntries.Add( entry );
				offset += entry.Size;
				fileNumber++;
			}

			return reader.BaseStream.Position;
		}

		internal static async Task WriteFileStreams( Stream outputStream , IEnumerable<KeyValuePair<string , Stream>> fileList )
		{
			foreach( var filesToAdd in fileList )
			{
				var fileStream = filesToAdd.Value;
				fileStream.Position = 0;
				await fileStream.CopyToLimitedAsync( outputStream , fileStream.Length );
			}
		}

		internal static async Task WriteFiles( BinaryWriter writer , Stream outputStream , IEnumerable<KeyValuePair<string , Stream>> fileList )
		{
			WriteFileList( writer , fileList );
			await WriteFileStreams( outputStream , fileList );
		}
		#endregion

		public static async Task<bool> Create( Dictionary<string , Stream> files , Stream outputStream , AddonInfo addonInfo )
		{
			var orderedFiles = files.OrderBy( kv => kv.Key );

			using( BinaryWriter buffer = new BinaryWriter( outputStream ) )
			{
				WriteHeader( buffer );
				WriteAddonInfo( buffer , addonInfo , DateTime.Now );
				await WriteFiles( buffer , outputStream , orderedFiles );

				//don't write CRC32 as it's not used anywhere
				buffer.Write( ( uint ) 0 );
			}

			return true;
		}

		/// <summary>
		/// Extract the given GMA inputstream
		/// </summary>
		/// <param name="inputStream">the raw stream of the gma</param>
		/// <param name="requestFileStream">the callback to write the output file to, return a null if the path is invalid</param>
		/// <param name="addonInfo"> the addon info object to feed info to, saving this is at your discretion</param>
		/// <returns></returns>
		public static async Task<bool> Extract( Stream inputStream , Func<string , Stream> requestFileStream , AddonInfo addonInfo )
		{
			inputStream.Position = 0;

			using( BinaryReader reader = new BinaryReader( inputStream ) )
			{
				var (gmadIdent, gmadFormatVersion) = ReadHeader( reader );

				if( !gmadIdent.SequenceEqual( Ident ) || gmadFormatVersion > Version )
				{
					return false;
				}

				AddonInfo newAddonInfo = ReadAddonInfo( reader , gmadFormatVersion );

				//the term populate here is correct, we don't want to override it again as the object might be one
				//passed from an existing file already

				PopulateOldAddonInfo( addonInfo , newAddonInfo );

				long gmadFileblock = ReadFileList( reader , out var gmadFileEntries );
				int badFileCount = 0;

				foreach( var entry in gmadFileEntries )
				{
					var stream = requestFileStream( entry.Name );

					if( stream is null )
					{
						stream = requestFileStream( $"badnames/{badFileCount}+.unk" );
						badFileCount++;
					}

					if( stream != null )
					{
						inputStream.Position = gmadFileblock + entry.Offset;
						await inputStream.CopyToLimitedAsync( stream , entry.Size );
					}
				}

				uint addoncrc = reader.ReadUInt32(); //not used during the extraction of gmas
			}


			return true;
		}


	}
}
