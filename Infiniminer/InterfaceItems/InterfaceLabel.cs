using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using LibreLancer;
using LibreLancer.Graphics;

namespace InterfaceItems
{
    class InterfaceLabel : InterfaceElement
    {
        public InterfaceLabel()
        {
        }

        public InterfaceLabel(Infiniminer.InfiniminerGame gameInstance)
        {
        }

        public InterfaceLabel(Infiniminer.InfiniminerGame gameInstance, Infiniminer.PropertyBag pb)
        {
            _P = pb;
        }

        public override void Render(RenderContext graphicsDevice)
        {
            if (visible&&text!="")
            {
                graphicsDevice.Renderer2D.DrawString("VT323", 20, text, new Vector2(size.X, size.Y), Color4.White);
            }
        }
    }
}
