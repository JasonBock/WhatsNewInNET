using System.Diagnostics.Tracing;

namespace Net45Playground
{
	[EventSource(Name = "WhatsNewInNet45")]
	public sealed class CustomEventSource
		: EventSource
	{
		[Event(1, Message = "Publish occurred: {0}", 
			Level = EventLevel.Informational)]
		public void Publish(string information)
		{
			this.WriteEvent(1, information);
		}
	}
}
