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
    [SprinklerModule("Tags")]
    public class TagTest : SprinklerTestClass
    {
        private const string TESTTAG = "http://readtag.hl7.nl";
        private const string TESTTAG2 = "http://readtag2.hl7.nl";
        private readonly string _otherTag;

        private ResourceEntry<Patient> P1, P2;
        private IEnumerable<Tag> nutags;

        public TagTest()
        {
            _otherTag = randomTag();
        }

        private string randomTag()
        {
            string s = new Random().Next().ToString();
            return string.Format("http://othertag{0}.hl7.nl", s);
        }

        [SprinklerTest("TA01", "Create and retrieve tags with create")]
        public void TestTagsOnCreate()
        {
            var tags = new List<Tag> {new Tag(TESTTAG, Tag.FHIRTAGSCHEME_GENERAL, "readTagTest")};
            Patient patient = DemoData.GetDemoPatient();

            Assert.Success(Client, () => P1 = Client.Create(patient, tags, true));

            if (P1.Tags == null)
                Assert.Fail("create did not return any tags");

            nutags = P1.Tags.FindByTerm(TESTTAG, Tag.FHIRTAGSCHEME_GENERAL);
            if (nutags.Count() != 1 || nutags.First().Label != "readTagTest")
                Assert.Fail("create did not return specified tag");
        }

        [SprinklerTest("TA01b", "Read tags from resource")]
        public void TestTagsOnRead()
        {
            Assert.SkipWhen(P1 == null);

            ResourceEntry<Patient> read = Client.Read<Patient>(P1.Id);
            if (read.Tags == null)
                Assert.Fail("read did not return any tags");

            nutags = P1.Tags.FindByTerm(TESTTAG, Tag.FHIRTAGSCHEME_GENERAL);
            if (nutags.Count() != 1 || nutags.First().Label != "readTagTest")
                Assert.Fail("read did not return specified tag");
        }
        
        [SprinklerTest("TA01c", "Read tags from historic resource")]
        public void TestTagsOnHistoryRead()
        {
            ResourceEntry<Patient> vread = Client.Read<Patient>(P1.SelfLink);
            if (vread.Tags == null)
                Assert.Fail("version read did not return any tags");

            nutags = P1.Tags.FindByTerm(TESTTAG, Tag.FHIRTAGSCHEME_GENERAL);
            if (nutags.Count() != 1 || nutags.First().Label != "readTagTest")
                Assert.Fail("version read did not return specified tag");

            P2 = P1;
        }

        [SprinklerTest("TA03", "Read tags from non existing resources")]
        public void TestTagsOnNonExisting()
        {
            // todo: tags from instance

            string id = "nonexisting", vid = id;
            string key = "Patient/nonexisting";

            ResourceEntry nep = null; // non-existing-patient
            IEnumerable<Tag> tags = null;

            Assert.Fails(Client, () => { nep = Client.Read<Patient>(key); }, HttpStatusCode.NotFound);
            Assert.IsTrue(nep == null, "Non existing patient instance should be zero");

            Assert.Fails(Client, () => { tags = Client.Tags<Patient>(id); }, HttpStatusCode.NotFound);

            Assert.Fails(Client, () => { tags = Client.Tags<Patient>(id, vid); },
                HttpStatusCode.NotFound);
        }

        [SprinklerTest("TA04", "Update tags with update")]
        public void UpdateTagsOnUpdate()
        {
            Assert.SkipWhen(P2 == null);

            // Update one tag, add another
            var newtags = new List<Tag>
            {
                new Tag(TESTTAG, Tag.FHIRTAGSCHEME_GENERAL, "readTagTest2"),
                new Tag(_otherTag, Tag.FHIRTAGSCHEME_GENERAL, "dummy")
            };
            
            P1 = null;
            Assert.Success(Client, 
                () => 
                { 
                    P1 = Client.Read<Patient>(P2.Id); 
                });

            P1.Tags = newtags;

            Assert.Success(Client, () => Client.Update(P1));

            ResourceEntry<Patient> read = Client.Read<Patient>(P1.Id);

            if (read.Tags == null)
                Assert.Fail("fetch after update did not return any tags");

            if (read.Tags.Count() != 2)
                Assert.Fail(String.Format("Wrong number of tags after update: {0}, expected 2", read.Tags.Count()));

            IEnumerable<Tag> nutags = read.Tags.FindByTerm(TESTTAG, Tag.FHIRTAGSCHEME_GENERAL);
            if (nutags.Count() != 1 || nutags.First().Label != "readTagTest2")
                Assert.Fail("update did not replace value in tag");

            IEnumerable<Tag> othertags = read.Tags.FindByTerm(_otherTag, Tag.FHIRTAGSCHEME_GENERAL);
            if (othertags.Count() != 1 || othertags.First().Label != "dummy")
                Assert.Fail("update failed to add new tag");

            P1 = read;
        }

        [SprinklerTest("TA05", "Retrieve server-wide tags")]
        public void GetServerWideTags()
        {
            IEnumerable<Tag> tags = null;

            Assert.Success(Client, () => tags = Client.WholeSystemTags());


            IEnumerable<string> tagStrings = tags.FilterOnFhirSchemes().Where(t => t.Term != null).Select(t => t.Term);

            if (!tagStrings.Contains(TESTTAG) || !tagStrings.Contains(_otherTag))
                Assert.Fail("expected tags not found in server-wide tag list");
        }

        [SprinklerTest("TA06", "Retrieve resource-wide tags")]
        public void GetResourceWideTags()
        {
            IEnumerable<Tag> tags = null;

            Assert.Success(Client, () => tags = Client.TypeTags<Patient>());

            IEnumerable<string> tagStrings = tags.FilterOnFhirSchemes().Where(t => t.Term != null).Select(t => t.Term);

            if (!tagStrings.Contains(TESTTAG) || !tagStrings.Contains(_otherTag))
                Assert.Fail("expected tags not found in resource-wide tag list");

            Assert.Success(Client, () => tags = Client.TypeTags<Conformance>());

            tagStrings = tags.FilterOnFhirSchemes().Where(t => t.Term != null).Select(t => t.Term);
            if (tagStrings.Contains(TESTTAG) || tagStrings.Contains(_otherTag))
                Assert.Fail("tags showed up while listing tags for another resource type");
        }

        [SprinklerTest("TA07", "Retrieve resource instance tags")]
        public void GetInstanceTags()
        {
            IEnumerable<Tag> tags = null;

            var identity = new ResourceIdentity(P1.SelfLink);

            Assert.Success(Client, () => tags = Client.Tags<Patient>(identity.Id));

            IEnumerable<string> tagStrings = tags.FilterOnFhirSchemes().Where(t => t.Term != null).Select(t => t.Term);

            if (!tagStrings.Contains(TESTTAG) || !tagStrings.Contains(_otherTag))
                Assert.Fail("expected tags not found in resource instance tag list");
        }

        [SprinklerTest("TA08", "Retrieve resource history tags")]
        public void GetInstanceHistoryTags()
        {
            IEnumerable<Tag> tags = null;

            var rl = new ResourceIdentity(P1.SelfLink);

            Assert.Success(Client, () => tags = Client.Tags<Patient>(rl.Id, rl.VersionId));

            IEnumerable<string> tagStrings = tags.FilterOnFhirSchemes().Where(t => t.Term != null).Select(t => t.Term);

            if (!tagStrings.Contains(TESTTAG) || !tagStrings.Contains(_otherTag))
                Assert.Fail("expected tags not found in resource instance tag list");
        }

        [SprinklerTest("TA09", "Search resource using tags")]
        public void SearchUsingTag()
        {
            var tag = new Tag(_otherTag, Tag.FHIRTAGSCHEME_GENERAL, "dummy");

            Bundle result = null;
            Assert.Success(Client, () => result = Client.Search<Patient>(new[] {"_tag=" + _otherTag}));

            if (result.Entries.ByTag(_otherTag).Count() != 1)
                Assert.Fail("could not retrieve patient by its tag");
        }

        [SprinklerTest("TA10", "Update tags using POST")]
        public void UpdateTagsUsingPost()
        {
            //var identity = new ResourceIdentity(P1.SelfLink).WithoutVersion();

            var update = new Tag(TESTTAG, Tag.FHIRTAGSCHEME_GENERAL, "newVersion");
            var existing = new Tag(_otherTag, Tag.FHIRTAGSCHEME_GENERAL);


            Assert.Success(
                Client, () => Client.AffixTags(P1.Id, new List<Tag> {update})
                );

            ResourceEntry<Patient> result = Client.Read<Patient>(P1.Id);

            if (result.Tags.Count() != 2)
                Assert.Fail("update modified the number of tags");

            if (!result.Tags.Any(t => t.Equals(existing)))
                Assert.Fail("update removed an existing but unchanged tag");

            if (!result.Tags.Any(t => t.Equals(update) && t.Label == update.Label))
                Assert.Fail("update did not change the tag");

            if (result.SelfLink != P1.SelfLink)
                Assert.Fail("updating the tags created a new version");
        }

        [SprinklerTest("TA11", "Update tags on version using POST")]
        public void UpdateTagsOnVersionUsingPost()
        {
            var identity = new ResourceIdentity(P1.SelfLink);

            var update = new Tag(TESTTAG, Tag.FHIRTAGSCHEME_GENERAL, "newVersionForVersion");
            var existing = new Tag(_otherTag, Tag.FHIRTAGSCHEME_GENERAL);

            Assert.Success(Client, () => Client.AffixTags(identity, new List<Tag> {update}));

            ResourceEntry<Patient> result = Client.Read<Patient>(P1.Id);

            if (result.Tags.Count() != 2)
                Assert.Fail("update modified the number of tags");

            if (!result.Tags.Any(t => t.Equals(existing)))
                Assert.Fail("update removed an existing but unchanged tag");

            if (!result.Tags.Any(t => t.Equals(update) && t.Label == update.Label))
                Assert.Fail("update did not change the tag");

            if (result.SelfLink != P1.SelfLink)
                Assert.Fail("updating the tags created a new version");

            //TODO: Check whether taglists on older versions remain unchanged
        }

        [SprinklerTest("TA12", "Delete tags on a resource using DELETE")]
        public void DeleteTagsUsingDelete()
        {
            ResourceIdentity identity = new ResourceIdentity(P1.SelfLink).WithoutVersion();

            var delete = new Tag(TESTTAG, Tag.FHIRTAGSCHEME_GENERAL);
            var existing = new Tag(_otherTag, Tag.FHIRTAGSCHEME_GENERAL);

            Assert.Success(Client, () => Client.DeleteTags(identity, new List<Tag> {delete}));

            ResourceEntry<Patient> result = Client.Read<Patient>(P1.Id);


            if (result.Tags.Count() != 1)
                Assert.Fail("delete resulted in an unexpected number of remaining tags");

            if (!result.Tags.Any(t => t.Equals(existing)))
                Assert.Fail("delete removed an existing tag the should be untouched");

            if (result.Tags.Any(t => t.Equals(delete)))
                Assert.Fail("delete did not remove the tag");

            if (result.SelfLink != P1.SelfLink)
                Assert.Fail("deleting the tags created a new version");

            ////TODO: Check whether taglists on older versions remain unchanged
        }

        [SprinklerTest("TA13", "Delete tags on a version using DELETE")]
        public void DeleteTagsOnVersionUsingDelete()
        {
            var identity = new ResourceIdentity(P1.SelfLink);

            var delete = new Tag(TESTTAG, Tag.FHIRTAGSCHEME_GENERAL);
            var existing = new Tag(_otherTag, Tag.FHIRTAGSCHEME_GENERAL);

            Assert.Success(Client, () => Client.DeleteTags(identity, new List<Tag> {delete}));

            ResourceEntry<Patient> result = Client.Read<Patient>(P1.Id);

            if (result.Tags.Count() != 1)
                Assert.Fail("delete resulted in an unexpected number of remaining tags");

            if (!result.Tags.Any(t => t.Equals(existing)))
                Assert.Fail("delete removed an existing tag the should be untouched");

            if (result.Tags.Any(t => t.Equals(delete)))
                Assert.Fail("delete did not remove the tag");

            if (result.SelfLink != P1.SelfLink)
                Assert.Fail("deleting the tags created a new version");

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