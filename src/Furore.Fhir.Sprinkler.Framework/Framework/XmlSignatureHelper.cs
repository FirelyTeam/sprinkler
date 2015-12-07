﻿/* 
 * Copyright (c) 2014, Furore (info@furore.com) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.github.com/furore-fhir/sprinkler/master/LICENSE
 */

using System;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;

namespace Furore.Fhir.Sprinkler.Framework.Framework
{
    namespace Framework
    {
        // This code contains parts of the code found at
        // http://www.wiktorzychla.com/2012/12/interoperable-xml-digital-signatures-c_20.html

        public class XmlSignatureHelper
        {
            public static bool VerifySignature(string xml)
            {
                if (xml == null) throw new ArgumentNullException("xml");

                var doc = new XmlDocument();
                doc.PreserveWhitespace = true;
                doc.LoadXml(xml);

                // If there's no signature => return that we are "valid"
                XmlNode signatureNode = findSignatureElement(doc);
                if (signatureNode == null) return true;

                var signedXml = new SignedXml(doc);
                signedXml.LoadXml((XmlElement) signatureNode);

                //var x509Certificates = signedXml.KeyInfo.OfType<KeyInfoX509Data>();
                //var certificate = x509Certificates.SelectMany(cert => cert.Certificates.Cast<X509Certificate2>()).FirstOrDefault();

                //if (certificate == null) throw new InvalidOperationException("Signature does not contain a X509 certificate public key to verify the signature");
                //return signedXml.CheckSignature(certificate, true);

                return signedXml.CheckSignature();
            }


            private static XmlNode findSignatureElement(XmlDocument doc)
            {
                XmlNodeList signatureElements = doc.DocumentElement.GetElementsByTagName("Signature",
                    "http://www.w3.org/2000/09/xmldsig#");
                if (signatureElements.Count == 1)
                    return signatureElements[0];
                if (signatureElements.Count == 0)
                    return null;
                throw new InvalidOperationException("Document has multiple xmldsig Signature elements");
            }


            public static bool IsSigned(string xml)
            {
                if (xml == null) throw new ArgumentNullException("xml");

                // First, a quick check, before reading the full document
                if (!xml.Contains("Signature")) return false;

                var doc = new XmlDocument();
                doc.LoadXml(xml);
                return findSignatureElement(doc) != null;
            }


            public static string Sign(string xml, X509Certificate2 certificate)
            {
                if (xml == null) throw new ArgumentNullException("xml");
                if (certificate == null) throw new ArgumentNullException("certificate");
                if (!certificate.HasPrivateKey)
                    throw new ArgumentException("certificate", "Certificate should have a private key");

                var doc = new XmlDocument();

                doc.PreserveWhitespace = true;
                doc.LoadXml(xml);

                var signedXml = new SignedXml(doc);
                signedXml.SigningKey = certificate.PrivateKey;

                // Attach certificate KeyInfo
                var keyInfoData = new KeyInfoX509Data(certificate);
                var keyInfo = new KeyInfo();
                keyInfo.AddClause(keyInfoData);
                signedXml.KeyInfo = keyInfo;

                // Attach transforms
                var reference = new Reference("");
                reference.AddTransform(new XmlDsigEnvelopedSignatureTransform(false));
                reference.AddTransform(new XmlDsigExcC14NTransform(false));
                signedXml.AddReference(reference);

                // Compute signature
                signedXml.ComputeSignature();
                XmlElement signatureElement = signedXml.GetXml();

                // Add signature to bundle
                doc.DocumentElement.AppendChild(doc.ImportNode(signatureElement, true));

                return doc.OuterXml;
            }
        }
    }
}