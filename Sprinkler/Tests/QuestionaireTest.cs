using Hl7.Fhir.Model;
using Sprinkler.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sprinkler.Tests
{
    [SprinklerTestModule("Questionnaire")]
    class QuestionaireTest : SprinklerTestClass
    {
        [SprinklerTest("QU01", "Download a completed questionnaire")]
        public void DownloadCompletedTest()
        {
            Bundle bundle = client.Search<Questionnaire>(new string[] { "status=Completed" });
            Uri id = bundle.Entries.First().Id;

            Questionnaire questionnaire = client.Read<Questionnaire>(id).Resource;
        }

        private void DisplayAvailableStatusses()
        {
            Bundle bundle = client.Search<Questionnaire>();

            IEnumerable<string> statusses =
            from entry in bundle.Entries.ByResourceType<Questionnaire>()
            select entry.Resource.Status.ToString();

                    foreach (string status in statusses.Distinct())
                    {
                        Console.WriteLine("-" + status);
                    }

        }

        [SprinklerTest("QU02", "Download a template questionnaire")]
        public void DownloadTemplate()
        {
            Bundle bundle = client.Search<Questionnaire>(new string[] { "status=Published" });
            Uri id = bundle.Entries.First().Id;

            Questionnaire questionnaire = client.Read<Questionnaire>(id).Resource;
        }

        [SprinklerTest("QU04", "Upload a new questionnaire")]
        public void UploadATemplate()
        {
            Bundle bundle = client.Search<Questionnaire>(new string[] { "status=Published" });
            Uri id = bundle.Entries.First().Id;

            Questionnaire questionnaire = client.Read<Questionnaire>(id).Resource;

            questionnaire.Author = new ResourceReference();
            questionnaire.Author.Url = new Uri("http://sprinkler.furore.com/questionnaire");
            questionnaire.Name = new CodeableConcept("http://sprinkler.furore.com/questionnaire", "SprinklerQuestionnaire", "Sprinkler Questionnaire");
            client.Create(questionnaire);
        }
        
    }
}
