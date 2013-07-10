using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FodySpring;

namespace AssemblyToProcess
{
    [Configurable]
    public class Class1 
    {
        public string InjectedString { get; set; }
    }
}

