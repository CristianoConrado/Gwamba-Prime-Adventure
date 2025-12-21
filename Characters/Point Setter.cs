using UnityEngine;
using System.Collections;
using GwambaPrimeAdventure.Connection;
namespace GwambaPrimeAdventure.Character
{
	[DisallowMultipleComponent, RequireComponent( typeof( BoxCollider2D ) )]
	internal sealed class PointSetter : StateController, ILoader
	{
		private static PointSetter Instance;
		[SerializeField, Tooltip( "The name of the hubby world scene." ), Space( WorldBuild.FIELD_SPACE_LENGTH * 2F )] private SceneField _hubbyWorldScene;
		[SerializeField, Tooltip( "If this point is faced to left." )] private bool _turnToLeft;
		[SerializeField, Tooltip( "Which point setter is setted when scene is the hubby world." )] private ushort _selfIndex;
		internal static Vector2 CheckedPoint => Instance ? Instance.transform.position : Vector2.zero;
		internal static bool TurnToLeft => Instance ? Instance._turnToLeft : false;
		public IEnumerator Load()
		{
			SaveController.Load( out SaveFile saveFile );
			if ( gameObject.scene.name == _hubbyWorldScene && !string.IsNullOrEmpty( saveFile.LastLevelEntered ) )
				if ( saveFile.LastLevelEntered.Contains( $"{_selfIndex}" ) )
					Instance = this;
			yield return null;
		}
		private void OnTriggerEnter2D( Collider2D other )
		{
			if ( GwambaState<GwambaMarker>.Instance.EqualObject( other.gameObject ) && this != Instance )
				Instance = this;
		}
	};
};
