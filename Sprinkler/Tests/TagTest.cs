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
using System.Net;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Sprinkler.Framework;

namespace Sprinkler.Tests
{
    [SprinklerTestModule("Tags")]
    public class TagTest : SprinklerTestClass
    {
        private const string NUTAG = "http://readtag.hl7.nl";
        private readonly string _otherTag;

        private ResourceEntry<Patient> latest;
        private ResourceEntry<Patient> original;

        public TagTest()
        {
            _otherTag = randomTag();
        }

        private string randomTag()
        {
            string s = new Random().Next().ToString();
            return string.Format("http://othertag{0}.hl7.nl", s);
        }

        [SprinklerTest("TA01", "Create and retrieve tags with create/read")]
        public void TestTagsOnCreateAndRead()
        {
            var tags = new List<Tag> {new Tag(NUTAG, Tag.FHIRTAGSCHEME_GENERAL, "readTagTest")};
            Patient patient = DemoData.GetDemoPatient();

            HttpTests.AssertSuccess(Client, () => latest = Client.Create(patient, tags, true));

            if (latest.Tags == null)
                TestResult.Fail("create did not return any tags");

            IEnumerable<Tag> nutags = latest.Tags.FindByTerm(NUTAG, Tag.FHIRTAGSCHEME_GENERAL);
            if (nutags.Count() != 1 || nutags.First().Label != "readTagTest")
                TestResult.Fail("create did not return specified tag");

            ResourceEntry<Patient> read = Client.Read<Patient>(latest.Id);
            if (read.Tags == null)
                TestResult.Fail("read did not return any tags");

            nutags = latest.Tags.FindByTerm(NUTAG, Tag.FHIRTAGSCHEME_GENERAL);
            if (nutags.Count() != 1 || nutags.First().Label != "readTagTest")
                TestResult.Fail("read did not return specified tag");

            ResourceEntry<Patient> vread = Client.Read<Patient>(latest.SelfLink);
            if (vread.Tags == null)
                TestResult.Fail("vread did not return any tags");

            nutags = latest.Tags.FindByTerm(NUTAG, Tag.FHIRTAGSCHEME_GENERAL);
            if (nutags.Count() != 1 || nutags.First().Label != "readTagTest")
                TestResult.Fail("vread did not return specified tag");

            original = latest;
        }

        [SprinklerTest("TA02", "Read tags from non existing resources")]
        public void TestTagsOnNonExisting()
        {
            // todo: tags from instance

            ResourceEntry nep = null; // non-existing-patient
            IEnumerable<Tag> tags = null;

            HttpTests.AssertFail(Client, () => { nep = Client.Read<Patient>("nonexisting"); }, HttpStatusCode.NotFound);
            TestResult.Assert(nep == null, "Non existing patient instance should be zero");

            HttpTests.AssertFail(Client, () => { tags = Client.Tags<Patient>("nonexisting"); }, HttpStatusCode.NotFound);

            HttpTests.AssertFail(Client, () => { tags = Client.Tags<Patient>("nonexisting", "nonexisting"); },
                HttpStatusCode.NotFound);
        }

        [SprinklerTest("TA03", "Update tags with update")]
        public void UpdateTagsOnUpdate()
        {
            if (original == null) TestResult.Skip();

            // Update one tag, add another
            var newtags = new List<Tag>
            {
                new Tag(NUTAG, Tag.FHIRTAGSCHEME_GENERAL, "readTagTest2"),
                new Tag(_otherTag, Tag.FHIRTAGSCHEME_GENERAL, "dummy")
            };

            HttpTests.AssertSuccess(Client, () => latest = Client.Read<Patient>(original.Id));

            latest.Tags = newtags;

            HttpTests.AssertSuccess(Client, () => Client.Update(latest));

            ResourceEntry<Patient> read = Client.Read<Patient>(latest.Id);

            if (read.Tags == null)
                TestResult.Fail("fetch after update did not return any tags");

            if (read.Tags.Count() != 2)
                TestResult.Fail(String.Format("Wrong number of tags after update: {0}, expected 2", read.Tags.Count()));

            IEnumerable<Tag> nutags = read.Tags.FindByTerm(NUTAG, Tag.FHIRTAGSCHEME_GENERAL);
            if (nutags.Count() != 1 || nutags.First().Label != "readTagTest2")
                TestResult.Fail("update did not replace value in tag");

            IEnumerable<Tag> othertags = read.Tags.FindByTerm(_otherTag, Tag.FHIRTAGSCHEME_GENERAL);
            if (othertags.Count() != 1 || othertags.First().Label != "dummy")
                TestResult.Fail("update failed to add new tag");

            latest = read;
        }

