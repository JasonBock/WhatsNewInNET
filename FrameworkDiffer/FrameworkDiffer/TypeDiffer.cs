using System;
using System.Xml;
using System.IO;

namespace FrameworkDiffer
{
	internal sealed class TypeDiffer
	{
		internal TypeDiffer(string bclBefore4File, 
			string bcl4File, string resultFile)
		{
			this.BclBefore4File = bclBefore4File;
			this.Bcl4File = bcl4File;
			this.ResultFile = resultFile;
			this.GetDifferences();
		}

		private bool HandleMethod(StreamWriter diffDocument, 
			XmlDocument assemblyBefore4, 
			XmlElement namespace4Element, 
			bool wasNamespacePrinted,
			XmlElement type4Element, 
			bool wasTypePrinted, 
			XmlElement method4Element)
		{
			var wasHandled = false;

			var methodBefore4Node = assemblyBefore4.SelectSingleNode(string.Format(
				"//Namespace[@Name='{0}']/Type[@Name='{1}']/Method[@Name='{2}']",
				namespace4Element.Attributes["Name"].Value,
				type4Element.Attributes["Name"].Value,
				method4Element.Attributes["Name"].Value));

			if(methodBefore4Node == null)
			{
				if(!wasNamespacePrinted)
				{
					diffDocument.WriteLine("Namespace: " + 
						namespace4Element.Attributes["Name"].Value);
				}

				if(!wasTypePrinted)
				{
					diffDocument.WriteLine("\tType: " + 
						type4Element.Attributes["Name"].Value + 
						", " +
						type4Element.Attributes["Assembly"].Value);
				}

				diffDocument.WriteLine("\t\tMethod: " + 
					method4Element.Attributes["Name"].Value + 
					" (new)");

				wasHandled = true;
			}

			return wasHandled;
		}

		private bool HandleType(StreamWriter diffDocument,
			XmlDocument assemblyBefore4, 
			XmlElement namespace4Element, 
			bool wasNamespacePrinted,
			XmlElement type4Element)
		{
			var wasHandled = false;

			var typeBefore4Node = assemblyBefore4.SelectSingleNode(
				string.Format("//Namespace[@Name='{0}']/Type[@Name='{1}']",
					namespace4Element.Attributes["Name"].Value,
					type4Element.Attributes["Name"].Value));

			if(typeBefore4Node == null)
			{
				if(!wasNamespacePrinted)
				{
					diffDocument.WriteLine("Namespace: " + 
						namespace4Element.Attributes["Name"].Value);
				}

				diffDocument.WriteLine("\tType: " + 
					type4Element.Attributes["Name"].Value + ", " +
					type4Element.Attributes["Assembly"].Value + 
					" (new)");

				foreach(var method4Node in type4Element.SelectNodes(
					"./Method"))
				{
					diffDocument.WriteLine("\t\tMethod: " + 
						(method4Node as XmlElement).Attributes["Name"].Value);
				}

				wasHandled = true;
			}

			return wasHandled;
		}

		private bool HandleNamespace(StreamWriter diffDocument,
			XmlDocument assemblyBefore4, 
			XmlElement namespace4Element)
		{
			var wasHandled = false;

			var namespaceBefore4Node = assemblyBefore4.SelectSingleNode(
				string.Format("//Namespace[@Name='{0}']", 
					namespace4Element.Attributes["Name"].Value));

			if(namespaceBefore4Node == null)
			{
				diffDocument.WriteLine("Namespace: " + 
					namespace4Element.Attributes["Name"].Value + 
					" (new)");

				foreach(var type4Node in namespace4Element.SelectNodes(
					"./Type"))
				{
					var type4Element = type4Node as XmlElement;
					diffDocument.WriteLine("\tType: " + 
						type4Element.Attributes["Name"].Value);
					
					foreach(var method4Node in 
						type4Element.SelectNodes("./Method"))
					{
						diffDocument.WriteLine("\t\tMethod: " + 
							(method4Node as XmlElement).Attributes["Name"].Value);
					}
				}

				wasHandled = true;
			}

			return wasHandled;
		}

		private void GetDifferences()
		{
			var assemblyBefore4 = new XmlDocument();
			assemblyBefore4.Load(this.BclBefore4File);
			var assembly4 = new XmlDocument();
			assembly4.Load(this.Bcl4File);

			using(var diffDocument = new StreamWriter(
				this.ResultFile))
			{
				foreach(var namespace4Node in assembly4.SelectNodes(
					"//Namespace"))
				{
					var namespace4Element = namespace4Node as XmlElement;
					Console.Out.WriteLine(
						namespace4Element.Attributes["Name"].Value);

					if(!this.HandleNamespace(diffDocument, 
						assemblyBefore4, namespace4Element))
					{
						var wasNamespacePrinted = false;

						foreach(var type4Node in 
							namespace4Element.SelectNodes("./Type"))
						{
							var type4Element = type4Node as XmlElement;

							if(!this.HandleType(diffDocument, 
								assemblyBefore4,
								namespace4Element, 
								wasNamespacePrinted, 
								type4Element))
							{
								var wasTypePrinted = false;

								foreach(var method4Node in 
									type4Element.SelectNodes("./Method"))
								{
									var method4Element = method4Node as XmlElement;

									if(this.HandleMethod(diffDocument, 
										assemblyBefore4,
										namespace4Element, 
										wasNamespacePrinted,
										type4Element, 
										wasTypePrinted, 
										method4Element))
									{
										wasNamespacePrinted = true;
										wasTypePrinted = true;
									}
								}
							}
							else
							{
								wasNamespacePrinted = true;
							}
						}
					}
				}
			}
		}

		private string BclBefore4File { get; set; }
		private string Bcl4File { get; set; }
		private string ResultFile { get; set; }
	}
}
