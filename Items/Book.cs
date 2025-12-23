using Cysharp.Threading.Tasks;
using GwambaPrimeAdventure.Connection;
using System.Threading;
using UnityEngine;
namespace GwambaPrimeAdventure.Item
{
	[DisallowMultipleComponent, RequireComponent( typeof( SpriteRenderer ), typeof( BoxCollider2D ) )]
	internal sealed class Book : StateController, ILoader, ICollectable
	{
		[Header( "Conditions" )]
		[SerializeField, Tooltip( "The sprite to show when the book gor cacthed." )] private Sprite _bookCacthed;
		public async UniTask Load()
		{
			CancellationToken destroyToken = this.GetCancellationTokenOnDestroy();
			await UniTask.Yield( PlayerLoopTiming.EarlyUpdate, destroyToken ).SuppressCancellationThrow();
			if ( destroyToken.IsCancellationRequested )
				return;
			SaveController.Load( out SaveFile saveFile );
			if ( saveFile.Books.ContainsKey( name ) )
			{
				if ( saveFile.Books[ name ] )
					GetComponent<SpriteRenderer>().sprite = _bookCacthed;
				await UniTask.Yield( PlayerLoopTiming.Update, destroyToken ).SuppressCancellationThrow();
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
