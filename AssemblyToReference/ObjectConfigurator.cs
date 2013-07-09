using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Spring.Context.Support;

public static class ObjectConfigurator
{
    public static void ConfigureObject(object objectToConfigure)
    {
        ContextRegistry.GetContext().ConfigureObject(objectToConfigure, objectToConfigure.GetType().Name);
    }
}
