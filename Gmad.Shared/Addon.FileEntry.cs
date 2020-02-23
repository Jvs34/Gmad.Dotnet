namespace Gmad.Shared
{
	public static partial class Addon
	{
		//not even used in gmad itself
		/*
		public unsafe struct Header
		{
			public fixed char Ident[4];
		}
		*/

		public struct FileEntry
		{
			public string Name;
			public long Size;
			public uint CRC;
			public uint FileNumber;
			public long Offset;
		}
	}
}
