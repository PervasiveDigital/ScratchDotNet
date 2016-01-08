using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PervasiveDigital.Scratch.DeploymentHelper.Properties
{
    internal sealed partial class Settings
    {
#if CLASSROOM
        public bool ClassroomMode => true;
#elif CLOUD
        public bool ClassroomMode => false;
#else
#error You must build either the cloud or classroom variant
#endif
    }
}
