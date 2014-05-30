/* 
 * Copyright (c) 2014, Furore (info@furore.com) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.github.com/furore-fhir/sprinkler/master/LICENSE
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using Hl7.Fhir.Rest;
using Hl7.Fhir.Support;
using Sprinkler.Framework;
using Sprinkler.Tests;

namespace Sprinkler
{
    class Program
    {       
        static void Main(string[] args)
        {
            //string FHIR_URL = "http://localhost.fiddler:1396/fhir";
            
            // string FHIR_URL = "http://localhost:1396/fhir";

            //string FHIR_URL = "http://fhirlab.apphb.com/fhir";
            //string FHIR_URL = "http://localhost.:62124/fhir";
            
            //string FHIR_URL = "http://spark.furore.com/fhir";

            //string FHIR_URL = "http://hl7connect.healthintersections.com.au/svc/fhir";

            //string FHIR_URL = "http://fhir.healthintersections.com.au/open";

            //string FHIR_URL = "http://healthfire.duteaudesign.com/svc";
            //string FHIR_URL = "http://172.28.174.240:9556/svc/fhir";
            //string FHIR_URL = "http://nprogram.azurewebsites.net";
            //string FHIR_URL = "https://api.fhir.me";
            //string FHIR_URL = "http://12.157.84.95/OnlineFHIRService/diagnosticorder";
            //string FHIR_URL = "http://12.157.84.54:8080/fhir";
            //string FHIR_URL = "http://12.157.84.197:18080/fhir";
            string FHIR_URL = "http://localhost/blaze/fhir";

            if (args.Count() == 1) FHIR_URL = args[0];

            Console.WriteLine("Sprinkler v0.6 - Conformance test suite for FHIR 0.8 (DSTU1)");

            //TestSets.Connectathon6(FHIR_URL);
            TestSets.Run(FHIR_URL);
            Console.WriteLine();
            Console.WriteLine("Ready (press any key to continue)");
            Console.ReadKey();
        }
    }
}
