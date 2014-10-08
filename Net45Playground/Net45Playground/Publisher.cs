using System;

namespace Net45Playground
{
	public sealed class Publisher
	{
		public event EventHandler Publish;

		public void DoPublish()
		{
			var @event = this.Publish;

			if (@event != null)
			{
				@event(this, EventArgs.Empty);
			}
		}
	}
}