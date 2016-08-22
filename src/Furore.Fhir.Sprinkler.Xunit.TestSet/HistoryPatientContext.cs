using System;
using System.Collections.Generic;
using System.Linq;
using Furore.Fhir.Sprinkler.Xunit.ClientUtilities.XunitFhirExtensions.ClassFixtures;
using Furore.Fhir.Sprinkler.XunitRunner.FhirExtensions;
using Hl7.Fhir.Model;

namespace Furore.Fhir.Sprinkler.Xunit.TestSet
{
    public class HistoryPatientContext :  DisposableTestDependencyContext<Patient>, ISetupAndTeardown
    {
        private readonly IList<string> versions = new List<string>();

        public IEnumerable<string> PatientVersions
        {
            get { return versions.ToList(); }
        }

        public Patient Patient
        {
            get { return Dependency; }
        }

        public DateTimeOffset CreationDate { get; private set; }

        public HistoryPatientContext()
        {
            CreationDate = DateTimeOffset.Now;
            Patient patient = CreatePatient();
            if (patient.Meta != null && patient.Meta.LastUpdated != null)
            {
                CreationDate = patient.Meta.LastUpdated.Value;
            }
            patient = this.Client.Create(patient);
            versions.Add(patient.VersionId);

            patient.Telecom.Add(new ContactPoint
            {
                System = ContactPoint.ContactPointSystem.Email,
                Value = "info@furore.com"
            });

            patient = Client.Update(patient, true);
            versions.Add(patient.VersionId);

            patient = Client.Update(patient, true);
            versions.Add(patient.VersionId);

            this.Dependency = patient;
        }

        private Patient CreatePatient(string family = "Adams", params string[] given)
        {
            var p = new Patient();
            var n = new HumanName();
            foreach (string g in given)
            {
                n.WithGiven(g);
            }

            n.AndFamily(family);
            p.Name = new List<HumanName>();
            p.Name.Add(n);
            return p;
        }

        public void Setup()
        {

        }

        public void Teardown()
        {
        }
    }
}