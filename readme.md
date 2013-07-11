Fody Spring
======================================================================

This is an add-in for [Fody](https://github.com/Fody/Fody/) 
------

Adds spring configuration to object constructors.

## Nuget 

Nuget package [https://nuget.org/packages/Spring.Fody](https://nuget.org/packages/Spring.Fody) 

To Install from the Nuget Package Manager Console 
    
    PM > Install-Package Spring.Fody
    
## How to use

### 1. Add Spring.Net

Fody Spring will not add a reference to Spring automatically due to Fody Nuget package limitations.

### 2. Add  Spring.Fody nuget package

Install the NullGuard package using NuGet. See [Using the package manager console](http://docs.nuget.org/docs/start-here/using-the-package-manager-console) for more info.

    Install-Package Spring.Fody
    
### 3. Configurable attribute

Add [Configurable] to any class you want to be configured when being instantiated.

	[Configurable]
	public class ConfigurableClass
	{
		public string InjectedProperty { get;set; }
		
		public ConfigurableClass() //It works in all constructors not just the default
		{
		  Console.WriteLine(InjectedProperty);
		}
	}

### 4. Name your spring objects as your class

The only requirement of Spring.Fody is that your Spring object name must be the same as your class' name and it should not be singleton (since we need to configure a different instance every time we call the new operator):

	<object id="ConfigurableClass" singleton="false">
		<property name="InjectedProperty" value="injected string with spring" />
	</object>


### 5. Compile

That's it!

Any time you create a new instance of your class with any of its constructor the instance will be first configured by Spring.Net.

As shown in the example above Injected properties are available in the constructor itself, so your code can always assume your object is configured.

### Avoid injecting

In some scenarios, such as unit testing, you will want to avoid depending on Spring framework to achieve this you just need to add an app setting to your test project as the one below

	<appSettings>
		<add key="Spring.Fody.AvoidConfiguration" value="true"/>
	</appSettings>

### How it works?
The `Configurable` class above will be compiled as shown below:

	[Configurable]
	public class Class1
	{
		private bool <>__isConfigured;
		public string InjectedString
		{
			get;
			set;
		}
		public Class1()
		{
			this.<>__EnsureConfiguration();
		}
		private void <>__EnsureConfiguration()
		{
			if (!this.<>__isConfigured)
			{
				string a = ConfigurationManager.AppSettings["Spring.Fody.AvoidConfiguration"];
				if (!string.Equals(a, "true", StringComparison.OrdinalIgnoreCase))
				{
					ContextRegistry.GetContext().ConfigureObject(this, this.GetType().Name);
					this.<>__isConfigured = true;
				}
			}
		}
	}