using UnityEngine;
using UnityEngine.Events;
namespace GwambaPrimeAdventure
{
	[Icon( WorldBuild.PROJECT_ICON )]
	public abstract class StateController : MonoBehaviour
	{
		private static UnityAction<bool> _setState;
		protected void Awake() => _setState += NewState;
		protected void OnDestroy() => _setState -= NewState;
		private void NewState( bool state ) => enabled = state;
		public static void SetState( bool newState ) => _setState?.Invoke( newState );
		protected sealed class WaitTime : CustomYieldInstruction
		{
			private readonly StateController _instance;
			private readonly bool _unscaled;
			private float _time;
			public override bool keepWaiting
			{
				get
				{
					if ( 0F < _time && _instance.isActiveAndEnabled )
						_time -= _unscaled ? Time.unscaledDeltaTime : Time.deltaTime;
					return 0F < _time;
				}
			}
			public WaitTime( StateController instance, float time, bool unscaled = false ) => (_instance, _time, _unscaled) = (instance, time, unscaled);
		};
	};
};
