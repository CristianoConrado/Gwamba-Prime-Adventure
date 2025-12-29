using UnityEngine;
namespace GwambaPrimeAdventure.Item.EventItem
{
	internal abstract class Activator : StateController
	{
		private
			Animator _animator;
		private readonly int
			IsOn = Animator.StringToHash( nameof( IsOn ) ),
			Use = Animator.StringToHash( nameof( Use ) ),
			UseAgain = Animator.StringToHash( nameof( UseAgain ) );
		private bool
			_used = false,
			_usedOne = false,
			_usable = true;
		[SerializeField, Tooltip( "The activator only can be activeted one time." ), Header( "Activator" )]
		private bool
			_oneActivation;
		protected bool Usable =>
			_usable;
		private new void Awake()
		{
			base.Awake();
			_animator = GetComponent<Animator>();
		}
		protected void OnEnable()
		{
			if ( _animator )
				_animator.SetFloat( IsOn, 1F );
		}
		protected void OnDisable()
		{
			if ( _animator )
				_animator.SetFloat( IsOn, 0F );
		}
		protected void Activation()
		{
			if ( _oneActivation && _usedOne )
				return;
			_used = !_used;
			if ( _oneActivation )
				_usable = false;
			if ( _animator )
				if ( _used )
					_animator.SetTrigger( Use );
				else
					_animator.SetTrigger( UseAgain );
			_usedOne = true;
			Receptor.ReceiveSignal( this );
		}
	};
};
