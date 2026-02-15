using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
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
		private static readonly ConcurrentDictionary<MessagePath, HashSet<IConnector>>
			_connectors = new ConcurrentDictionary<MessagePath, HashSet<IConnector>>();
		private
			MessageData _messageData;
		public static UniTask Include( IConnector connector )
		{
			HashSet<IConnector> gettedConnectors = _connectors.GetOrAdd( connector.Path, _ => new HashSet<IConnector>() );
			gettedConnectors.Add( connector );
			return UniTask.CompletedTask;
		}
		public static UniTask Exclude( IConnector connector )
		{
			if ( _connectors.TryGetValue( connector.Path, out HashSet<IConnector> gettedConnectors ) )
			{
				gettedConnectors.Remove( connector );
				if ( 0 >= gettedConnectors.Count )
					_connectors.TryRemove( connector.Path, out _ );
			}
			return UniTask.CompletedTask;
		}
		public static Sender Create() => new Sender();
		public void SetFormat( MessageFormat format ) => _messageData.Format = format;
		public void SetAdditionalData( object additionalData ) => _messageData.AdditionalData = additionalData;
		public void SetToggle( bool toggle ) => _messageData.ToggleValue = toggle;
		public void SetNumber( byte number ) => _messageData.NumberValue = number;
		public void Send( MessagePath path )
		{
			if ( _connectors.TryGetValue( path, out HashSet<IConnector> connectors ) )
			{
				IConnector[] snapshot = new IConnector[ connectors.Count ];
				connectors.CopyTo( snapshot );
				foreach ( IConnector connector in snapshot.AsSpan() )
					connector?.Receive( _messageData );
			}
		}
	};
};
