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
			Vector2[] _trail;
		private Vector2
			_movementDirection = Vector2.zero,
			_pointOrigin = Vector2.zero,
			_sizeDetection = Vector2.one * 2F,
			_targetPoint = Vector2.zero;
		private ushort
			_pointIndex = 0;
		private bool
			_normal = true,
			_returnOrigin = false,
			_afterDash = false,
			_returnDash = false;
		[SerializeField, Tooltip( "The flying statitics of this enemy." ), Header( "Flying Enemy" )]
		private FlyingStatistics
			_statistics;
		[SerializeField, Tooltip( "If this enemy will repeat the same way it makes before." )]
		private bool
			_repeatWay;
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
			PolygonCollider2D trail = GetComponent<PolygonCollider2D>();
			_trail = new Vector2[ trail.points.Length ];
			for ( ushort i = 0; trail.points.Length > i; i++ )
				_trail[ i ] = transform.parent ? trail.offset + trail.points[ i ] + (Vector2) transform.position : trail.points[ i ];
			_movementDirection = Vector2.right * _movementSide;
			_pointOrigin = Rigidbody.position;
			_sizeDetection = _sizeDetection * _statistics.LookDistance;
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
			if ( _statistics.LookPerception && !_isDashing && CharacterExporter.GwambaLocalization().InsideRectangle( _pointOrigin, _sizeDetection ) )
			{
				_detected = true;
				for ( short x = -1; 1 >= x; x++ )
				{
					for ( short y = -1; 1 >= y; y++ )
						if ( 0 == x && 0 == y || x != y && ( 0 == x || 0 == y ) )
						{
							_originCast = Rigidbody.position + _selfCollider.offset;
							_targetPoint = CharacterExporter.GwambaLocalization();
							_originCast.Set( _originCast.x + x * _selfCollider.radius, _originCast.y + y * _selfCollider.radius );
							_targetPoint.Set( _targetPoint.x + x * _selfCollider.radius, _targetPoint.y + y * _selfCollider.radius );
							if ( !( _detected = !Physics2D.Linecast( _originCast, _targetPoint, WorldBuild.SCENE_LAYER_MASK ) ) )
								break;
						}
					if ( !_detected )
						break;
				}
				if ( _detected )
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
