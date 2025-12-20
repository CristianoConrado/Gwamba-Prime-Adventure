using UnityEngine;
using GwambaPrimeAdventure.Character;
using GwambaPrimeAdventure.Connection;
namespace GwambaPrimeAdventure.Item
{
	[DisallowMultipleComponent, RequireComponent( typeof( Transform ), typeof( SpriteRenderer ), typeof( CircleCollider2D ) ), RequireComponent( typeof( Transitioner ), typeof( IInteractable ) )]
	internal sealed class LevelGoal : StateController
	{
		private static LevelGoal Instance;
		private bool _blocked = false;
		[Header( "Scene Interactions" )]
		[SerializeField, Tooltip( "If this will go direct to the boss." )] private SceneField _goToBoss;
		[SerializeField, Tooltip( "If theres a dialog after the goal." )] private bool _enterInDialog;
		[SerializeField, Tooltip( "If this object will be saved as already existent object." )] private bool _saveOnSpecifics;
		private new void Awake()
		{
			base.Awake();
			if ( Instance )
			{
				Destroy( gameObject, WorldBuild.MINIMUM_TIME_SPACE_LIMIT );
				return;
			}
			Instance = this;
		}
		private void OnTriggerEnter2D( Collider2D other )
		{
			if ( _blocked || !CharacterExporter.EqualGwamba( other.gameObject ) )
				return;
			_blocked = true;
			SaveController.Load( out SaveFile saveFile );
			if ( !saveFile.LevelsCompleted[ ushort.Parse( $"{gameObject.scene.name[ ^1 ]}" ) - 1 ] )
			{
				saveFile.LevelsCompleted[ ushort.Parse( $"{gameObject.scene.name[ ^1 ]}" ) - 1 ] = true;
				SaveController.WriteSave( saveFile );
			}
			if ( _saveOnSpecifics && !saveFile.GeneralObjects.Contains( name ) )
			{
				saveFile.GeneralObjects.Add( name );
				SaveController.WriteSave( saveFile );
			}
			SettingsController.Load( out Settings settings );
			if ( _enterInDialog && settings.DialogToggle )
				GetComponent<IInteractable>().Interaction();
			else if ( 0 <= ushort.Parse( $"{gameObject.scene.name[ ^1 ]}" ) - 1 && !saveFile.DeafetedBosses[ ushort.Parse( $"{gameObject.scene.name[ ^1 ]}" ) - 1 ] )
				GetComponent<Transitioner>().Transicion( _goToBoss );
			else
				GetComponent<Transitioner>().Transicion();
		}
	};
};
