using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
namespace GwambaPrimeAdventure
{
	[Icon( WorldBuild.PROJECT_ICON ), RequireComponent( typeof( Transform ) )]
	internal sealed class ObjectLoader : MonoBehaviour
	{
		private static readonly List<ILoader> _loader = new List<ILoader>();
		public async UniTask Load( ProgressBar progressBar )
		{
			_loader.Clear();
			GetComponentsInChildren<ILoader>( _loader );
			CancellationToken destroyToken = this.GetCancellationTokenOnDestroy();
			float progress = 0F;
			for ( ushort i = 0; _loader.Count > i; i++ )
			{
				await _loader[ i ].Load().AttachExternalCancellation( destroyToken );
				progressBar.value -= progress;
				progressBar.value += progress = ( i + 1F ) / _loader.Count;
			}
			if ( 0 >= _loader.Count )
				progressBar.value += ++SceneInitiator.ProgressIndex / SceneInitiator.ProgressIndex;
			else
				progressBar.value += ++SceneInitiator.ProgressIndex - progressBar.value;
			transform.DetachChildren();
			Destroy( gameObject );
		}
	};
};
