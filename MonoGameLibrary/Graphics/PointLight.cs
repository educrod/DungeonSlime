using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGameLibrary.Graphics;

public class PointLight
{
    /// <summary>
    /// The position of the light in world space
    /// </summary>
    public Vector2 Position { get; set; }

    /// <summary>
    /// The color tint of the light
    /// </summary>
    public Color Color { get; set; } = Color.White;

    /// <summary>
    /// The render target that holds the shadow map
    /// </summary>
    public RenderTarget2D ShadowBuffer { get; set; }


    /// <summary>
    /// The radius of the light in pixels
    /// </summary>
    public int Radius { get; set; } = 250;

    public PointLight()
    {
        var viewPort = Core.GraphicsDevice.Viewport;
        ShadowBuffer = new RenderTarget2D(Core.GraphicsDevice, viewPort.Width, viewPort.Height, false, SurfaceFormat.Color,  DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
    }

    public void DrawShadowBuffer(List<ShadowCaster> shadowCasters)
    {
        Core.GraphicsDevice.SetRenderTarget(ShadowBuffer);
        Core.GraphicsDevice.Clear(Color.White);
    
        Core.ShadowHullMaterial.SetParameter("LightPosition", Position);
        var screenSize = new Vector2(ShadowBuffer.Width, ShadowBuffer.Height);
        Core.SpriteBatch.Begin(
                effect: Core.ShadowHullMaterial.Effect, 
                rasterizerState: RasterizerState.CullNone
                );
        foreach (var caster in shadowCasters)
        {
            for (var i = 0; i < caster.Points.Count; i++)
            {
                var a = caster.Position + caster.Points[i];
                var b = caster.Position + caster.Points[(i + 1) % caster.Points.Count];
    
                var aToB = (b - a) / screenSize;
                var packed = PackVector2_SNorm(aToB);
                Core.SpriteBatch.Draw(Core.Pixel, a, packed);
            }
        }
    
        Core.SpriteBatch.End();
    }

    public static void DrawShadows(
        List<PointLight> pointLights,
        List<ShadowCaster> shadowCasters)
    {
        foreach (var light in pointLights)
        {
            light.DrawShadowBuffer(shadowCasters);
        }
    }

    public static Color PackVector2_SNorm(Vector2 vec)  
    {  
        // Clamp to [-1, 1)  
        vec = Vector2.Clamp(vec, new Vector2(-1f), new Vector2(1f - 1f / 32768f));  
    
        short xInt = (short)(vec.X * 32767f); // signed 16-bit  
        short yInt = (short)(vec.Y * 32767f);  
    
        byte r = (byte)((xInt >> 8) & 0xFF);  
        byte g = (byte)(xInt & 0xFF);  
        byte b = (byte)((yInt >> 8) & 0xFF);  
        byte a = (byte)(yInt & 0xFF);  
    
        return new Color(r, g, b, a);  
    }

    public static void Draw(SpriteBatch spriteBatch, List<PointLight> pointLights, Texture2D normalBuffer)
    {
        Core.PointLightMaterial.SetParameter("NormalBuffer", normalBuffer);
        spriteBatch.Begin(
            effect: Core.PointLightMaterial.Effect,
            blendState: BlendState.Additive,
            sortMode: SpriteSortMode.Immediate
            );

        foreach (var light in pointLights)
        {
            Core.PointLightMaterial.SetParameter("ShadowBuffer", light.ShadowBuffer);
            var diameter = light.Radius * 2;
            var rect = new Rectangle((int)(light.Position.X - light.Radius), (int)(light.Position.Y - light.Radius), diameter, diameter);
            spriteBatch.Draw(normalBuffer, rect, light.Color);
        }

        spriteBatch.End();
    }

}
