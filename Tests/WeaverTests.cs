using System;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

[TestFixture]
public class WeaverTests
{
    Assembly assembly;

    [TestFixtureSetUp]
    public void Setup()
    {
        assembly = WeaverHelper.WeaveAssembly();
    }

    [Test]
    public void ValidateSpringIsInjected()
    {
        var type = assembly.GetType("AssemblyToProcess.ClassWithNoCtors");
        var instance = (dynamic)Activator.CreateInstance(type);

        Assert.AreEqual("injected string with spring", instance.InjectedString);
    }

    [Test]
    public void ValidateSupportsClassesWithStaticCtors()
    {
        var type = assembly.GetType("AssemblyToProcess.ClassWithStaticCtor");
        var instance = (dynamic)Activator.CreateInstance(type);

        Assert.AreEqual("injected string with spring", instance.InjectedString);
    }

    [Test]
    public void ValidateSupportsClassesWithNoDefaultCtor()
    {
        var type = assembly.GetType("AssemblyToProcess.ClassWithNoDefaultCtor");
        var instance = (dynamic)Activator.CreateInstance(type, "arg");

        Assert.AreEqual("injected string with spring", instance.InjectedString);
    }

    [Test]
    public void ValidateSupportsUseInjectedPropertiesInsideCtor()
    {
        var type = assembly.GetType("AssemblyToProcess.ClassUsingInjectedPropertyInsideCtor");
        var instance = (dynamic)Activator.CreateInstance(type, " appended text");

        Assert.AreEqual("injected string with spring appended text", instance.InjectedString);
    }

    [Test]
    public void ValidateSupportsClassWithManyCtorsCallingDefault()
    {
        var type = assembly.GetType("AssemblyToProcess.ClassWithManyCtors");
        var instance = (dynamic)Activator.CreateInstance(type);

        Assert.AreEqual("injected string with spring", instance.InjectedString);
    }

    [Test]
    public void ValidateSupportsClassWithManyCtorsCallingNotDefaultThatCallsDefault()
    {
        var type = assembly.GetType("AssemblyToProcess.ClassWithManyCtors");
        var instance = (dynamic)Activator.CreateInstance(type, " appended text 1");

        Assert.AreEqual("injected string with spring appended text 1", instance.InjectedString);
    }

    [Test]
    public void ValidateSupportsClassWithManyCtorsCallingInChain()
    {
        var type = assembly.GetType("AssemblyToProcess.ClassWithManyCtors");
        var instance = (dynamic)Activator.CreateInstance(type, " appended text 1", " appended text 2");

        Assert.AreEqual("injected string with spring appended text 1 appended text 2", instance.InjectedString);
    }

#if(DEBUG)
    [Test]
    public void PeVerify()
    {
        Verifier.Verify(assembly.CodeBase.Remove(0, 8));
    }
#endif

}