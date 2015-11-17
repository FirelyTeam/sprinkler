using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using Hl7.Fhir.Client;
using Hl7.Fhir.Model;
using Hl7.Fhir.Support;

namespace Sprinkler
{
    [SprinklerTestModule("Tags")]
    public class TagTest
    {
        FhirClient client;

        public TagTest(Uri fhirUri)
        {
            client = new FhirClient(fhirUri);
        }

        private const string NUTAG = "http://readtag.hl7.nl";
        private const string OTHERTAG = "http://othertag.hl7.nl";

        ResourceEntry<Patient> latest = null;
        ResourceEntry<Patient> original = null;

        [SprinklerTest("create and retrieve tags with create/read")]
        public void TestTagsOnCreateAndRead()
        {
            var tags = new List<Tag>() { new Tag(NUTAG, Tag.FHIRTAGNS, "readTagTest") };
            
            HttpTests.AssertSuccess(client, () => latest = client.Create<Patient>(DemoData.GetDemoPatient(),tags));

            if(latest.Tags == null)
                TestResult.Fail("create did not return any tags");

            var nutags = latest.Tags.FindByTerm(NUTAG, Tag.FHIRTAGNS);
            if (nutags.Count() != 1 || nutags.First().Label != "readTagTest")
                TestResult.Fail("create did not return specified tag");

            var read = client.Fetch<Patient>(latest.Id);
            if (read.Tags == null)
                TestResult.Fail("read did not return any tags");
            nutags = latest.Tags.FindByTerm(NUTAG, Tag.FHIRTAGNS);
            if (nutags.Count() != 1 || nutags.First().Label != "readTagTest")
                TestResult.Fail("read did not return specified tag");

            var vread = client.Fetch<Patient>(latest.SelfLink);
            if (vread.Tags == null)
                TestResult.Fail("vread did not return any tags");
            nutags = latest.Tags.FindByTerm(NUTAG, Tag.FHIRTAGNS);
            if (nutags.Count() != 1 || nutags.First().Label != "readTagTest")
                TestResult.Fail("vread did not return specified tag");

            original = latest;
        }

        [SprinklerTest("update tags with update")]
        public void UpdateTagsOnUpdate()
        {
            if (original == null) TestResult.Skipped();

            // Update one tag, add another
            var tags = new List<Tag>() { 
                new Tag(NUTAG, Tag.FHIRTAGNS, "readTagTest2"), 
                new Tag(OTHERTAG, Tag.FHIRTAGNS, "dummy") };

            HttpTests.AssertSuccess(client, () => latest = client.Fetch<Patient>(original.Id));

            latest.Tags = tags;

            HttpTests.AssertSuccess(client, () => client.Update<Patient>(latest));

            var read = client.Fetch<Patient>(latest.Id);

            if (read.Tags == null)
                TestResult.Fail("fetch after update did not return any tags");

            if (read.Tags.Count() != 2)
                TestResult.Fail(String.Format("Wrong number of tags after update: {0}, expected 2", read.Tags.Count()));
            var nutags = read.Tags.FindByTerm(NUTAG,Tag.FHIRTAGNS);
            if (nutags.Count() != 1 || nutags.First().Label != "readTagTest2")
                TestResult.Fail("update did not replace value in tag");
            var othertags = read.Tags.FindByTerm(OTHERTAG,Tag.FHIRTAGNS);
            if(othertags.Count() != 1 || othertags.First().Label != "dummy")
                TestResult.Fail("update failed to add new tag");

            latest = read;
        }


        [SprinklerTest("retrieve server-wide tags")]
        public void GetServerWideTags()
        {
            IEnumerable<Tag> tags = null;

            HttpTests.AssertSuccess(client, () => tags = client.GetTags());

            var tagStrings = tags.FilterFhirTags().Where(t=>t.Term != null).Select(t=>t.Term);

            if (!tagStrings.Contains(NUTAG) || !tagStrings.Contains(OTHERTAG))
                TestResult.Fail("expected tags not found in server-wide tag list");
        }


        [SprinklerTest("retrieve resource-wide tags")]
        public void GetResourceWideTags()
        {
            IEnumerable<Tag> tags = null;

            HttpTests.AssertSuccess(client, () => tags = client.GetTags(ResourceType.Patient));

            var tagStrings = tags.FilterFhirTags().Where(t => t.Term != null).Select(t => t.Term);

            if (!tagStrings.Contains(NUTAG) || !tagStrings.Contains(OTHERTAG))
                TestResult.Fail("expected tags not found in resource-wide tag list");

            HttpTests.AssertSuccess(client, () => tags = client.GetTags(ResourceType.Conformance));
            tagStrings = tags.FilterFhirTags().Where(t => t.Term != null).Select(t => t.Term);
            if (tagStrings.Contains(NUTAG) || tagStrings.Contains(OTHERTAG))
                TestResult.Fail("tags showed up while listing tags for another resource type");
        }

        [SprinklerTest("retrieve resource instance tags")]
        public void GetInstanceTags()
        {
            IEnumerable<Tag> tags = null;

            var rl = new ResourceLocation(latest.SelfLink);

            HttpTests.AssertSuccess(client, () => tags = client.GetTags(ResourceType.Patient, rl.Id));

            var tagStrings = tags.FilterFhirTags().Where(t => t.Term != null).Select(t => t.Term);

            if (!tagStrings.Contains(NUTAG) || !tagStrings.Contains(OTHERTAG))
                TestResult.Fail("expected tags not found in resource instance tag list");
        }

