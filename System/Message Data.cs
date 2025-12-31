namespace GwambaPrimeAdventure
{
	public struct MessageData
	{
		public MessageFormat Format
		{
			get;
			internal set;
		}
		public object AdditionalData
		{
			get;
			internal set;
		}
		public bool? ToggleValue
		{
			get;
			internal set;
		}
		public ushort? NumberValue
		{
			get;
			internal set;
		}
	};
	public enum MessagePath
	{
		None,
		System,
		Hud,
		Character,
		Enemy,
		Item,
		EventItem,
		Story
	};
	public enum MessageFormat
	{
		None,
		State,
		Event
	};
};
