using UnityEngine;
namespace GwambaPrimeAdventure.Item.EventItem
{
	[DisallowMultipleComponent]
	internal sealed class DestructionActivator : Activator
	{
		[SerializeField, Tooltip( "If this activator will activate after the destruction." ), Header( "Destruction Activator" )] private bool _activate;
		private new void OnDestroy()
		{
			base.OnDestroy();
			if ( _activate )
				Activation();
		}
	};
};
