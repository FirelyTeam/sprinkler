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
using Hl7.Fhir.Support;
using Hl7.Fhir.Rest;
using Hl7.Fhir.Model;

namespace Sprinkler.Framework
{
    public class HttpTests
    {
        public static void AssertContentTypePresent(FhirClient client)
        {
            if (String.IsNullOrEmpty(client.LastResponseDetails.ContentType))
                TestResult.Fail("Mandatory Content-Type header missing");
        }

        public static void AssertResourceResponseConformsTo(FhirClient client, ResourceFormat format)
        {
            AssertValidResourceContentTypePresent(client);

            var type = client.LastResponseDetails.ContentType;
            if (ContentType.GetResourceFormatFromContentType(type) != format)
                TestResult.Fail(String.Format("{0} is not acceptable when expecting {1}", type, format.ToString()));
        }

        public static void AssertBodyNotEmpty(FhirClient client)
        {
            if (client.LastResponseDetails.Body == null)
                TestResult.Fail("Body is empty");
        }

        public static void AssertLastModifiedPresent(FhirClient client)
        {
            if (String.IsNullOrEmpty(client.LastResponseDetails.LastModified))
                TestResult.Fail("Mandatory Last-Modified header missing");
        }

        public static void AssertContentLocationPresentAndValid(FhirClient client)
        {
            if (String.IsNullOrEmpty(client.LastResponseDetails.ContentLocation))
                TestResult.Fail("Mandatory Content-Location header missing");

            AssertContentLocationValidIfPresent(client);
        }

        public static void AssertContentLocationValidIfPresent(FhirClient client)
        {
            if (!String.IsNullOrEmpty(client.LastResponseDetails.ContentLocation))
            {
                var rl = new ResourceIdentity(client.LastResponseDetails.ContentLocation);

                if (rl.Id == null)
                    TestResult.Fail("Content-Location does not have an id in it");

                if (rl.VersionId == null)
                    TestResult.Fail("Content-Location is not a version-specific url");
            }
        }


        public static void AssertLocationPresentAndValid(FhirClient client)
        {
            if (String.IsNullOrEmpty(client.LastResponseDetails.Location))
                TestResult.Fail("Mandatory Location header missing");

            var rl = new ResourceIdentity(client.LastResponseDetails.Location);

            if (rl.Id == null)
                TestResult.Fail("Location does not have an id in it");

            if (rl.VersionId == null)
                TestResult.Fail("Location is not a version-specific url");
        }

        public static void AssertSuccess(FhirClient client, Action action)
        {
            try
            {
                action();
            }
            catch (FhirOperationException foe)
            {
                TestResult.Fail(foe, String.Format("Call failed (http result {0})", client.LastResponseDetails.Result));
            }
        }

        public static void AssertNoContent(FhirClient client)
        {
            if (client.LastResponseDetails.Result != HttpStatusCode.NoContent)
                TestResult.Fail(string.Format("Expected HTTP status {0}, received {1}", HttpStatusCode.NoContent, client.LastResponseDetails.Result));
        }

        public static void AssertFail(FhirClient client, Action action, HttpStatusCode? expected = null)
        {
            try
            {
                action();
                TestResult.Fail("Unexpected success result (" + client.LastResponseDetails.Result + ")");
            }
            catch (FhirOperationException)
            {
                if (expected != null && client.LastResponseDetails.Result != expected)
                    TestResult.Fail(String.Format("Expected http result {0} but got {1}", expected,
                                            client.LastResponseDetails.Result));
            }
        }

        public static void AssertValidResourceContentTypePresent(FhirClient client)
        {
            AssertContentTypePresent(client);

            if (!ContentType.IsValidResourceContentType(client.LastResponseDetails.ContentType))
                TestResult.Fail("expected xml or json content type, but received " + client.LastResponseDetails.ContentType);
            if (client.LastResponseDetails.CharacterEncoding != Encoding.UTF8)
                TestResult.Fail("content type does not specify UTF8");
        }

        public static void AssertValidBundleContentTypePresent(FhirClient client)
        {
            AssertContentTypePresent(client);

            if (!ContentType.IsValidBundleContentType(client.LastResponseDetails.ContentType))
                TestResult.Fail("expected Atom xml or json bundle content type, but received " + client.LastResponseDetails.ContentType);
            if (client.LastResponseDetails.CharacterEncoding != Encoding.UTF8)
                TestResult.Fail("content type does not specify UTF8");
        }

        public static void AssertEntryIdsArePresentAndAbsoluteUrls(Bundle b)
        {
            if (b.Entries.Any(e => e.Id == null || e.SelfLink == null))
                TestResult.Fail("Some id/selflinks in the bundle are null");

            if (!b.Entries.All(e => e.Id.IsAbsoluteUri && e.SelfLink.IsAbsoluteUri))
                TestResult.Fail("Some id/selflinks in the bundle are relative");
        }

        public static void AssertCorrectNumberOfResults(int expected, int actual, string message = "")
        {
            switch (actual.CompareTo(expected))
            {
                case -1: TestResult.Fail("Too little results for " + message); return;
                case 1: TestResult.Fail("Too many results for " + message); return;
                default: return;
            }
        }

        internal static void AssertHttpOk(FhirClient client)
        {
            if (client.LastResponseDetails.Result != HttpStatusCode.OK)
                TestResult.Fail("Got status code " + client.LastResponseDetails.Result +
                    ". Did you install the standard test-set?");
        }

        internal static void AssertHasAllForwardNavigationLinks(Bundle history)
        {
            if (history.Links.FirstLink == null ||
                history.Links.NextLink == null ||
                history.Links.LastLink == null)
                TestResult.Fail("Expecting first, next and last link to be present");
        }

    }
}
