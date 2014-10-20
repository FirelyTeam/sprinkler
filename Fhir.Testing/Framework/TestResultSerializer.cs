using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Xsl;

namespace Sprinkler.Framework
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
