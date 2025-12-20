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
			(_instance, RootElement) = (this, GetComponent<UIDocument>().rootVisualElement.Q<VisualElement>( nameof( RootElement ) ));
			(CharacterIcon, CharacterName) = (RootElement.Q<VisualElement>( nameof( CharacterIcon ) ), RootElement.Q<Label>( nameof( CharacterName ) ));
			(CharacterSpeach, AdvanceSpeach) = (RootElement.Q<Label>( nameof( CharacterSpeach ) ), RootElement.Q<Button>( nameof( AdvanceSpeach ) ));
		}
	};
};
