using UnityEngine;
using System.IO;
namespace GwambaPrimeAdventure.Connection
{
	internal struct FilesArchive
	{
		public string
			SaveFile1,
			SaveFile2,
			SaveFile3,
			SaveFile4;
	};
	public static class FilesController
	{
		private static readonly string
			FilesArchivePath = $"{Application.persistentDataPath}/Save Files.txt";
		private static FilesArchive LoadFiles()
		{
			return File.Exists( FilesArchivePath ) 
				? FileEncoder.ReadData<FilesArchive>( FilesArchivePath ) 
				: new FilesArchive()
				{
					SaveFile1 = "Save File 1",
					SaveFile2 = "Save File 2",
					SaveFile3 = "Save File 3",
					SaveFile4 = "Save File 4"
				};
		}
		public static string Select( ushort actualSaveFile ) =>
			actualSaveFile switch
			{
				1 => LoadFiles().SaveFile1,
				2 => LoadFiles().SaveFile2,
				3 => LoadFiles().SaveFile3,
				4 => LoadFiles().SaveFile4,
				_ => null
			};
		internal static string SaveData( (ushort actualSaveFile, string newSaveName) set = default )
		{
			if ( File.Exists( Select( set.actualSaveFile ) ) )
				File.Delete( Select( set.actualSaveFile ) );
			FilesArchive newFilesArchive = LoadFiles();
			string newSaveName = set.actualSaveFile switch
			{
				1 => newFilesArchive.SaveFile1 = set.newSaveName,
				2 => newFilesArchive.SaveFile2 = set.newSaveName,
				3 => newFilesArchive.SaveFile3 = set.newSaveName,
				4 => newFilesArchive.SaveFile4 = set.newSaveName,
				_ => null
			};
			FileEncoder.WriteData( newFilesArchive, FilesArchivePath );
			return newSaveName;
		}
	};
};
