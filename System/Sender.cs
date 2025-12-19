using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;
namespace GwambaPrimeAdventure
{
	public sealed class Sender
	{
		private Sender()
		{
			_messageData = new MessageData()
			{
				Format = MessageFormat.None,
				AdditionalData = null,
				ToggleValue = null,
				NumberValue = null
			};
		}
		private static readonly ConcurrentDictionary<MessagePath, HashSet<IConnector>> _connectors = new();
		private MessageData _messageData;
		public static ValueTask Include(IConnector connector)
		{
			HashSet<IConnector> gettedConnectors = _connectors.GetOrAdd(connector.Path, _ => new HashSet<IConnector>());
			gettedConnectors.Add(connector);
			return new ValueTask(Task.CompletedTask);
		}
		public static ValueTask Exclude(IConnector connector)
		{
			if (_connectors.TryGetValue(connector.Path, out HashSet<IConnector> gettedConnectors))
			{
				gettedConnectors.Remove(connector);
				if (0 >= gettedConnectors.Count)
					_connectors.TryRemove(connector.Path, out _);
			}
			return new ValueTask(Task.CompletedTask);
		}
		public static Sender Create() => new();
		public void SetFormat(MessageFormat format) => _messageData.Format = format;
		public void SetAdditionalData(object additionalData) => _messageData.AdditionalData = additionalData;
		public void SetToggle(bool toggle) => _messageData.ToggleValue = toggle;
		public void SetNumber(ushort number) => _messageData.NumberValue = number;
		public void Send(MessagePath path)
		{
			if (_connectors.TryGetValue(path, out HashSet<IConnector> connectors))
			{
				IConnector[] snapshot = new IConnector[connectors.Count];
				connectors.CopyTo(snapshot);
				foreach (IConnector connector in snapshot.AsSpan())
					connector?.Receive(_messageData);
			}
		}
	};
};
