using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace JaszCore.Utils
{
    public static class MethodUtils
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static string GetCurrentMethod()
        {
            var st = new StackTrace();
            var sf = st.GetFrame(1);

            return sf.GetMethod().Name;
        }
    }
}
