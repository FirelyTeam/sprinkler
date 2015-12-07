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
using System.Reflection;

namespace Furore.Fhir.Sprinkler.Framework.Utilities
{
    public static class AttributeHelper
    {
        public static IEnumerable<Type> GetTypesWithAttribute<T>(this Assembly assembly)
        {
            return assembly.GetTypes().Where(type => type.GetCustomAttributes(typeof(T), true).Length > 0);
        }
    }
}
