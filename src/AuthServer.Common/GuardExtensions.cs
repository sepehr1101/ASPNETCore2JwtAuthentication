using System;

namespace AuthServer.Common
{
    public static class GuardExtensions
    {
        /// <summary>
        /// Checks if the argument is null.
        /// </summary>
        public static void CheckArgumentIsNull(this object o, string name)
        {
            if (o == null)
                throw new ArgumentNullException(name);
        }

        public static void CheckStringIsNullOrWhiteSpace(this string str)
        {
            if(String.IsNullOrWhiteSpace(str))
                throw new ArgumentException(String.Concat(str ," is Null"));
        }
    }
}