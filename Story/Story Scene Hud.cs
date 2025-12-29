using UnityEngine;
using UnityEngine.UIElements;
namespace GwambaPrimeAdventure.Story
{
	[DisallowMultipleComponent, Icon( WorldBuild.PROJECT_ICON ), RequireComponent( typeof( Transform ), typeof( UIDocument ) )]
	internal sealed class StorySceneHud : MonoBehaviour
	{
		private static
			StorySceneHud _instance;
		internal VisualElement SceneImage
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
			SceneImage = GetComponent<UIDocument>().rootVisualElement.Q<VisualElement>( nameof( SceneImage ) );
		}
	};
};