        [SprinklerTest("retrieve resource history tags")]
        public void GetInstanceHistoryTags()
        {
            IEnumerable<Tag> tags = null;

            var rl = new ResourceLocation(latest.SelfLink);

            HttpTests.AssertSuccess(client, () => tags = client.GetTags(ResourceType.Patient, rl.Id, rl.VersionId));

            var tagStrings = tags.FilterFhirTags().Where(t => t.Term != null).Select(t => t.Term);

            if (!tagStrings.Contains(NUTAG) || !tagStrings.Contains(OTHERTAG))
                TestResult.Fail("expected tags not found in resource instance tag list");
        }

        [SprinklerTest("search resource using tags")]
        public void SearchUsingTag()
        {
            var tag = new Tag(OTHERTAG, Tag.FHIRTAGNS, "dummy");

            Bundle result = null;
            HttpTests.AssertSuccess(client, () => result = client.Search(ResourceType.Patient, "_tag", OTHERTAG));

            if (result.Entries.ByTag(OTHERTAG).Count() != 1)
                TestResult.Fail("could not retrieve patient by its tag");
        }

        [SprinklerTest("update tags using POST")]
        public void UpdateTagsUsingPost()
        {
            var rl = new ResourceLocation(latest.SelfLink);

            var update = new Tag(NUTAG, Tag.FHIRTAGNS, "newVersion");
            var existing = new Tag(OTHERTAG, Tag.FHIRTAGNS);

            HttpTests.AssertSuccess(client, () => client.AffixTags(
                        new List<Tag> { update }, ResourceType.Patient, rl.Id));

            var result = client.Fetch<Patient>(latest.Id);

            if (result.Tags.Count() != 2)
                TestResult.Fail("update modified the number of tags");

            if( !result.Tags.Any( t => t.Equals(existing) ) )
                TestResult.Fail("update removed an existing but unchanged tag");

            if (!result.Tags.Any(t => t.Equals(update) && t.Label == update.Label))
                TestResult.Fail("update did not change the tag");

            if (result.SelfLink != latest.SelfLink)
                TestResult.Fail("updating the tags created a new version");
        }

        [SprinklerTest("update tags on version using POST")]
        public void UpdateTagsOnVersionUsingPost()
        {
            var rl = new ResourceLocation(latest.SelfLink);

            var update = new Tag(NUTAG, Tag.FHIRTAGNS, "newVersionForVersion");
            var existing = new Tag(OTHERTAG, Tag.FHIRTAGNS);

            HttpTests.AssertSuccess(client, () => client.AffixTags(
                        new List<Tag> { update }, ResourceType.Patient, rl.Id, rl.VersionId));

            var result = client.Fetch<Patient>(latest.Id);

            if (result.Tags.Count() != 2)
                TestResult.Fail("update modified the number of tags");

            if (!result.Tags.Any(t => t.Equals(existing)))
                TestResult.Fail("update removed an existing but unchanged tag");

            if (!result.Tags.Any(t => t.Equals(update) && t.Label == update.Label))
                TestResult.Fail("update did not change the tag");

            if (result.SelfLink != latest.SelfLink)
                TestResult.Fail("updating the tags created a new version");

            //TODO: Check whether taglists on older versions remain unchanged
        }

        [SprinklerTest("delete tags on a resource using DELETE")]
        public void DeleteTagsUsingDelete()
        {
            var rl = new ResourceLocation(latest.SelfLink);

            var delete = new Tag(NUTAG, Tag.FHIRTAGNS);
            var existing = new Tag(OTHERTAG, Tag.FHIRTAGNS);

            HttpTests.AssertSuccess(client, () => client.DeleteTags(
                new List<Tag> { delete }, ResourceType.Patient, rl.Id));

            var result = client.Fetch<Patient>(latest.Id);

            if(result.Tags.Count() != 1)
                TestResult.Fail("delete resulted in an unexpected number of remaining tags");

            if (!result.Tags.Any(t => t.Equals(existing)))
                TestResult.Fail("delete removed an existing tag the should be untouched");

            if (result.Tags.Any(t => t.Equals(delete)))
                TestResult.Fail("delete did not remove the tag");

            if (result.SelfLink != latest.SelfLink)
                TestResult.Fail("deleting the tags created a new version");

            //TODO: Check whether taglists on older versions remain unchanged
        }

        [SprinklerTest("delete tags on a version using DELETE")]
        public void DeleteTagsOnVersionUsingDelete()
        {
            var rl = new ResourceLocation(latest.SelfLink);

            var delete = new Tag(NUTAG, Tag.FHIRTAGNS);
            var existing = new Tag(OTHERTAG, Tag.FHIRTAGNS);

            HttpTests.AssertSuccess(client, () => client.DeleteTags(
                new List<Tag> { delete }, ResourceType.Patient, rl.Id, rl.VersionId));

            var result = client.Fetch<Patient>(latest.Id);

            if (result.Tags.Count() != 1)
                TestResult.Fail("delete resulted in an unexpected number of remaining tags");

            if (!result.Tags.Any(t => t.Equals(existing)))
                TestResult.Fail("delete removed an existing tag the should be untouched");

            if (result.Tags.Any(t => t.Equals(delete)))
                TestResult.Fail("delete did not remove the tag");

            if (result.SelfLink != latest.SelfLink)
                TestResult.Fail("deleting the tags created a new version");

            //TODO: Check whether taglists on older versions remain unchanged
        }
    }
}
