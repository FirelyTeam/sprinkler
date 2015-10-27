using System;
using System.Collections.Generic;
using System.Linq;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Sprinkler.Framework;

namespace Fhir.Testing.Framework
{
    public class BundleAssert
    {
        public static void CheckMinimumNumberOfElementsInBundle(Bundle bundle, int expected)
        {
            if (bundle.Entry.Count < expected)
            {
                Assert.Fail("At least {0} elements expected, found {1}", expected, bundle.Entry.Count);
            }
        }

        public static void CheckMaximumNumberOfElementsInBundle(Bundle bundle, int expected)
        {
            if (bundle.Entry.Count > expected)
            {
                Assert.Fail("At most {0} elements expected, found {1}", expected, bundle.Entry.Count);
            }
        }

        public static void CheckConditionForAllElements(Bundle bundle, Predicate<Bundle.BundleEntryComponent> condition, string conditionDescription)
        {
            if (!bundle.Entry.TrueForAll(condition))
            {
                Assert.Fail("Not all entries respect condition: {0}", conditionDescription);
            }
        }

        public static void CheckConditionForResources(Bundle bundle, Func<Resource, bool> condition, string conditionDescription)
        {
            if (bundle.GetResources().Any(r => condition(r) == false))
            {
                Assert.Fail("Not all entries respect condition: {0}", conditionDescription);
            }
        }

        public static void CheckConditionForResources<T>(Bundle bundle, Func<T, bool> condition, string conditionDescription)
            where T:Resource
        {
            if (bundle.GetResources().Cast<T>().Any(condition) == false)
            {
                Assert.Fail("Not all entries respect condition: {0}", conditionDescription);
            }
        }

        public static void CheckTypeForResources<T>(Bundle bundle)
        {
            if (bundle.GetResources().Any(r => r.GetType() != typeof(T)))
            {
                Assert.Fail("Not all resources are {0}", typeof(T).Name);
            }
        }

        public static void CheckConditionForResourcesWithIdInformation(Bundle bundle, Func<Resource, bool> condition, string conditionDescription)
        {
            foreach (var resource in bundle.GetResources())
            {
                if (!condition(resource))
                {
                    Assert.Fail("Resource with id {0} does not respect condition {1}", resource.Id, conditionDescription);
                }
            }
        }

        public static void CheckConditionForResourcesWithVersionIdInformation(Bundle bundle, Func<Resource, bool> condition, string conditionDescription)
        {
            foreach (var resource in bundle.GetResources())
            {
                if (!condition(resource))
                {
                    Assert.Fail("Resource with id {0} does not respect condition {1}", resource.VersionId, conditionDescription);
                }
            }
        }

        public static void CheckResourcesInOrder<T>(Bundle bundle, Func<Resource, T> keySelector, string conditionDescription = null) where T: IComparable<T>
        {
            Resource prevResource = null;
            foreach (var resource in bundle.GetResources())
            {
                if (prevResource != null)
                {
                    if (keySelector(prevResource).CompareTo(keySelector(resource)) > 0)
                    {
                        Assert.Fail("Resources are not correctly ordered. First out of order resource is {0}", resource.VersionId);
                    }
                }
                prevResource = resource;
            }
        }

        public static void CheckResourcesInReverseOrder<T>(Bundle bundle, Func<Resource, T> keySelector, string conditionDescription = null) where T : IComparable<T>
        {
            Resource prevResource = null;
            foreach (var resource in bundle.GetResources())
            {
                if (prevResource != null)
                {
                    if (keySelector(prevResource).CompareTo(keySelector(resource)) < 0)
                    {
                        Assert.Fail("Resources are not correctly ordered. First out of order resource is {0}", resource.VersionId);
                    }
                }
                prevResource = resource;
            }
        }

        public static void CheckConditionForAllElementsWithEntryInformation(Bundle bundle, Predicate<Bundle.BundleEntryComponent> condition, Func<Bundle.BundleEntryComponent, string> entryInformation, string conditionDescription)
        {
            foreach (var entry in bundle.Entry)
            {
                if (!condition(entry))
                {
                    Assert.Fail("Entry {0} does not respect condition {1}", entryInformation(entry), conditionDescription);
                }
            }
         }


        public static void ContainsAllVersionIds(Bundle bundle, IEnumerable<string> versionsList)
        {
            IList<string> versionsInBundle = bundle.Entry.Select(entry => new ResourceIdentity(entry.Request.Url).VersionId).ToList();
            foreach (string item in versionsList)
            {
                if (!versionsInBundle.Contains(item))
                {
                    Assert.Fail("Bundle does not contain all expected versions");
                }
            }
        }

        public static void CheckBundleEmpty(Bundle bundle)
        {
            if (bundle.Entry.Count != 0)
                Assert.Fail("Bundle should be empty");
        }
    }
}