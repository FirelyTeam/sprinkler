using System;
using System.Collections.Generic;
using System.Linq;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Furore.Fhir.Sprinkler.XunitRunner.FhirExtensions
{
    [TraitDiscoverer("Furore.Fhir.Sprinkler.XunitRunner.FhirExtensions.CodeTraitDiscoverer", "Furore.Fhir.Sprinkler.XunitRunner")]
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class CodeAttribute : Attribute, ITraitAttribute
    {
        public CodeAttribute(string category) { }
    }

    public class CodeTraitDiscoverer : ITraitDiscoverer
    {
        public static string KEY = "Code";

        public CodeTraitDiscoverer()
        {

        }

        public IEnumerable<KeyValuePair<string, string>> GetTraits(IAttributeInfo traitAttribute)
        {
            var ctorArgs = traitAttribute.GetConstructorArguments().ToList();
            yield return new KeyValuePair<string, string>(KEY, ctorArgs[0].ToString());
        }
    }
}