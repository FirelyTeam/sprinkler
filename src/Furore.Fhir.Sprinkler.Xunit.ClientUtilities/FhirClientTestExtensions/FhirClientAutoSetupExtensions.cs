using System;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;

namespace Furore.Fhir.Sprinkler.Xunit.ClientUtilities.FhirClientTestExtensions
{
    internal static class FhirClientAutoSetupExtensions
    {
        internal static IAutoSetupFixture<T> AutoSetupFixture<T>(this FhirClient client, T fixture) where T : Resource
        {
            return new AutoSetupFixture<T>(client, client.Create(fixture));
        }
        internal static IAutoSetupFixture<Resource> AutoSetupFixture(this FhirClient client, Resource resource)
        {
            return AutoSetupFixture(client, resource, resource.GetType());
        }
        internal static IAutoSetupFixture<Resource> AutoSetupFixture(this FhirClient client, Resource resource,
            Type resourceType)
        {
            if (!string.IsNullOrEmpty(resource.Id))
            {
                resource = client.Update(resource);
            }
            else
            {
                resource = client.Create(resource);

            }
            Type setupFixture = typeof(AutoSetupFixture<>);
            return
                (IAutoSetupFixture<Resource>)
                    Activator.CreateInstance(setupFixture.MakeGenericType(resourceType), new object[] { client, resource });
        }
    }
    public interface IAutoSetupFixture<out T> : IDisposable where T : Resource
    {
        T Fixture { get; }
    }

    public sealed class AutoSetupFixture<T> : IAutoSetupFixture<T> where T : Resource
    {
        private readonly FhirClient client;
        private bool disposed = false;

        public T Fixture { get; }

        public AutoSetupFixture(FhirClient client, T importedResource)
        {
            this.client = client;
            this.Fixture = importedResource;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                client.Delete(Fixture);
            }

            disposed = true;
        }
    }
}