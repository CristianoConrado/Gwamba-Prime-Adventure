using UnityEngine;
using GwambaPrimeAdventure.Enemy.Supply;
namespace GwambaPrimeAdventure.Enemy
{
	[DisallowMultipleComponent]
	internal sealed class DeathEnemy : EnemyProvider, IDestructible
	{
		private float
			_summonTime = 0F,
			_deathTime = 0F;
		private bool
			_isDead = false;
		[SerializeField, Tooltip( "The enemy who will be destroyed." ), Header( "Death Enemy" )]
		private
			EnemyController _enemyToDie;
		[SerializeField, Tooltip( "The death statitics of this enemy." )]
		private
			DeathStatistics _statistics;
		private void Start()
		{
			_sender.SetFormat( MessageFormat.State );
			_sender.SetToggle( false );
			_summonTime = _statistics.TimeToSummon;
			_deathTime = _statistics.TimeToDie;
		}
		private void Update()
		{
			if ( 0F < _summonTime )
				if ( 0F >= ( _summonTime -= Time.deltaTime ) )
				{
					_sender.Send( MessagePath.Enemy );
					_isDead = true;
				}
			if ( _isDead )
				if ( 0F >= ( _deathTime -= Time.deltaTime ) )
				{
					_isDead = false;
					if ( _statistics.ChildEnemy )
						Instantiate( _statistics.ChildEnemy, _statistics.SpawnPoint, Quaternion.identity, transform ).transform.SetParent( null );
					if ( _statistics.ChildProjectile )
						Instantiate( _statistics.ChildProjectile, _statistics.SpawnPoint, Quaternion.identity, transform ).transform.SetParent( null );
					Destroy( _enemyToDie ? _enemyToDie.gameObject : gameObject );
				}
		}
		private void OnTriggerEnter2D( Collider2D other )
		{
			if ( !_isDead && _statistics.OnTouch && other.TryGetComponent<IDestructible>( out _ ) )
			{
				_sender.Send( MessagePath.Enemy );
				_isDead = true;
			}
		}
		public new bool Hurt( ushort damage )
		{
			if ( _isDead )
				return false;
			if ( 0 >= Health - (short) damage && !_statistics.OnlyTimer )
			{
				_sender.Send( MessagePath.Enemy );
				return _isDead = true;
			}
			return base.Hurt( damage );
		}
	};
};
