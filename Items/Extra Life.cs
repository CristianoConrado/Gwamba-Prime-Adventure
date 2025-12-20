using UnityEngine;
using System.Collections;
using GwambaPrimeAdventure.Connection;
namespace GwambaPrimeAdventure.Item
{
	[DisallowMultipleComponent, RequireComponent( typeof( Transform ), typeof( SpriteRenderer ), typeof( BoxCollider2D ) )]
	internal sealed class ExtraLife : StateController, ILoader, ICollectable
	{
		[Header( "Condition" )]
		[SerializeField, Tooltip( "If this object will be saved as already existent object." )] private bool _saveOnSpecifics;
		public IEnumerator Load()
		{
			SaveController.Load( out SaveFile saveFile );
			if ( saveFile.LifesAcquired.Contains( name ) )
				Destroy( gameObject );
			yield return null;
		}
		public void Collect()
		{
			SaveController.Load( out SaveFile saveFile );
			if ( 100 > saveFile.Lifes )
				saveFile.Lifes += 1;
			saveFile.LifesAcquired.Add( name );
			if ( _saveOnSpecifics && !saveFile.GeneralObjects.Contains( name ) )
				saveFile.GeneralObjects.Add( name );
			SaveController.WriteSave( saveFile );
			Destroy( gameObject );
		}
	};
};
