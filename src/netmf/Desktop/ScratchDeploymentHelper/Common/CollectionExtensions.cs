//-----------------------------------------------------------
//  (c) 2015 Pervasive Digital LLC
//
// This work is licensed under the Creative Commons 
//    Attribution-ShareAlike 4.0 International License.
// http://creativecommons.org/licenses/by-sa/4.0/
//
//-----------------------------------------------------------
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
