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
    public class Assert
    {
        public static void ContentTypePresent(FhirClient client)
        {
            if (String.IsNullOrEmpty(client.LastResponseDetails.ContentType))
                Assert.Fail("Mandatory Content-Type header missing");
        }

        public static void ResourceResponseConformsTo(FhirClient client, ResourceFormat format)
        {
            ValidResourceContentTypePresent(client);

            string type = client.LastResponseDetails.ContentType;
            if (ContentType.GetResourceFormatFromContentType(type) != format)
                Assert.Fail(String.Format("{0} is not acceptable when expecting {1}", type, format));
        }

        public static void BodyNotEmpty(FhirClient client)
        {
            if (client.LastResponseDetails.Body == null)
                Assert.Fail("Body is empty");
        }

        public static void LastModifiedPresent(FhirClient client)
        {
            if (String.IsNullOrEmpty(client.LastResponseDetails.LastModified))
                Assert.Fail("Mandatory Last-Modified header missing");
        }

        public static void ContentLocationPresentAndValid(FhirClient client)
        {
            if (String.IsNullOrEmpty(client.LastResponseDetails.ContentLocation))
                Assert.Fail("Mandatory Content-Location header missing");

            ContentLocationValidIfPresent(client);
        }

        public static void ContentLocationValidIfPresent(FhirClient client)
        {
            if (!String.IsNullOrEmpty(client.LastResponseDetails.ContentLocation))
            {
                var rl = new ResourceIdentity(client.LastResponseDetails.ContentLocation);

                if (rl.Id == null)
                    Assert.Fail("Content-Location does not have an id in it");

                if (rl.VersionId == null)
                    Assert.Fail("Content-Location is not a version-specific url");
            }
        }

        public static void LocationPresentAndValid(FhirClient client)
        {
            if (String.IsNullOrEmpty(client.LastResponseDetails.Location))
                Assert.Fail("Mandatory Location header missing");

            var rl = new ResourceIdentity(client.LastResponseDetails.Location);

            if (rl.Id == null)
                Assert.Fail("Location does not have an id in it");

            if (rl.VersionId == null)
                Assert.Fail("Location is not a version-specific url");
        }

        public static void Success(FhirClient client, Action action)
        {
            try
            {
                action();
            }
            catch (FhirOperationException foe)
            {
                Assert.Fail(foe, String.Format("Call failed (http result {0})", client.LastResponseDetails.Result));
            }
        }

        public static void Skip(string message = null)
        {
            throw new TestFailedException(TestOutcome.Skipped, message);
        }

        public static void SkipWhen(bool condition, string message = null)
        {
            if (condition) Skip(message);
        }

        /// <summary>
        ///     Use this AssertFail if you want to examine the result afterwards (typically: an OperationOutcome).
        /// </summary>
        /// <param name="client"></param>
        /// <param name="action"></param>
        /// <param name="result"></param>
        /// <param name="expected"></param>
        /// <returns></returns>
        public static void Fails<TOut>(FhirClient client, Func<TOut> action, out TOut result,
            HttpStatusCode? expected = null)
        {
            try
            {
                result = action();
                Assert.Fail("Unexpected success result (" + client.LastResponseDetails.Result + ")");
            }
            catch (FhirOperationException)
            {
                result = default(TOut);
                if (expected != null && client.LastResponseDetails.Result != expected)
                    Assert.Fail("Expected http result {0} but got {1}", expected, client.LastResponseDetails.Result);
            }
        }

        public static void Fails(FhirClient client, Action action, HttpStatusCode? expected = null)
        {
            try
            {
                action();
                Assert.Fail("Unexpected success result (" + client.LastResponseDetails.Result + ")");
            }
            catch (FhirOperationException)
            {
                if (expected != null && client.LastResponseDetails.Result != expected)
                    Assert.Fail(String.Format("Expected http result {0} but got {1}", expected,
                        client.LastResponseDetails.Result));
            }
        }

        public static void ValidResourceContentTypePresent(FhirClient client)
        {
            ContentTypePresent(client);

            if (!ContentType.IsValidResourceContentType(client.LastResponseDetails.ContentType))
                Assert.Fail("expected xml or json content type, but received " +
                                client.LastResponseDetails.ContentType);
            if (client.LastResponseDetails.CharacterEncoding != Encoding.UTF8)
                Assert.Fail("content type does not specify UTF8");
        }

        public static void ValidBundleContentTypePresent(FhirClient client)
        {
            ContentTypePresent(client);

            if (!ContentType.IsValidBundleContentType(client.LastResponseDetails.ContentType))
                Assert.Fail("expected Atom xml or json bundle content type, but received " +
                                client.LastResponseDetails.ContentType);
            if (client.LastResponseDetails.CharacterEncoding != Encoding.UTF8)
                Assert.Fail("content type does not specify UTF8");
        }

        public static void EntryIdsArePresentAndAbsoluteUrls(Bundle b)
        {
            if (b.Entries.Any(e => e.Id == null || e.SelfLink == null))
                Assert.Fail("Some id/selflinks in the bundle are null");

            if (!b.Entries.All(e => e.Id.IsAbsoluteUri && e.SelfLink.IsAbsoluteUri))
                Assert.Fail("Some id/selflinks in the bundle are relative");
        }

        public static void CorrectNumberOfResults(int expected, int actual, string messageFormat = "")
        {
            string formattedMessage = String.Format(messageFormat, expected, actual);
            switch (actual.CompareTo(expected))
            {
                case -1:
                    Assert.Fail("Too little results: " + formattedMessage);
                    return;
                case 1:
                    Assert.Fail("Too many results: " + formattedMessage);
                    return;
                default:
                    return;
            }
        }

        internal static void HttpOk(FhirClient client)
        {
            if (client.LastResponseDetails.Result != HttpStatusCode.OK)
                Assert.Fail("Got status code " + client.LastResponseDetails.Result +
                                ". Did you install the standard test-set?");
        }

        internal static void HasAllForwardNavigationLinks(Bundle history)
        {
            if (history.Links.FirstLink == null ||
                history.Links.NextLink == null ||
                history.Links.LastLink == null)
                Assert.Fail("Expecting first, next and last link to be present");
        }

        public static void IsTrue(bool assertion, string message)
        {
            if (!assertion)
                Assert.Fail(message);
        }

        public static void Fail(string message)
        {
            throw new TestFailedException(message);
        }

        public static void Fail(string message, params object[] args)
        {
            throw new TestFailedException(string.Format(message, args));
        }

        public static void Fail(Exception inner, string message = "Exception caught")
        {
            throw new TestFailedException(message, inner);
        }
    }
}