//-------------------------------------------------------------------------
//  (c) 2015 Pervasive Digital LLC
//
//  This file is part of Scratch for .Net Micro Framework
//
//  "Scratch for .Net Micro Framework" is free software: you can 
//  redistribute it and/or modify it under the terms of the 
//  GNU General Public License as published by the Free Software 
//  Foundation, either version 3 of the License, or (at your option) 
//  any later version.
//
//  "Scratch for .Net Micro Framework" is distributed in the hope that
//  it will be useful, but WITHOUT ANY WARRANTY; without even the implied
//  warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See
//  the GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with "Scratch for .Net Micro Framework". If not, 
//  see <http://www.gnu.org/licenses/>.
//
//  This file has also been distributed previously under an Apache 2.0
//  license and you may elect to use that license with this file. This 
//  does not affect the licensing of any other file in Scratch for
//  .Net Micro Framework.
//-------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PervasiveDigital.Scratch.DeploymentHelper.Common
{
    public static class CollectionHelpers
    {
        public static void AddRange<T>(this ICollection<T> destination,
                                       IEnumerable<T> source)
        {
            foreach (T item in source)
            {
                destination.Add(item);
            }
        }

        public static int FindIndex<T>(this IEnumerable<T> items, Func<T, bool> func)
        {
            if (items == null)
                throw new ArgumentNullException("items");
            if (func == null)
                throw new ArgumentNullException("func");

            int retVal = 0;
            foreach (var item in items)
            {
                if (func(item)) 
                    return retVal;
                retVal++;
            }
            return -1;
        }
        
        public static int IndexOf<T>(this IEnumerable<T> items, T item) 
        { 
            return items.FindIndex(i => EqualityComparer<T>.Default.Equals(item, i)); 
        }
    }
}
