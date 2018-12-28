using System;
using System.Runtime.Intrinsics.X86;
using Xunit;

namespace Lecs.Tests.Attributes
{
    /// <summary>
    /// Theory that is only executed if Avx2 is supported on this platform
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class Avx2TheoryAttribute : TheoryAttribute
    {
        public Avx2TheoryAttribute()
        {
            if (!Avx2.IsSupported)
                this.Skip = "Needs avx2 support";
        }
    }
}
