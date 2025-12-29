using UnityEngine;
using UnityEngine.UIElements;
namespace GwambaPrimeAdventure.Item
{
	[DisallowMultipleComponent, Icon( WorldBuild.PROJECT_ICON ), RequireComponent( typeof( Transform ), typeof( UIDocument ) )]
	internal sealed class LevelGateHud : MonoBehaviour
	{
		internal UIDocument Document
		{
			get;
			private set;
		}
		internal VisualElement RootElement
        {
            get;
            private set;
        }
        internal Button Level
        {
            get;
            private set;
        }
        internal Button Boss
        {
            get;
            private set;
        }
        internal Button Scenes
        {
            get;
            private set;
        }
        private void Awake()
		{
			Document = GetComponent<UIDocument>();
			RootElement = Document.rootVisualElement.Q<VisualElement>( nameof( RootElement ) );
			Level = RootElement.Q<Button>( nameof( Level ) );
			Boss = RootElement.Q<Button>( nameof( Boss ) );
			Scenes = RootElement.Q<Button>( nameof( Scenes ) );
		}
	};
};
