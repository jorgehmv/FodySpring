using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Spring.Context.Support;

public static class ObjectConfigurator
{
    public static void ConfigureObject(object objectToConfigure)
    {
        string avoidConfiguration = ConfigurationManager.AppSettings["Spring.Fody.AvoidConfiguration"];
        if (!string.Equals(avoidConfiguration, "true", StringComparison.OrdinalIgnoreCase))
        {
            ContextRegistry.GetContext().ConfigureObject(objectToConfigure, objectToConfigure.GetType().Name);
        }
    }
}
