using System.Collections.Generic;

namespace Furore.Fhir.Sprinkler.Framework.Configuration
{
    public interface IAssembliesNameProvider
    {
        IEnumerable<string> GetTestAssemblies();
    }
}