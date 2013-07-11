using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FodySpring;

namespace AssemblyToProcess
{
    [Configurable]
    public class ClassWithNoCtors
    {
        public string InjectedString { get; set; }
    }

    [Configurable]
    public class ClassWithStaticCtor
    {
        private static readonly string someField;
        static ClassWithStaticCtor()
        {
            someField = "'supports static ctors' means it simply ignores them and do not break";
        }

        public string InjectedString { get; set; }
    }

    [Configurable]
    public class ClassWithNoDefaultCtor
    {
        public ClassWithNoDefaultCtor(string arg)
        {
        }

        public string InjectedString { get; set; }
    }

    [Configurable]
    public class ClassUsingInjectedPropertyInsideCtor
    {
        public ClassUsingInjectedPropertyInsideCtor(string textToAppend)
        {
            InjectedString += textToAppend;
        }

        public string InjectedString { get; set; }
    }

    [Configurable]
    public class ClassWithManyCtors
    {
        public ClassWithManyCtors()
        {
        }

        public ClassWithManyCtors(string arg)
            : this()
        {
            InjectedString += arg;
        }

        public ClassWithManyCtors(string arg1, string arg2)
            : this(arg1)
        {
            InjectedString += arg2;
        }

        public string InjectedString { get; set; }
    }
}

