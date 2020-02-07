using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Gmad.Shared
{
	public static partial class Addon
	{
		/// <summary>
		/// Extract the given GMA inputstream
		/// </summary>
		/// <param name="inputStream">the raw stream of the gma</param>
		/// <param name="requestFileStream">the callback to write the output file to</param>
		/// <returns></returns>
		public static int Extract( Stream inputStream , Func<string, Stream> requestFileStream )
		{
			return 0;
		}
	}
}
