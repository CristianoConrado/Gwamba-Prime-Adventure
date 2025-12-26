using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using UnityEngine.UIElements;
namespace GwambaPrimeAdventure.Story
{
	[DisallowMultipleComponent, Icon( WorldBuild.PROJECT_ICON ), RequireComponent( typeof( Transform ) )]
	internal sealed class StoryTeller : MonoBehaviour
	{
		private StorySceneHud _storySceneHud;
		private CancellationToken _destroyToken;
		private ushort
			_imageIndex = 0;
		[SerializeField, Tooltip( "The object that handles the hud of the story scene." ), Header( "Scene Objects" )] private StorySceneHud
			_storySceneHudObject;
		[SerializeField, Tooltip( "The object that carry the scene settings." )] private StorySceneObject
			_storySceneObject;
		private async UniTask FadeImage( bool appear )
		{
			if ( appear )
				for ( float i = 0F; 1F > _storySceneHud.SceneImage.style.opacity.value; i += 1E-1F )
				{
					_storySceneHud.SceneImage.style.opacity = i;
					await UniTask.Yield( PlayerLoopTiming.Update, _destroyToken, true ).SuppressCancellationThrow();
					if ( _destroyToken.IsCancellationRequested )
						return;
				}
			else
				for ( float i = 1F; 0F < _storySceneHud.SceneImage.style.opacity.value; i -= 1E-1F )
				{
					_storySceneHud.SceneImage.style.opacity = i;
					await UniTask.Yield( PlayerLoopTiming.Update, _destroyToken, true ).SuppressCancellationThrow();
					if ( _destroyToken.IsCancellationRequested )
						return;
				}
		}
		internal void ShowScene()
		{
			_storySceneHud = Instantiate( _storySceneHudObject, transform );
			_destroyToken = this.GetCancellationTokenOnDestroy();
			_storySceneHud.SceneImage.style.backgroundImage = Background.FromTexture2D( _storySceneObject.SceneComponents[ _imageIndex = 0 ].Image );
			FadeImage( true ).Forget();
		}
		internal async UniTask NextSlide()
		{
			if ( _storySceneObject.SceneComponents[ _imageIndex ].Equals( _storySceneObject.SceneComponents[ ^1 ] ) )
				return;
			await FadeImage( false ).SuppressCancellationThrow();
			if ( _destroyToken.IsCancellationRequested )
				return;
			_imageIndex = (ushort) ( _storySceneObject.SceneComponents.Length - 1 > _imageIndex ? _imageIndex + 1 : 0 );
			_storySceneHud.SceneImage.style.backgroundImage = Background.FromTexture2D( _storySceneObject.SceneComponents[ _imageIndex ].Image );
			await FadeImage( true ).SuppressCancellationThrow();
			if ( _destroyToken.IsCancellationRequested )
				return;
			if ( _storySceneObject.SceneComponents[ _imageIndex ].OffDialog )
			{
				await UniTask.WaitForSeconds( _storySceneObject.SceneComponents[ _imageIndex ].TimeToDesapear, true, PlayerLoopTiming.Update, _destroyToken, true ).SuppressCancellationThrow();
				if ( _destroyToken.IsCancellationRequested )
					return;
				if ( _storySceneObject.SceneComponents[ _imageIndex ].JumpToNext )
					await NextSlide().SuppressCancellationThrow();
				if ( _destroyToken.IsCancellationRequested )
					return;
			}
		}
		internal async void CloseScene()
		{
			await FadeImage( false ).SuppressCancellationThrow();
			if ( _destroyToken.IsCancellationRequested )
				return;
			Destroy( _storySceneHud.gameObject );
		}
	};
};
