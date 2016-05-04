using System;
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
				//this.Type = type;
			}
		}

		private static List<Notification> warnings = new List<Notification>(),
								messages = new List<Notification>(); 


		public static void Warn(Solution s, Project p, string message)
		{
			warnings.Add(new Notification(s, p, message));
		}

		public static void Inform(Solution s, Project p, string message)
		{
			messages.Add(new Notification(s, p, message));
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
	}
}
