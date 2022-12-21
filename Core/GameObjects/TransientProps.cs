using System;
using System.Runtime.InteropServices;



namespace OpenTemple.Core.GameObjects;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct TransientProps : IDisposable
{
    public int renderColor;
    public int renderColors;
    public int renderPalette;
    public int renderScale;
    public SparseArray<int>? renderAlpha;
    public int renderX;
    public int renderY;
    public int renderWidth;
    public int renderHeight;
    public int palette;
    public int color;
    public int colors;
    public int renderFlags;
    public int tempId;
    public int lightHandle;
    public SparseArray<int>? overlayLightHandles;
    public int findNode;
    public int animationHandle;
    public int grappleState;

    public void Dispose()
    {
        renderAlpha?.Dispose();
        renderAlpha = null;
        overlayLightHandles?.Dispose();
        overlayLightHandles = null;
    }

    public object? GetFieldValue(obj_f field)
    {
        switch (field)
        {
            case obj_f.render_color:
                return renderColor;
            case obj_f.render_colors:
                return renderColors;
            case obj_f.render_palette:
                return renderPalette;
            case obj_f.render_scale:
                return renderScale;
            case obj_f.render_alpha:
                return renderAlpha;
            case obj_f.render_x:
                return renderX;
            case obj_f.render_y:
                return renderY;
            case obj_f.render_width:
                return renderWidth;
            case obj_f.render_height:
                return renderHeight;
            case obj_f.palette:
                return palette;
            case obj_f.color:
                return color;
            case obj_f.colors:
                return colors;
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
            case obj_f.render_color:
                renderColor = (int) newValue;
                break;
            case obj_f.render_colors:
                renderColors = (int) newValue;
                break;
            case obj_f.render_palette:
                renderPalette = (int) newValue;
                break;
            case obj_f.render_scale:
                renderScale = (int) newValue;
                break;
            case obj_f.render_alpha:
                if (!ReferenceEquals(renderAlpha, newValue))
                {
                    renderAlpha?.Dispose();
                }

                renderAlpha = (SparseArray<int>) newValue;
                break;
            case obj_f.render_x:
                renderX = (int) newValue;
                break;
            case obj_f.render_y:
                renderY = (int) newValue;
                break;
            case obj_f.render_width:
                renderWidth = (int) newValue;
                break;
            case obj_f.render_height:
                renderHeight = (int) newValue;
                break;
            case obj_f.palette:
                palette = (int) newValue;
                break;
            case obj_f.color:
                color = (int) newValue;
                break;
            case obj_f.colors:
                colors = (int) newValue;
                break;
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