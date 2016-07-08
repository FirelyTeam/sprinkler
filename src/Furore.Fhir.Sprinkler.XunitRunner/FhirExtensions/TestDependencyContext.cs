using System;
using Hl7.Fhir.Model;

namespace Furore.Fhir.Sprinkler.XunitRunner.FhirExtensions
{
    public class TestDependencyContext<T> where T:Resource
    {
        public T Dependency
        {
            get { return dependency; }
            set
            {
                dependency = value;
                id = null;
                if (dependency != null)
                {
                    id = GetId();
                }
            }
        }

        public string Id
        {
            get { return id; }
        }
        private T dependency;
        private string id;

        private string GetId()
        {
            if (Dependency == null)
                throw new ArgumentNullException();
            return Dependency.GetReferenceId();
        }
      
    }
}