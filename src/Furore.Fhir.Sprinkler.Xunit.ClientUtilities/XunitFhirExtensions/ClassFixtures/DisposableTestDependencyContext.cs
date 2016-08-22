using Hl7.Fhir.Model;

namespace Furore.Fhir.Sprinkler.Xunit.ClientUtilities.XunitFhirExtensions.ClassFixtures
{
    public class DisposableTestDependencyContext<T> : TestDependencyContext<T> where T : Resource
    {
        public override void Dispose()
        {
            if (Dependency != null)
            {
                Client.Delete(Dependency);
            }
            base.Dispose();
        }
    }
}