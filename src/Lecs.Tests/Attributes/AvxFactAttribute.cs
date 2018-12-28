using System;
using System.Runtime.Intrinsics.X86;
using Xunit;

namespace Lecs.Tests.Attributes
{
    /// <summary>
    /// Fact that is only executed if Avx is supported on this platform
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class AvxFactAttribute : FactAttribute
    {
        public AvxFactAttribute()
        {
            if (!Avx.IsSupported)
                this.Skip = "Needs avx support";
        }
    }
}
