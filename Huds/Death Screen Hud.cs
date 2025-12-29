using UnityEngine;
using UnityEngine.UIElements;
namespace GwambaPrimeAdventure.Hud
{
	[DisallowMultipleComponent, Icon( WorldBuild.PROJECT_ICON ), RequireComponent( typeof( Transform ), typeof( UIDocument ) )]
	internal sealed class DeathScreenHud : MonoBehaviour
	{
		private static
			DeathScreenHud _instance;
		internal VisualElement RootElement
        {
            get;
            private set;
        }
        internal VisualElement Curtain
        {
            get;
            private set;
        }
        internal Label Text
        {
            get;
            private set;
        }
        internal Button Continue
        {
            get;
            private set;
        }
        internal Button OutLevel
        {
            get;
            private set;
        }
        internal Button GameOver
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
			Curtain = RootElement.Q<VisualElement>( nameof( Curtain ) );
			Text = RootElement.Q<Label>( nameof( Text ) );
			Continue = RootElement.Q<Button>( nameof( Continue ) );
			OutLevel = RootElement.Q<Button>( nameof( OutLevel ) );
			GameOver = RootElement.Q<Button>( nameof( GameOver ) );
		}
	};
};
