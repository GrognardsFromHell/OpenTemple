namespace OpenTemple.Core.GFX.Materials;

public class RasterizerSpec
{
    // How are tesellated vertices filled? Default is solid.
    public bool wireframe = false;

    // Indicates whether front-facing or back-facing triangles
    // should be culled or neither. In D3D, by default, the
    // indices need to be clock wise for the front
    public CullMode cullMode = CullMode.Back;

    // Enable or disable scissor culling
    public bool scissor = true;

    protected bool Equals(RasterizerSpec other)
    {
        return wireframe == other.wireframe && cullMode == other.cullMode && scissor == other.scissor;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj.GetType() != GetType())
        {
            return false;
        }

        return Equals((RasterizerSpec) obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = wireframe.GetHashCode();
            hashCode = (hashCode * 397) ^ (int) cullMode;
            hashCode = (hashCode * 397) ^ scissor.GetHashCode();
            return hashCode;
        }
    }

    public static bool operator ==(RasterizerSpec left, RasterizerSpec right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(RasterizerSpec left, RasterizerSpec right)
    {
        return !Equals(left, right);
    }
}