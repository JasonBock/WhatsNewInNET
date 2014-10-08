using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace FrameworkDiffer
{
	internal sealed class TypeLister
	{
		internal TypeLister(string[] assemblyBasePaths,
			string listFile)
		{
			this.AssemblyBasePaths = assemblyBasePaths;
			this.ListFile = listFile;
			this.SaveAssemblies(this.CreateTypeList());
		}

		private void SaveAssemblies(
			SortedDictionary<string,
			List<TypeInformation>> typesInAssemblies)
		{
			var document = new XDocument();
			var typesElement = new XElement("Types");
			document.Add(typesElement);

			foreach(var typesInNamespace in typesInAssemblies)
			{
				var namespaceElement = new XElement("Namespace");
				var namespaceAttribute = new XAttribute(
					"Name", typesInNamespace.Key);
				namespaceElement.Add(namespaceAttribute);
				typesElement.Add(namespaceElement);

				foreach(var type in typesInNamespace.Value)
				{
					var typeElement = new XElement("Type");
					var typeNameAttribute = new XAttribute(
						"Name", type.Name);
					var typeAssemblyAttribute = new XAttribute(
						"Assembly", type.Assembly);
					typeElement.Add(typeNameAttribute);
					typeElement.Add(typeAssemblyAttribute);
					namespaceElement.Add(typeElement);

					type.Sort();

					foreach(var method in type)
					{
						var methodElement = new XElement("Method");
						var methodNameAttribute = new XAttribute(
							"Name", method);
						methodElement.Add(methodNameAttribute);
						typeElement.Add(methodElement);
					}
				}
			}

			document.Save(this.ListFile);
		}

		private SortedDictionary<string, List<TypeInformation>>
			CreateTypeList()
		{
			var typeList = new SortedDictionary<string, List<TypeInformation>>();

			foreach(var assemblyPath in this.AssemblyBasePaths)
			{
				Console.Out.WriteLine("Processing assembly path " + assemblyPath + "...");
				foreach(var assemblyFile in Directory.GetFiles(
					assemblyPath, "*.dll",
					SearchOption.TopDirectoryOnly))
				{
					Assembly assembly = null;

					try
					{
						assembly = Assembly.LoadFrom(assemblyFile);
					}
					catch (BadImageFormatException) { }

					if (assembly != null)
					{
						this.AddTypesFromAssembly(assembly, typeList);
					}
				}
			}

			return typeList;
		}

		private void AddTypesFromAssembly(Assembly assembly,
			SortedDictionary<string, List<TypeInformation>> typeList)
		{
			try
			{
				foreach (var type in
					from assemblyType in assembly.GetTypes()
					where (assemblyType.IsPublic &&
					 !assemblyType.IsCOMObject)
					select assemblyType)
				{
					List<TypeInformation> typesInNamespace = null;
					var typeNamespace = type.Namespace ?? string.Empty;

					if (!typeList.ContainsKey(typeNamespace))
					{
						typesInNamespace = new List<TypeInformation>();
						typeList.Add(typeNamespace, typesInNamespace);
					}
					else
					{
						typesInNamespace = typeList[typeNamespace];
					}

					var typeInformation = new TypeInformation(type.Name, assembly.FullName);

					foreach (var method in type.GetMethods(
						BindingFlags.Public | BindingFlags.Instance |
						BindingFlags.Static | BindingFlags.DeclaredOnly))
					{
						try
						{
							typeInformation.Add(method.ToString());
						}
						catch (BadImageFormatException) { }
					}

					typesInNamespace.Add(typeInformation);
				}
			}
			catch (ReflectionTypeLoadException) { }
		}

		private string[] AssemblyBasePaths { get; set; }
		private string ListFile { get; set; }

		private sealed class TypeInformation : List<string>
		{
			public TypeInformation(string name, string assembly)
			{
				this.Name = name;
				this.Assembly = assembly;
			}

			public string Assembly { get; private set; }
			public string Name { get; private set; }
		}
	}
}
