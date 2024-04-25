using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Text;
using LibreLancer;
using LibreLancer.Graphics.Vertices;
using LibreLancer.Graphics;

namespace Infiniminer
{
    public class SkyplaneEngine
    {
        InfiniminerGame gameInstance;
        PropertyBag _P;
        Texture2D texNoise;
        Random randGen;
        VertexPositionTexture[] vertices;
        private VertexBuffer vertexBuffer;
        
        public SkyplaneEngine(InfiniminerGame gameInstance)
        {
            this.gameInstance = gameInstance;
            
            // Generate a noise texture.
            randGen = new Random();
            texNoise = new Texture2D( gameInstance.RenderContext, 64, 64);
            uint[] noiseData = new uint[64*64];
            for (int i = 0; i < 64 * 64; i++)
                if (randGen.Next(32) == 0)
                    noiseData[i] = 0xFFFFFFFF;
                else
                    noiseData[i] = 0xFF000000;
            texNoise.SetData(noiseData);


            // Create our vertices.
            vertices = new VertexPositionTexture[6];
            vertices[0] = new VertexPositionTexture(new Vector3(-210, 100, -210), new Vector2(0, 0));
            vertices[1] = new VertexPositionTexture(new Vector3(274, 100, -210), new Vector2(1, 0));
            vertices[2] = new VertexPositionTexture(new Vector3(274, 100, 274), new Vector2(1, 1));
            vertices[3] = new VertexPositionTexture(new Vector3(-210, 100, -210), new Vector2(0, 0));
            vertices[4] = new VertexPositionTexture(new Vector3(274, 100, 274), new Vector2(1, 1));
            vertices[5] = new VertexPositionTexture(new Vector3(-210, 100, 274), new Vector2(0, 1));
            vertexBuffer = new VertexBuffer(gameInstance.RenderContext, typeof(VertexPositionTexture), 6);
            vertexBuffer.SetData(vertices);
        }

        public void Render(RenderContext renderContext)
        {
            // If we don't have _P, grab it from the current gameInstance.
            // We can't do this in the constructor because we are created in the property bag's constructor!
            if (_P == null)
                _P = gameInstance.propertyBag;

            // Draw the skybox.
            var effect = Effects.SkyPlane.Get();
            effect.Time = (float) gameInstance.TotalTime;
            effect.World = Matrix4x4.Identity;
            texNoise.SetFiltering(TextureFiltering.Nearest);
            texNoise.BindTo(0);
            renderContext.Shader = effect.Shader; 
            renderContext.Cull = false;
            renderContext.DepthEnabled = false;
            vertexBuffer.Draw(PrimitiveTypes.TriangleList, 0, vertices.Length / 3);
            renderContext.DepthEnabled = true;
        }
    }
}
