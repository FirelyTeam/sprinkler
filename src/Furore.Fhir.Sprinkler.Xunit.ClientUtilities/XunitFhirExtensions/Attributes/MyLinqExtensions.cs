using System.Collections.Generic;
using System.Linq;

namespace Furore.Fhir.Sprinkler.Xunit.ClientUtilities.XunitFhirExtensions.Attributes
{
    public static class MyLinqExtensions
    {
        public static IEnumerable<T[]> BatchArray<T>(
            this IEnumerable<T> source, int batchSize)
        {
            return Batch<T>(source, batchSize).Select(x => x.ToArray());
        }

        public static IEnumerable<IEnumerable<T>> Batch<T>(
            this IEnumerable<T> source, int batchSize)
        {
            using (var enumerator = source.GetEnumerator())
                while (enumerator.MoveNext())
                    yield return YieldBatchElements(enumerator, batchSize);
        }

        private static IEnumerable<T> YieldBatchElements<T>(
            IEnumerator<T> source, int batchSize)
        {
            for (int i = 0; i < batchSize; i++)
            {
                yield return source.Current;
                if (i < (batchSize - 1) && (source.MoveNext() == false))
                    break;

            }
        }

        public static IEnumerable<T> UnionAll<T>(
           this IEnumerable<T> source, IEnumerable<T> values)
        {
            foreach (T t in source)
            {
                yield return t;
            }
            foreach (T t in values)
            {
                yield return t;
            }
        }
    }
}