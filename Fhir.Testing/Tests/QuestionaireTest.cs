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
using Hl7.Fhir.Model;
using Sprinkler.Framework;

namespace Sprinkler.Tests
{
    [SprinklerTestModule("Questionnaire")]
    internal class QuestionaireTest : SprinklerTestClass
    {
        [SprinklerTest("QU01", "Download a completed questionnaire")]
        public void DownloadCompletedTest()
        {
            var completedQuestionaire = new Questionnaire
            {
                Status = Questionnaire.QuestionnaireStatus.Completed,
                Authored = "2013-08-13"
            };
            Client.Create(completedQuestionaire);

            Bundle bundle = Client.Search<Questionnaire>(new[] {"status=completed"});
            Uri id = bundle.Entries.First().Id;

            Questionnaire questionnaire = Client.Read<Questionnaire>(id).Resource;
        }

        [SprinklerTest("QU02", "Reupload as a template")]
        public void ReuploadAsTemplate()
        {
            // There  are no templates. So we have to create one:
            Bundle bundle = Client.Search<Questionnaire>(new[] {"status=completed"});
            Uri id = bundle.Entries.First().Id;

            Questionnaire questionnaire = Client.Read<Questionnaire>(id).Resource;
            questionnaire.Status = Questionnaire.QuestionnaireStatus.Published;
            Client.Create(questionnaire);
        }

        private void DisplayAvailableStatusses()
        {
            Bundle bundle = Client.Search<Questionnaire>();

            IEnumerable<string> statusses =
                from entry in bundle.Entries.ByResourceType<Questionnaire>()
                select entry.Resource.Status.ToString();

            foreach (string status in statusses.Distinct())
            {
                Console.WriteLine("-" + status); // TODO (er): where do you want to write here?
            }
        }

        [SprinklerTest("QU03", "Download a template questionnaire")]
        public void DownloadTemplate()
        {
            Bundle bundle = Client.Search<Questionnaire>(new[] {"status=published"});
            Uri id = bundle.Entries.First().Id;

            Questionnaire questionnaire = Client.Read<Questionnaire>(id).Resource;
        }

        [SprinklerTest("QU04", "Upload a new questionnaire")]
        public void UploadATemplate()
        {
            Bundle bundle = Client.Search<Questionnaire>(new[] {"status=published"});
            Uri id = bundle.Entries.First().Id;

            Questionnaire questionnaire = Client.Read<Questionnaire>(id).Resource;

            questionnaire.Author = new ResourceReference();
            questionnaire.Author.Url = new Uri("http://sprinkler.furore.com/questionnaire");
            questionnaire.Name = new CodeableConcept("http://sprinkler.furore.com/questionnaire",
                "SprinklerQuestionnaire", "Sprinkler Questionnaire");
            Client.Create(questionnaire);
        }
    }
}