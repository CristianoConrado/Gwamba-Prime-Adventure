using UnityEngine;
using System.Threading;
using Cysharp.Threading.Tasks;
namespace GwambaPrimeAdventure
{
	[DisallowMultipleComponent, Icon( WorldBuild.PROJECT_ICON ), RequireComponent( typeof( Transform ) )]
	public sealed class SceneInitiator : MonoBehaviour
	{
		private static SceneInitiator _instance;
		[SerializeField, Tooltip( "The object that handles the hud of the trancision." )] private TransicionHud _transicionHud;
		[SerializeField, Tooltip( "The objects to be lodaed." )] private ObjectLoader[] _objectLoaders;
		internal static ushort ProgressIndex = 0;
		private void Awake()
		{
			if ( _instance )
			{
				Destroy( gameObject, WorldBuild.MINIMUM_TIME_SPACE_LIMIT );
				return;
			}
			_instance = this;
		}
		private async void Start()
		{
			if ( !_instance || this != _instance )
				return;
			CancellationToken destroyToken = this.GetCancellationTokenOnDestroy();
			TransicionHud transicionHud = Instantiate( _transicionHud, transform );
			(transicionHud.RootElement.style.opacity, transicionHud.LoadingBar.highValue, ProgressIndex) = (1F, _objectLoaders.Length, 0);
			ObjectLoader requestedLoader;
			foreach ( ObjectLoader loader in _objectLoaders )
			{
				await ( requestedLoader = Instantiate( loader ) ).Load( transicionHud.LoadingBar ).AttachExternalCancellation( destroyToken ).SuppressCancellationThrow();
				if ( destroyToken.IsCancellationRequested )
				{
					Destroy( requestedLoader.gameObject );
					Application.Quit();
					return;
				}
			}
			for ( float i = 1F; 0F < transicionHud.RootElement.style.opacity.value; i -= 1E-1F )
			{
				transicionHud.RootElement.style.opacity = i;
				await UniTask.Yield( PlayerLoopTiming.Update, destroyToken, true ).SuppressCancellationThrow();
				if ( destroyToken.IsCancellationRequested )
				{
					Application.Quit();
					return;
				}
			}
			Destroy( gameObject );
			StateController.SetState( true );
		}
		public static bool IsInTrancision() => _instance;
	};
};
