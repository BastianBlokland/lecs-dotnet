using System;
using System.Runtime.Intrinsics.X86;
using Xunit;

namespace Lecs.Tests.Attributes
{
    /// <summary>
    /// Fact that is only executed if Avx2 is supported on this platform
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class Avx2FactAttribute : FactAttribute
    {
        public Avx2FactAttribute()
        {
            if (!Avx2.IsSupported)
                this.Skip = "Needs avx2 support";
        }
    }
}
