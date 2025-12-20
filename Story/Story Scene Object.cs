using UnityEngine;
using System;
namespace GwambaPrimeAdventure.Story
{
	[CreateAssetMenu( fileName = "Scene", menuName = "Story/Scene", order = 1 )]
	internal sealed class StorySceneObject : ScriptableObject
	{
		[field: SerializeField, Tooltip( "The collection of objects that carry the background settings." ), Header( "Scene Component Collection" ), Space( WorldBuild.FIELD_SPACE_LENGTH * 2F )]
		internal SceneComponent[] SceneComponents { get; private set; }
		[Serializable]
		internal struct SceneComponent
		{
			[field: SerializeField, Tooltip( "The main imgae that is placed in the hud." ), Header( "Components" ), Space( WorldBuild.FIELD_SPACE_LENGTH * 2F )] internal Texture2D Image { get; private set; }
			[field: SerializeField, Tooltip( "If the dialog will go turned off during the scene." )] internal bool OffDialog { get; private set; }
			[field: SerializeField, Tooltip( "The amount of time that the dialog will be turned off." )] internal float TimeToDesapear { get; private set; }
			[field: SerializeField, Tooltip( "If the dialog will jump to the next slide." )] internal bool JumpToNext { get; private set; }
		};
	};
};
