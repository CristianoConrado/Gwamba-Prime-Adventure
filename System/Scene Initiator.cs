using UnityEngine;
using System.Collections;
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
		private IEnumerator Start()
		{
			if ( !_instance || this != _instance )
				yield break;
			TransicionHud transicionHud = Instantiate( _transicionHud, transform );
			(transicionHud.RootElement.style.opacity, transicionHud.LoadingBar.highValue, ProgressIndex) = (1F, _objectLoaders.Length, 0);
			foreach ( ObjectLoader loader in _objectLoaders )
				yield return StartCoroutine( Instantiate( loader ).Load( transicionHud.LoadingBar ) );
			for ( float i = 1F; 0F < transicionHud.RootElement.style.opacity.value; i -= 1E-1F )
				yield return transicionHud.RootElement.style.opacity = i;
			Destroy( gameObject );
			StateController.SetState( true );
		}
		public static bool IsInTrancision() => _instance;
	};
};
