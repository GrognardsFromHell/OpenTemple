using System;
using System.Runtime.InteropServices;



namespace OpenTemple.Core.GameObjects;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct TransientProps : IDisposable
{
    public int renderFlags;
    public int tempId;
    public int lightHandle;
    public SparseArray<int>? overlayLightHandles;
    public int findNode;
    public int animationHandle;
    public int grappleState;

    public void Dispose()
    {
        overlayLightHandles?.Dispose();
        overlayLightHandles = null;
    }

    public object? GetFieldValue(obj_f field)
    {
        switch (field)
        {
            case obj_f.render_flags:
                return renderFlags;
            case obj_f.temp_id:
                return tempId;
            case obj_f.light_handle:
                return lightHandle;
            case obj_f.overlay_light_handles:
                return overlayLightHandles;
            case obj_f.find_node:
                return findNode;
            case obj_f.animation_handle:
                return animationHandle;
            case obj_f.grapple_state:
                return grappleState;
            default:
                throw new ArgumentOutOfRangeException(nameof(field), field, null);
        }
    }

    public void SetFieldValue(obj_f field, object newValue)
    {
        switch (field)
        {
            case obj_f.render_flags:
                renderFlags = (int) newValue;
                break;
            case obj_f.temp_id:
                tempId = (int) newValue;
                break;
            case obj_f.light_handle:
                lightHandle = (int) newValue;
                break;
            case obj_f.overlay_light_handles:
                if (!ReferenceEquals(overlayLightHandles, newValue))
                {
                    overlayLightHandles?.Dispose();
                }

                overlayLightHandles = (SparseArray<int>) newValue;
                break;
            case obj_f.find_node:
                findNode = (int) newValue;
                break;
            case obj_f.animation_handle:
                animationHandle = (int) newValue;
                break;
            case obj_f.grapple_state:
                grappleState = (int) newValue;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(field), field, null);
        }
    }
}