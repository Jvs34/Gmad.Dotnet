﻿using Gmad.Shared.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Epoch.net;

namespace Gmad.Shared
{
	public static partial class Addon
	{
		public static bool Create( Dictionary<string , Stream> files , Stream outputStream , AddonInfo addonInfo )
		{
			var orderedFiles = files.OrderBy( kv => kv.Key );

			using( BinaryWriter buffer = new BinaryWriter( outputStream ) )
			{
				buffer.Write( Ident.ToCharArray() );

				buffer.Write( ( char ) Version );

				buffer.Write( ( ulong ) 0 ); //unused steamid

				buffer.Write( ( ulong ) DateTime.Now.ToEpochTimestamp() );

				buffer.Write( ( char ) 0 ); //required content, this hasn't been worked on

				buffer.WriteBootilString( addonInfo.Title );

				string jsonDescription = CreateJsonDescription( addonInfo );

				buffer.WriteBootilString( jsonDescription );

				buffer.WriteBootilString( "Author Name" ); //unused author name

				buffer.Write( 1 ); //unused addon version

				uint fileNum = 0;
				foreach( var filesToAdd in orderedFiles )
				{
					uint crc = 0; //Willox mentioned that CRC is never used even in gmod itself, cool
					long size = filesToAdd.Value.Length;
					fileNum++;
					buffer.Write( fileNum );
					buffer.WriteBootilString( filesToAdd.Key );
					buffer.Write( size );
					buffer.Write( crc );
				}
				fileNum = 0;
				buffer.Write( fileNum );

				foreach( var filesToAdd in orderedFiles )
				{
					var stream = filesToAdd.Value;
					stream.Position = 0;
					stream.CopyToLimited( outputStream , stream.Length );
				}

				//don't write CRC32 as it's not used anywhere
				buffer.Write( ( uint ) 0 );
			}


			return true;
		}
	}
}
