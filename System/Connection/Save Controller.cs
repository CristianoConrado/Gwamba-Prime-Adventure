using UnityEngine;
using System.IO;
using System.Collections.Generic;
namespace GwambaPrimeAdventure.Connection
{
	public struct SaveFile
	{
		public ushort
			Lifes,
			Coins;
		public Dictionary<string, bool> Books;
		public List<string>
			LifesAcquired,
			GeneralObjects,
			BooksName;
		public List<bool> BooksValue;
		public string LastLevelEntered;
		public bool[]
			LevelsCompleted,
			DeafetedBosses;
	};
	public static class SaveController
	{
		private static SaveFile
			_saveFile = LoadFile();
		private static ushort
			_actualSaveFile = 0;
		public static bool FileExists() => File.Exists( $@"{Application.persistentDataPath}\{FilesController.Select( _actualSaveFile )}.txt" );
		public static void Load( out SaveFile saveFile ) => saveFile = _saveFile;
		private static SaveFile LoadFile()
		{
			SaveFile saveFile = new()
			{
				LifesAcquired = new List<string>(),
				Lifes = 10,
				Coins = 0,
				Books = new Dictionary<string, bool>(),
				GeneralObjects = new List<string>(),
				BooksName = new List<string>(),
				BooksValue = new List<bool>(),
				LastLevelEntered = "",
				LevelsCompleted = new bool[ WorldBuild.LEVELS_COUNT ],
				DeafetedBosses = new bool[ WorldBuild.LEVELS_COUNT ]
			};
			string actualSaveFile = FilesController.Select( _actualSaveFile );
			if ( string.IsNullOrEmpty( actualSaveFile ) )
				return saveFile;
			string actualPath = $@"{Application.persistentDataPath}\{actualSaveFile}.txt";
			if ( File.Exists( actualPath ) )
			{
				bool filesCondition = FilesController.Select( 1 ) != actualSaveFile && FilesController.Select( 2 ) != actualSaveFile;
				if ( filesCondition && FilesController.Select( 3 ) != actualSaveFile && FilesController.Select( 4 ) != actualSaveFile )
				{
					File.Delete( actualPath );
					return saveFile;
				}
				saveFile = FileEncoder.ReadData<SaveFile>( actualPath );
				saveFile.Books = new Dictionary<string, bool>();
				for ( ushort i = 0; saveFile.BooksName.Count > i; i++ )
					saveFile.Books.Add( saveFile.BooksName[ i ], saveFile.BooksValue[ i ] );
			}
			return saveFile;
		}
		public static void WriteSave( SaveFile saveFile ) => _saveFile = saveFile;
		public static void RefreshData() => _saveFile = LoadFile();
		public static void SetActualSaveFile( ushort actualSaveFile )
		{
			_actualSaveFile = actualSaveFile;
			RefreshData();
		}
		public static void RenameData( ushort actualSave, string newName )
		{
			if ( string.IsNullOrEmpty( newName ) )
				return;
			FilesController.SaveData( (actualSave, newName) );
			string actualPath = $@"{Application.persistentDataPath}\{FilesController.Select( actualSave )}.txt";
			if ( File.Exists( actualPath ) )
				FileEncoder.WriteData( FileEncoder.ReadData<SaveFile>( actualPath ), $@"{Application.persistentDataPath}\{newName}.txt" );
		}
		public static string DeleteData( ushort actualSave )
		{
			string actualPath = $@"{Application.persistentDataPath}\{FilesController.Select( actualSave )}.txt";
			if ( File.Exists( actualPath ) )
				File.Delete( actualPath );
			return FilesController.SaveData( (actualSave, $"Data File {actualSave}") );
		}
		public static void SaveData()
		{
			FilesController.SaveData();
			string actualSaveFile = FilesController.Select( _actualSaveFile );
			if ( string.IsNullOrEmpty( actualSaveFile ) )
				return;
			SaveFile newSaveFile = _saveFile;
			newSaveFile.BooksName = new List<string>( _saveFile.Books?.Count > 0f ? _saveFile.Books.Keys : new List<string>() );
			newSaveFile.BooksValue = new List<bool>( _saveFile.Books?.Count > 0f ? _saveFile.Books.Values : new List<bool>() );
			FileEncoder.WriteData( newSaveFile, $@"{Application.persistentDataPath}\{actualSaveFile}.txt" );
		}
	};
};
