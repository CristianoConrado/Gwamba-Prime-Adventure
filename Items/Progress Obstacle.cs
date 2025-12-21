using UnityEngine;
using System.Collections;
using GwambaPrimeAdventure.Connection;
namespace GwambaPrimeAdventure.Item
{
	[DisallowMultipleComponent, RequireComponent( typeof( Collider2D ) )]
	internal sealed class ProgressObstacle : StateController, ILoader
	{
		[Header( "Progress Interactions" )]
		[SerializeField, Tooltip( "The index that this object will check if theres anything completed." )] private ushort _progressIndex;
		[SerializeField, Tooltip( "If the index is about the boss." )] private bool _isBossProgress;
		[SerializeField, Tooltip( "If this object will be saved as already existent object." )] private bool _saveOnSpecifics;
		public IEnumerator Load()
		{
			SaveController.Load( out SaveFile saveFile );
			if ( _isBossProgress ? saveFile.DeafetedBosses[ _progressIndex - 1 ] : saveFile.LevelsCompleted[ _progressIndex - 1 ] )
			{
				if ( _saveOnSpecifics && !saveFile.GeneralObjects.Contains( name ) )
				{
					saveFile.GeneralObjects.Add( name );
					SaveController.WriteSave( saveFile );
				}
				Destroy( gameObject );
			}
			yield return null;
		}
	};
};
