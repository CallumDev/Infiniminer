using System;
using System.Collections.Generic;

using System.Text;
using System.Diagnostics;
using LibreLancer;
using LibreLancer.Graphics;
using Infiniminer;

namespace InterfaceItems
{
    class InterfaceElement
    {
        public bool visible = false;
        public bool enabled = false;
        public string text = "";
        //public Vector2 position = Vector2.Zero;
        public Rectangle size = new Rectangle();
        public Infiniminer.PropertyBag _P;

        public InterfaceElement()
        {
        }

        public InterfaceElement(Infiniminer.InfiniminerGame gameInstance, Infiniminer.PropertyBag pb)
        {
            _P = pb;
        }

        public virtual void OnCharEntered(string e)
        {
        }

        public virtual void OnKeyDown(Keys key)
        {
        }

        public virtual void OnKeyUp(Keys key)
        {
        }

        public virtual void OnMouseDown(MouseButton button, int x, int y)
        {
        }

        public virtual void OnMouseUp(MouseButton button, int x, int y)
        {
        }

        public virtual void OnMouseScroll(int scrollWheelValue)
        {
        }

        public virtual void Render(RenderContext graphicsDevice)
        {

        }
    }
}
