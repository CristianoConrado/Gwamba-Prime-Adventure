using Cysharp.Threading.Tasks;
using GwambaPrimeAdventure.Enemy.Supply;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;
namespace GwambaPrimeAdventure.Enemy
{
	[DisallowMultipleComponent]
	internal sealed class SummonerEnemy : EnemyProvider, IEnemyLoader, ISummoner, IConnector
	{
		private
			GameObject _summonObject;
		private readonly Queue<UnityAction>
			_queuedSummons = new Queue<UnityAction>();
		private UnityAction
			_summonEvent = null;
		private
			CancellationToken _destroyToken;
		private Vector2
			_summonPosition = Vector2.zero;
		private Vector2Int
			_summonIndex = Vector2Int.zero;
		private InstantiateParameters
			_instantiateParameters = new InstantiateParameters();
		private readonly int
			Summon = Animator.StringToHash( nameof( Summon ) );
		private byte
			_randomSummonIndex = 0;
		private float[]
			_summonTime,
			_structureTime;
		private float
			_gravityScale = 0F;
		private bool[]
			_isSummonTime;
		private bool
			_isTimeout = false,
			_waitResult = false;
		[SerializeField, Tooltip( "The summoner statitics of this enemy." ), Header( "Summoner Enemy" )]
		private
			SummonerStatistics _statistics;
		private new void Awake()
		{
			base.Awake();
			_sender.SetFormat( MessageFormat.State );
			Sender.Include( this );
		}
		private new void OnDestroy()
		{
			base.OnDestroy();
			_queuedSummons.Clear();
			Sender.Exclude( this );
		}
		public void Load()
		{
			_destroyToken = this.GetCancellationTokenOnDestroy();
			_structureTime = new float[ _statistics.SummonPointStructures.Length ];
			_summonTime = new float[ _statistics.TimedSummons.Length ];
			_isSummonTime = new bool[ _statistics.TimedSummons.Length ];
			_gravityScale = Rigidbody.gravityScale;
			_randomSummonIndex = (byte) UnityEngine.Random.Range( 0, _statistics.TimedSummons.Length );
			for ( byte i = 0; _statistics.TimedSummons.Length > i; i++ )
				_isSummonTime[ i ] = true;
			for ( byte i = 0; _statistics.TimedSummons.Length > i; i++ )
				_summonTime[ i ] = _statistics.TimedSummons[ i ].SummonTime;
			for ( byte i = 0; _statistics.SummonPointStructures.Length > i; i++ )
				Instantiate( _statistics.SummonPointStructures[ i ].SummonPointObject, _statistics.SummonPointStructures[ i ].Point, Quaternion.identity ).GetTouch( this, i );
		}
		private void Unstop()
		{
			_sender.SetToggle( true );
			_sender.Send( MessagePath.Enemy );
			Rigidbody.gravityScale = _gravityScale;
		}
		private void SummonEvent() => _summonEvent?.Invoke();
		private async void Summoning( SummonObject summon )
		{
			_queuedSummons.Enqueue( StopToSummon );
			(_isTimeout, _waitResult) = await UniTask.WaitWhile(
				predicate: () =>
				{
					if ( _summonEvent is not null )
						return true;
					foreach ( UnityAction queuedSummon in _queuedSummons )
						if ( queuedSummon != _queuedSummons.Peek() )
							return true;
					return false;
				},
				timing: PlayerLoopTiming.Update,
				cancellationToken: _destroyToken,
				cancelImmediately: true )
				.SuppressCancellationThrow().TimeoutWithoutException( TimeSpan.FromSeconds( _statistics.TimeToCancel ), DelayType.DeltaTime, PlayerLoopTiming.Update );
			if ( _destroyToken.IsCancellationRequested || _isTimeout && !_waitResult )
			{
				_queuedSummons.Clear();
				return;
			}
			_summonEvent = _queuedSummons.Peek();
			Animator.SetTrigger( Summon );
			if ( summon.StopToSummon )
			{
				_sender.SetToggle( false );
				_sender.Send( MessagePath.Enemy );
				if ( summon.ParalyzeToSummon )
					Rigidbody.gravityScale = 0F;
			}
			void StopToSummon()
			{
				_summonIndex.Set( 0, 0 );
				_instantiateParameters.parent = summon.LocalPoints ? transform : null;
				_instantiateParameters.worldSpace = !summon.LocalPoints;
				for ( ushort i = 0; summon.QuantityToSummon > i; i++ )
				{
					_summonPosition = summon.Self
						? (Vector2) transform.position
						: ( summon.Random ? summon.SummonPoints[ UnityEngine.Random.Range( 0, summon.SummonPoints.Length ) ] : summon.SummonPoints[ _summonIndex.y ] );
					_summonPosition.x *= transform.localScale.x.CompareTo( 0F );
					_summonObject = Instantiate( summon.Summons[ _summonIndex.x ], _summonPosition, summon.Summons[ _summonIndex.x ].transform.rotation, _instantiateParameters );
					_summonObject.transform.SetParent( null );
					_summonIndex.x = summon.Summons.Length - 1 > _summonIndex.x ? _summonIndex.x + 1 : 0;
					_summonIndex.y = summon.SummonPoints.Length - 1 > _summonIndex.y ? _summonIndex.y + 1 : 0;
				}
				_queuedSummons.Dequeue();
				_summonEvent = null;
			}
		}
		private void IndexedSummon( ushort summonIndex )
		{
			if ( 0F < _summonTime[ summonIndex ] )
				if ( 0F >= ( _summonTime[ summonIndex ] -= Time.deltaTime ) )
				{
					if ( _isSummonTime[ summonIndex ] )
					{
						Summoning( _statistics.TimedSummons[ summonIndex ] );
						_summonTime[ summonIndex ] = _statistics.TimedSummons[ summonIndex ].SkipPostSummon 
							?  _statistics.TimedSummons[ summonIndex ].SummonTime 
							:  _statistics.TimedSummons[ summonIndex ].PostSummonTime;
					}
					else
						_summonTime[ summonIndex ] = _statistics.TimedSummons[ summonIndex ].SummonTime;
					_isSummonTime[ summonIndex ] = !_isSummonTime[ summonIndex ];
					if ( _statistics.RandomTimedSummons && _isSummonTime[ summonIndex ] )
						_randomSummonIndex = (byte) UnityEngine.Random.Range( 0, _statistics.TimedSummons.Length );
				}
		}
		private void Update()
		{
			if ( Animator.GetBool( Stop ) || IsStunned || SceneInitiator.IsInTransition() )
				return;
			for ( byte i = 0; _structureTime.Length > i; i++ )
				if ( 0F < _structureTime[ i ] )
					_structureTime[ i ] -= Time.deltaTime;
			if ( _statistics.RandomTimedSummons && 0 < _statistics.TimedSummons.Length )
				IndexedSummon( _randomSummonIndex );
			else
				for ( byte i = 0; _statistics.TimedSummons.Length > i; i++ )
					IndexedSummon( i );
		}
		public void OnSummon( byte summonIndex )
		{
			if ( 0F < _structureTime[ summonIndex ] || Animator.GetBool( Stop ) )
				return;
			_structureTime[ summonIndex ] = _statistics.SummonPointStructures[ summonIndex ].TimeToUse;
			Summoning( _statistics.SummonPointStructures[ summonIndex ].Summon );
		}
		public void Receive( MessageData message )
		{
			if ( message.AdditionalData is not null && message.AdditionalData is EnemyProvider[] enemies && 0 < enemies.Length )
				foreach ( EnemyProvider enemy in enemies )
					if ( enemy && this == enemy )
						if ( MessageFormat.State == message.Format && message.ToggleValue.HasValue )
							Animator.SetBool( Stop, !message.ToggleValue.Value );
						else if ( MessageFormat.Event == message.Format && _statistics.HasEventSummon && 0 < _statistics.EventSummons.Length )
							if ( _statistics.RandomReactSummons )
								Summoning( _statistics.EventSummons[ UnityEngine.Random.Range( 0, _statistics.EventSummons.Length ) ] );
							else if ( message.NumberValue.HasValue && message.NumberValue.Value < _statistics.EventSummons.Length && 0 >= message.NumberValue.Value )
								Summoning( _statistics.EventSummons[ message.NumberValue.Value ] );
		}
	};
};
