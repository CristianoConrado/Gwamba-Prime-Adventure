using Cysharp.Threading.Tasks;
using GwambaPrimeAdventure.Connection;
using System.Threading;
using UnityEngine;
namespace GwambaPrimeAdventure.Item
{
	[DisallowMultipleComponent, RequireComponent( typeof( SpriteRenderer ), typeof( BoxCollider2D ) )]
	internal sealed class ExtraLife : StateController, ILoader, ICollectable
	{
		[SerializeField, Tooltip( "If this object will be saved as already existent object." ), Header( "Condition" )] private bool
			_saveOnSpecifics;
		public async UniTask Load()
		{
			CancellationToken destroyToken = this.GetCancellationTokenOnDestroy();
			await UniTask.Yield( PlayerLoopTiming.EarlyUpdate, destroyToken, true ).SuppressCancellationThrow();
			if ( destroyToken.IsCancellationRequested )
				return;
			SaveController.Load( out SaveFile saveFile );
			if ( saveFile.LifesAcquired.Contains( name ) )
				Destroy( gameObject );
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
