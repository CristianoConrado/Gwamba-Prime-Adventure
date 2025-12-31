using Cysharp.Threading.Tasks;
using NaughtyAttributes;
using System.Threading;
using UnityEngine;
namespace GwambaPrimeAdventure.Item.EventItem
{
	[DisallowMultipleComponent, RequireComponent( typeof( Rigidbody2D ), typeof( Collider2D ), typeof( PolygonCollider2D ) )]
	internal sealed class Plataform : StateController, ILoader, ISignalReceptor, IPlataform
	{
		private
			Rigidbody2D _rigidbody;
		private
			Vector2[] _trail;
		private ushort
			_waypointIndex = 0;
		private
			bool _isActive = true;
		[SerializeField, Tooltip( "The speed at which the platform moves." )]
		private
			float _movementSpeed;
		[SerializeField, Tooltip( "If this plataform is a receptor." )]
		private
			bool _isReceptor;
		[SerializeField, ShowIf( nameof( _isReceptor ) ), Tooltip( "If this plataform is initially active." )]
		private
			bool _initialActive;
		public async UniTask Load()
		{
			CancellationToken destroyToken = this.GetCancellationTokenOnDestroy();
			await UniTask.Yield( PlayerLoopTiming.EarlyUpdate, destroyToken, true ).SuppressCancellationThrow();
			if ( destroyToken.IsCancellationRequested )
				return;
			_rigidbody = GetComponent<Rigidbody2D>();
			PolygonCollider2D trail = GetComponent<PolygonCollider2D>();
			_trail = new Vector2[ trail.points.Length ];
			for ( ushort i = 0; trail.points.Length > i; i++ )
				_trail[ i ] = transform.parent ? trail.offset + trail.points[ i ] + (Vector2) transform.position : trail.points[ i ];
			if ( _isReceptor )
				_isActive = _initialActive;
		}
		private void Update()
		{
			if ( !_isActive )
				return;
			if ( Vector2.Distance( _rigidbody.position, _trail[ _waypointIndex ] ) <= WorldBuild.MINIMUM_TIME_SPACE_LIMIT )
				_waypointIndex = (ushort) ( _trail.Length > _waypointIndex ? _waypointIndex + 1 : 0 );
			_rigidbody.MovePosition( Vector2.MoveTowards( _rigidbody.position, _trail[ _waypointIndex ], Time.deltaTime * _movementSpeed ) );
		}
		public void Execute() => _isActive = !_isActive;
	};
};
