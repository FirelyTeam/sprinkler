using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Furore.Fhir.Sprinkler.Runner.Contracts
{
    public interface ITestRunner
    {
        void Run(string[] tests);
    }
}
