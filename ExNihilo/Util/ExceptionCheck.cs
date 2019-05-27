using System;
using System.Runtime.CompilerServices;

namespace ExNihilo.Util
{
    public static class ExceptionCheck
    {
        public static void AssertCondition(bool condition, string message = "", [CallerFilePath] string file = null,
            [CallerMemberName] string caller = null, [CallerLineNumber] int lineNumber = 0)
        {
            if (!condition)
                throw new Exception("Assertion Failed in " + caller + " (line " + lineNumber + " of " + file + ")\n\t" + message);
        }

        public static void Fail(string message = "", [CallerFilePath] string file = null,
            [CallerMemberName] string caller = null, [CallerLineNumber] int lineNumber = 0)
        {
            throw new Exception("Failed in " + caller + " (line " + lineNumber + " of " + file + ")\n\t" + message);
        }
    }
}
