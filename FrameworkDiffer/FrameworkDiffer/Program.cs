using System;
using System.IO;

namespace FrameworkDiffer
{
	class Program
	{
		private const string BCLBefore4Listing = "BCLBefore4.xml";
		private const string BCL2Location =
			@"C:\Windows\Microsoft.NET\Framework\v2.0.50727";
		private const string BCL3Location =
			@"C:\Windows\Microsoft.NET\Framework\v3.0";
		private const string BCL3ProgramFilesLocation =
			@"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\v3.0";
		private const string BCL35Location =
			@"C:\Windows\Microsoft.NET\Framework\v3.5";
		private const string BCL35ProgramFilesLocation =
			@"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\v3.5";
		private const string BCL4Listing = "BCL4.xml";
		private const string BCL4Location =
			@"C:\Windows\Microsoft.NET\Framework\v4.0.30319";
		private const string BCL45Listing = "BCL4.5.xml";
		private const string BCL451Listing = "BCL4.5.1.xml";
		private const string Result45File = "ResultsBeta4.5.txt";
		private const string Result451File = "ResultsBeta4.5.1.txt";

		static void Main(string[] args)
		{
			if (typeof(Guid).Assembly.FullName.Contains("4.0"))
			{
				Console.Out.WriteLine("Creating 4.5.1 listing...");
				new TypeLister(new string[] { Program.BCL4Location },
					 Program.BCL451Listing);
			}
			else
			{
				//Console.Out.WriteLine("Creating before 4.0 listing...");
				//new TypeLister(new string[] { Program.BCL2Location, 
				//	Program.BCL3Location, Program.BCL3ProgramFilesLocation, 
				//	Program.BCL35Location, Program.BCL35ProgramFilesLocation },
				//Program.BCLBefore4Listing);
			}

			if (File.Exists(Program.BCL45Listing) &&
				File.Exists(Program.BCL451Listing))
			{
				Console.Out.WriteLine("Diffing files...");
				new TypeDiffer(Program.BCL45Listing,
					Program.BCL451Listing, Program.Result451File);
			}
		}
	}
}
