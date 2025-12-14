using UnityEngine;
using System.Collections;
namespace GwambaPrimeAdventure
{
	[DisallowMultipleComponent, Icon(WorldBuild.PROJECT_ICON), RequireComponent(typeof(Transform))]
	public sealed class SceneInitiator : MonoBehaviour
	{
		private static SceneInitiator _instance;
		[SerializeField, Tooltip("The object that handles the hud of the trancision.")] private TransicionHud _transicionHud;
		[SerializeField, Tooltip("The objects to be lodaed.")] private ObjectLoader[] _objectLoaders;
		internal static ushort ProgressIndex = 0;
		private void Awake()
		{
			if (_instance)
			{
				Destroy(gameObject, WorldBuild.MINIMUM_TIME_SPACE_LIMIT);
				return;
			}
			_instance = this;
		}
		private IEnumerator Start()
		{
			if (!_instance || this != _instance)
				yield break;
			TransicionHud transicionHud = Instantiate(_transicionHud, transform);
			(transicionHud.RootElement.style.opacity, transicionHud.LoadingBar.highValue, ProgressIndex) = (1F, _objectLoaders.Length, 0);
			for (ushort i = 0; i < _objectLoaders.Length; i++)
				yield return StartCoroutine(Instantiate(_objectLoaders[i]).Load(transicionHud.LoadingBar));
			Destroy(gameObject);
			StateController.SetState(true);
		}
		public static bool IsInTrancision() => _instance;
	};
};
