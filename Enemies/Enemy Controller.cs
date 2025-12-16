using UnityEngine;
using Unity.Cinemachine;
using System.Collections;
using GwambaPrimeAdventure.Connection;
using GwambaPrimeAdventure.Enemy.Utility;
namespace GwambaPrimeAdventure.Enemy
{
	[DisallowMultipleComponent, SelectionBase, RequireComponent(typeof(Transform), typeof(Rigidbody2D), typeof(Collider2D)), RequireComponent(typeof(CinemachineImpulseSource))]
	internal sealed class EnemyController : Control, IConnector, IOccludee, IDestructible
   {
		private EnemyProvider[] _selfEnemies;
		[Header("Enemy Statistics")]
		[SerializeField, Tooltip("The control statitics of this enemy.")] private EnemyStatistics _statistics;
		internal EnemyStatistics ProvidenceStatistics => _statistics;
		internal Rigidbody2D Rigidbody => _rigidbody;
		public MessagePath Path => MessagePath.Enemy;
		public short Health => _vitality;
		internal short Vitality { get => _vitality; set => _vitality = value; }
		internal short ArmorResistance { get => _armorResistance; set => _armorResistance = value; }
		internal float StunTimer { get => _stunTimer; set => _stunTimer = value; }
		internal bool IsStunned { get => _stunned; set => _stunned = value; }
		public bool Occlude => !_statistics.FadeOverTime;
		private new void Awake()
		{
			base.Awake();
			(_selfEnemies, _rigidbody, _screenShaker) = (GetComponents<EnemyProvider>(), GetComponent<Rigidbody2D>(), GetComponent<CinemachineImpulseSource>());
			_destructibleEnemy = _selfEnemies[0];
			for (ushort i = 0; _selfEnemies.Length - 1 > i; i++)
				if (_selfEnemies[i + 1].DestructilbePriority > _selfEnemies[i].DestructilbePriority)
					_destructibleEnemy = _selfEnemies[i + 1];
			Sender.Include(this);
		}
		private new void OnDestroy()
		{
			base.OnDestroy();
			SaveController.Load(out SaveFile saveFile);
			if (_statistics.SaveOnSpecifics && !saveFile.GeneralObjects.Contains(name))
			{
				saveFile.GeneralObjects.Add(name);
				SaveController.WriteSave(saveFile);
			}
			Sender.Exclude(this);
		}
		internal void OnEnable() => (_rigidbody.linearVelocity, _rigidbody.gravityScale) = (_guardedLinearVelocity, _statistics.GravityScale);
		internal void OnDisable() => (_guardedLinearVelocity, _rigidbody.linearVelocity, _rigidbody.gravityScale) = (_rigidbody.linearVelocity, Vector2.zero, 0F);
		private IEnumerator Start()
		{
			SaveController.Load(out SaveFile saveFile);
			if (_statistics.SaveOnSpecifics && saveFile.GeneralObjects.Contains(name))
			{
				Destroy(gameObject);
				yield break;
			}
			foreach (EnemyProvider enemy in _selfEnemies)
				enemy.enabled = false;
			yield return new WaitWhile(() => SceneInitiator.IsInTrancision());
			(_vitality, _armorResistance, _fadeTime) = ((short)_statistics.Vitality, (short)_statistics.HitResistance, _statistics.TimeToFadeAway);
			foreach (EnemyProvider enemy in _selfEnemies)
				enemy.enabled = true;
		}
		private void Update()
		{
			if (SceneInitiator.IsInTrancision())
				return;
			if (_statistics.FadeOverTime)
				if (0F >= (_fadeTime -= Time.deltaTime))
					Destroy(gameObject);
			if (_stunned)
				if (0F >= (_stunTimer -= Time.deltaTime))
				{
					_stunned = false;
					OnEnable();
				}
		}
		private void OnTriggerEnter2D(Collider2D other)
		{
			if (!_statistics.NoHit && other.TryGetComponent<IDestructible>(out var destructible) && destructible.Hurt(_statistics.Damage))
			{
				destructible.Stun(_statistics.Damage, _statistics.StunTime);
				_screenShaker.GenerateImpulse(_statistics.HurtShake);
				EffectsController.HitStop(_statistics.HitStopTime, _statistics.HitSlowTime);
			}
		}
		public bool Hurt(ushort damage)
		{
			if (_statistics.NoDamage || 0 >= damage)
				return false;
			return _destructibleEnemy.Hurt(damage);
		}
		public void Stun(ushort stunStength, float stunTime)
		{
			if (_statistics.NoStun || _stunned)
				return;
			_destructibleEnemy.Stun(stunStength, stunTime);
		}
		public void Receive(MessageData message)
		{
			if (MessageFormat.None == message.Format && message.ToggleValue.HasValue)
			{
				OnDisable();
				for (ushort i = 0; _selfEnemies.Length > i; i++)
					_selfEnemies[i].enabled = message.ToggleValue.Value;
			}
		}
	};
};
