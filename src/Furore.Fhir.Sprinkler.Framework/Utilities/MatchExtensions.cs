/* 
 * Copyright (c) 2014, Furore (info@furore.com) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.github.com/furore-fhir/sprinkler/master/LICENSE
 */

using System.Collections.Generic;
using System.Linq;

namespace Furore.Fhir.Sprinkler.Framework.Utilities
{
    public static class MatchExtensions
    {

        public static bool HasMatchFor(this IEnumerable<string> matches, string code)
        {
            return matches.Any(m => code.StartsWith(m));

        }
    }
}
