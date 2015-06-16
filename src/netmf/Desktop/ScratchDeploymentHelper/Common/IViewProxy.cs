//-----------------------------------------------------------
//  (c) 2015 Pervasive Digital LLC
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
