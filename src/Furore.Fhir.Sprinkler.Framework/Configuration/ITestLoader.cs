using System;
using System.Collections.Generic;

namespace Furore.Fhir.Sprinkler.Framework.Configuration
{
    public interface ITestLoader
    {
        IEnumerable<Type> GetTestModules();
    }
}