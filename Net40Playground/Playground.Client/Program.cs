using Spackle.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Device.Location;
using System.IO;
using System.Numerics;
using System.Runtime.Caching;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Playground.Client
{
	class Program
	{
		static void Main(string[] args)
		{
			//Program.CreateBigIntegers();
			//Program.CreateComplexNumbers();
			//Program.CreateInformationRichBigInteger();
			//Program.TimeBigIntegerCreation();
			//Program.CacheBigIntegers();
			//Program.ShowAttributesOnAssembly();
			//Program.CauseUnhandledException();
			//Program.CauseHandledException();
			//Program.CreateContractedBigIntegerWithLotsOfInformation();
			//Program.CreateFailFastBigIntegerWithLotsOfInformation();
			//Program.CreateBigIntegerWithOptions(args.Length > 0 ? args[0] : string.Empty);
			//Program.PrettyPrintBigNumbers();
			//Program.CreateBigIntegersConcurrently();
			//Program.StreamBigIntegers();
			//Program.ListAllBigIntegerFiles();
			//Program.PrintWhereIAm();
			Program.LazyBigNumbers();
		}

		private static void CreateBigIntegers()
		{
			var x = NumberGenerator.CreateBigInteger(50);
			var y = NumberGenerator.CreateBigInteger(50);

			Console.Out.WriteLine("x = " + x);
			Console.Out.WriteLine("y = " + y);
			Console.Out.WriteLine("x + y = " + (x + y));
			Console.Out.WriteLine("x - y = " + (x - y));
			Console.Out.WriteLine("x * y = " + (x * y));
			Console.Out.WriteLine("x / y = " + (x / y));
		}

		private static void CreateComplexNumbers()
		{
			var c = NumberGenerator.CreateComplex();
			var d = NumberGenerator.CreateComplex();

			Console.Out.WriteLine("c = " + c);
			Console.Out.WriteLine("d = " + d);
			Console.Out.WriteLine("c + d = " + (c + d));
			Console.Out.WriteLine("c - d = " + (c - d));
			Console.Out.WriteLine("c * d = " + (c * d));
			Console.Out.WriteLine("c / d = " + (c / d));
		}

		private static void CreateInformationRichBigInteger()
		{
			var xInfo = NumberGenerator.CreateBigIntegerWithLotsOfInformation(50);
			
			Console.Out.WriteLine("Item1 = " + xInfo.Item1);
			Console.Out.WriteLine("Item2 = " + xInfo.Item2.ToString("R"));
			Console.Out.WriteLine("Item3 = " + xInfo.Item3);
		}

		private static void TimeBigIntegerCreation()
		{
			for(var i = 1u; i < 1000000u; i *= 10u)
			{
				Console.Out.WriteLine("Digits: " + i + 
					"\t\tTime: " + new Action(() =>
				{
					var value = NumberGenerator.CreateBigInteger(i);
				}).Time());
			}
		}

		private static void CacheBigIntegers()
		{
			var handle = new ManualResetEvent(false);

			var cache = MemoryCache.Default;
			Console.Out.WriteLine("Polling interval: " + cache.PollingInterval);

			cache.Add("Value1", NumberGenerator.CreateBigInteger(50),
				new CacheItemPolicy() { SlidingExpiration = TimeSpan.FromSeconds(5) });
			cache.Add(new CacheItem("Value2", NumberGenerator.CreateBigInteger(50)),
				new CacheItemPolicy() { 
					SlidingExpiration = TimeSpan.FromSeconds(5), 
					RemovedCallback = (arguments) =>
					{
						Console.Out.WriteLine("Removal reason: " + arguments.RemovedReason);
						handle.Set();
					}});

			Console.Out.WriteLine(cache["Value1"].ToString());
			Console.Out.WriteLine(cache["Value2"].ToString());

			Console.Out.WriteLine("Waiting for Value2 to be removed at time {0}...", 
				DateTime.Now.ToString("R"));
			handle.WaitOne();
			Console.Out.WriteLine("Value2 was removed at time {0}...",
				DateTime.Now.ToString("R"));
		}

		private static void ShowAttributesOnAssembly()
		{
			foreach(var data in typeof(NumberGenerator).Assembly.GetCustomAttributesData())
			{
				Console.Out.WriteLine("{0} {1}",
					data.Constructor.DeclaringType.FullName, data.Constructor.ToString());
				Console.Out.WriteLine("\tConstructor Argument Data:");

				foreach(var constructorData in data.ConstructorArguments)
				{
					Console.Out.WriteLine("\tType: {0}, Value: {1}",
						constructorData.ArgumentType.FullName,
						constructorData.Value.ToString());
				}

				Console.Out.WriteLine();

				Console.Out.WriteLine("\tNamed Argument Data:");

				foreach(var namedData in data.NamedArguments)
				{
					Console.Out.WriteLine("\tName: {0}, Type: {1}, Value: {2}",
						namedData.MemberInfo.Name,
						namedData.TypedValue.ArgumentType.FullName,
						namedData.TypedValue.Value.ToString());
				}

				Console.Out.WriteLine();
				Console.Out.WriteLine();
			}
		}

		private static void CauseUnhandledException()
		{
			AppDomain.CurrentDomain.FirstChanceException +=
			(sender, o) =>
			{
				Console.Out.WriteLine("First Chance Exception");
				Console.Out.WriteLine();
				o.Exception.Print(Console.Out);
			};

			AppDomain.CurrentDomain.UnhandledException +=
			(sender, o) =>
			{
				Console.Out.WriteLine("Unhandled Exception");
				Console.Out.WriteLine();
				(o.ExceptionObject as Exception).Print(Console.Out);
			};

			NumberGenerator.CreateBigInteger(0);
		}

		private static void CauseHandledException()
		{
			AppDomain.CurrentDomain.FirstChanceException +=
			(sender, o) =>
			{
				Console.Out.WriteLine("First Chance Exception");
				Console.Out.WriteLine();
				o.Exception.Print(Console.Out);
			};

			AppDomain.CurrentDomain.UnhandledException +=
			(sender, o) =>
			{
				Console.Out.WriteLine("Unhandled Exception");
				Console.Out.WriteLine();
				(o.ExceptionObject as Exception).Print(Console.Out);
			};

			try
			{
				NumberGenerator.CreateBigInteger(0);
			}
			catch(ArgumentException)
			{
				Console.Out.WriteLine();
				Console.Out.WriteLine("ArgumentException Caught");
				Console.Out.WriteLine();
			}
		}

		private static void CreateContractedBigIntegerWithLotsOfInformation()
		{
			var xInfo = NumberGenerator.CreateContractedBigIntegerWithLotsOfInformation(5);
			Console.Out.WriteLine("Item1 = " + xInfo.Item1);
			Console.Out.WriteLine("Item2 = " + xInfo.Item2.ToString("R"));
			Console.Out.WriteLine("Item3 = " + xInfo.Item3);
		}

		private static void CreateFailFastBigIntegerWithLotsOfInformation()
		{
			try
			{
				var xInfo = NumberGenerator.CreateContractedBigIntegerWithLotsOfInformation(0);
				Console.Out.WriteLine("Item1 = " + xInfo.Item1);
				Console.Out.WriteLine("Item2 = " + xInfo.Item2.ToString("R"));
				Console.Out.WriteLine("Item3 = " + xInfo.Item3);
			}
			catch(ArgumentException e)
			{
				Environment.FailFast("Stop right now!", e);
			}
		}

		private static void CreateBigIntegerWithOptions(string option)
		{
			CreationOptions options;

			if(!string.IsNullOrWhiteSpace(option))
			{
				if(Enum.TryParse<CreationOptions>(option, out options))
				{
					Console.Out.WriteLine(NumberGenerator.CreateBigInteger(5, options));
					Console.Out.WriteLine("CreateOdd? " + options.HasFlag(CreationOptions.CreateOdd));
					Console.Out.WriteLine("CreateEven? " + options.HasFlag(CreationOptions.CreateEven));
				}
				else
				{
					Console.Out.WriteLine("You gave me a value, " + option + ", that doesn't map to a CreationOptions value.");
				}
			}
			else
			{
				Console.Out.WriteLine("You gave me an empty value or a value with only whitespace.");
			}
		}

		private static void PrettyPrintBigNumbers()
		{
			var numbers = new List<BigInteger>();

			for(var i = 0; i < 10; i++)
			{
				numbers.Add(NumberGenerator.CreateBigInteger(10));
			}

			var builder = new StringBuilder(
				string.Join<BigInteger>(", ", numbers));
			Console.Out.WriteLine(builder);
			builder.Clear();
			Console.Out.WriteLine(builder);
		}

		private static void CreateBigIntegersConcurrently()
		{
			var values = new ConcurrentBag<BigInteger>();
			var tasks = new List<Task>();

			for(var i = 0; i < Environment.ProcessorCount * 2; i++)
			{
				tasks.Add(Task.Factory.StartNew(() =>
				{
					values.Add(NumberGenerator.CreateBigInteger(2500));
				}));
			}

			Task.WaitAll(tasks.ToArray());

			var sortedValues = new SortedSet<BigInteger>(values);

			foreach(var value in sortedValues)
			{
				Console.Out.WriteLine(value);
			}
		}

		private static void StreamBigIntegers()
		{
			using(var stream = new MemoryStream())
			{
				for(var i = 0; i < 10; i++)
				{
					var number = NumberGenerator.CreateBigInteger(10).ToByteArray();
					stream.Write(number, 0, number.Length);
				}

				stream.Position = 0;
				stream.Flush();

				using(var newStream = new MemoryStream())
				{
					stream.CopyTo(newStream);
					newStream.Position = 0;
					var data = new byte[newStream.Length];
					newStream.Read(data, 0, data.Length);
					Console.Out.WriteLine(new BigInteger(data));
				}
			}
		}

		private static void ListAllBigIntegerFiles()
		{
			foreach(var file in Directory.EnumerateFiles(
				Directory.GetCurrentDirectory(), "*.big", SearchOption.TopDirectoryOnly))
			{
				File.Delete(file);
			}

			var tasks = new List<Task>();

			for(var i = 0; i < Environment.ProcessorCount * 2; i++)
			{
				tasks.Add(Task.Factory.StartNew(() =>
				{
					var data = NumberGenerator.CreateBigIntegerWithLotsOfInformation(100);
					File.WriteAllLines(data.Item3.ToString("N") + ".big", new string[] { data.Item1.ToString() });
				}));
			}

			Task.WaitAll(tasks.ToArray());

			foreach(var file in Directory.EnumerateFiles(
				Directory.GetCurrentDirectory(), "*.big", SearchOption.TopDirectoryOnly))
			{
				Console.Out.WriteLine(file);

				foreach(var line in File.ReadLines(file))
				{
					Console.Out.WriteLine(line);
				}
			}
		}

		private static void PrintWhereIAm()
		{
			var resolver = new CivicAddressResolver();
			var address = resolver.ResolveAddress(
				new GeoCoordinate() { Latitude = 44.9636674186754, 
					Longitude = -93.267997741 });

			Console.Out.WriteLine(address);
		}

		private static void LazyBigNumbers()
		{
			var lazyNumber = new Lazy<BigInteger>();

			Console.Out.WriteLine(lazyNumber.IsValueCreated);
			Console.Out.WriteLine(lazyNumber.Value);
			Console.Out.WriteLine(lazyNumber.IsValueCreated);
			Console.Out.WriteLine(lazyNumber.Value);

			Console.Out.WriteLine();

			var lazyNumberWithFactory = new Lazy<BigInteger>(() =>
			{
				return NumberGenerator.CreateBigInteger(50);
			});

			Console.Out.WriteLine(lazyNumberWithFactory.IsValueCreated);
			Console.Out.WriteLine(lazyNumberWithFactory.Value);
			Console.Out.WriteLine(lazyNumberWithFactory.IsValueCreated);
			Console.Out.WriteLine(lazyNumberWithFactory.Value);
		}
	}
}
