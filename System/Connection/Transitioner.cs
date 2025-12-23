using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading;
using Cysharp.Threading.Tasks;
namespace GwambaPrimeAdventure.Connection
{
	[DisallowMultipleComponent, Icon( WorldBuild.PROJECT_ICON ), RequireComponent( typeof( Transform ) )]
	public sealed class Transitioner : MonoBehaviour
	{
		[Header( "Scene Interaction" )]
		[SerializeField, Tooltip( "The object that handles the hud of the trancision." )] private TransicionHud _transicionHud;
		[SerializeField, Tooltip( "The scene that will be trancisionate to." )] private SceneField _sceneTransicion;
		[SerializeField, Tooltip( "The scene of the menu." )] private SceneField _menuScene;
		public async void Transicion( SceneField scene = null )
		{
			if ( TransicionHud.Exists() )
				return;
			StateController.SetState( false );
			TransicionHud transicionHud = Instantiate( _transicionHud );
			CancellationToken destroyToken = this.GetCancellationTokenOnDestroy();
			for ( float i = 0F; 1F > transicionHud.RootElement.style.opacity.value; i += 1E-1F )
			{
				transicionHud.RootElement.style.opacity = i;
				await UniTask.Yield( PlayerLoopTiming.EarlyUpdate, destroyToken ).SuppressCancellationThrow();
				if ( destroyToken.IsCancellationRequested )
				{
					Application.Quit();
					return;
				}
			}
			SceneField newScene = scene ?? _sceneTransicion;
			SaveController.Load( out SaveFile saveFile );
			if ( SceneManager.GetActiveScene().name != newScene )
				if ( newScene.SceneName.Contains( $"{1..( WorldBuild.LEVELS_COUNT + 1 )}" ) )
					saveFile.LastLevelEntered = newScene;
			AsyncOperation asyncOperation = SceneManager.LoadSceneAsync( newScene, LoadSceneMode.Single );
			if ( newScene != _menuScene )
			{
				await UniTask.WaitUntil( () => asyncOperation.isDone, PlayerLoopTiming.Update, destroyToken ).SuppressCancellationThrow();
				if ( destroyToken.IsCancellationRequested )
				{
					Application.Quit();
					return;
				}
			}
			else
			{
				transicionHud.LoadingBar.highValue = 100F;
				while ( !asyncOperation.isDone )
				{
					transicionHud.LoadingBar.value = asyncOperation.progress * 100F;
					await UniTask.Yield( PlayerLoopTiming.EarlyUpdate, destroyToken ).SuppressCancellationThrow();
					if ( destroyToken.IsCancellationRequested )
					{
						Application.Quit();
						return;
					}
				}
			}
			asyncOperation.allowSceneActivation = true;
		}
	};
};
