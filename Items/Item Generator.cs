using UnityEngine;
using System.Collections.Generic;
using NaughtyAttributes;
namespace GwambaPrimeAdventure.Enemy
{
	[DisallowMultipleComponent, RequireComponent( typeof( Transform ) )]
	internal sealed class ItemGenerator : StateController
	{
		private readonly List<GameObject> _itemsGenerated = new();
		private float _timeGeneration = 0F;
		private bool _continueGeneration = true;
		[Header( "Generation Statistics" )]
		[SerializeField, Tooltip( "The item to be generated." )] private GameObject _generatedItem;
		[SerializeField, Tooltip( "The amount of items that have to be generated." )] private ushort _quantityToGenerate;
		[SerializeField, Tooltip( "The amount of time to waits to generation." )] private float _generationTime;
		[SerializeField, Tooltip( "If the items generated are to be keeped in existence." )] private bool _existentItems;
		[SerializeField, HideIf( nameof( _existentItems ) ), Tooltip( "If the quantity of the generation is limited." )] private bool _especifiedGeneration;
		[SerializeField, HideIf( nameof( _existentItems ) ), ShowIf( nameof( _especifiedGeneration ) ), Tooltip( "If this generator will destroy the entire object." )] private bool _destroyObject;
		private void Update()
		{
			if ( _continueGeneration && 0F < _timeGeneration )
				if ( 0F >= ( _timeGeneration -= Time.deltaTime ) )
				{
					_timeGeneration = _generationTime;
					_itemsGenerated.Add( Instantiate( _generatedItem, transform.position, transform.rotation ) );
				}
			if ( _existentItems )
			{
				_itemsGenerated.RemoveAll( item => !item );
				_continueGeneration = _quantityToGenerate != _itemsGenerated.Count;
			}
			else if ( _especifiedGeneration && _itemsGenerated.Count == _quantityToGenerate )
				if ( _destroyObject )
					Destroy( gameObject );
				else
					Destroy( this );
		}
	};
};
