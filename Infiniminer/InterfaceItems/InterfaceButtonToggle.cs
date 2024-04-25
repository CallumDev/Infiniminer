using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Numerics;
using StateMasher;
using Infiniminer;
using LibreLancer;
using LibreLancer.Graphics;

namespace InterfaceItems
{
    class InterfaceButtonToggle : InterfaceElement
    {
        private bool midClick = false;
        public bool clicked = false;
        public string offText = "Off";
        public string onText = "On";

        public InterfaceButtonToggle()
        {
        }

        public InterfaceButtonToggle(Infiniminer.InfiniminerGame gameInstance)
        {
        }

        public InterfaceButtonToggle(Infiniminer.InfiniminerGame gameInstance, Infiniminer.PropertyBag pb)
        {
            _P = pb;
        }

        public override void OnMouseDown(MouseButton button, int x, int y)
        {
            if (enabled && size.Contains(x, y))
            {
                midClick = true;
            }
            else
                midClick = false;
        }

        public override void OnMouseUp(MouseButton button, int x, int y)
        {
            if (enabled && midClick && size.Contains(x, y))
            {
                clicked = !clicked;
                _P.PlaySound(Infiniminer.InfiniminerSound.ClickLow);
            }
            midClick = false;
        }

        public override void Render(RenderContext graphicsDevice)
        {
            if (visible && size.Width > 0 && size.Height > 0)
            {
                Color4 drawColour = new Color4(1f, 1f, 1f, 1f);

                if (!enabled)
                    drawColour = new Color4(.7f, .7f, .7f, 1f);
                else if (midClick)
                    drawColour = new Color4(.85f, .85f, .85f, 1f);
                
                
                //Draw base button
                graphicsDevice.Renderer2D.FillRectangle(size, drawColour);

                //Draw button text
                string dispText = offText;
                if (clicked)
                    dispText = onText;

                graphicsDevice.Renderer2D.DrawString(Fonts.UiFont, dispText, new Vector2(size.X + size.Width / 2 - graphicsDevice.Renderer2D.MeasureString(Fonts.UiFont, dispText).X / 2, size.Y + size.Height / 2 - 8), Color4.Black);

                if (text != "")
                {
                    //Draw text
                    graphicsDevice.Renderer2D.DrawString(Fonts.UiFont, text, new Vector2(size.X, size.Y - 20), enabled ? Color4.White : new Color4(.7f, .7f, .7f, 1f));//drawColour);
                }
            }
        }
    }
}
