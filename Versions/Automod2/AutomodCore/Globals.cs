using System;
using System.Collections.Generic;
using System.Text;

namespace Automod
{
    static class Globals
    {
        public static Dictionary<ulong, string> vcCreators;
        public static bool deleteGuard;

        static Globals()
        {
            vcCreators = new Dictionary<ulong, string>();
            deleteGuard = false;
        }
    }
}
