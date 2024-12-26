using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class CustomShaderGUI: ShaderGUI
{

	MaterialEditor editor;
	Object[] materials;
	MaterialProperty[] properties;


    void SetProperty(string name, float value)
    {
        MaterialProperty property = FindProperty(name, properties);
        property.floatValue = value;
    }

    void SetProperty(string name, string keyword,bool value)
    {
        SetProperty(name, value ? 1f : 0f);
        SetKeyword(keyword, value);
    }

    bool Clipping{
        set{
            SetProperty("_Clipping", "_CLIPPING", value);
        }
    }

    bool PremultiplyAlpha{
        set{
            SetProperty("_PremulAlpha", "_PREMULTIPLY_ALPHA", value);
        }
    }

    BlendMode SrcBlend{
        set{
            SetProperty("_SrcBlend", (float)value);
        }
    }

    BlendMode DstBlend{
        set{
            SetProperty("_DstBlend", (float)value);
        }
    }


    RenderQueue RenderQueue{
        set{
            SetProperty("_RenderQueue", (int)value);
        }
    }

    bool PressButton(string name){
        if(GUILayout.Button(name)){
            editor.RegisterPropertyChangeUndo(name);
            return true;
        }
        return false;
    }

    void OpaquePreset()
    {
        if(PressButton("Opaque")){
            Clipping = false;
            PremultiplyAlpha = false;
            SrcBlend = BlendMode.SrcAlpha;
            DstBlend = BlendMode.OneMinusSrcAlpha;
            ZWrite = true;
            RenderQueue = RenderQueue.Geometry;
        }
    }

    void ClipPreset()
    {
        if(PressButton("Clip")){
            Clipping = true;
            PremultiplyAlpha = false;
            SrcBlend = BlendMode.One;
            DstBlend = BlendMode.Zero;
            ZWrite = true;
            RenderQueue = RenderQueue.AlphaTest;
        }
    }

    void TransparentPreset()
    {
        if(PressButton("Transparent")){
            Clipping = false;
            PremultiplyAlpha = true;
            SrcBlend = BlendMode.One;   
            DstBlend = BlendMode.OneMinusSrcAlpha;
            ZWrite = false;
            RenderQueue = RenderQueue.Transparent;
        }
    }

    void FadePreset()
    {
        if(PressButton("Fade")){
            Clipping = false;
            PremultiplyAlpha = true;
            SrcBlend = BlendMode.SrcAlpha;   
            DstBlend = BlendMode.OneMinusSrcAlpha;
            ZWrite = false;
            RenderQueue = RenderQueue.Transparent;
        }
    }
    bool ZWrite{
        set{
            SetProperty("_ZWrite", value ? 1f : 0f);
        }
    }
    
    

    void SetKeyword(string keyword, bool enabled)
    {
        if (enabled)
        {
            foreach (Material material in materials)
            {
                material.EnableKeyword(keyword);
            }
        }
        else
        {
            foreach (Material material in materials)
            {
                material.DisableKeyword(keyword);
            }
        }
    }

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        base.OnGUI(materialEditor, properties);
        editor = materialEditor;
        materials = materialEditor.targets;
        this.properties = properties;

        OpaquePreset();
        ClipPreset();
        TransparentPreset();
        FadePreset();


    }
}
