using System;
using System.IO;
using Furore.Fhir.Sprinkler.FhirUtilities.ResourceManagement;

namespace Furore.Fhir.Sprinkler.Xunit.ClientUtilities.XunitFhirExtensions.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class FixtureConfigurationAttribute : Attribute
    {
        private readonly string fixturesRootPath;
        private readonly FixtureType fixtureType;

        public FixtureConfigurationAttribute(string fixturesRootPath)
        {
            this.fixturesRootPath = fixturesRootPath;
        }

        public FixtureConfigurationAttribute(string fixturesRootPath, FixtureType fixtureType)
        {
            this.fixturesRootPath = fixturesRootPath;
            this.fixtureType = fixtureType;
        }

        public FixtureConfigurationAttribute(FixtureType fixtureType)
        {
            this.fixtureType = fixtureType;
        }

        public FixtureConfiguration GetFixtureConfiguration()
        {
            return new FixtureConfiguration()
            {
                FixturesRootPath = ((!string.IsNullOrEmpty(fixturesRootPath)) && Path.IsPathRooted(fixturesRootPath))
                ? fixturesRootPath : Path.Combine(TestConfiguration.AssemblyRootDirectory, "Resourcesx", fixturesRootPath ?? string.Empty),
                FixtureType = fixtureType
            };
        }
    }
}