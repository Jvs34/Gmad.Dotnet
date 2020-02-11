using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Gmad.Shared
{
	public static partial class Addon
	{
		public static int Create( Dictionary<string , Stream> files , Stream outputStream )
		{
			using( BinaryWriter buffer = new BinaryWriter( outputStream ) )
			{
				buffer.Write( Ident.ToCharArray() );
				buffer.Write( ( char ) Version );
				buffer.Write( ( ulong ) 0 );
				//buffer.Write(  (ulong)  )
			}


			return 0;
		}
	}
}
