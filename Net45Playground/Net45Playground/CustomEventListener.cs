using System;
using System.Diagnostics.Tracing;
using System.Linq;

namespace Net45Playground
{
	public sealed class CustomEventListener
		: EventListener
	{
		public CustomEventListener()
			: base()
		{
			this.EnableEvents(
				(from @event in EventSource.GetSources()
				 where @event.Guid == EventSource.GetGuid(typeof(CustomEventSource))
				 select @event).Single(), EventLevel.Informational);
		}

		protected override void OnEventWritten(EventWrittenEventArgs eventData)
		{
			Console.Out.WriteLine(eventData.Message,
				eventData.Payload.ToArray());
		}
	}
}
