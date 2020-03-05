using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

#if NEWAPI
namespace Gmad.Shared.NewAPI
{
	public class Addon
	{
		public AddonInfo Info { get; set; }

		public void AddFile( string relativePath , Stream fileStream )
		{

		}

		public void RemoveFile( string relativePath )
		{

		}

		public void SaveToStream( Stream fileStream )
		{

		}

		public async Task SaveToStreamAsync( Stream fileStream )
		{

		}

		public void Extract()
		{

		}

		public async Task ExtractAsync()
		{

		}
	}
}
#endif