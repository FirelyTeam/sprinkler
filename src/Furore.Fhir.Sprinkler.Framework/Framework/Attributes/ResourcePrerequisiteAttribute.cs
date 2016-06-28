using System;

namespace Furore.Fhir.Sprinkler.Framework.Framework.Attributes
{
    public class ResourcePrerequisiteAttribute : Attribute
    {
        public string ResourceFile { get; set; }
        public string ResourceKey { get; set; }
        public Type ResourceType { get; set; }

        public ResourcePrerequisiteAttribute(string file)
        {
            this.ResourceFile = file;
        }

        public ResourcePrerequisiteAttribute(Type resourceType)
        {
            this.ResourceType = resourceType;
        }
    }
}