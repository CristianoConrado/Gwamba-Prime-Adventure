#if UNITY_EDITOR
using UnityEngine;
using System.IO;
using System.Runtime.CompilerServices;
namespace GwambaPrimeAdventure
{
	public interface IInfoLogger
	{
		public void LogInfo( object message, [CallerMemberName] string member = "", [CallerFilePath] string file = "", [CallerLineNumber] int line = 0 )
			=> Debug.Log( $"[{Path.GetFileName( file )} : {line} - {member}] " + message );
	};
	public sealed class InfoLogger : IInfoLogger
	{
		public static IInfoLogger Informer = new InfoLogger();
		private InfoLogger() { }
	};
};
#endif
