using System.Collections.Generic;

namespace OpenTemple.Core.GFX.Materials;

public class DepthStencilSpec
{
        
    // Enables depth testing
    public bool depthEnable = true;

    // Enables writing to the depth buffer
    public bool depthWrite = true;

    // Comparison function used for depth test
    public ComparisonFunc depthFunc = ComparisonFunc.Less;

    private sealed class DepthEnableDepthWriteDepthFuncEqualityComparer : IEqualityComparer<DepthStencilSpec>
    {
        public bool Equals(DepthStencilSpec x, DepthStencilSpec y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (ReferenceEquals(x, null))
            {
                return false;
            }

            if (ReferenceEquals(y, null))
            {
                return false;
            }

            if (x.GetType() != y.GetType())
            {
                return false;
            }

            return x.depthEnable == y.depthEnable && x.depthWrite == y.depthWrite && x.depthFunc == y.depthFunc;
        }

        public int GetHashCode(DepthStencilSpec obj)
        {
            unchecked
            {
                var hashCode = obj.depthEnable.GetHashCode();
                hashCode = (hashCode * 397) ^ obj.depthWrite.GetHashCode();
                hashCode = (hashCode * 397) ^ (int) obj.depthFunc;
                return hashCode;
            }
        }
    }

    public static IEqualityComparer<DepthStencilSpec> DepthEnableDepthWriteDepthFuncComparer { get; } = new DepthEnableDepthWriteDepthFuncEqualityComparer();
}