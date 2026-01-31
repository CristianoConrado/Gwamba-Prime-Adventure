using Cysharp.Threading.Tasks;
using System.Collections;
using System.Threading;
using UnityEngine;
namespace GwambaPrimeAdventure
{
	[DisallowMultipleComponent, Icon( WorldBuild.PROJECT_ICON ), RequireComponent( typeof( Transform ) )]
	public sealed class SceneInitiator : MonoBehaviour
	{
		private static
			SceneInitiator _instance;
		[SerializeField, Tooltip( "The object that handles the hud of the trancision." )]
		private
			TransicionHud _transicionHud;
		[SerializeField, Tooltip( "The objects to be loaded." )]
		private
			ObjectLoader[] _objectLoaders;
		internal static ushort
			ProgressIndex = 0;
		private void Awake()
		{
			if ( _instance )
			{
				Destroy( gameObject, WorldBuild.MINIMUM_TIME_SPACE_LIMIT );
				return;
			}
			_instance = this;
		}
		private IEnumerator Start()
		{
			if ( !_instance || this != _instance )
				yield break;
			TransicionHud transicionHud = Instantiate( _transicionHud, transform );
			transicionHud.RootElement.style.opacity = 1F;
			transicionHud.LoadingBar.highValue = _objectLoaders.Length;
			ProgressIndex = 0;
			ObjectLoader requestedLoader;
			CancellationToken destroyToken = this.GetCancellationTokenOnDestroy();
			foreach ( ObjectLoader loader in _objectLoaders )
			{
				requestedLoader = Instantiate( loader );
				yield return requestedLoader.Load( transicionHud.LoadingBar ).AttachExternalCancellation( destroyToken ).SuppressCancellationThrow().ToCoroutine();
				if ( destroyToken.IsCancellationRequested )
				{
					Destroy( requestedLoader.gameObject );
					Application.Quit();
					yield break;
				}
			}
			for ( float i = 1F; 0F < transicionHud.RootElement.style.opacity.value; i -= 1E-1F )
			{
				yield return transicionHud.RootElement.style.opacity = i;
				if ( destroyToken.IsCancellationRequested )
				{
					Application.Quit();
					yield break;
				}
			}
			Destroy( gameObject );
			StateController.SetState( true );
		}
		public static bool IsInTransition() => _instance;
	};
};
