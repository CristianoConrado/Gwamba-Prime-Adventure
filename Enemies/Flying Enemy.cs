using GwambaPrimeAdventure.Character;
using GwambaPrimeAdventure.Enemy.Supply;
using UnityEngine;
namespace GwambaPrimeAdventure.Enemy
{
	[DisallowMultipleComponent, RequireComponent( typeof( PolygonCollider2D ) )]
	internal sealed class FlyingEnemy : MovingEnemy, IEnemyLoader, IConnector
	{
		private
			CircleCollider2D _selfCollider;
		private
			Transform _detectionObject;
		private
			CapsuleCollider2D _detectionCollider;
		private readonly RaycastHit2D[]
			_dashCheck = new RaycastHit2D[ (ushort) WorldBuild.PIXELS_PER_UNIT ];
		private
			Vector2[] _trail;
		private
			ContactFilter2D _dashFilter;
		private Vector2
			_movementDirection = Vector2.up,
			_pointOrigin = Vector2.zero,
			_targetPoint = Vector2.zero;
		private readonly int
			Chase = Animator.StringToHash( nameof( Chase ) );
		private byte
			_pointIndex = 0;
		private ushort
			_dashSize = 0;
		private bool
			_returnOrigin = false,
			_afterDash = false,
			_returnDash = false;
		[SerializeField, Tooltip( "The flying statitics of this enemy." ), Header( "Flying Enemy" )]
		private
			FlyingStatistics _statistics;
		[SerializeField, Tooltip( "If this enemy will repeat the same way it makes before." )]
		private
			bool _repeatWay;
		private new void Awake()
		{
			base.Awake();
			_selfCollider = _collider as CircleCollider2D;
			PolygonCollider2D trail = GetComponent<PolygonCollider2D>();
			_trail = new Vector2[ trail.points.Length ];
			for ( byte i = 0; trail.points.Length > i; i++ )
				_trail[ i ] = transform.parent ? trail.offset + trail.points[ i ] + (Vector2) transform.position : trail.points[ i ];
			_dashFilter = new ContactFilter2D()
			{
				layerMask = WorldBuild.SCENE_LAYER_MASK,
				useLayerMask = true,
				useTriggers = false
			};
			_pointOrigin = Rigidbody.position;
			Sender.Include( this );
		}
		private new void OnDestroy()
		{
			base.OnDestroy();
			Sender.Exclude( this );
		}
		public void Load()
		{
			GameObject detectionObject = new( "Detection Collider", typeof( CapsuleCollider2D ) )
			{
				layer = gameObject.layer,
				tag = tag
			};
			( _detectionObject = detectionObject.transform ).SetParent( transform );
			_detectionObject.localPosition = Vector3.zero;
			_detectionCollider = detectionObject.GetComponent<CapsuleCollider2D>();
			_detectionCollider.size = new Vector2( 2F * _selfCollider.radius, 4F * _selfCollider.radius );
			_detectionCollider.direction = CapsuleDirection2D.Vertical;
			_detectionCollider.isTrigger = true;
			_detectionCollider.contactCaptureLayers = WorldBuild.SCENE_LAYER_MASK;
			_detectionCollider.callbackLayers = WorldBuild.SCENE_LAYER_MASK;
		}
		private void Chasing()
		{
			_returnOrigin = true;
			if ( !Animator.GetBool( Chase ) )
				Animator.SetBool( Chase, true );
			if ( _returnDash )
			{
				Rigidbody.MovePosition( Vector2.MoveTowards( Rigidbody.position, _pointOrigin, Time.fixedDeltaTime * _statistics.ReturnSpeed ) );
				_returnDash = Vector2.Distance( Rigidbody.position, _targetPoint ) <= _statistics.TargetDistance;
				return;
			}
			else if ( !_isDashing && Vector2.Distance( Rigidbody.position, _targetPoint ) <= _statistics.TargetDistance )
				if ( _statistics.DetectionStop )
				{
					Animator.SetBool( Move, false );
					Animator.SetBool( Chase, false );
					Animator.SetBool( Dash, false );
					Animator.SetBool( Stop, true );
					_stoppedTime = _statistics.StopTime;
					return;
				}
				else
				{
					Animator.SetBool( Move, false );
					Animator.SetBool( Chase, false );
					Animator.SetBool( Dash, _isDashing = true );
				}
			if ( _isDashing )
				Rigidbody.MovePosition( Vector2.MoveTowards( Rigidbody.position, _targetPoint, Time.fixedDeltaTime * _statistics.DashSpeed ) );
			else
			{
				_movementDirection = Vector2.MoveTowards( _movementDirection, ( _targetPoint - Rigidbody.position ).normalized, Time.fixedDeltaTime * _statistics.RotationSpeed );
				Rigidbody.MovePosition( Vector2.MoveTowards( Rigidbody.position, Rigidbody.position + _movementDirection, Time.fixedDeltaTime * _statistics.MovementSpeed ) );
			}
			_originCast = Rigidbody.position + _selfCollider.offset + ( _targetPoint - _originCast ).normalized;
			_dashSize = (ushort) Physics2D.CircleCast( _originCast, _selfCollider.radius, ( _targetPoint - _originCast ).normalized, _dashFilter, _dashCheck, 5E-1F );
			if ( _isDashing && Vector2.Distance( Rigidbody.position, _targetPoint ) <= WorldBuild.MINIMUM_TIME_SPACE_LIMIT || 0 < _dashSize )
				if ( _statistics.DetectionStop )
				{
					Animator.SetBool( Move, false );
					Animator.SetBool( Chase, false );
					Animator.SetBool( Dash, false );
					Animator.SetBool( Stop, _returnDash = _afterDash = true );
					_stoppedTime = _statistics.AfterTime;
				}
				else
				{
					Animator.SetBool( Dash, _isDashing = !( _returnDash = true ) );
					Animator.SetBool( Chase, true );
				}
		}
		private void Trail()
		{
			if ( _returnOrigin )
			{
				if ( !Animator.GetBool( Move ) )
					Animator.SetBool( Move, true );
				Rigidbody.MovePosition( Vector2.MoveTowards( Rigidbody.position, _pointOrigin, Time.fixedDeltaTime * _statistics.ReturnSpeed ) );
				transform.TurnScaleX( _pointOrigin.x < Rigidbody.position.x );
				_movementDirection = ( _pointOrigin - Rigidbody.position ).normalized;
				_returnOrigin = Vector2.Distance( Rigidbody.position, _pointOrigin ) > WorldBuild.MINIMUM_TIME_SPACE_LIMIT;
			}
			else if ( 0 < _trail.Length )
			{
				if ( !Animator.GetBool( Move ) )
					Animator.SetBool( Move, true );
				if ( Vector2.Distance( Rigidbody.position, _trail[ _pointIndex ] ) <= WorldBuild.MINIMUM_TIME_SPACE_LIMIT )
					_pointIndex = (byte) ( _pointIndex < _trail.Length - 1 ? _pointIndex + 1 : 0 );
				Rigidbody.MovePosition( Vector2.MoveTowards( Rigidbody.position, _trail[ _pointIndex ], Time.fixedDeltaTime * _statistics.MovementSpeed ) );
				transform.TurnScaleX( _trail[ _pointIndex ].x < Rigidbody.position.x );
				_movementDirection = ( _trail[ _pointIndex ] - Rigidbody.position ).normalized;
				_pointOrigin = Rigidbody.position;
			}
		}
		private void Update()
		{
			if ( IsStunned || SceneInitiator.IsInTransition() )
				return;
			if ( _statistics.DetectionStop && Animator.GetBool( Stop ) )
				if ( 0F >= ( _stoppedTime -= Time.deltaTime ) )
				{
					Animator.SetBool( Move, false );
					Animator.SetBool( Chase, false );
					Animator.SetBool( Dash, false );
					Animator.SetBool( Stop, false );
					(_isDashing, _afterDash) = (!_afterDash, false);
					Animator.SetBool( Dash, _isDashing );
				}
		}
		private new void FixedUpdate()
		{
			base.FixedUpdate();
			if ( Animator.GetBool( Stop ) || IsStunned || SceneInitiator.IsInTransition() )
				return;
			if ( _statistics.Target )
			{
				_movementDirection = Vector2.MoveTowards(
					current: _movementDirection,
					target: ( (Vector2) _statistics.Target.position - Rigidbody.position ).normalized,
					maxDistanceDelta: Time.fixedDeltaTime * _statistics.RotationSpeed );
				Rigidbody.MovePosition( Vector2.MoveTowards( Rigidbody.position, Rigidbody.position + _movementDirection, Time.fixedDeltaTime * _statistics.MovementSpeed ) );
				transform.TurnScaleX( _statistics.Target.position.x < Rigidbody.position.x );
				return;
			}
			if ( _statistics.EndlessPursue )
			{
				_movementDirection = Vector2.MoveTowards(
					current: _movementDirection,
					target: ( CharacterExporter.GwambaLocalization() - Rigidbody.position ).normalized,
					maxDistanceDelta: Time.fixedDeltaTime * _statistics.RotationSpeed );
				Rigidbody.MovePosition( Vector2.MoveTowards( Rigidbody.position, Rigidbody.position + _movementDirection, Time.fixedDeltaTime * _statistics.MovementSpeed ) );
				transform.TurnScaleX( CharacterExporter.GwambaLocalization().x < Rigidbody.position.x );
				return;
			}
			if ( !_isDashing )
				_detected = false;
			if ( _statistics.LookPerception && !_isDashing && CharacterExporter.GwambaLocalization().InsideCircleCast( _pointOrigin, _statistics.LookDistance ) )
			{
				_movementDirection = ( CharacterExporter.GwambaLocalization() - ( Rigidbody.position + _selfCollider.offset ) ).normalized;
				_detectionObject.rotation = Quaternion.AngleAxis( Mathf.Atan2( _movementDirection.y, _movementDirection.x ) * Mathf.Rad2Deg - 90F, Vector3.forward );
				_sizeCast.Set( _detectionCollider.size.x, Vector2.Distance( CharacterExporter.GwambaLocalization(), Rigidbody.position + _selfCollider.offset ) );
				_originCast.Set( 0F, ( _detectionCollider.size = _sizeCast ).y / 2F );
				_detectionCollider.offset = _originCast;
				if ( _detected = !_detectionCollider.IsTouchingLayers( WorldBuild.SCENE_LAYER_MASK ) )
				{
					transform.TurnScaleX( CharacterExporter.GwambaLocalization().x < transform.position.x );
					_targetPoint = CharacterExporter.GwambaLocalization();
				}
			}
			if ( _detected || _returnDash )
				Chasing();
			else
				Trail();
		}
		public new void Receive( MessageData message )
		{
			if ( message.AdditionalData is not null && message.AdditionalData is EnemyProvider[] enemies && 0 < enemies.Length )
				foreach ( EnemyProvider enemy in enemies )
					if ( enemy && this == enemy )
						base.Receive( message );
		}
	};
};
