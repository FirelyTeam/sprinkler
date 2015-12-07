using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Furore.Fhir.Sprinkler.Framework.Framework;
using Hl7.Fhir.Model;

namespace Furore.Fhir.Sprinkler.TestSet
{
    public class AllDomainResourcesTestGenerator : IDynamicTestGenerator
    {
        public IEnumerable<MethodInfo> GetTestMethods()
        {
            var allDomainTypes = typeof (Patient).Assembly.GetTypes().Where(t => t.BaseType == typeof (DomainResource)).ToList();
            foreach (Type allDomainType in allDomainTypes)
            {
               yield return typeof(AllDomainResourcesDynamicTest).GetMethod("TestSomeResource").MakeGenericMethod(allDomainType);
            }
        }
    }
}