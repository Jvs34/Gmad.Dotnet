﻿using Epoch.net;
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
		/// <summary>
		/// Extract the given GMA inputstream
		/// </summary>
		/// <param name="inputStream">the raw stream of the gma</param>
		/// <param name="requestFileStream">the callback to write the output file to</param>
		/// <param name="addonInfo"> the addon info object to feed info to, saving this is at your discretion</param>
		/// <returns></returns>
		public static async Task<bool> Extract( Stream inputStream , Func<string , Stream> requestFileStream , AddonInfo addonInfo )
		{
			inputStream.Position = 0;

			using( BinaryReader reader = new BinaryReader( inputStream ) )
			{
				char[] gmadIdent = reader.ReadChars( 4 );
				if( !gmadIdent.SequenceEqual( Ident ) )
				{
					return false;
				}

				char gmadFormatVersion = reader.ReadChar();
				if( gmadFormatVersion > Version )
				{
					return false;
				}

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

				List<FileEntry> gmadFileEntries = new List<FileEntry>();

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

				long gmadFileblock = reader.BaseStream.Position;


				//gmad here reads the json then sets the description, type and tags


				int badFileCount = 0;

				foreach( var entry in gmadFileEntries )
				{
					//sets the position of the buffer to fileblock +  file.Offset
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


				addonInfo.Title = gmadTitle;

				//the term populate here is correct, we don't want to override it again as the object might be one
				//passed from an existing file already
				PopulateFromDescription( addonInfo , gmadAddonJson );
			}


			return true;
		}


	}
}
