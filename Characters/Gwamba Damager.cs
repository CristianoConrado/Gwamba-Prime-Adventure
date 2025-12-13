using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections.Generic;
using NaughtyAttributes;
namespace GwambaPrimeAdventure.Character
{
	[DisallowMultipleComponent, Icon("Assets/Huds/Designs/Gwamba Prime Series Logo.png"), RequireComponent(typeof(Transform), typeof(SpriteRenderer), typeof(Collider2D))]
	internal sealed class GwambaDamager : StateController, IDestructible
	{
		private SpriteRenderer _spriteRenderer;
		internal readonly List<IDestructible> damagedes = new();
		internal event Predicate<ushort> DamagerHurt;
		internal event UnityAction<ushort, float> DamagerStun;
		internal event UnityAction<GwambaDamager, IDestructible> DamagerAttack;
		private Color _alphaChanger = new();
		[SerializeField, BoxGroup("Statistics"), Tooltip("If this Gwamba's part will take damage."), Space(WorldBuild.FIELD_SPACE_LENGTH * 2F)] private bool _takeDamage;
		[field: SerializeField, BoxGroup("Statistics"), HideIf(nameof(_takeDamage)), Tooltip("The velocity of the screen shake on the attack.")] internal Vector2 AttackShake { get; private set; }
		[field: SerializeField, BoxGroup("Statistics"), HideIf(nameof(_takeDamage)), Tooltip("The amount of damage that the attack of Gwamba hits.")] internal ushort AttackDamage { get; private set; }
		[field: SerializeField, BoxGroup("Statistics"), HideIf(nameof(_takeDamage)), Min(0F), Tooltip("The amount of time the attack screen shake will be applied.")]
		internal float AttackShakeTime { get; private set; }
		[field: SerializeField, BoxGroup("Statistics"), HideIf(nameof(_takeDamage)), Min(0F), Tooltip("The amount of time that this Gwamba's attack stun does.")] internal float StunTime { get; private set; }
		internal float Alpha
		{
			get => _spriteRenderer.color.a;
			set
			{
				_alphaChanger.a = value;
				_spriteRenderer.color = _alphaChanger;
			}
		}
		public short Health => 0;
		private new void Awake()
		{
			base.Awake();
			_spriteRenderer = GetComponent<SpriteRenderer>();
			_alphaChanger = _spriteRenderer.color;
		}
		public bool Hurt(ushort damage)
		{
			if (_takeDamage)
				return DamagerHurt.Invoke(damage);
			return false;
		}
		public void Stun(ushort stunStength, float stunTime)
		{
			if (_takeDamage)
				DamagerStun.Invoke(stunStength, stunTime);
		}
		private void OnTriggerEnter2D(Collider2D other)
		{
			if (!_takeDamage && other.TryGetComponent<IDestructible>(out var destructible))
				if (!damagedes.Contains(destructible))
					DamagerAttack.Invoke(this, destructible);
		}
	};
};
