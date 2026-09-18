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
        Core.GraphicsDevice.Clear(Color.Black);
    
        Core.ShadowHullMaterial.SetParameter("LightPosition", Position);
        var screenSize = new Vector2(ShadowBuffer.Width, ShadowBuffer.Height);
        Core.SpriteBatch.Begin(
                effect: Core.ShadowHullMaterial.Effect, 
                rasterizerState: RasterizerState.CullNone
                );
        foreach (var caster in shadowCasters)
        {
            var posA = caster.A;
            // TODO: pack the (B-A) vector into the color channel.
            Core.SpriteBatch.Draw(Core.Pixel, posA, Color.White);
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


    public static void Draw(SpriteBatch spriteBatch, List<PointLight> pointLights, Texture2D normalBuffer)
    {
        Core.PointLightMaterial.SetParameter("NormalBuffer", normalBuffer);
        spriteBatch.Begin(
            effect: Core.PointLightMaterial.Effect,
            blendState: BlendState.Additive
            );

        foreach (var light in pointLights)
        {
            var diameter = light.Radius * 2;
            var rect = new Rectangle((int)(light.Position.X - light.Radius), (int)(light.Position.Y - light.Radius), diameter, diameter);
            spriteBatch.Draw(normalBuffer, rect, light.Color);
        }

        spriteBatch.End();
    }

}
