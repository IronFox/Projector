﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projector
{
	public static class EventLog
	{
		//public enum EventType
		//{
		//	Message,
		//	Warning
		//}

		public struct Notification
		{
			public readonly string Text;
			public readonly Solution Solution;
			public readonly Project Project;
			public readonly DateTime Stamp;
			//public readonly EventType Type;

			public override string ToString()
			{
				return (Project != null ? (Solution != null ? Solution.Name + "/" : "") + Project.Name +": ": (Solution != null ? Solution.Name + ": " : "") ) + Text;
			}

			public Notification(Solution s, Project p, string message/*, EventType type*/)
			{
				this.Project = p;
				this.Text = message;
				Solution = s;
				Stamp = DateTime.Now;
				//this.Type = type;
			}
		}

		private static List<Notification> warnings = new List<Notification>(),
								messages = new List<Notification>();
		private static DateTime lastEvent = DateTime.Now;

		private static Solution current = null, currentWarn = null;
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



		//public IEnumerable<Notification> Events { get { return events; } }


		public static void Clear()
		{
			warnings.Clear();
			messages.Clear();
		}

		//public IEnumerable<Notification> All(EventType type)
		//{
		//	foreach (var ev in events)
		//		if (ev.Type == type)
		//			yield return ev;
		//}

		public static IEnumerable<Notification> Messages
		{
			get
			{
				return messages;
			}
		}

		public static IEnumerable<Notification> Warnings
		{
			get
			{
				return warnings;
			}
		}

		public static DateTime LastEvent
		{
			get
			{
				return lastEvent;
			}
		}

		public static bool LogToConsole { get; internal set; }
	}
}
