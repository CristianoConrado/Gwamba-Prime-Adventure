using Cysharp.Threading.Tasks;
using GwambaPrimeAdventure.Enemy.Supply;
using System.Threading;
using UnityEngine;
namespace GwambaPrimeAdventure.Enemy
{
	[DisallowMultipleComponent]
	internal sealed class DeathEnemy : EnemyProvider, ILoader, IDestructible
	{
		private
			InstantiateParameters _deathParameters;
		private readonly int
			Dying = Animator.StringToHash( nameof( Dying ) );
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
		public async UniTask Load()
		{
			CancellationToken destroyToken = this.GetCancellationTokenOnDestroy();
			await UniTask.Yield( PlayerLoopTiming.EarlyUpdate, destroyToken, true ).SuppressCancellationThrow();
			if ( destroyToken.IsCancellationRequested )
				return;
			_deathParameters = new InstantiateParameters()
			{
				parent = transform,
				worldSpace = false
			};
			_sender.SetFormat( MessageFormat.State );
			_sender.SetToggle( false );
			_deathTime = _statistics.TimeToDie;
		}
		private void SummonDeathExecution()
		{
			if ( _statistics.ChildEnemy )
				Instantiate( _statistics.ChildEnemy, _statistics.SpawnPoint, Quaternion.identity, _deathParameters ).transform.SetParent( null );
			if ( _statistics.ChildProjectile )
				Instantiate( _statistics.ChildProjectile, _statistics.SpawnPoint, Quaternion.identity, _deathParameters ).transform.SetParent( null );
			Destroy( _enemyToDie ? _enemyToDie.gameObject : gameObject );
		}
		private void Update()
		{
			if ( SceneInitiator.IsInTransition() || _isDead )
				return;
			if ( 0F < _deathTime )
				if ( 0F >= ( _deathTime -= Time.deltaTime ) )
				{
					Animator.SetTrigger( Dying );
					_sender.Send( MessagePath.Enemy );
					_isDead = true;
				}
		}
		private void OnTriggerEnter2D( Collider2D other )
		{
			if ( !_isDead && _statistics.OnTouch && other.TryGetComponent<IDestructible>( out _ ) )
			{
				Animator.SetTrigger( Dying );
				_sender.Send( MessagePath.Enemy );
				_isDead = true;
			}
		}
		public new bool Hurt( byte damage )
		{
			if ( _isDead )
				return false;
			if ( 0 >= Health - damage && !_statistics.UseTimer )
			{
				Animator.SetTrigger( Dying );
				_sender.Send( MessagePath.Enemy );
				return _isDead = true;
			}
			return base.Hurt( damage );
		}
	};
};
