using UnityEngine;
namespace GwambaPrimeAdventure.Item.EventItem
{
	[DisallowMultipleComponent, RequireComponent( typeof( Collider2D ) )]
	internal sealed class HitActivator : Activator, IDestructible
	{
		[SerializeField, Tooltip( "The amount of damage that this object have to receive real damage." ), Header( "Hit Activator" )]
		private
			ushort _biggerDamage;
		public short Health => 0;
		public bool Hurt( ushort damage )
		{
			if ( damage >= _biggerDamage && Usable )
				Activation();
			return damage >= _biggerDamage && Usable;
		}
		public void Stun( ushort stunStrength, float stunTime ) { }
	};
};
