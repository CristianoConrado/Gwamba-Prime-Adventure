using UnityEngine;
using GwambaPrimeAdventure.Character;
namespace GwambaPrimeAdventure.Enemy.Supply
{
	[DisallowMultipleComponent, RequireComponent( typeof( Collider2D ) )]
	public sealed class SummonPoint : StateController
	{
		private ISummoner _summoner;
		private ushort _summonIndex;
		[Header( "Interactions" )]
		[SerializeField, Tooltip( "If this point will destroy itself after use." )] private bool _destroyAfter;
		[SerializeField, Tooltip( "If this point will trigger with other object." )] private bool _hasTarget;
		public void GetTouch( ISummoner summoner, ushort summonIndex ) => (_summoner, _summonIndex) = (summoner, summonIndex);
		private void OnTriggerEnter2D( Collider2D other )
		{
			if ( _hasTarget )
			{
				if ( CharacterExporter.EqualGwamba( other.gameObject ) )
					_summoner.OnSummon( _summonIndex );
			}
			else if ( other.TryGetComponent<ISummoner>( out _ ) )
				_summoner.OnSummon( _summonIndex );
			if ( _destroyAfter )
				Destroy( gameObject );
		}
	};
};
