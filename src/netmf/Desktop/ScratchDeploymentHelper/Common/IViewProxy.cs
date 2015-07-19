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
    public interface IViewProxy<T> : IDisposable
    {
        T ViewSource { get; set; }
    }

    public interface IViewProxyWithParent<TParent, TItem> : IViewProxy<TItem>
    {
        TParent Parent { get; set; }
    }
}
