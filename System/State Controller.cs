using UnityEngine;
using UnityEngine.Events;
namespace GwambaPrimeAdventure
{
	[Icon( WorldBuild.PROJECT_ICON ), RequireComponent( typeof( Transform ) )]
	public abstract class StateController : MonoBehaviour
	{
		private static UnityAction<bool> _setState;
		protected void Awake() => _setState += NewState;
		protected void OnDestroy() => _setState -= NewState;
		private void NewState( bool state ) => enabled = state;
		public static void SetState( bool newState ) => _setState?.Invoke( newState );
	};
};
