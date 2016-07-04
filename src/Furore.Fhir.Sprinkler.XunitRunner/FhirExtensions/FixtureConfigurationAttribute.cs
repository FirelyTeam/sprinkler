using System;
using Furore.Fhir.Sprinkler.ClientUtilities.ResourceManagement;

namespace Furore.Fhir.Sprinkler.XunitRunner.FhirExtensions
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

        public FixtureConfiguration GetFixtureConfiguration()
        {
            return new FixtureConfiguration()
            {
                FixturesRootPath = fixturesRootPath,
                FixtureType = fixtureType
            };
        }
    }
}