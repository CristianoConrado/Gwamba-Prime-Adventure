using Cysharp.Threading.Tasks;
using GwambaPrimeAdventure.Connection;
using NaughtyAttributes;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
namespace GwambaPrimeAdventure.Item.EventItem
{
	[DisallowMultipleComponent, RequireComponent( typeof( ISignalReceptor ) )]
	internal sealed class Receptor : StateController, ILoader
	{
		private static readonly HashSet<Receptor>
			_selfes = new HashSet<Receptor>();
		private readonly HashSet<Activator>
			_activatorsNeeded = new HashSet<Activator>();
		private 
			ISignalReceptor _receptor;
		private ushort
			_signals = 0,
			_1X1Index = 0;
		private float
			_signalTimer = 0F;
		private bool
			_onlyOneActivation = false;
		[SerializeField, Tooltip( "The activators that this will receive a signal." ), Header( "Receptor" )]
		private
			Activator[] _activators;
		[SerializeField, Tooltip( "If this will receive a signal from specifics or existent objects." )]
		private
			string[] _specificsObjects;
		[SerializeField, Tooltip( "The amount of time to wait for active after receive the signal." )]
		private
			float _timeToActivate;
		[SerializeField, Tooltip( "If this will activate for every activator activated." )]
		private
			bool _1X1;
		[SerializeField, HideIf( nameof( _1X1 ) ), Tooltip( "If is needed only one activator to activate." )]
		private
			bool _oneNeeded;
		[SerializeField, ShowIf( nameof( _oneNeeded ) ), HideIf( nameof( _1X1 ) ), Tooltip( "If it will be inactive after one activation" )]
		private
			bool _oneActivation;
		[SerializeField, HideIf( EConditionOperator.Or, nameof( _1X1 ), nameof( _oneNeeded ) ), Tooltip( "If are multiples activators needed to activate." )]
		private
			bool _multiplesNeeded;
		[SerializeField, ShowIf( nameof( _multiplesNeeded ) ), HideIf( EConditionOperator.Or, nameof( _1X1 ), nameof( _oneNeeded ) )]
		[Tooltip( "The amount activators needed to activate." )]
		private
			ushort _quantityNeeded;
		private new void Awake()
		{
			base.Awake();
			_receptor = GetComponent<ISignalReceptor>();
			_selfes.Add( this );
		}
		private new void OnDestroy()
		{
			base.OnDestroy();
			_selfes.Remove( this );
		}
		public async UniTask Load()
		{
			CancellationToken destroyToken = this.GetCancellationTokenOnDestroy();
			await UniTask.Yield( PlayerLoopTiming.EarlyUpdate, destroyToken, true ).SuppressCancellationThrow();
			if ( destroyToken.IsCancellationRequested )
				return;
			SaveController.Load( out SaveFile saveFile );
			if ( 0 < _specificsObjects.Length )
				foreach ( string specificObject in _specificsObjects )
					if ( saveFile.GeneralObjects.Contains( specificObject ) )
						_receptor.Execute();
			foreach ( Activator activator in _activators )
				_activatorsNeeded.Add( activator );
		}
		private void Update()
		{
			if ( 0F < _signalTimer )
				if ( 0F >= ( _signalTimer -= Time.deltaTime ) )
					NormalSignal();
		}
		private void NormalSignal()
		{
			if ( _onlyOneActivation )
				return;
			if ( _1X1 )
			{
				_receptor.Execute();
				_activatorsNeeded.Remove( _activators[ _1X1Index++ ] );
				if ( 0 >= _activatorsNeeded.Count )
					foreach ( Activator activator in _activators )
						_activatorsNeeded.Add( activator );
			}
			else if ( _oneNeeded )
			{
				_receptor.Execute();
				if ( _oneActivation )
					_onlyOneActivation = true;
			}
			else if ( _multiplesNeeded )
			{
				_signals += 1;
				if ( _signals >= _quantityNeeded )
				{
					_signals = 0;
					_receptor.Execute();
				}
			}
			else
			{
				_signals += 1;
				if ( _activators.Length <= _signals )
				{
					_signals = 0;
					_receptor.Execute();
				}
			}
		}
		private void Signal()
		{
			if ( 0F < _timeToActivate )
				_signalTimer = _timeToActivate;
			else
				NormalSignal();
		}
		internal static void ReceiveSignal( Activator signal )
		{
			foreach ( Receptor receptor in _selfes )
				if ( receptor._activatorsNeeded.Contains( signal ) )
					receptor.Signal();
		}
	};
    internal interface ISignalReceptor
    {
        public void Execute();
    };
};
