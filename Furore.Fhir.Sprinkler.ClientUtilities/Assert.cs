using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;

namespace Furore.Fhir.Sprinkler.ClientUtilities
{
    public class Assert
    {
        public static void ContentTypePresent(FhirClient client)
        {
            if (String.IsNullOrEmpty(client.LastResult.GetHeader("Content-Type").FirstOrDefault()))
                Fail("Mandatory Content-Type header missing");
        }

        public static void ResourceResponseConformsTo(FhirClient client, ResourceFormat format)
        {
            ValidResourceContentTypePresent(client);

            string type = client.LastResult.GetHeader("Content-Type").FirstOrDefault();
            //TODO: temporary hack. Must be removed when ContentType.GetResourceFormatFromContentType will correctly treat the charset element.
            int charsetFormat = type.IndexOf(";", 0, StringComparison.Ordinal);
            type = type.Substring(0, charsetFormat);
            if (ContentType.GetResourceFormatFromContentType(type) != format)
                Assert.Fail(String.Format("{0} is not acceptable when expecting {1}", type, format));
        }

        public static void BodyNotEmpty(FhirClient client)
        {
            if (client.LastResult.GetBody() == null)
                Fail("Body is empty");
        }

        public static void LastModifiedPresent(FhirClient client)
        {
            if (client.LastResult.LastModified == null)
                Fail("Mandatory Last-Modified header missing");
        }

        public static void ContentLocationPresentAndValid(FhirClient client)
        {
            if (String.IsNullOrEmpty(client.LastResult.GetHeader("Content-Location").FirstOrDefault()))
                Fail("Mandatory Content-Location header missing");

            ContentLocationValidIfPresent(client);
        }

        public static void ContentLocationValidIfPresent(FhirClient client)
        {
            if (!String.IsNullOrEmpty(client.LastResult.GetHeader("Content-Location").FirstOrDefault()))
            {
                var rl = new ResourceIdentity(client.LastResult.GetHeader("Content-Location").FirstOrDefault());

                if (rl.Id == null)
                    Fail("Content-Location does not have an id in it");

                if (rl.VersionId == null)
                    Fail("Content-Location is not a version-specific url");
            }
        }

        public static void LocationPresentAndValid(FhirClient client)
        {
            if (String.IsNullOrEmpty(client.LastResult.Location))
                Fail("Mandatory Location header missing");

            var rl = new ResourceIdentity(client.LastResult.Location);

            if (rl.Id == null)
                Fail("Location does not have an id in it");

            if (rl.VersionId == null)
                Fail("Location is not a version-specific url");
        }

        public static void Success(FhirClient client, Action action)
        {
            try
            {
                action();
            }
            catch (FhirOperationException foe)
            {
                Fail(foe, String.Format("Call failed (http result {0})", client.LastResult.Status));
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
                Fail("Unexpected success result (" + client.LastResult.Status + ")");
            }
            catch (FhirOperationException)
            {
                result = default(TOut);
                if (expected != null && client.LastResult.Status != ((int)expected).ToString())
                    Fail("Expected http result {0} but got {1}", expected, client.LastResult.Status);
            }
        }

        public static void Fails(FhirClient client, Action action, HttpStatusCode? expected = null)
        {
            try
            {
                action();
                Fail("Unexpected success result (" + client.LastResult.Status + ")");
            }
            catch (FhirOperationException)
            {
                if (expected != null && client.LastResult.Status != ((int)expected).ToString()) //HttpStatusCode To String???
                    Assert.Fail(String.Format("Expected http result {0} but got {1}", expected,
                         client.LastResult.Status));
            }
        }

        public static void ValidResourceContentTypePresent(FhirClient client)
        {
            ContentTypePresent(client);

            if (!ContentType.IsValidResourceContentType(client.LastResult.GetHeader("Content-Type").FirstOrDefault()))
                Fail("expected xml or json content type, but received " +
                                client.LastResult.GetHeader("Content-Type").FirstOrDefault());
            //if (client.LastResponseDetails.CharacterEncoding != Encoding.UTF8)
            //    Assert.Fail("content type does not specify UTF8");
        }

        public static void ValidBundleContentTypePresent(FhirClient client)
        {
            ContentTypePresent(client);

            if (!ContentType.IsValidBundleContentType(client.LastResult.GetHeader("Content-Type").FirstOrDefault()))
                Fail("expected Atom xml or json bundle content type, but received " +
                                client.LastResult.GetHeader("Content-Type").FirstOrDefault());
            //if (client.LastResponseDetails.CharacterEncoding != Encoding.UTF8)
            //    Assert.Fail("content type does not specify UTF8");
        }

        public static void EntryIdsArePresentAndAbsoluteUrls(Bundle b)
        {
            if (b.Entry.Any(e => e.Resource != null && (e.Resource.Id == null && e.Resource.VersionId == null)))
                Fail("Some id/versionId's in the bundle are null");

            // todo: DSTU2 this no longer works in DSTU2
            //if (!b.Entry. All(e => (e.Resource != null && e.Resource.ResourceIdentity().IsAbsoluteUri ))
            //    Assert.Fail("Some id/versionId's in the bundle are relative");
        }

        public static void CorrectNumberOfResults(int expected, int actual, string messageFormat = "")
        {
            string formattedMessage = String.Format(messageFormat, expected, actual);
            switch (actual.CompareTo(expected))
            {
                case -1:
                    Fail("Too little results: " + formattedMessage);
                    return;
                case 1:
                    Fail("Too many results: " + formattedMessage);
                    return;
                default:
                    return;
            }
        }

        public static void HttpOk(FhirClient client)
        {
            if (client.LastResult.Status != ((int)HttpStatusCode.OK).ToString())
                Fail("Got status code " + client.LastResult.Status +
                                ". Did you install the standard test-set?");
        }

        public static void AssertStatusCode(FhirClient client, HttpStatusCode expected)
        {
            if (client.LastResult.Status != ((int)expected).ToString())
            {
                Fail("Expected http result {0} but got {1}", expected, client.LastResult.Status);
            }
        }

        public static void AssertStatusCode(FhirClient client, params HttpStatusCode[] expected)
        {
            string[] expectedStatuses = expected.Select(e => ((int)e).ToString()).ToArray();
            if (!expectedStatuses.Contains(client.LastResult.Status))
            {
                Fail("Received ttp result {0} is not one of the expected statues {1}", client.LastResult.Status, string.Join(",", expectedStatuses));
            }
        }

        public static void HasAllForwardNavigationLinks(Bundle history)
        {
            if (history.FirstLink == null ||
                history.NextLink == null ||
                history.LastLink == null)
                Fail("Expecting first, next and last link to be present");
        }

        public static void IsTrue(bool assertion, string message)
        {
            if (!assertion)
                Fail(message);
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
