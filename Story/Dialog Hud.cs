using UnityEngine;
using UnityEngine.UIElements;
namespace GwambaPrimeAdventure.Story
{
	[DisallowMultipleComponent, Icon( WorldBuild.PROJECT_ICON ), RequireComponent( typeof( Transform ), typeof( UIDocument ) )]
	internal sealed class DialogHud : MonoBehaviour
	{
		static private DialogHud _instance;
		internal VisualElement RootElement { get; private set; }
		internal VisualElement CharacterIcon { get; private set; }
		internal Label CharacterName { get; private set; }
		internal Label CharacterSpeach { get; private set; }
		internal Button AdvanceSpeach { get; private set; }
		private void Awake()
		{
			if ( _instance )
			{
				Destroy( gameObject, WorldBuild.MINIMUM_TIME_SPACE_LIMIT );
				return;
			}
			_instance = this;
			RootElement = GetComponent<UIDocument>().rootVisualElement.Q<VisualElement>( nameof( RootElement ) );
			CharacterIcon = RootElement.Q<VisualElement>( nameof( CharacterIcon ) );
			CharacterName = RootElement.Q<Label>( nameof( CharacterName ) );
			CharacterSpeach = RootElement.Q<Label>( nameof( CharacterSpeach ) );
			AdvanceSpeach = RootElement.Q<Button>( nameof( AdvanceSpeach ) );
		}
	};
};
