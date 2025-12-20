using UnityEngine;
using System;
using NaughtyAttributes;
namespace GwambaPrimeAdventure.Story
{
	[CreateAssetMenu( fileName = "Dialog", menuName = "Story/Dialog", order = 0 )]
	internal class DialogObject : ScriptableObject
	{
		[field: SerializeField, Tooltip( "The collection of the object that contais the speach." ), Header( "Dialog Components" ), Space( WorldBuild.FIELD_SPACE_LENGTH * 2F )]
		internal Speach[] Speachs { get; private set; }
		[field: SerializeField, Tooltip( "If the interaction have to be desactivated after the speach." )] internal bool DesactiveInteraction { get; private set; }
		[field: SerializeField, Tooltip( "If the object will destruct." )] internal bool EndDestroy { get; private set; }
		[field: SerializeField, ShowIf( nameof( EndDestroy ) ), Tooltip( "The amount of time to wait to destroy after the speach." )] internal float TimeToDestroy { get; private set; }
		[field: SerializeField, Tooltip( "If the trancision have to be activated after the speach." )] internal bool Transition { get; private set; }
		[field: SerializeField, ShowIf( nameof( Transition ) ), Tooltip( "The scene to trancision to after the speach." )] internal SceneField SceneToTransition { get; private set; }
		[field: SerializeField, Tooltip( "If the animation will play." )] internal bool ActivateAnimation { get; private set; }
		[field: SerializeField, ShowIf( nameof( ActivateAnimation ) ), Tooltip( "The animation to play after the speach." )] internal string Animation { get; private set; }
		[field: SerializeField, Tooltip( "If this object will be saved as already existent object." )] internal bool SaveOnEspecific { get; private set; }
		[Serializable]
		internal struct Speach
		{
			[field: SerializeField, Tooltip( "The image icon of the character that is speaking." ), Header( "Speach Components" ), Space( WorldBuild.FIELD_SPACE_LENGTH * 2F )] internal Sprite Model { get; private set; }
			[field: SerializeField, Tooltip( "The name of the character that is speaking." )] internal string CharacterName { get; private set; }
			[field: SerializeField, TextArea( 0, 12 ), Tooltip( "The speach of the character that is speaking." )] internal string SpeachText { get; private set; }
			[field: SerializeField, Tooltip( "If after the speach the next slide of story scene have to come." )] internal bool NextSlide { get; private set; }
		};
	};
};
