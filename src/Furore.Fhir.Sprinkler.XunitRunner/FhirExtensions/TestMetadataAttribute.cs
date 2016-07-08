using System;
using System.Collections.Generic;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Furore.Fhir.Sprinkler.XunitRunner.FhirExtensions
{
    [TraitDiscoverer("Furore.Fhir.Sprinkler.XunitRunner.FhirExtensions.MetadataTraitDiscoverer",
        "Furore.Fhir.Sprinkler.XunitRunner")]
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class TestMetadataAttribute : Attribute, ITraitAttribute
    {
        public string Code { get; private set; }
        public string Description { get; private set; }

        public TestMetadataAttribute(string code, string description)
        {
            this.Code = code;
            this.Description = description;

        }
    }

    public class MetadataTraitDiscoverer : ITraitDiscoverer
    {
        public static string CodeKey = "Code";
        public static string DescriptionKey = "Description";

        public MetadataTraitDiscoverer()
        {

        }

        public IEnumerable<KeyValuePair<string, string>> GetTraits(IAttributeInfo traitAttribute)
        {
            yield return new KeyValuePair<string, string>(CodeKey, traitAttribute.GetNamedArgument<string>(CodeKey));
            yield return new KeyValuePair<string, string>(DescriptionKey, traitAttribute.GetNamedArgument<string>(DescriptionKey));
        }
    }
}