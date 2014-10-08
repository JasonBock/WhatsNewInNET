using System;
using System.Windows;

namespace Net45Playground
{
	public sealed class Listener
	{
		private byte[] data;

		public Listener(Publisher publisher, bool useWeakEventManager)
		{
			this.data = new byte[10000];

			if (useWeakEventManager)
			{
				WeakEventManager<Publisher, EventArgs>
					.AddHandler(publisher, "Publish", this.OnPublisherPublish);
			}
			else
			{
				publisher.Publish += this.OnPublisherPublish;
			}
		}

		private void OnPublisherPublish(object sender, EventArgs e)
		{
			Console.Out.WriteLine(DateTime.UtcNow.ToString());
		}
	}
}
