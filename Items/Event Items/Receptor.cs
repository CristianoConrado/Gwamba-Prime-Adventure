using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using GwambaPrimeAdventure.Connection;
namespace GwambaPrimeAdventure.Item.EventItem
{
	[DisallowMultipleComponent, RequireComponent(typeof(Transform), typeof(IReceptorSignal))]
	internal sealed class Receptor : StateController, ILoader
	{
		private static readonly List<Receptor> _selfes = new();
		private readonly List<Activator> _usedActivators = new();
		private Activator _signalActivator;
		private IReceptorSignal _receptor;
		private ushort _signals = 0;
		private float _signalTimer = 0F;
		private bool _onlyOneActivation = false;
		[Header("Receptor")]
		[SerializeField, Tooltip("The activators that this will receive a signal.")] private Activator[] _activators;
		[SerializeField, Tooltip("If this will receive a signal from specifics or existent objects.")] private string[] _specificsObjects;
		[SerializeField, Tooltip("The amount of time to wait for active after receive the signal.")] private float _timeToActivate;
		[SerializeField, Tooltip("If this will activate for every activator activated.")] private bool _1X1;
		[SerializeField, HideIf(nameof(_1X1)), Tooltip("If are multiples activators needed to activate.")] private bool _multiplesNeeded;
		[SerializeField, HideIf(nameof(_1X1)), ShowIf(nameof(_multiplesNeeded)), Tooltip("The quantity of multiples activators needed to activate.")] private ushort _quantityNeeded;
		[SerializeField, HideIf(nameof(_1X1)), Tooltip("If is needed only one activator to activate.")] private bool _oneNeeded;
		[SerializeField, HideIf(nameof(_1X1)), ShowIf(nameof(_oneNeeded)), Tooltip("If it will be inactive after one activation")] private bool _oneActivation;
		private new void Awake()
		{
			base.Awake();
			_receptor = GetComponent<IReceptorSignal>();
			_selfes.Add(this);
		}
		private new void OnDestroy()
		{
			base.OnDestroy();
			_selfes.Remove(this);
		}
		public IEnumerator Load()
		{
			SaveController.Load(out SaveFile saveFile);
			if (0 < _specificsObjects.Length)
				foreach (string specificObject in _specificsObjects)
					if (saveFile.GeneralObjects.Contains(specificObject))
						_receptor.Execute();
			yield return null;
		}
		private void Update()
		{
			if (0F < _signalTimer)
				if (0F >= (_signalTimer -= Time.deltaTime))
					NormalSignal();
		}
		private void NormalSignal()
		{
			if (_onlyOneActivation)
				return;
			if (_usedActivators.ToArray() == _activators)
				_usedActivators.Clear();
			if (_1X1)
			{
				for (ushort i = 0; _activators.Length > i; i++)
					if (_activators[i] == _signalActivator && !_usedActivators.Contains(_activators[i]))
					{
						_usedActivators.Add(_activators[i]);
						_receptor.Execute();
						return;
					}
			}
			else if (_multiplesNeeded)
			{
				for (ushort i = 0; _activators.Length > i; i++)
					if (_activators[i] == _signalActivator)
						_signals += 1;
				if (_signals >= _quantityNeeded)
				{
					_signals = 0;
					_receptor.Execute();
				}
			}
			else if (_oneNeeded)
			{
				for (ushort i = 0; _activators.Length > i; i++)
					if (_activators[i] == _signalActivator)
					{
						_receptor.Execute();
						if (_oneActivation)
							_onlyOneActivation = true;
						return;
					}
			}
			else
			{
				for (ushort i = 0; _activators.Length > i; i++)
					if (_activators[i] == _signalActivator)
						_signals += 1;
				if (_activators.Length <= _signals)
				{
					_signals = 0;
					_receptor.Execute();
				}
			}
		}
		internal static void ReceiveSignal(Activator signal)
		{
			foreach (Receptor receptor in _selfes)
				receptor.Signal(signal);
		}
		private void Signal(Activator signal)
		{
			_signalActivator = signal;
			if (0F < _timeToActivate)
				_signalTimer = _timeToActivate;
			else
				NormalSignal();
		}
	};
	internal interface IReceptorSignal
	{
		public void Execute();
	};
};
