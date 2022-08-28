using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epicweb.Optimizely.QuickNavExtension
{
    public static class StringExtentions
    {
        public static string ReplaceAfter(this string orignal, string replace, string with)
        {
            int index = orignal.IndexOf(replace);
            if (index >= 0)
            {
                orignal = orignal.Substring(0, index);
                orignal +=  replace;
            }
            return orignal.Replace(replace, with);
        }
    }
}
