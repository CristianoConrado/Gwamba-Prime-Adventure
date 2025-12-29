using UnityEngine;
using UnityEngine.UIElements;
namespace GwambaPrimeAdventure
{
	[DisallowMultipleComponent, Icon( WorldBuild.PROJECT_ICON ), RequireComponent( typeof( Transform ), typeof( UIDocument ) )]
	public sealed class TransicionHud : MonoBehaviour
	{
		private static TransicionHud _instance;
		public VisualElement RootElement
		{
			get;
			private set;
		}
		public ProgressBar LoadingBar
		{
			get;
			private set;
		}
		private void Awake()
		{
			if ( _instance )
			{
				Destroy( gameObject, WorldBuild.MINIMUM_TIME_SPACE_LIMIT );
				return;
			}
			_instance = this;
			RootElement = GetComponent<UIDocument>().rootVisualElement.Q<VisualElement>( nameof( RootElement ) );
			LoadingBar = RootElement.Q<ProgressBar>( nameof( LoadingBar ) );
		}
		public static bool Exists() => _instance;
	};
};
