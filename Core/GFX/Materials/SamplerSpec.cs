using System.Collections.Generic;

namespace OpenTemple.Core.GFX.Materials
{

    /**
     * Describes the state of one of the texture samplers.
     */
    public class SamplerSpec
    {
        public TextureFilterType minFilter = TextureFilterType.Linear;
        public TextureFilterType magFilter = TextureFilterType.Linear;
        public TextureFilterType mipFilter = TextureFilterType.Linear;

        public TextureAddress addressU = TextureAddress.Wrap;
        public TextureAddress addressV = TextureAddress.Wrap;

        private sealed class SamplerSpecEqualityComparer : IEqualityComparer<SamplerSpec>
        {
            public bool Equals(SamplerSpec x, SamplerSpec y)
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

                return x.minFilter == y.minFilter && x.magFilter == y.magFilter && x.mipFilter == y.mipFilter && x.addressU == y.addressU && x.addressV == y.addressV;
            }

            public int GetHashCode(SamplerSpec obj)
            {
                unchecked
                {
                    var hashCode = (int) obj.minFilter;
                    hashCode = (hashCode * 397) ^ (int) obj.magFilter;
                    hashCode = (hashCode * 397) ^ (int) obj.mipFilter;
                    hashCode = (hashCode * 397) ^ (int) obj.addressU;
                    hashCode = (hashCode * 397) ^ (int) obj.addressV;
                    return hashCode;
                }
            }
        }

        public static IEqualityComparer<SamplerSpec> SamplerSpecComparer { get; } = new SamplerSpecEqualityComparer();
    }

}