/* 
 * Copyright (c) 2014, Furore (info@furore.com) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.github.com/furore-fhir/sprinkler/master/LICENSE
 */

using Hl7.Fhir.Rest;

namespace Furore.Fhir.Sprinkler.Framework.Utilities
{
    public class SprinklerTestClass
    {
        protected FhirClient Client;

        public void SetClient(FhirClient client)
        {
            Client = client;
        }
    }
}