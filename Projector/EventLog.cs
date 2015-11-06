using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projector
{
	public class EventLog
	{
		public enum EventType
		{
			Message,
			Warning
		}

		public struct Notification
		{
			public readonly string Text;
			public readonly Project Project;
			public readonly EventType Type;

			public override string ToString()
			{
				return (Type == EventType.Warning ? "WARNING: " : "") + (Project != null ? Project.Name +": ": "") + Text;
			}

			public Notification(Project p, string message, EventType type)
			{
				this.Project = p;
				this.Text = message;
				this.Type = type;
			}
		}

		private List<Notification> events = new List<Notification>();


		public void Warn(Project p, string message)
		{
			events.Add(new Notification(p, message, EventType.Warning));
		}

		public void Inform(Project p, string message)
		{
			events.Add(new Notification(p, message, EventType.Message));
		}



		public IEnumerable<Notification> Events { get { return events; } }


		public void Clear()
		{
			events.Clear();
		}

		public IEnumerable<Notification> All(EventType type)
		{
			foreach (var ev in events)
				if (ev.Type == type)
					yield return ev;
		}

		public IEnumerable<Notification> Messages
		{
			get
			{
				return All(EventType.Message);
			}
		}

		public IEnumerable<Notification> Warnings
		{
			get
			{
				return All(EventType.Warning);
			}
		}
	}
}
