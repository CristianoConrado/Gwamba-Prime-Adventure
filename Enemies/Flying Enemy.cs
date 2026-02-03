using Cysharp.Threading.Tasks;
using GwambaPrimeAdventure.Character;
using GwambaPrimeAdventure.Enemy.Supply;
using System.Threading;
using UnityEngine;
namespace GwambaPrimeAdventure.Enemy
{
	[DisallowMultipleComponent, RequireComponent( typeof( PolygonCollider2D ) )]
	internal sealed class FlyingEnemy : MovingEnemy, ILoader, IConnector
	{
		private
			CircleCollider2D _selfCollider;
		private
			Transform _detectionObject;
		private
			CapsuleCollider2D _detectionCollider;
		private
			Vector2[] _trail;
		private Vector2
			_movementDirection = Vector2.zero,
			_pointOrigin = Vector2.zero,
			_targetPoint = Vector2.zero;
		private ushort
			_pointIndex = 0;
		private bool
			_normal = true,
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
			Sender.Include( this );
		}
		private new void OnDestroy()
		{
			base.OnDestroy();
			Sender.Exclude( this );
		}
		public async UniTask Load()
		{
			CancellationToken destroyToken = this.GetCancellationTokenOnDestroy();
			await UniTask.Yield( PlayerLoopTiming.EarlyUpdate, destroyToken, true ).SuppressCancellationThrow();
			if ( destroyToken.IsCancellationRequested )
				return;
			GameObject detectionObject = new( "Detection Collider", typeof( CapsuleCollider2D ) )
			{
				layer = WorldBuild.ENEMY_LAYER,
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
			await UniTask.Yield( PlayerLoopTiming.Update, destroyToken, true ).SuppressCancellationThrow();
			if ( destroyToken.IsCancellationRequested )
				return;
			PolygonCollider2D trail = GetComponent<PolygonCollider2D>();
			_trail = new Vector2[ trail.points.Length ];
			for ( ushort i = 0; trail.points.Length > i; i++ )
				_trail[ i ] = transform.parent ? trail.offset + trail.points[ i ] + (Vector2) transform.position : trail.points[ i ];
			_movementDirection = Vector2.right * _movementSide;
			_pointOrigin = Rigidbody.position;
		}
		private void Chase()
		{
			_returnOrigin = true;
			if ( _returnDash )
			{
				Rigidbody.MovePosition( Vector2.MoveTowards( Rigidbody.position, _pointOrigin, Time.fixedDeltaTime * _statistics.ReturnSpeed ) );
				_returnDash = Vector2.Distance( Rigidbody.position, _targetPoint ) <= _statistics.TargetDistance;
				return;
			}
			else if ( !_isDashing && Vector2.Distance( Rigidbody.position, _targetPoint ) <= _statistics.TargetDistance )
				if ( _statistics.DetectionStop )
				{
					(_stopWorking, _stoppedTime) = (true, _statistics.StopTime);
					return;
				}
				else
					_isDashing = true;
			if ( _isDashing )
				Rigidbody.MovePosition( Vector2.MoveTowards( Rigidbody.position, _targetPoint, Time.fixedDeltaTime * _statistics.DashSpeed ) );
			else
			{
				_movementDirection = Vector2.MoveTowards( _movementDirection, ( _targetPoint - Rigidbody.position ).normalized, Time.fixedDeltaTime * _statistics.RotationSpeed );
				Rigidbody.MovePosition( Vector2.MoveTowards( Rigidbody.position, Rigidbody.position + _movementDirection, Time.fixedDeltaTime * _statistics.MovementSpeed ) );
			}
			if ( _isDashing && Vector2.Distance( Rigidbody.position, _targetPoint ) <= WorldBuild.MINIMUM_TIME_SPACE_LIMIT )
				if ( _statistics.DetectionStop )
				{
					_stopWorking = _returnDash = _afterDash = true;
					_stoppedTime = _statistics.AfterTime;
				}
				else
					_isDashing = !( _returnDash = true );
		}
		private void Trail()
		{
			if ( _returnOrigin )
			{
				Rigidbody.MovePosition( Vector2.MoveTowards( Rigidbody.position, _pointOrigin, Time.fixedDeltaTime * _statistics.ReturnSpeed ) );
				transform.TurnScaleX( _pointOrigin.x < Rigidbody.position.x );
				_returnOrigin = Vector2.Distance( Rigidbody.position, _pointOrigin ) > WorldBuild.MINIMUM_TIME_SPACE_LIMIT;
			}
			else if ( 0 < _trail.Length )
			{
				if ( Vector2.Distance( Rigidbody.position, _trail[ _pointIndex ] ) <= WorldBuild.MINIMUM_TIME_SPACE_LIMIT )
					if ( _repeatWay )
						_pointIndex = (ushort) ( _pointIndex < _trail.Length - 1 ? _pointIndex + 1 : 0 );
					else if ( _normal )
					{
						_pointIndex += 1;
						_normal = _pointIndex != _trail.Length - 1;
					}
					else if ( !_normal )
					{
						_pointIndex -= 1;
						_normal = _pointIndex == 0;
					}
				Rigidbody.MovePosition( Vector2.MoveTowards( Rigidbody.position, _trail[ _pointIndex ], Time.fixedDeltaTime * _statistics.MovementSpeed ) );
				transform.TurnScaleX( _trail[ _pointIndex ].x < Rigidbody.position.x );
				_pointOrigin = Rigidbody.position;
			}
		}
		private void Update()
		{
			if ( IsStunned )
				return;
			if ( _statistics.DetectionStop && _stopWorking )
				if ( 0F >= ( _stoppedTime -= Time.deltaTime ) )
				{
					_stopWorking = false;
					(_isDashing, _afterDash) = (!_afterDash, false);
				}
		}
		private void FixedUpdate()
		{
			if ( _stopWorking || IsStunned )
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
			if ( _isDashing )
			{
				_originCast = Rigidbody.position + _selfCollider.offset + ( _targetPoint - _originCast ).normalized;
				if ( Physics2D.CircleCast( _originCast, _selfCollider.radius, ( _targetPoint - _originCast ).normalized, _selfCollider.radius / 2f, WorldBuild.SCENE_LAYER_MASK ) )
					if ( _statistics.DetectionStop )
					{
						_stopWorking = _returnDash = _afterDash = true;
						_stoppedTime = _statistics.AfterTime;
					}
					else
						_isDashing = !( _returnDash = true );
			}
			else
				_detected = false;
			if ( _statistics.LookPerception && !_isDashing && CharacterExporter.GwambaLocalization().InsideCircle( _pointOrigin, _statistics.LookDistance ) )
			{
				_originCast = ( CharacterExporter.GwambaLocalization() - ( Rigidbody.position + _selfCollider.offset ) ).normalized;
				_detectionObject.rotation = Quaternion.AngleAxis( Mathf.Atan2( _originCast.y, _originCast.x ) * Mathf.Rad2Deg - 90F, Vector3.forward );
				_sizeCast.x = _detectionCollider.size.x;
				_sizeCast.y = Vector2.Distance( CharacterExporter.GwambaLocalization(), Rigidbody.position + _selfCollider.offset );
				_detectionCollider.size = _sizeCast;
				_originCast.Set( _sizeCast.y / 2F * transform.localScale.x.CompareTo( 0F ) * _originCast.x, _sizeCast.y / 2F * _originCast.y );
				_detectionCollider.offset = _originCast;
				if ( _detected = !_detectionCollider.IsTouchingLayers( WorldBuild.SCENE_LAYER_MASK ) )
				{
					transform.TurnScaleX( CharacterExporter.GwambaLocalization().x < transform.position.x );
					_targetPoint = CharacterExporter.GwambaLocalization();
				}
			}
			if ( _detected || _returnDash )
				Chase();
			else
				Trail();
		}
		public new void Receive( MessageData message )
		{
			if ( message.AdditionalData is not null && message.AdditionalData is EnemyProvider[] enemies && 0 < enemies.Length )
				foreach ( EnemyProvider enemy in enemies )
					if ( enemy && this == enemy )
					{
						base.Receive( message );
						return;
					}
		}
	};
};
