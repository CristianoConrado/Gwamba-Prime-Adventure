using GwambaPrimeAdventure.Connection;
using UnityEngine;
namespace GwambaPrimeAdventure.Item
{
	[DisallowMultipleComponent, RequireComponent( typeof( SpriteRenderer ), typeof( BoxCollider2D ) )]
	internal sealed class Book : StateController, ICollectable
	{
		[SerializeField, Tooltip( "The sprite to show when the book gor cacthed." ), Header( "Conditions" )]
		private
			Sprite _bookCacthed;
		private void Start()
		{
			SaveController.Load( out SaveFile saveFile );
			if ( saveFile.Books.ContainsKey( name ) )
			{
				if ( saveFile.Books[ name ] )
					GetComponent<SpriteRenderer>().sprite = _bookCacthed;
				return;
			}
			saveFile.Books.Add( name, false );
		}
		public void Collect()
		{
			SaveController.Load( out SaveFile saveFile );
			if ( !saveFile.Books[ name ] )
				saveFile.Books[ name ] = true;
			GetComponent<SpriteRenderer>().sprite = _bookCacthed;
			if ( !saveFile.GeneralObjects.Contains( name ) )
				saveFile.GeneralObjects.Add( name );
			SaveController.WriteSave( saveFile );
		}
	};
};
