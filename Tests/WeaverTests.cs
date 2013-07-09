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
        var type = assembly.GetType("AssemblyToProcess.Class1");
        var instance = (dynamic)Activator.CreateInstance(type);

        Assert.AreEqual("injected string with spring", instance.InjectedString);
    }

#if(DEBUG)
    [Test]
    public void PeVerify()
    {
        Verifier.Verify(assembly.CodeBase.Remove(0, 8));
    }
#endif

}