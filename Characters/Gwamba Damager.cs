using Cysharp.Threading.Tasks;
using NaughtyAttributes;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;
namespace GwambaPrimeAdventure.Character
{
	[DisallowMultipleComponent, RequireComponent( typeof( SpriteRenderer ), typeof( Collider2D ) )]
	internal sealed class GwambaDamager : StateController, IDestructible
	{
		private SpriteRenderer _spriteRenderer;
		internal readonly HashSet<IDestructible>
			damagedes = new HashSet<IDestructible>();
		internal event Predicate<ushort> DamagerHurt;
		internal event UnityAction<ushort, float> DamagerStun;
		internal event UnityAction<GwambaDamager, IDestructible> DamagerAttack;
		private Color
			_alphaChanger = new Color();
		[SerializeField, Tooltip( "If this Gwamba's part will take damage." ), Space( WorldBuild.FIELD_SPACE_LENGTH * 2F )] private bool
			_takeDamage;
		[field: SerializeField, HideIf( nameof( _takeDamage ) ), Tooltip( "The velocity of the screen shake on the attack." )] internal Vector2
			AttackShake { get; private set; }
		[field: SerializeField, HideIf( nameof( _takeDamage ) ), Tooltip( "The amount of damage that the attack of Gwamba hits." )] internal ushort
			AttackDamage { get; private set; }
		[field: SerializeField, HideIf( nameof( _takeDamage ) ), Min( 0F ), Tooltip( "The amount of time the attack screen shake will be applied." )] internal float
			AttackShakeTime { get; private set; }
		[field: SerializeField, HideIf( nameof( _takeDamage ) ), Min( 0F ), Tooltip( "The amount of time that this Gwamba's attack stun does." )] internal float
			StunTime { get; private set; }
		internal float Alpha
		{
			get => _spriteRenderer ? _spriteRenderer.color.a : 0F;
			set
			{
				if ( !_spriteRenderer )
					return;
				_alphaChanger.a = value;
				_spriteRenderer.color = _alphaChanger;
			}
		}
		public short Health => 0;
		private async void Start()
		{
			CancellationToken destroyToken = this.GetCancellationTokenOnDestroy();
			await UniTask.WaitWhile( () => SceneInitiator.IsInTrancision(), PlayerLoopTiming.Update, destroyToken, true ).SuppressCancellationThrow();
			if ( destroyToken.IsCancellationRequested )
				return;
			_spriteRenderer = GetComponent<SpriteRenderer>();
			_alphaChanger = _spriteRenderer.color;
		}
		private void OnTriggerEnter2D( Collider2D other )
		{
			if ( !_takeDamage && other.TryGetComponent<IDestructible>( out var destructible ) && !damagedes.Contains( destructible ) )
				DamagerAttack.Invoke( this, destructible );
		}
		public bool Hurt( ushort damage ) => _takeDamage && DamagerHurt.Invoke( damage );
		public void Stun( ushort stunStength, float stunTime )
		{
			if ( _takeDamage )
				DamagerStun.Invoke( stunStength, stunTime );
		}
	};
};
