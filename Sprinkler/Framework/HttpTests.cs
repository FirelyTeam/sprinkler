/* 
 * Copyright (c) 2014, Furore (info@furore.com) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.github.com/furore-fhir/sprinkler/master/LICENSE
 */

using System;
using System.Linq;
using System.Net;
using System.Text;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;

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

            string type = client.LastResponseDetails.ContentType;
            if (ContentType.GetResourceFormatFromContentType(type) != format)
                TestResult.Fail(String.Format("{0} is not acceptable when expecting {1}", type, format));
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

        /// <summary>
        ///     Use this AssertFail if you want to examine the result afterwards (typically: an OperationOutcome).
        /// </summary>
        /// <param name="client"></param>
        /// <param name="action"></param>
        /// <param name="result"></param>
        /// <param name="expected"></param>
        /// <returns></returns>
        public static void AssertFail<TOut>(FhirClient client, Func<TOut> action, out TOut result,
            HttpStatusCode? expected = null)
        {
            result = default(TOut);
            try
            {
                result = action();
                TestResult.Fail("Unexpected success result (" + client.LastResponseDetails.Result + ")");
            }
            catch (FhirOperationException)
            {
                if (expected != null && client.LastResponseDetails.Result != expected)
                    TestResult.Fail(String.Format("Expected http result {0} but got {1}", expected,
                        client.LastResponseDetails.Result));
            }
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
                TestResult.Fail("expected xml or json content type, but received " +
                                client.LastResponseDetails.ContentType);
            if (client.LastResponseDetails.CharacterEncoding != Encoding.UTF8)
                TestResult.Fail("content type does not specify UTF8");
        }

        public static void AssertValidBundleContentTypePresent(FhirClient client)
        {
            AssertContentTypePresent(client);

            if (!ContentType.IsValidBundleContentType(client.LastResponseDetails.ContentType))
                TestResult.Fail("expected Atom xml or json bundle content type, but received " +
                                client.LastResponseDetails.ContentType);
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

        public static void AssertCorrectNumberOfResults(int expected, int actual, string messageFormat = "")
        {
            string formattedMessage = String.Format(messageFormat, expected, actual);
            switch (actual.CompareTo(expected))
            {
                case -1:
                    TestResult.Fail("Too little results: " + formattedMessage);
                    return;
                case 1:
                    TestResult.Fail("Too many results: " + formattedMessage);
                    return;
                default:
                    return;
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