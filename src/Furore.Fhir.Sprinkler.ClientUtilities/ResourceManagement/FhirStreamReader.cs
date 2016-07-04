using System;
using System.IO;
using System.Text;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;

namespace Furore.Fhir.Sprinkler.ClientUtilities.ResourceManagement
{
    public class FhirStreamReader : StreamReader
    {
        public FhirStreamReader(Stream stream) : base(stream)
        {
        }

        public FhirStreamReader(Stream stream, bool detectEncodingFromByteOrderMarks) : base(stream, detectEncodingFromByteOrderMarks)
        {
        }

        public FhirStreamReader(Stream stream, Encoding encoding) : base(stream, encoding)
        {
        }

        public FhirStreamReader(Stream stream, Encoding encoding, bool detectEncodingFromByteOrderMarks) : base(stream, encoding, detectEncodingFromByteOrderMarks)
        {
        }

        public FhirStreamReader(Stream stream, Encoding encoding, bool detectEncodingFromByteOrderMarks, int bufferSize) : base(stream, encoding, detectEncodingFromByteOrderMarks, bufferSize)
        {
        }

        public FhirStreamReader(Stream stream, Encoding encoding, bool detectEncodingFromByteOrderMarks, int bufferSize, bool leaveOpen) : base(stream, encoding, detectEncodingFromByteOrderMarks, bufferSize, leaveOpen)
        {
        }

        public FhirStreamReader(string path) : base(path)
        {
        }

        public FhirStreamReader(string path, bool detectEncodingFromByteOrderMarks) : base(path, detectEncodingFromByteOrderMarks)
        {
        }

        public FhirStreamReader(string path, Encoding encoding) : base(path, encoding)
        {
        }

        public FhirStreamReader(string path, Encoding encoding, bool detectEncodingFromByteOrderMarks) : base(path, encoding, detectEncodingFromByteOrderMarks)
        {
        }

        public FhirStreamReader(string path, Encoding encoding, bool detectEncodingFromByteOrderMarks, int bufferSize) : base(path, encoding, detectEncodingFromByteOrderMarks, bufferSize)
        {
        }

        public Resource ReadResource()
        {
            var data = ReadToEnd();
            if (FhirParser.ProbeIsJson(data))
            {
                return FhirParser.ParseResourceFromJson(data);
            }
            else if (FhirParser.ProbeIsXml(data))
            {
                return FhirParser.ParseResourceFromXml(data);
            }
            else
            {
                throw new FormatException("Data is neither Json nor Xml");
            }
        }
    }
}