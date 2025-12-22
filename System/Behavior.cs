using Cysharp.Threading.Tasks;
namespace GwambaPrimeAdventure
{
	public interface ILoader
	{
		public UniTask Load();
	};
	public interface IConnector
	{
		public MessagePath Path { get; }
		public void Receive( MessageData message );
	};
	public interface IOccludee
	{
		public bool Occlude { get; }
	};
	public interface IDestructible
	{
		public short Health { get; }
		public bool Hurt( ushort damage );
		public void Stun( ushort stunStength, float stunTime );
	};
	public interface IInteractable
	{
		public void Interaction();
	};
	public interface ICollectable
	{
		public void Collect();
	};
};
