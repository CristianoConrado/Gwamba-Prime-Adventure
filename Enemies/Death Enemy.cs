using UnityEngine;
using GwambaPrimeAdventure.Enemy.Supply;
namespace GwambaPrimeAdventure.Enemy
{
	[DisallowMultipleComponent]
	internal sealed class DeathEnemy : EnemyProvider, IDestructible
	{
		private float
			_deathTime = 0F;
		private bool
			_isDead = false;
		[SerializeField, Tooltip( "The enemy who will be destroyed." ), Header( "Death Enemy" )]
		private
			EnemyController _enemyToDie;
		[SerializeField, Tooltip( "The death statitics of this enemy." )]
		private
			DeathStatistics _statistics;
		private void Update()
		{
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
				_sender.SetFormat( MessageFormat.State );
				_sender.SetToggle( false );
				_sender.Send( MessagePath.Enemy );
				_deathTime = _statistics.TimeToDie;
				_isDead = true;
			}
		}
		public new bool Hurt( ushort damage )
		{
			if ( _isDead )
				return false;
			if ( 0 >= Health - (short) damage )
			{
				_sender.SetFormat( MessageFormat.State );
				_sender.SetToggle( false );
				_sender.Send( MessagePath.Enemy );
				_deathTime = _statistics.TimeToDie;
				return _isDead = true;
			}
			return base.Hurt( damage );
		}
	};
};
