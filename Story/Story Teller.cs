using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;
namespace GwambaPrimeAdventure.Story
{
	[DisallowMultipleComponent, Icon(WorldBuild.PROJECT_ICON), RequireComponent(typeof(Transform))]
	internal sealed class StoryTeller : MonoBehaviour
	{
		private StorySceneHud _storySceneHud;
		private ushort _imageIndex = 0;
		[Header("Scene Objects")]
		[SerializeField, Tooltip("The object that handles the hud of the story scene.")] private StorySceneHud _storySceneHudObject;
		[SerializeField, Tooltip("The object that carry the scene settings.")] private StorySceneObject _storySceneObject;
		private IEnumerator FadeImage(bool appear)
		{
			if (appear)
				for (float i = 0F; 1F > _storySceneHud.SceneImage.style.opacity.value; i += 1E-1F)
					yield return _storySceneHud.SceneImage.style.opacity = i;
			else
				for (float i = 1F; 0F < _storySceneHud.SceneImage.style.opacity.value; i -= 1E-1F)
					yield return _storySceneHud.SceneImage.style.opacity = i;
		}
		internal void ShowScene()
		{
			_storySceneHud = Instantiate(_storySceneHudObject, transform);
			_storySceneHud.SceneImage.style.backgroundImage = Background.FromTexture2D(_storySceneObject.SceneComponents[_imageIndex = 0].Image);
			StartCoroutine(FadeImage(true));
		}
		internal IEnumerator NextSlide()
		{
			if (_storySceneObject.SceneComponents[_imageIndex].Equals(_storySceneObject.SceneComponents[^1]))
				yield break;
			yield return StartCoroutine(FadeImage(false));
			_imageIndex = (ushort)(_storySceneObject.SceneComponents.Length - 1 > _imageIndex ? _imageIndex + 1 : 0);
			_storySceneHud.SceneImage.style.backgroundImage = Background.FromTexture2D(_storySceneObject.SceneComponents[_imageIndex].Image);
			yield return StartCoroutine(FadeImage(true));
			if (_storySceneObject.SceneComponents[_imageIndex].OffDialog)
			{
				yield return new WaitForSeconds(_storySceneObject.SceneComponents[_imageIndex].TimeToDesapear);
				if (_storySceneObject.SceneComponents[_imageIndex].JumpToNext)
					yield return StartCoroutine(NextSlide());
			}
		}
		internal void CloseScene()
		{
			StartCoroutine(DestroyScene());
			IEnumerator DestroyScene()
			{
				yield return StartCoroutine(FadeImage(false));
				Destroy(_storySceneHud.gameObject);
			}
		}
	};
};

