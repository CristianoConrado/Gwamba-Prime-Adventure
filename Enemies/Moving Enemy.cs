using UnityEngine;
using System.Collections.Generic;
using GwambaPrimeAdventure.Character;
using GwambaPrimeAdventure.Enemy.Supply;
namespace GwambaPrimeAdventure.Enemy
{
	internal abstract class MovingEnemy : EnemyProvider, IConnector
	{
		protected readonly List<ContactPoint2D>
			_groundContacts = new List<ContactPoint2D>( (byte) WorldBuild.PIXELS_PER_UNIT );
		protected Vector2
			_originCast = Vector2.zero,
			_sizeCast = Vector2.zero;
		protected const float
			MINIMUM_VELOCITY = 1E-3F;
		protected readonly int
			Idle = Animator.StringToHash( nameof( Idle ) ),
			Move = Animator.StringToHash( nameof( Move ) ),
			Dash = Animator.StringToHash( nameof( Dash ) );
		protected float
			_stoppedTime = 0F;
		protected sbyte
			_movementSide = 1;
		protected bool
			_detected = false,
			_begining = false;
		[SerializeField, Tooltip( "The moving statitics of this enemy." ), Header( "Moving Enemy" )]
		private
			MovingStatistics _moving;
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
		protected void FixedUpdate()
		{
			if ( SceneInitiator.IsInTransition() )
				return;
			if ( !_begining )
			{
				_begining = true;
				_movementSide = (sbyte) ( ( CharacterExporter.GwambaLocalization().x < transform.position.x ? -1 : 1 ) * ( _moving.InvertMovementSide ? -1 : 1 ) );
				transform.TurnScaleX( _movementSide );
			}
			if ( Animator.GetBool( Move ) && Mathf.Abs( Rigidbody.linearVelocityX ) <= MINIMUM_VELOCITY )
				Animator.SetBool( Move, false );
			if ( !OnGround )
				if ( !Animator.GetBool( Fall ) && 0F > Rigidbody.linearVelocityY )
					Animator.SetBool( Fall, true );
				else if ( Animator.GetBool( Fall ) && 0F < Rigidbody.linearVelocityY )
					Animator.SetBool( Fall, false );
		}
		protected void OnCollisionStay2D( Collision2D collision )
		{
			if ( SceneInitiator.IsInTransition() || WorldBuild.SCENE_LAYER != collision.gameObject.layer || OnGround && Mathf.Abs( Rigidbody.linearVelocityY ) <= MINIMUM_VELOCITY )
				return;
			_collider.GetContacts( _groundContacts );
			if ( OnGround = _groundContacts.Exists( contact => _moving.CheckGroundLimit <= contact.normal.y ) )
			{
				if ( !Animator.GetBool( Idle ) && Mathf.Abs( Rigidbody.linearVelocityX ) <= MINIMUM_VELOCITY )
					Animator.SetBool( Idle, true );
				else if ( Animator.GetBool( Idle ) && Mathf.Abs( Rigidbody.linearVelocityX ) > MINIMUM_VELOCITY )
					Animator.SetBool( Idle, false );
				if ( Animator.GetBool( Fall ) )
					Animator.SetBool( Fall, false );
			}
		}
		protected void OnCollisionExit2D( Collision2D collision )
		{
			if ( SceneInitiator.IsInTransition() || WorldBuild.SCENE_LAYER == collision.gameObject.layer )
			{
				_collider.GetContacts( _groundContacts );
				if ( _groundContacts.Exists( contact => _moving.CheckGroundLimit <= contact.normal.y ) )
					return;
				OnGround = false;
			}
		}
		public void Receive( MessageData message )
		{
			if ( MessageFormat.State == message.Format && message.ToggleValue.HasValue )
			{
				Animator.SetBool( Stop, !message.ToggleValue.Value );
				if ( Animator.GetBool( Stop ) && Animator.GetBool( Move ) )
					Animator.SetBool( Move, false );
				if ( Animator.GetBool( Stop ) && Animator.GetBool( Dash ) )
					Animator.SetBool( Dash, false );
			}
		}
	};
};
