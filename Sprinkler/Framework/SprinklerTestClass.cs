using Hl7.Fhir.Rest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sprinkler.Framework
{
    public class SprinklerTestClass
    {
        protected FhirClient client;
        public void SetClient(FhirClient client)
        {
            this.client = client;
        }
    }
}
