using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Gmad.Shared.Extensions
{
	internal static class StreamExtensions
	{
		internal static void CopyToLimited( this Stream inputStream , Stream outputStream , long limit , int bufferSize = 81920 )
		{
			long bytesLeftToRead = limit;

			if( bufferSize > limit )
			{
				bufferSize = ( int ) limit;
			}

			byte[] buffer = new byte[bufferSize];

			while( bytesLeftToRead > 0 )
			{
				int bytesToRead = bufferSize;

				//if we're about to read over the limit, clamp it down to whatever is left
				if( ( bytesLeftToRead - bytesToRead ) < 0 )
				{
					bytesToRead = ( int ) bytesLeftToRead;
				}

				int bytesRead = inputStream.Read( buffer , 0 , bytesToRead );

				//now immediately write to the output stream

				outputStream.Write( buffer , 0 , bytesRead );

				bytesLeftToRead -= bytesRead;
			}

		}

		internal static async Task CopyToLimitedAsync( this Stream inputStream , Stream outputStream , long limit , int bufferSize = 81920 , CancellationToken token = default )
		{
			long bytesLeftToRead = limit;

			if( bufferSize > limit )
			{
				bufferSize = ( int ) limit;
			}

			byte[] buffer = new byte[bufferSize];

			while( bytesLeftToRead > 0 )
			{
				int bytesToRead = bufferSize;

				//if we're about to read over the limit, clamp it down to whatever is left
				if( ( bytesLeftToRead - bytesToRead ) < 0 )
				{
					bytesToRead = ( int ) bytesLeftToRead;
				}

				int bytesRead = await inputStream.ReadAsync( buffer , 0 , bytesToRead , token );

				//now immediately write to the output stream

				await outputStream.WriteAsync( buffer , 0 , bytesRead , token );

				bytesLeftToRead -= bytesRead;
			}

		}
	}
}
