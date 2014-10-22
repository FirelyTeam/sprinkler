/* 
 * Copyright (c) 2014, Furore (info@furore.com) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.github.com/furore-fhir/sprinkler/master/LICENSE
 */

using System.Linq;
using Hl7.Fhir.Model;
using Sprinkler.Framework;

namespace Sprinkler.Tests
{
    [SprinklerModule("MAILBOX")]
    public class MailboxTest : SprinklerTestClass
    {
        private Bundle _deliveryResult;

        //TODO send this bundle signed => signature should still work on binary


        [SprinklerTest("MA01", "Posting an XDS feed to /Mailbox")]
        public void TestPostXds()
        {
            Bundle xdsBundle = DemoData.GetDemoConn5ExampleBundle();
            xdsBundle.SetBundleType(BundleType.Document);

            _deliveryResult = Client.DeliverToMailbox(xdsBundle);

            if (_deliveryResult.Entries.Count != 12)
                Assert.Fail("Result bundle should contain exactly two resources");
            if (_deliveryResult.Entries.ByResourceType<DocumentReference>().Count() != 1)
                Assert.Fail("Result bundle should contain one DocumentReference");
            if (_deliveryResult.Entries.ByResourceType<Binary>().Count() != 1)
                Assert.Fail("Result bundle should contain one Binary");
        }


        [SprinklerTest("MA02", "Read contents of mailbox post")]
        public void TestMailboxPost()
        {
            if (_deliveryResult == null) TestResult.Skip();

            ResourceEntry<DocumentReference> dref1 = _deliveryResult.Entries.ByResourceType<DocumentReference>().First();
            ResourceEntry<Binary> bin1 = _deliveryResult.Entries.ByResourceType<Binary>().First();

            ResourceEntry<DocumentReference> dref = Client.Read<DocumentReference>(dref1.SelfLink);
            ResourceEntry<Binary> bin = Client.Read<Binary>(bin1.SelfLink);

            if (!bin.SelfLink.ToString().EndsWith(dref.Resource.Location.ToString()))
                Assert.Fail("DocumentReference does not seem to refer to the included binary");

            if (!bin.Resource.ContentType.Contains("xml"))
                Assert.Fail("Binary's content-type should have been xml");

            //TODO: Compare binary with sent data
        }


        [SprinklerTest("MA03", "Posting an XDS feed with internal references to /Mailbox")]
        public void TestPostXdsWidthCid()
        {
            Bundle xdsBundle = DemoData.GetDemoConn5CidExampleBundle();
            xdsBundle.SetBundleType(BundleType.Document);

            _deliveryResult = Client.DeliverToMailbox(xdsBundle);

            if (_deliveryResult.Entries.Count != 7)
                Assert.Fail("Result bundle should contain exactly 7 resources");
            if (_deliveryResult.Entries.ByResourceType<DocumentReference>().Count() != 1)
                Assert.Fail("Result bundle should contain one DocumentReference");
            if (_deliveryResult.Entries.ByResourceType<Binary>().Count() != 1)
                Assert.Fail("Result bundle should contain one Binary");
            Assert.EntryIdsArePresentAndAbsoluteUrls(_deliveryResult);
        }
    }
}