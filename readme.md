Fody Spring
======================================================================

This is an add-in for [Fody](https://github.com/Fody/Fody/) 
------

Adds spring configuration to object constructors.

## Nuget 

Nuget package https://nuget.org/packages/Spring.Fody 

To Install from the Nuget Package Manager Console 
    
    PM> Install-Package Spring.Fody
    
## How to use

### 1. Add Spring.Net

Fody Spring will not add a reference to Spring automatically due to Fody Nuget package limitations.

### 2. Add  Spring.Fody nuget package

Install the NullGuard package using NuGet. See [Using the package manager console](http://docs.nuget.org/docs/start-here/using-the-package-manager-console) for more info.

    Install-Package Spring.Fody
    
### 3. Configurable attribute

Add [Configurable] to any class you want to be configured when being instantiated.

### 4. Compile

Any time you create a new instance of your class with any of its constructor the instance will be first configured by Spring.Net.

	[Configurable]
	public class ConfigurableClass
	{
		public string InjectedProperty { get;set; }
		
		public ConfigurableClass() //It works in all constructors not just the default
		{
		  Console.WriteLine(InjectedProperty);
		}
	}

As shown below Injected properties are available in the constructor itself.