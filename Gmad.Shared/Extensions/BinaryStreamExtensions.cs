using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Gmad.Shared.Extensions
{
	internal static class BinaryStreamExtensions
	{
		internal static void WriteBootilString( this BinaryWriter writer , string str , bool addNull = true )
		{
			foreach( char character in str )
			{
				writer.Write( character );
			}

			if( addNull )
			{
				writer.Write( ( char ) 0 );
			}
		}

		internal static string ReadBootilString( this BinaryReader reader )
		{
			List<char> characters = new List<char>();

			while( true )
			{
				char character = reader.ReadChar();
				if( character == ( char ) 0 )
				{
					break;
				}
				characters.Add( character );
			}

			return string.Concat( characters );
		}

	}
}
