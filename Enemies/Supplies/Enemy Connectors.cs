using UnityEngine;
using Unity.Cinemachine;
using System.Collections.Generic;
namespace GwambaPrimeAdventure.Enemy.Supply
{
	[DisallowMultipleComponent, SelectionBase]
	public abstract class Control : StateController
	{
		protected
			Rigidbody2D _rigidbody;
		protected
			CinemachineImpulseSource _screenShaker;
		protected
			IDestructible _destructibleEnemy;
		protected Vector2
			_guardedLinearVelocity = Vector2.zero;
		protected short
			_vitality = 0,
			_armorResistance = 0;
		protected float
			_fadeTime = 0F,
			_stunTimer = 0F;
		protected bool
			_stunned = false;
	};
	[DisallowMultipleComponent, RequireComponent( typeof( SpriteRenderer ), typeof( Collider2D ) )]
	public abstract class Projectile : StateController
	{
		protected
			Rigidbody2D _rigidbody;
		protected
			CinemachineImpulseSource _screenShaker;
		protected readonly List<Projectile>
			_projectiles = new List<Projectile>();
		protected Vector2
			_projectilePosition = Vector2.zero;
		protected Quaternion
			_projectileRotation = Quaternion.identity;
		protected Vector2Int
			_oldCellPosition = Vector2Int.zero,
			_cellPosition = Vector2Int.zero;
		protected short
			_vitality = 0;
		protected ushort
			_angleMulti = 0,
			_pointToJump = 0,
			_pointToBreak = 0,
			_internalBreakPoint = 0,
			_pointToReturn = 0,
			_internalReturnPoint = 0;
		protected float
			_deathTimer = 0F,
			_stunTimer = 0F;
		protected bool
			_breakInUse = false;
	};
	public interface IJumper
	{
		public void OnJump( ushort jumpIndex );
	};
	public interface ISummoner
	{
		public void OnSummon( ushort summonIndex );
	};
	public interface ITeleporter
	{
		public void OnTeleport( ushort teleportIndex );
	};
};