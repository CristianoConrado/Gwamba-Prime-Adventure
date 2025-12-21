using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;
using System.Collections.Generic;
namespace GwambaPrimeAdventure
{
	[Icon( WorldBuild.PROJECT_ICON )]
	internal sealed class ObjectLoader : MonoBehaviour
	{
		private static readonly List<ILoader> _loader = new();
		public IEnumerator Load( ProgressBar progressBar )
		{
			_loader.Clear();
			GetComponentsInChildren<ILoader>( _loader );
			float progress = 0F;
			for ( ushort i = 0; _loader.Count > i; i++ )
			{
				yield return StartCoroutine( _loader[ i ].Load() );
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
