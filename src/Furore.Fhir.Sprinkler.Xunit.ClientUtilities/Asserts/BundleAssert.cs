using System;
using System.Collections.Generic;
using System.Linq;
using Furore.Fhir.Sprinkler.Runner.Contracts;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Xunit;
using Xunit.Sdk;

namespace Furore.Fhir.Sprinkler.Xunit.ClientUtilities
{
    public class BundleAssert
    {
        public static void CheckMinimumNumberOfElementsInBundle(Bundle bundle, int expected)
        {
            Assert.True(bundle.Entry.Count >= expected,
                string.Format("At least {0} elements expected, found {1}", expected, bundle.Entry.Count));
        }

        public static void CheckMaximumNumberOfElementsInBundle(Bundle bundle, int expected)
        {
            if (bundle.Entry.Count > expected)
            {
                FhirAssert.Fail("At most {0} elements expected, found {1}", expected, bundle.Entry.Count);
            }
        }

        public static void CheckConditionForAllElements(Bundle bundle, Predicate<Bundle.EntryComponent> condition,
            string conditionDescription)
        {
            if (!bundle.Entry.TrueForAll(condition))
            {
                FhirAssert.Fail("Not all entries respect condition: {0}", conditionDescription);
            }
        }

        public static void CheckConditionForResources(Bundle bundle, Func<Resource, bool> condition,
            string conditionDescription)
        {
            if (bundle.GetResources().Any(r => condition(r) == false))
            {
                FhirAssert.Fail("Not all entries respect condition: {0}", conditionDescription);
            }
        }

        public static void CheckConditionForResources<T>(Bundle bundle, Func<T, bool> condition,
            string conditionDescription)
            where T : Resource
        {
            if (bundle.GetResources().Cast<T>().Any(condition) == false)
            {
                FhirAssert.Fail("Not all entries respect condition: {0}", conditionDescription);
            }
        }

        public static void CheckTypeForResources<T>(Bundle bundle)
        {
            if (bundle.GetResources().Any(r => r.GetType() != typeof (T)))
            {
                FhirAssert.Fail("Not all resources are {0}", typeof (T).Name);
            }
        }

        public static void CheckContainedResources<T>(Bundle bundle, IEnumerable<string> ids)
        {
            CheckTypeForResources<T>(bundle);
            foreach (var id in ids)
            {
                if (!bundle.ContainsResource(id))
                {
                    FhirAssert.Fail("Expected resource with id = {0} not found in result", id);
                }
            }
        }

        public static void CheckConditionForResourcesWithIdInformation(Bundle bundle, Func<Resource, bool> condition,
            string conditionDescription)
        {
            foreach (var resource in bundle.GetResources())
            {
                if (!condition(resource))
                {
                    FhirAssert.Fail("Resource with id {0} does not respect condition {1}", resource.Id,
                        conditionDescription);
                }
            }
        }

        public static void CheckConditionForResourcesWithVersionIdInformation(Bundle bundle,
            Func<Resource, bool> condition, string conditionDescription)
        {
            foreach (var resource in bundle.GetResources())
            {
                if (!condition(resource))
                {
                    FhirAssert.Fail("Resource with id {0} does not respect condition {1}", resource.VersionId,
                        conditionDescription);
                }
            }
        }

        public static void CheckResourcesInOrder<T>(Bundle bundle, Func<Resource, T> keySelector,
            string conditionDescription = null) where T : IComparable<T>
        {
            Resource prevResource = null;
            foreach (var resource in bundle.GetResources())
            {
                if (prevResource != null)
                {
                    if (keySelector(prevResource).CompareTo(keySelector(resource)) > 0)
                    {
                        FhirAssert.Fail("Resources are not correctly ordered. First out of order resource is {0}",
                            resource.VersionId);
                    }
                }
                prevResource = resource;
            }
        }

        public static void CheckResourcesInReverseOrder<T>(Bundle bundle, Func<Resource, T> keySelector,
            string conditionDescription = null) where T : IComparable<T>
        {
            Resource prevResource = null;
            foreach (var resource in bundle.GetResources())
            {
                if (prevResource != null)
                {
                    if (keySelector(prevResource).CompareTo(keySelector(resource)) < 0)
                    {
                        FhirAssert.Fail("Resources are not correctly ordered. First out of order resource is {0}",
                            resource.VersionId);
                    }
                }
                prevResource = resource;
            }
        }

        public static void CheckConditionForAllElementsWithEntryInformation(Bundle bundle,
            Predicate<Bundle.EntryComponent> condition, Func<Bundle.EntryComponent, string> entryInformation,
            string conditionDescription)
        {
            foreach (var entry in bundle.Entry)
            {
                if (!condition(entry))
                {
                    FhirAssert.Fail("Entry {0} does not respect condition {1}", entryInformation(entry),
                        conditionDescription);
                }
            }
        }


        public static void ContainsAllVersionIds(Bundle bundle, IEnumerable<string> versionsList)
        {
            try
            {
                Assert.Equal(versionsList, bundle.GetResources().Select(r => r.Meta.VersionId));
            }
            catch (EqualException ex)
            {
                throw new TestFailedException("Bundle does not contain all expected versions", ex);
            }
        }

        public static void CheckBundleEmpty(Bundle bundle)
        {
            if (bundle.Entry.Count != 0)
                FhirAssert.Fail("Bundle should be empty");
        }
    }
}
