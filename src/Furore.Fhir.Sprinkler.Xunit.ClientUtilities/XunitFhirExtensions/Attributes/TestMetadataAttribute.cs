using System;
using System.Collections.Generic;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Furore.Fhir.Sprinkler.Xunit.ClientUtilities.XunitFhirExtensions.Attributes
{
    [TraitDiscoverer("Furore.Fhir.Sprinkler.Xunit.ClientUtilities.XunitFhirExtensions.Attributes.MetadataTraitDiscoverer",
       "Furore.Fhir.Sprinkler.Xunit.ClientUtilities")]
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class TestMetadataAttribute : Attribute, ITraitAttribute
    {
        public string[] Codes { get; private set; }
        public string Code { get; private set; }
        public string Description { get; private set; }

        public TestMetadataAttribute(string code, string description)
        {
            this.Code = code;
            this.Description = description;

        }

        public TestMetadataAttribute(string[] codes, string description)
        {
            this.Codes = codes;
            this.Description = description;

        }
    }

    public class MetadataTraitDiscoverer : ITraitDiscoverer
    {
        public static string CodeKey = "Code";
        public static string DescriptionKey = "Description";
        public static string CodesKey = "Codes";

        public MetadataTraitDiscoverer()
        {

        }

        public IEnumerable<KeyValuePair<string, string>> GetTraits(IAttributeInfo traitAttribute)
        {
            string code = traitAttribute.GetNamedArgument<string>(CodeKey);
            string[] codes = traitAttribute.GetNamedArgument<string[]>(CodesKey);
            string description = traitAttribute.GetNamedArgument<string>(DescriptionKey);
            if (code != null)
            {
                yield return new KeyValuePair<string, string>(CodeKey, code);
            }
            if (codes != null)
            {
                foreach (string c in codes)
                {
                    yield return new KeyValuePair<string, string>(CodeKey, c);
                }
            }
            if (description != null)
            {
            }
            yield return new KeyValuePair<string, string>(DescriptionKey, description);
        }
    }
}
