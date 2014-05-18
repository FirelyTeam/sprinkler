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
using System.IO;
using System.Xml;
using Newtonsoft.Json;
using Hl7.Fhir.Rest;
using Hl7.Fhir.Support;
using Hl7.Fhir.Model;
using Sprinkler.Framework;
using Hl7.Fhir.Serialization;
using System.Security.Cryptography.X509Certificates;

namespace Sprinkler.Tests
{
    [SprinklerTestModule("MAILBOX")]
    public class MailboxTest : SprinklerTestClass
    {
        private Bundle _deliveryResult;
        private bool supportsDocumentMailbox;

        //TODO send this bundle signed => signature should still work on binary


        [SprinklerTest("MA01", "Posting an XDS feed to /Mailbox")]
        public void TestPostXds()
        {
            var statement = client.Conformance().Resource;
            supportsDocumentMailbox = statement.Rest
                .Any(r => r.DocumentMailbox != null && r.DocumentMailbox.Any());
            if (!supportsDocumentMailbox) 
                TestResult.Skip();

            Bundle xdsBundle = DemoData.GetDemoConn5ExampleBundle();
            xdsBundle.SetBundleType(BundleType.Document);

            _deliveryResult = client.DeliverToMailbox(xdsBundle);

            // TODO: None of these conditions are required by the spec. We need some
            // HL7 profiles that specify the server's processing of a posted document.
            if (_deliveryResult.Entries.Count < 2)
                TestResult.Fail("Result bundle should contain at least two resources");
            if(_deliveryResult.Entries.ByResourceType<DocumentReference>().Count() != 1)
                TestResult.Fail("Result bundle should contain one DocumentReference");
            if (_deliveryResult.Entries.ByResourceType<Binary>().Count() != 1)
                TestResult.Fail("Result bundle should contain one Binary");
        }

        [SprinklerTest("MA02", "Read contents of mailbox post")]
        public void Test()
        {
            if (_deliveryResult == null) TestResult.Skip();

            var dref1 = _deliveryResult.Entries.ByResourceType<DocumentReference>().First();
            var bin1 = _deliveryResult.Entries.ByResourceType<Binary>().First();

            var dref = client.Read<DocumentReference>(dref1.SelfLink);
            var bin = client.Read<Binary>(bin1.SelfLink);

            // Location link can be version specific or not.
            if (! (bin.SelfLink.ToString().EndsWith(dref.Resource.Location.ToString()) ||
                   bin.Id.ToString().EndsWith(dref.Resource.Location.ToString())))
                TestResult.Fail("DocumentReference does not seem to refer to the included binary");

            if(!bin.Resource.ContentType.Contains("xml"))
                TestResult.Fail("Binary's content-type should have been xml");

            //TODO: Compare binary with sent data
        }


        [SprinklerTest("MA03", "Posting an XDS feed with internal references to /Mailbox")]
        public void TestPostXdsWidthCid()
        {
            if (!supportsDocumentMailbox)
                TestResult.Skip();

            Bundle xdsBundle = DemoData.GetDemoConn5CidExampleBundle();
            xdsBundle.SetBundleType(BundleType.Document);

            _deliveryResult = client.DeliverToMailbox(xdsBundle);

            if (_deliveryResult.Entries.Count < 2)
                TestResult.Fail("Result bundle should contain at least 2 resources");
            if (_deliveryResult.Entries.ByResourceType<DocumentReference>().Count() != 1)
                TestResult.Fail("Result bundle should contain one DocumentReference");
            if (_deliveryResult.Entries.ByResourceType<Binary>().Count() != 1)
                TestResult.Fail("Result bundle should contain one Binary");
            HttpTests.AssertEntryIdsArePresentAndAbsoluteUrls(_deliveryResult);
        }
    }
}
