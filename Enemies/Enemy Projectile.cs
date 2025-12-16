using UnityEngine;
using Unity.Cinemachine;
using System.Collections;
using GwambaPrimeAdventure.Character;
using GwambaPrimeAdventure.Connection;
using GwambaPrimeAdventure.Enemy.Utility;
namespace GwambaPrimeAdventure.Enemy
{
	[DisallowMultipleComponent, RequireComponent(typeof(Transform), typeof(SpriteRenderer), typeof(Rigidbody2D)), RequireComponent(typeof(CinemachineImpulseSource), typeof(Collider2D))]
	internal sealed class EnemyProjectile : Projectile, IDestructible
	{
		private Vector2 _projectilePosition = Vector2.zero;
		private Quaternion _projectileRotation = Quaternion.identity;
		[Header("Projectile")]
		[SerializeField, Tooltip("The statitics of this projectile.")] private ProjectileStatistics _statistics;
		public short Health => _vitality;
		private new void Awake()
		{
			base.Awake();
			(_rigidbody, _screenShaker) = (GetComponent<Rigidbody2D>(), GetComponent<CinemachineImpulseSource>());
		}
		private new void OnDestroy()
		{
			base.OnDestroy();
			StopAllCoroutines();
		}
		private void CommonInstance()
		{
			for (ushort i = 0; _statistics.QuantityToSummon > i; i++)
			{
				if (_statistics.UseSelfRotation)
					_projectileRotation = Quaternion.AngleAxis(transform.eulerAngles.z + _statistics.BaseAngle + _statistics.SpreadAngle * i, Vector3.forward);
				else
					_projectileRotation = Quaternion.AngleAxis(_statistics.BaseAngle + _statistics.SpreadAngle * i, Vector3.forward);
				Instantiate(_statistics.SecondProjectile, transform.position, _projectileRotation);
			}
		}
		private void CellInstance()
		{
			if (_oldCellPosition != _cellPosition)
			{
				_oldCellPosition = _cellPosition;
				if (0 == _pointToJump)
				{
					if (_pointToBreak >= _internalBreakPoint)
						if (_pointToReturn++ >= _internalReturnPoint)
							(_pointToBreak, _breakInUse) = (0, _statistics.AlwaysBreak);
					if (!_breakInUse || _pointToBreak < _internalBreakPoint)
					{
						if (_breakInUse)
							(_pointToBreak, _pointToReturn) = ((ushort)(_pointToBreak + 1), 0);
						(_pointToJump, _projectileRotation) = (_statistics.JumpPoints, Quaternion.AngleAxis(_statistics.BaseAngle + _statistics.SpreadAngle * _angleMulti, Vector3.forward));
						_projectilePosition.Set(_cellPosition.x + 5E-1F, _cellPosition.y + 5E-1F);
						if (_statistics.UseQuantity)
							_projectiles.Add(Instantiate(_statistics.SecondProjectile, _projectilePosition, _projectileRotation));
						else
							Instantiate(_statistics.SecondProjectile, _projectilePosition, _projectileRotation);
						_angleMulti++;
					}
				}
				else if (0 < _pointToJump)
					_pointToJump--;
			}
		}
		private void CellInstanceRange()
		{
			float distance = Physics2D.Raycast(transform.position, transform.up, _statistics.DistanceRay, WorldBuild.SCENE_LAYER_MASK).distance;
			if (_statistics.UseQuantity)
				distance = _statistics.QuantityToSummon;
			for (ushort i = 0; distance > i; i++)
			{
				_cellPosition.Set((int)(_cellPosition.x + transform.up.x), (int)(_cellPosition.y + transform.up.y));
				CellInstance();
			}
		}
		private IEnumerator ParabolicProjectile()
		{
			float time = 0F;
			float x;
			float y;
			while (_statistics.TimeToFade > time)
			{
				time += Time.fixedDeltaTime;
				x = Mathf.Cos(_statistics.BaseAngle * Mathf.Deg2Rad);
				y = Mathf.Sin(_statistics.BaseAngle * Mathf.Deg2Rad);
				_rigidbody.MovePosition(_statistics.MovementSpeed * time * new Vector2(x, y - 5E-1F * -Physics2D.gravity.y * Mathf.Pow(time, 2)));
				yield return null;
			}
			_parabolicEvent = null;
		}
		private void Start()
		{
			(_vitality, _pointToJump, _breakInUse, _internalBreakPoint) = ((short)_statistics.Vitality, _statistics.JumpPoints, _statistics.UseBreak, _statistics.BreakPoint);
			(_internalReturnPoint, _deathTimer) = (_statistics.ReturnPoint, _statistics.TimeToFade);
			if (_statistics.RandomBreak)
			{
				_internalBreakPoint = (ushort)Random.Range(_statistics.BreakPoint, _statistics.ReturnPoint - _statistics.MinimumRandomValue);
				if (_statistics.MinimumRandomValue > _internalReturnPoint - _internalBreakPoint)
					for (ushort i = 0; _statistics.MinimumRandomValue - (_internalReturnPoint - _internalBreakPoint) > i; i++)
						if (_statistics.MinimumRandomValue >= _internalBreakPoint)
							_internalReturnPoint++;
						else
							_internalBreakPoint--;
				else if (_statistics.ExtrictRandom && _statistics.MinimumRandomValue < _internalReturnPoint - _internalBreakPoint)
					for (ushort i = 0; _statistics.MinimumRandomValue - (_internalReturnPoint - _internalBreakPoint) > i; i++)
						if (_statistics.MinimumRandomValue >= _internalBreakPoint)
							_internalBreakPoint++;
						else
							_internalReturnPoint--;
			}
			_cellPosition.Set((int)transform.position.x, (int)transform.position.y);
			_oldCellPosition = _cellPosition;
			if (_statistics.SideMovement)
				transform.rotation = Quaternion.AngleAxis(_statistics.InvertSide ? 90F : -90F, Vector3.forward);
			if (_statistics.SecondProjectile && _statistics.InCell && !_statistics.ContinuosSummon)
				CellInstanceRange();
			else if (_statistics.SecondProjectile && !_statistics.InCell && !_statistics.InDeath)
				CommonInstance();
			if (!_statistics.StayInPlace)
				if (_statistics.UseForce)
					_rigidbody.AddForce((_statistics.InvertSide ? -transform.up : transform.up) * _statistics.MovementSpeed, _statistics.ForceMode);
				else if (!_statistics.RotationMatter)
					_rigidbody.linearVelocity = (_statistics.InvertSide ? -transform.up : transform.up) * _statistics.MovementSpeed;
		}
		private void Death()
		{
			if (_statistics.InDeath)
				if (_statistics.EnemyOnDeath)
					Instantiate(_statistics.EnemyOnDeath, transform.position, _statistics.EnemyOnDeath.transform.rotation);
				else if (_statistics.SecondProjectile)
					if (_statistics.InCell)
					{
						_cellPosition.Set((int)transform.position.x, (int)transform.position.y);
						CellInstanceRange();
					}
					else
						CommonInstance();
			Destroy(gameObject);
		}
		private void Update()
		{
			if (_rigidbody.IsSleeping())
				if (0F >= (_stunTimer -= Time.deltaTime))
					_rigidbody.WakeUp();
			if (0F >= (_deathTimer -= Time.deltaTime))
				Death();
		}
		private void FixedUpdate()
		{
			if (_rigidbody.IsSleeping())
				return;
			if (_statistics.EndlessPursue)
			{
				transform.TurnScaleX(GwambaStateMarker.Localization.x < transform.position.x);
				transform.up = Vector2.MoveTowards(transform.up, (GwambaStateMarker.Localization - (Vector2)transform.position).normalized, Time.fixedDeltaTime * _statistics.RotationSpeed);
				_rigidbody.linearVelocity = transform.up * _statistics.MovementSpeed;
				return;
			}
			if (_statistics.SecondProjectile && _statistics.InCell && _statistics.ContinuosSummon)
			{
				if (_statistics.UseQuantity && _statistics.QuantityToSummon == _projectiles.Count || _statistics.StayInPlace)
					return;
				_cellPosition.Set((int)transform.position.x, (int)transform.position.y);
				CellInstance();
			}
			_rigidbody.rotation += _statistics.RotationSpeed * Time.fixedDeltaTime;
			if (_statistics.ParabolicMovement)
				if (_parabolicEvent is null)
					_parabolicEvent = ParabolicProjectile();
				else
					_parabolicEvent?.MoveNext();
			else if (!_statistics.StayInPlace && _statistics.RotationMatter)
				_rigidbody.linearVelocity = (_statistics.InvertSide ? -transform.up : transform.up) * _statistics.MovementSpeed;
		}
		private void OnTriggerEnter2D(Collider2D other)
		{
			if (_statistics.NoHit)
				return;
			if (other.TryGetComponent<IDestructible>(out var destructible) && destructible.Hurt(_statistics.Damage))
			{
				destructible.Stun(_statistics.Damage, _statistics.StunTime);
				_screenShaker.GenerateImpulse(_statistics.HurtShake);
				EffectsController.HitStop(_statistics.HitStopTime, _statistics.HitSlowTime);
				if (!_statistics.NoDeathHit)
					Death();
			}
			else if (!_statistics.NoDeathCollision)
			{
				_screenShaker.GenerateImpulse(_statistics.CollideShake);
				Death();
			}
		}
		public bool Hurt(ushort damage)
		{
			if (_statistics.NoDamage || 0 >= damage || 0 >= _statistics.Vitality)
				return false;
			if (0 >= (_vitality -= (short)damage))
				Death();
			return true;
		}
		public void Stun(ushort stunStength, float stunTime)
		{
			if (_rigidbody.IsSleeping())
				return;
			_rigidbody.Sleep();
			_stunTimer = stunTime;
		}
	};
};
