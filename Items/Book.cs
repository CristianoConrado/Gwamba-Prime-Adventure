using UnityEngine;
using System.Collections;
using GwambaPrimeAdventure.Connection;
namespace GwambaPrimeAdventure.Item
{
	[DisallowMultipleComponent, RequireComponent( typeof( SpriteRenderer ), typeof( BoxCollider2D ) )]
	internal sealed class Book : StateController, ILoader, ICollectable
	{
		[Header( "Conditions" )]
		[SerializeField, Tooltip( "The sprite to show when the book gor cacthed." )] private Sprite _bookCacthed;
		public IEnumerator Load()
		{
			SaveController.Load( out SaveFile saveFile );
			if ( saveFile.Books.ContainsKey( name ) )
			{
				if ( saveFile.Books[ name ] )
					GetComponent<SpriteRenderer>().sprite = _bookCacthed;
				yield break;
			}
			saveFile.Books.Add( name, false );
			yield return null;
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