        [SprinklerTest("TA04", "Retrieve server-wide tags")]
        public void GetServerWideTags()
        {
            IEnumerable<Tag> tags = null;

            HttpTests.AssertSuccess(Client, () => tags = Client.WholeSystemTags());


            IEnumerable<string> tagStrings = tags.FilterOnFhirSchemes().Where(t => t.Term != null).Select(t => t.Term);

            if (!tagStrings.Contains(NUTAG) || !tagStrings.Contains(_otherTag))
                TestResult.Fail("expected tags not found in server-wide tag list");
        }

        [SprinklerTest("TA05", "Retrieve resource-wide tags")]
        public void GetResourceWideTags()
        {
            IEnumerable<Tag> tags = null;

            HttpTests.AssertSuccess(Client, () => tags = Client.TypeTags<Patient>());

            IEnumerable<string> tagStrings = tags.FilterOnFhirSchemes().Where(t => t.Term != null).Select(t => t.Term);

            if (!tagStrings.Contains(NUTAG) || !tagStrings.Contains(_otherTag))
                TestResult.Fail("expected tags not found in resource-wide tag list");

            HttpTests.AssertSuccess(Client, () => tags = Client.TypeTags<Conformance>());

            tagStrings = tags.FilterOnFhirSchemes().Where(t => t.Term != null).Select(t => t.Term);
            if (tagStrings.Contains(NUTAG) || tagStrings.Contains(_otherTag))
                TestResult.Fail("tags showed up while listing tags for another resource type");
        }

        [SprinklerTest("TA06", "Retrieve resource instance tags")]
        public void GetInstanceTags()
        {
            IEnumerable<Tag> tags = null;

            var identity = new ResourceIdentity(latest.SelfLink);

            HttpTests.AssertSuccess(Client, () => tags = Client.Tags<Patient>(identity.Id));

            IEnumerable<string> tagStrings = tags.FilterOnFhirSchemes().Where(t => t.Term != null).Select(t => t.Term);

            if (!tagStrings.Contains(NUTAG) || !tagStrings.Contains(_otherTag))
                TestResult.Fail("expected tags not found in resource instance tag list");
        }

        [SprinklerTest("TA07", "Retrieve resource history tags")]
        public void GetInstanceHistoryTags()
        {
            IEnumerable<Tag> tags = null;

            var rl = new ResourceIdentity(latest.SelfLink);

            HttpTests.AssertSuccess(Client, () => tags = Client.Tags<Patient>(rl.Id, rl.VersionId));

            IEnumerable<string> tagStrings = tags.FilterOnFhirSchemes().Where(t => t.Term != null).Select(t => t.Term);

            if (!tagStrings.Contains(NUTAG) || !tagStrings.Contains(_otherTag))
                TestResult.Fail("expected tags not found in resource instance tag list");
        }

        [SprinklerTest("TA08", "Search resource using tags")]
        public void SearchUsingTag()
        {
            var tag = new Tag(_otherTag, Tag.FHIRTAGSCHEME_GENERAL, "dummy");

            Bundle result = null;
            HttpTests.AssertSuccess(Client, () => result = Client.Search<Patient>(new[] {"_tag=" + _otherTag}));

            if (result.Entries.ByTag(_otherTag).Count() != 1)
                TestResult.Fail("could not retrieve patient by its tag");
        }

