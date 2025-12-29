using UnityEngine;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using GwambaPrimeAdventure.Character;
using GwambaPrimeAdventure.Enemy.Supply;
namespace GwambaPrimeAdventure.Enemy
{
	internal abstract class MovingEnemy : EnemyProvider, IConnector
	{
		protected readonly List<ContactPoint2D>
			_groundContacts = new List<ContactPoint2D>( (int) WorldBuild.PIXELS_PER_UNIT );
		protected Vector2
			_originCast = Vector2.zero,
			_sizeCast = Vector2.zero;
		protected float
			_stoppedTime = 0F;
		protected short
			_movementSide = 1;
		protected bool
			_detected = false,
			_isDashing = false;
		[SerializeField, Tooltip( "The moving statitics of this enemy." ), Header( "Moving Enemy" )]
		private MovingStatistics
			_moving;
		protected bool OnGround
		{
			get;
			private set;
		}
		protected new void Awake()
		{
			base.Awake();
			_sender.SetFormat( MessageFormat.State );
			Sender.Include( this );
		}
		protected new void OnDestroy()
		{
			base.OnDestroy();
			Sender.Exclude( this );
		}
		protected async void Start()
		{
			CancellationToken destroyToken = this.GetCancellationTokenOnDestroy();
			await UniTask.WaitWhile( () => SceneInitiator.IsInTrancision(), PlayerLoopTiming.Update, destroyToken, true ).SuppressCancellationThrow();
			if ( destroyToken.IsCancellationRequested )
				return;
			_movementSide = (short) ( ( CharacterExporter.GwambaLocalization().x < transform.position.x ? -1 : 1 ) * ( _moving.InvertMovementSide ? -1 : 1 ) );
			transform.TurnScaleX( _movementSide );
		}
		protected void OnCollisionStay2D( Collision2D collision )
		{
			if ( WorldBuild.SCENE_LAYER != collision.gameObject.layer || ( OnGround && Mathf.Abs( Rigidbody.linearVelocityY ) <= WorldBuild.MINIMUM_TIME_SPACE_LIMIT * 10F ) )
				return;
			_collider.GetContacts( _groundContacts );
			_originCast.Set( transform.position.x + _collider.offset.x, transform.position.y + _collider.offset.y - _collider.bounds.extents.y * transform.up.y );
			_sizeCast.Set( _collider.bounds.size.x, WorldBuild.SNAP_LENGTH );
			_groundContacts.RemoveAll( contact => contact.point.OutsideRectangle( _originCast, _sizeCast ) );
			OnGround = 0 < _groundContacts.Count;
		}
		protected void OnCollisionExit2D( Collision2D collision )
		{
			if ( WorldBuild.SCENE_LAYER == collision.gameObject.layer )
				OnGround = false;
		}
		public void Receive( MessageData message )
		{
			if ( MessageFormat.State == message.Format && message.ToggleValue.HasValue )
				_stopWorking = !message.ToggleValue.Value;
		}
	};
};
