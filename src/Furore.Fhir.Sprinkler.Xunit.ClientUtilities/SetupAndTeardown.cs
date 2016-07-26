using System;

namespace Furore.Fhir.Sprinkler.Xunit.TestSet
{
    public abstract class SetupAndTeardown : ISetupAndTeardown, IDisposable
    {
        public SetupAndTeardown()
        {
            this.Setup();
        }

        public void Dispose()
        {
            this.Teardown();
        }

        public abstract void Setup();
        public abstract void Teardown();
    }
}