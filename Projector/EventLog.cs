using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projector
{
	/// <summary>
	/// Log cache
	/// </summary>
	public static class EventLog
	{
		/// <summary>
		/// Notification created by <see cref="EventLog.Warn(Solution, Project, string)"/> or <see cref="EventLog.Inform(Solution, Project, string)"/>
		/// </summary>
		public struct Notification
		{
			/// <summary>
			/// Notification content
			/// </summary>
			public readonly string Text;
			/// <summary>
			/// Notification solution context. May be null
			/// </summary>
			public readonly Solution Solution;
			/// <summary>
			/// Notification project context. May be null
			/// </summary>
			public readonly Project Project;
			/// <summary>
			/// Time stamp of this notification
			/// </summary>
			public readonly DateTime Stamp;

			public override string ToString()
			{
				return (Project != null ? (Solution != null ? Solution.Name + "/" : "") + Project.Name +": ": (Solution != null ? Solution.Name + ": " : "") ) + Text;
			}

			public Notification(Solution s, Project p, string message)
			{
				this.Project = p;
				this.Text = message;
				Solution = s;
				Stamp = DateTime.Now;
			}
		}

		private static List<Notification> warnings = new List<Notification>(),
								messages = new List<Notification>();
		private static DateTime lastEvent = DateTime.Now;

		private static Solution current = null, currentWarn = null;

		/// <summary>
		/// Logs a warning
		/// </summary>
		/// <param name="s">Solution context. May be null</param>
		/// <param name="p">Project context. May be null</param>
		/// <param name="message">Warning message</param>
		public static void Warn(Solution s, Project p, string message)
		{
			var msg = new Notification(s, p, message);
			if (LogToConsole)
			{
				Console.Error.WriteLine("Warning: "+Projector.ProjectView.LogNextEvent(ref currentWarn, msg));
				return;
			}
			warnings.Add(msg);
			lastEvent = DateTime.Now;
		}

		/// <summary>
		/// Logs a notification
		/// </summary>
		/// <param name="s">Solution context. May be null</param>
		/// <param name="p">Project context. May be null</param>
		/// <param name="message">Message</param>
		public static void Inform(Solution s, Project p, string message)
		{
			var msg = new Notification(s, p, message);
			if (LogToConsole)
			{
				Console.WriteLine(Projector.ProjectView.LogNextEvent(ref current, msg));
				return;
			}
			messages.Add(msg);
			lastEvent = DateTime.Now;
		}


		/// <summary>
		/// Clears all recorded warnings and messages
		/// </summary>
		public static void Clear()
		{
			warnings.Clear();
			messages.Clear();
		}

		/// <summary>
		/// All recorded messages since the last call to <see cref="Clear"/>
		/// </summary>
		public static IEnumerable<Notification> Messages
		{
			get
			{
				return messages;
			}
		}

		/// <summary>
		/// All recorded warnings since the last call to <see cref="Clear"/>
		/// </summary>
		public static IEnumerable<Notification> Warnings
		{
			get
			{
				return warnings;
			}
		}

		/// <summary>
		/// Timestamp of the last recorded warning or notification
		/// </summary>
		public static DateTime LastEvent
		{
			get
			{
				return lastEvent;
			}
		}

		/// <summary>
		/// If set true, notifications and warnings are logged to the system console as they occur
		/// and not added to the internal notification/warning records.
		/// False by default
		/// </summary>
		public static bool LogToConsole { get; internal set; }
	}
}
