using Cysharp.Threading.Tasks;
using GwambaPrimeAdventure.Connection;
using System.Threading;
using UnityEngine;
namespace GwambaPrimeAdventure.Character
{
	[DisallowMultipleComponent, RequireComponent( typeof( BoxCollider2D ) )]
	internal sealed class PointSetter : StateController, ILoader
	{
		private static
			PointSetter Instance;
		[SerializeField, Tooltip( "The name of the hubby world scene." ), Space( WorldBuild.FIELD_SPACE_LENGTH * 2F )]
		private
			SceneField _hubbyWorldScene;
		[SerializeField, Tooltip( "If this point is the initial point to be the instance." )]
		private
			bool _initial;
		[SerializeField, Tooltip( "If this point is faced to left." )]
		private
			bool _turnToLeft;
		[SerializeField, Tooltip( "Which point setter is setted when scene is the hubby world." )]
		private
			ushort _selfIndex;
		internal static Vector2 CheckedPoint =>
			Instance ? Instance.transform.position : Vector2.zero;
		internal static bool TurnToLeft =>
			Instance ? Instance._turnToLeft : false;
		public async UniTask Load()
		{
			CancellationToken destroyToken = this.GetCancellationTokenOnDestroy();
			await UniTask.Yield( PlayerLoopTiming.EarlyUpdate, destroyToken, true ).SuppressCancellationThrow();
			if ( destroyToken.IsCancellationRequested )
				return;
			SaveController.Load( out SaveFile saveFile );
			if ( gameObject.scene.name == _hubbyWorldScene && !string.IsNullOrEmpty( saveFile.LastLevelEntered ) && saveFile.LastLevelEntered.Contains( $"{_selfIndex}" ) )
				Instance = this;
			else if ( _initial && !Instance )
				Instance = this;
		}
		private void OnTriggerEnter2D( Collider2D other )
		{
			if ( GwambaState<GwambaMarker>.Instance.EqualObject( other.gameObject ) && this != Instance )
				Instance = this;
		}
	};
};