        [SprinklerTest("TA09", "Update tags using POST")]
        public void UpdateTagsUsingPost()
        {
            var identity = new ResourceIdentity(latest.SelfLink);

            var update = new Tag(NUTAG, Tag.FHIRTAGSCHEME_GENERAL, "newVersion");
            var existing = new Tag(_otherTag, Tag.FHIRTAGSCHEME_GENERAL);


            HttpTests.AssertSuccess(
                Client, () => Client.AffixTags(identity, new List<Tag> {update})
                );

            ResourceEntry<Patient> result = Client.Read<Patient>(latest.Id);

            if (result.Tags.Count() != 2)
                TestResult.Fail("update modified the number of tags");

            if (!result.Tags.Any(t => t.Equals(existing)))
                TestResult.Fail("update removed an existing but unchanged tag");

            if (!result.Tags.Any(t => t.Equals(update) && t.Label == update.Label))
                TestResult.Fail("update did not change the tag");

            if (result.SelfLink != latest.SelfLink)
                TestResult.Fail("updating the tags created a new version");
        }

        [SprinklerTest("TA10", "Update tags on version using POST")]
        public void UpdateTagsOnVersionUsingPost()
        {
            var identity = new ResourceIdentity(latest.SelfLink);

            var update = new Tag(NUTAG, Tag.FHIRTAGSCHEME_GENERAL, "newVersionForVersion");
            var existing = new Tag(_otherTag, Tag.FHIRTAGSCHEME_GENERAL);

            HttpTests.AssertSuccess(Client, () => Client.AffixTags(identity, new List<Tag> {update}));

            ResourceEntry<Patient> result = Client.Read<Patient>(latest.Id);

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

        [SprinklerTest("TA11", "Delete tags on a resource using DELETE")]
        public void DeleteTagsUsingDelete()
        {
            ResourceIdentity identity = new ResourceIdentity(latest.SelfLink).WithoutVersion();

            var delete = new Tag(NUTAG, Tag.FHIRTAGSCHEME_GENERAL);
            var existing = new Tag(_otherTag, Tag.FHIRTAGSCHEME_GENERAL);

            HttpTests.AssertSuccess(Client, () => Client.DeleteTags(identity, new List<Tag> {delete}));

            ResourceEntry<Patient> result = Client.Read<Patient>(latest.Id);


            if (result.Tags.Count() != 1)
                TestResult.Fail("delete resulted in an unexpected number of remaining tags");

            if (!result.Tags.Any(t => t.Equals(existing)))
                TestResult.Fail("delete removed an existing tag the should be untouched");

            if (result.Tags.Any(t => t.Equals(delete)))
                TestResult.Fail("delete did not remove the tag");

            if (result.SelfLink != latest.SelfLink)
                TestResult.Fail("deleting the tags created a new version");

            ////TODO: Check whether taglists on older versions remain unchanged
        }

        [SprinklerTest("TA12", "Delete tags on a version using DELETE")]
        public void DeleteTagsOnVersionUsingDelete()
        {
            var identity = new ResourceIdentity(latest.SelfLink);

            var delete = new Tag(NUTAG, Tag.FHIRTAGSCHEME_GENERAL);
            var existing = new Tag(_otherTag, Tag.FHIRTAGSCHEME_GENERAL);

            HttpTests.AssertSuccess(Client, () => Client.DeleteTags(identity, new List<Tag> {delete}));

            ResourceEntry<Patient> result = Client.Read<Patient>(latest.Id);

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

    public static class TagListExtensions
    {
        public static IEnumerable<Tag> FindByTerm(this IEnumerable<Tag> list, string tag, Uri scheme = null)
        {
            return list.Where(t => t.Term == tag && t.Scheme == (scheme ?? Tag.FHIRTAGSCHEME_GENERAL));
        }

        public static IEnumerable<BundleEntry> ByTag(this IEnumerable<BundleEntry> entries, string tag)
        {
            return entries.Where(e => e.Tags.Any(t => t.Term == tag));
        }
    }
}