using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Net45Playground
{
	class Program
	{
		static void Main(string[] args)
		{
			//Program.DemonstrateSynchronousIO();
			//Program.DemonstrateAsynchronousIO();
			//Program.DemonstrateAsynchronousIOWithIssues();
			//Program.DemonstrateAwaitAsyncIO().Wait();
			//Program.DemonstrateAsynchronousIOWithNoIssues().Wait();
			//Program.DemonstrateRegularExpressionTimeout(Regex.InfiniteMatchTimeout);
			//Program.DemonstrateRegularExpressionTimeout(TimeSpan.FromMilliseconds(100d));
			//Program.DemonstrateTraditionalEventHandling();
			//Program.DemonstrateWeakEventManagerEventHandling();
			//Program.DemonstrateTypesInReflection();
			//Program.DemonstrateCallerInformationAttributes();
			//Program.DemonstrateCallerInformationAttributes("Main", "d:\\...", 29);
			//Program.DemonstrateCallerInformationAttributes(
			//	"wrongMember", "wrongFilePath", 42);
			//Program.DemonstrateCallerInformationWithReflection();
			Program.DemonstrateTracing();
			//Program.DemonstrateReadOnlyCollection();
			//Program.DemonstrateCompression();
			//Program.DemostrateReturnValue();
		}

		private static void DemonstrateRegularExpressionTimeout(TimeSpan timeout)
		{
			var data = string.Concat<Guid>(Program.GetGuids(500000));
			var stopwatch = Stopwatch.StartNew();
			var matches = Regex.Matches(
				data, "(aa)*aa(bb)*bb(cc)*cc", RegexOptions.IgnoreCase, timeout);
			var count = matches.Count;
			stopwatch.Stop();

			Console.Out.WriteLine(count);
			Console.Out.WriteLine(stopwatch.Elapsed.ToString());
		}

		private static void DemonstrateCallerInformationWithReflection()
		{
			var method = typeof(Program).GetMethod("DemonstrateCallerInformationAttributes",
				BindingFlags.NonPublic | BindingFlags.Static, null,
				new[] { typeof(string), typeof(string), typeof(int) }, null);
			method.Invoke(null, new object[] { "1", "2", 3 });
		}

		private static void DemonstrateTracing()
		{
			using (var source = new CustomEventSource())
			{
				using (var listener = new CustomEventListener())
				{
					source.Publish("First publish.");
					source.Publish("Second publish.");
					source.Publish("Third publish.");
				}
			}
		}

		private static void DemonstrateReadOnlyCollection()
		{
			var key = Guid.NewGuid();
			var items = new ReadOnlyDictionary<Guid, string>(
				new Dictionary<Guid, string>() { { key, "data" } });
			Console.Out.WriteLine(items[key]);
		}

		private static void DemonstrateCompression()
		{
			using (var zipStream = new FileStream("data.zip", FileMode.Create))
			{
				using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create))
				{
					using (var writer = new StreamWriter(archive.CreateEntry(
						"SomeFolder\\data.txt").Open()))
					{
						writer.WriteLine("Here's some data.");
					}
				}
			}

			using (var zipStream = new FileStream("data.zip", FileMode.Open))
			{
				using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Read))
				{
					using (var writer = new StreamReader(archive.GetEntry(
						"SomeFolder\\data.txt").Open()))
					{
						Console.Out.WriteLine(writer.ReadLine());
					}
				}
			}
		}

		private static void DemonstrateTraditionalEventHandling()
		{
			var startingWorkingSet = Environment.WorkingSet;

			var publisher = new Publisher();

			for (var i = 0; i < 100; i++)
			{
				for (var j = 0; j < 100; j++)
				{
					var listener = new Listener(publisher, false);
				}

				Console.Out.WriteLine("Working set before Collect(): {0}", Environment.WorkingSet);
				GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
				Console.Out.WriteLine("Working set after Collect(): {0}", Environment.WorkingSet);
			}

			Console.Out.WriteLine("Starting working set: {0}", startingWorkingSet);
			Console.Out.WriteLine("Final working set: {0}", Environment.WorkingSet);
			Console.Out.WriteLine("Difference: {0}", Environment.WorkingSet - startingWorkingSet);
		}

		private static void DemonstrateWeakEventManagerEventHandling()
		{
			var startingWorkingSet = Environment.WorkingSet;

			var publisher = new Publisher();

			for (var i = 0; i < 100; i++)
			{
				for (var j = 0; j < 100; j++)
				{
					var listener = new Listener(publisher, true);
				}

				Console.Out.WriteLine("Working set before Collect(): {0}", Environment.WorkingSet);
				GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
				Console.Out.WriteLine("Working set after Collect(): {0}", Environment.WorkingSet);
			}

			Console.Out.WriteLine("Starting working set: {0}", startingWorkingSet);
			Console.Out.WriteLine("Final working set: {0}", Environment.WorkingSet);
			Console.Out.WriteLine("Difference: {0}", Environment.WorkingSet - startingWorkingSet);
		}

		private static async Task DemonstrateAwaitAsyncIO()
		{
			const string fileName = "DemonstrateSynchronousIO.txt";

			if (File.Exists(fileName))
			{
				File.Delete(fileName);
			}

			using (var writer = new StreamWriter(fileName))
			{
				foreach (var data in Program.GetGuids(10))
				{
					await writer.WriteLineAsync(data.ToString());
				}
			}

			using (var reader = new StreamReader(fileName))
			{
				while (reader.Peek() >= 0)
				{
					Console.WriteLine(await reader.ReadLineAsync());
				}
			}
		}

		private static void DemonstrateAsynchronousIO()
		{
			const string fileName = "DemonstrateSynchronousIO.txt";

			if (File.Exists(fileName))
			{
				File.Delete(fileName);
			}

			using (var writer = new StreamWriter(fileName))
			{
				var writerTasks = new List<Task>();

				foreach (var data in Program.GetGuids(10))
				{
					writerTasks.Add(writer.WriteLineAsync(data.ToString()));
				}

				Task.WaitAll(writerTasks.ToArray());
			}

			using (var reader = new StreamReader(fileName))
			{
				while (reader.Peek() >= 0)
				{
					var line = reader.ReadLineAsync();
					line.Wait();
					Console.WriteLine(line.Result);
				}
			}
		}

		private sealed class DelayedStringWriter
			: StringWriter
		{
			public override Task WriteAsync(string value)
			{
				return Task.Factory.StartNew(() =>
					{
						Thread.Sleep(2000);
						this.Write(value);
						Console.Out.WriteLine("DelayedStringWriter.WriteAsync() finished.");
					});
			}
		}

		private static void DemonstrateAsynchronousIOWithIssues()
		{
			using (var writer = new DelayedStringWriter())
			{
				foreach (var data in Program.GetGuids(1))
				{
					writer.WriteAsync(data.ToString());
				}
			}

			Console.Out.WriteLine("Program.DemonstrateAsynchronousIOWithIssues() finished.");
		}

		private static async Task DemonstrateAsynchronousIOWithNoIssues()
		{
			using (var writer = new DelayedStringWriter())
			{
				foreach (var data in Program.GetGuids(1))
				{
					await writer.WriteAsync(data.ToString());
				}
			}

			Console.Out.WriteLine("Program.DemonstrateAsynchronousIOWithNoIssues() finished.");
		}

		private static void DemonstrateSynchronousIO()
		{
			const string fileName = "DemonstrateSynchronousIO.txt";

			if (File.Exists(fileName))
			{
				File.Delete(fileName);
			}

			using (var writer = new StreamWriter(fileName))
			{
				foreach (var data in Program.GetGuids(10))
				{
					writer.WriteLine(data.ToString());
				}
			}

			using (var reader = new StreamReader(fileName))
			{
				while (reader.Peek() >= 0)
				{
					Console.WriteLine(reader.ReadLine());
				}
			}
		}

		private static void DemonstrateCallerInformationAttributes(
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string filePath = "",
			[CallerLineNumber] int lineNumber = 0)
		{
			Console.Out.WriteLine(string.Format("{0} : {1}, {2}",
				memberName, filePath, lineNumber));
		}

		private static void DemonstrateTypesInReflection()
		{
			var type = typeof(Program);
			var typeInfo = type.GetTypeInfo();
			Console.Out.WriteLine(typeInfo.AsType().Name);

			Console.Out.WriteLine();

			foreach (var assemblyTitleAttribute in
				typeInfo.Assembly.GetCustomAttributes<AssemblyTitleAttribute>())
			{
				Console.Out.WriteLine(assemblyTitleAttribute.Title);
			}

			Console.Out.WriteLine();

			foreach (var attribute in
				typeInfo.Assembly.GetCustomAttributes<Attribute>())
			{
				Console.Out.WriteLine(attribute.GetType().Name);
			}
		}

		private static int DemostrateReturnValue()
		{
			return new Random().Next();
		}

		private static IEnumerable<Guid> GetGuids(int count)
		{
			for (var i = 0; i < count; i++)
			{
				yield return Guid.NewGuid();
			}
		}
	}
}
