/* 
 * Copyright (c) 2014, Furore (info@furore.com) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.github.com/furore-fhir/sprinkler/master/LICENSE
 */

namespace Furore.Fhir.Sprinkler.Framework.Framework
{
    public static class TestResultSerializer
    {
        /*public static void SerializeTo(this TestResults results, TextWriter writer, TextReader stylesheet = null)
        {
            var serializer = new XmlSerializer(typeof(TestResults));
            if (stylesheet == null)
            {
                serializer.Serialize(writer, results);
            }
            else
            {
                SerializeAndTransform(results, writer, stylesheet, serializer);
            }
        }

        private static void SerializeAndTransform(TestResults results, TextWriter writer, TextReader stylesheet, XmlSerializer serializer)
        {
            var outputXml = new StringWriter();
            serializer.Serialize(outputXml, results);
            var xslt = new XslCompiledTransform();
            using (var xmlReader = XmlReader.Create(stylesheet))
            {
                xslt.Load(xmlReader);
            }
            using (var inputXml = new StringReader(outputXml.ToString()))
            {
                var writerSettings = xslt.OutputSettings;
                xslt.Transform(XmlReader.Create(inputXml), XmlWriter.Create(writer, writerSettings));
            }
        }
         */
    }
          
}
