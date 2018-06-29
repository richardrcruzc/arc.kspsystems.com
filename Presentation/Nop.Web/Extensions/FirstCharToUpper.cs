using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Web.Extensions
{
    public class StringExtend
    {
        public static string FirstCharToUpper(string input)
        {
            if (String.IsNullOrEmpty(input) || String.IsNullOrWhiteSpace(input))
                return input;
               // throw new ArgumentException("ARGH!");
            return input.First().ToString().ToUpper() + input.Substring(1);
        }
    }
}
