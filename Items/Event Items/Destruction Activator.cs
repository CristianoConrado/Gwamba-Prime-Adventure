using UnityEngine;
namespace GwambaPrimeAdventure.Item.EventItem
{
	[DisallowMultipleComponent]
	internal sealed class DestructionActivator : Activator
	{
		[Header( "Destruction Activator" )]
		[SerializeField, Tooltip( "If this activator will activate after the destruction." )] private bool _activate;
		private new void OnDestroy()
		{
			base.OnDestroy();
			if ( _activate )
				Activation();
		}
	};
};
