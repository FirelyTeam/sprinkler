using System;

namespace Furore.Fhir.Sprinkler.Xunit.TestSet
{
    public abstract class SetupAndTeardown : ISetupAndTeardown, IDisposable
    {
        public void Dispose()
        {
            this.Teardown();
        }

        public abstract void Teardown();
    }
}