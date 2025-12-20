using UnityEngine;
using System.Collections;
using System.Threading.Tasks;
using GwambaPrimeAdventure.Enemy.Utility;
namespace GwambaPrimeAdventure.Enemy
{
	[DisallowMultipleComponent]
	internal sealed class SummonerEnemy : EnemyProvider, ILoader, ISummoner, IConnector
	{
		private IEnumerator _summonEvent;
		private Vector2 _summonPosition = Vector2.zero;
		private Vector2Int _summonIndex = Vector2Int.zero;
		private InstantiateParameters _instantiateParameters = new();
		private ushort _randomSummonIndex = 0;
		private float[]
			_summonTime,
			_structureTime;
		private float
			_fullStopTime = 0F,
			_stopTime = 0F,
			_gravityScale = 0F;
		private bool[]
			_isSummonTime,
			_stopPermanently;
		private bool
			_stopSummon = false,
			_waitStop = false;
		[Header( "Summoner Enemy" )]
		[SerializeField, Tooltip( "The summoner statitics of this enemy." )] private SummonerStatistics _statistics;
		private new void Awake()
		{
			base.Awake();
			_sender.SetFormat( MessageFormat.State );
			Sender.Include( this );
		}
		private new void OnDestroy()
		{
			base.OnDestroy();
			Sender.Exclude( this );
		}
		public IEnumerator Load()
		{
			_structureTime = new float[ _statistics.SummonPointStructures.Length ];
			(_summonTime, _isSummonTime) = (new float[ _statistics.TimedSummons.Length ], _stopPermanently = new bool[ _statistics.TimedSummons.Length ]);
			(_gravityScale, _randomSummonIndex) = (Rigidbody.gravityScale, (ushort) Random.Range( 0, _statistics.TimedSummons.Length + 1 ));
			for ( ushort i = 0; _statistics.TimedSummons.Length > i; i++ )
				_isSummonTime[ i ] = true;
			for ( ushort i = 0; _statistics.TimedSummons.Length > i; i++ )
				_summonTime[ i ] = _statistics.TimedSummons[ i ].SummonTime;
			for ( ushort i = 0; _statistics.SummonPointStructures.Length > i; i++ )
				Instantiate( _statistics.SummonPointStructures[ i ].SummonPointObject, _statistics.SummonPointStructures[ i ].Point, Quaternion.identity ).GetTouch( this, i );
			yield return null;
		}
		private async void Summon( SummonObject summon )
		{
			while ( _summonEvent is not null )
				await Task.Yield();
			_summonEvent = StopToSummon();
			_summonEvent.MoveNext();
			if ( summon.InstantlySummon )
				_summonEvent.MoveNext();
			IEnumerator StopToSummon()
			{
				if ( summon.StopToSummon )
				{
					_sender.SetToggle( false );
					_sender.Send( MessagePath.Enemy );
					if ( summon.ParalyzeToSummon )
						Rigidbody.gravityScale = 0F;
					(_fullStopTime, _waitStop) = (_stopTime = summon.TimeToStop, summon.WaitStop);
					yield return null;
				}
				_summonIndex.Set( 0, 0 );
				(_instantiateParameters.parent, _instantiateParameters.worldSpace) = (summon.LocalPoints ? transform : null, !summon.LocalPoints);
				for ( ushort i = 0; summon.QuantityToSummon > i; i++ )
				{
					_summonPosition = summon.Self
						? (Vector2) transform.position
						: ( summon.Random ? summon.SummonPoints[ Random.Range( 0, summon.SummonPoints.Length + 1 ) ] : summon.SummonPoints[ _summonIndex.y ] );
					Instantiate( summon.Summons[ _summonIndex.x ], _summonPosition, summon.Summons[ _summonIndex.x ].transform.rotation, _instantiateParameters ).transform.SetParent( null );
					_summonIndex.Set( summon.Summons.Length - 1 > _summonIndex.x ? _summonIndex.x + 1 : 0, summon.SummonPoints.Length - 1 > _summonIndex.y ? _summonIndex.y + 1 : 0 );
				}
				_summonEvent = null;
			}
		}
		private void IndexedSummon( ushort summonIndex )
		{
			if ( _stopPermanently[ summonIndex ] )
				return;
			if ( _stopSummon )
			{
				if ( _statistics.TimedSummons[ summonIndex ].StopPermanently && !_stopPermanently[ summonIndex ] )
					_stopPermanently[ summonIndex ] = true;
				return;
			}
			if ( 0F < _summonTime[ summonIndex ] )
				if ( 0F >= ( _summonTime[ summonIndex ] -= Time.deltaTime ) )
				{
					if ( _isSummonTime[ summonIndex ] )
					{
						Summon( _statistics.TimedSummons[ summonIndex ] );
						_summonTime[ summonIndex ] = _statistics.TimedSummons[ summonIndex ].PostSummonTime;
					}
					else
						_summonTime[ summonIndex ] = _statistics.TimedSummons[ summonIndex ].SummonTime;
					_isSummonTime[ summonIndex ] = !_isSummonTime[ summonIndex ];
					if ( _statistics.RandomTimedSummons && _isSummonTime[ summonIndex ] )
						_randomSummonIndex = (ushort) Random.Range( 0, _statistics.TimedSummons.Length );
				}
		}
		private void Update()
		{
			if ( IsStunned )
				return;
			for ( ushort i = 0; _structureTime.Length > i; i++ )
				if ( 0F < _structureTime[ i ] )
					_structureTime[ i ] -= Time.deltaTime;
			if ( 0F < _stopTime )
			{
				if ( _fullStopTime / 2F >= ( _stopTime -= Time.deltaTime ) && !_waitStop && _summonEvent is not null )
					_summonEvent.MoveNext();
				if ( 0F >= _stopTime )
				{
					_sender.SetToggle( true );
					_sender.Send( MessagePath.Enemy );
					Rigidbody.gravityScale = _gravityScale;
					if ( _waitStop )
						_summonEvent?.MoveNext();
				}
			}
			if ( _statistics.RandomTimedSummons && 0 < _statistics.TimedSummons.Length )
				IndexedSummon( _randomSummonIndex );
			else
				for ( ushort i = 0; i < _statistics.TimedSummons.Length; i++ )
					IndexedSummon( i );
		}
		public void OnSummon( ushort summonIndex )
		{
			if ( 0F < _structureTime[ summonIndex ] )
				return;
			_structureTime[ summonIndex ] = _statistics.SummonPointStructures[ summonIndex ].TimeToUse;
			Summon( _statistics.SummonPointStructures[ summonIndex ].Summon );
		}
		public void Receive( MessageData message )
		{
			if ( message.AdditionalData is not null && message.AdditionalData is EnemyProvider[] enemies && 0 < enemies.Length )
				foreach ( EnemyProvider enemy in enemies )
					if ( enemy && this == enemy )
					{
						if ( MessageFormat.State == message.Format && message.ToggleValue.HasValue )
							_stopSummon = !message.ToggleValue.Value;
						else if ( MessageFormat.Event == message.Format && _statistics.HasEventSummon && 0 < _statistics.EventSummons.Length )
							if ( _statistics.RandomReactSummons )
								Summon( _statistics.EventSummons[ Random.Range( 0, _statistics.EventSummons.Length + 1 ) ] );
							else if ( message.NumberValue.HasValue && message.NumberValue.Value < _statistics.EventSummons.Length && 0 >= message.NumberValue.Value )
								Summon( _statistics.EventSummons[ message.NumberValue.Value ] );
						return;
					}
		}
	};
};
