using UnityEngine;
namespace GwambaPrimeAdventure.Item.EventItem
{
	[DisallowMultipleComponent, RequireComponent( typeof( Collider2D ) )]
	internal sealed class HitActivator : Activator, IDestructible
	{
		[SerializeField, Tooltip( "The amount of damage that this object have to receive real damage." ), Header( "Hit Activator" )]
		private
			byte _biggerDamage;
		public IDestructible Source => this;
		public byte Health => 0;
		public bool Hurt( byte damage )
		{
			if ( damage >= _biggerDamage && Usable )
				Activation();
			return damage >= _biggerDamage && Usable;
		}
		public void Stun( byte stunStrength, float stunTime ) { }
	};
};
