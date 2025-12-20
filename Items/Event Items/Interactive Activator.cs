using UnityEngine;
namespace GwambaPrimeAdventure.Item.EventItem
{
	[DisallowMultipleComponent, RequireComponent( typeof( Collider2D ) )]
	internal sealed class InteractiveActivator : Activator, IInteractable
	{
		private readonly Sender _sender = Sender.Create();
		public void Interaction()
		{
			if ( Usable )
			{
				Activation();
				if ( !Usable )
				{
					_sender.SetFormat( MessageFormat.State );
					_sender.SetToggle( false );
					_sender.SetAdditionalData( gameObject );
					_sender.Send( MessagePath.Hud );
				}
			}
		}
	};
};
