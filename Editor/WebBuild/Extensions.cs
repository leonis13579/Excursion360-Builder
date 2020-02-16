using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Packages.tour_creator.Editor.WebBuild
{
    static class Extensions
    {
        public static string GetExportedId(this State state)
        {
            return "state_" + state.GetInstanceID();
        }
    }
}
