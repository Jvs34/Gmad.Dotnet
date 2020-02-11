using System;
using System.Collections.Generic;
using System.Text;

namespace Gmad.Shared
{
	public static partial class Addon
	{
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


		public static bool IsPathAllowed( string path , List<string> ignoreWildcard = null )
		{
			//TODO: find a way to condense this without garbage
			if( ignoreWildcard != null )
			{
				foreach( var wildcard in ignoreWildcard )
				{
					if( IsPathAllowed( path , wildcard ) )
					{
						return true;
					}
				}
			}

			foreach( var wildcard in Wildcards )
			{
				if( IsPathAllowed( path , wildcard ) )
				{
					return true;
				}
			}

			return false;
		}

		public static bool IsPathAllowed( string str , string wildcard )
		{
			int strIndex = 0;
			int wildIndex = 0;

			while( strIndex < str.Length && wildcard[wildIndex] != '*' )
			{
				if( !wildcard[wildIndex] == str[strIndex] && wildcard[wildIndex] != '?' )
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

			/*
				const char *cp = 0, *mp = 0;

				while ((*string) && (*wild != '*')) {
					if ((*wild != *string) && (*wild != '?')) {
						return false;
					}
					wild++;
					string++;
				}
			 */

			/*
			 * globber( const char * string , const char * wild )
			const char *cp = 0, *mp = 0;

			while ((*string) && (*wild != '*')) {
				if ((*wild != *string) && (*wild != '?')) {
					return false;
				}
				wild++;
				string++;
			}

			while (*string) {
				if (*wild == '*') {
					if (!*++wild) {
						return true;
					}
					mp = wild;
					cp = string+1;
				} else if ((*wild == *string) || (*wild == '?')) {
					wild++;
					string++;
				} else {
					wild = mp;
					string = cp++;
				}
			}

			while (*wild == '*') {
				wild++;
			}
			return !*wild;
			*/
		}
	}
}
