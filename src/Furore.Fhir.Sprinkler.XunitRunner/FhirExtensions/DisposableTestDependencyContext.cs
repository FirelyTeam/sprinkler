using System;
using Hl7.Fhir.Model;

namespace Furore.Fhir.Sprinkler.XunitRunner.FhirExtensions
{
    public class DisposableTestDependencyContext<T> : TestDependencyContext<T>, IDisposable where T : Resource
    {
        public void Dispose()
        {
            if (Dependency != null)
            {
                Client.Delete(Dependency);
            }
        }
    }
}