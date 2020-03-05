using System.Collections.Generic;

namespace Gmad.Shared
{
	//not even used in gmad itself
	/*
	internal unsafe struct Header
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

		public override bool Equals( object obj )
		{
			return obj is FileEntry entry &&
					 Name == entry.Name &&
					 Size == entry.Size &&
					 CRC == entry.CRC &&
					 FileNumber == entry.FileNumber &&
					 Offset == entry.Offset;
		}

		public override int GetHashCode()
		{
			var hashCode = -675244057;
			hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode( Name );
			hashCode = hashCode * -1521134295 + Size.GetHashCode();
			hashCode = hashCode * -1521134295 + CRC.GetHashCode();
			hashCode = hashCode * -1521134295 + FileNumber.GetHashCode();
			hashCode = hashCode * -1521134295 + Offset.GetHashCode();
			return hashCode;
		}

		public static bool operator ==( FileEntry left , FileEntry right )
		{
			return left.Equals( right );
		}

		public static bool operator !=( FileEntry left , FileEntry right )
		{
			return !( left == right );
		}
	}
}
