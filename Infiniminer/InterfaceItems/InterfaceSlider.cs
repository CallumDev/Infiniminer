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
    class InterfaceSlider : InterfaceElement
    {
        //public float sliderPercent = 0f;
        public float minVal = 0f;
        public float maxVal = 1f;
        private bool sliding = false;
        public float value = 0;
        public bool integers = false;

        private InfiniminerGame game;

        public InterfaceSlider(Infiniminer.InfiniminerGame gameInstance)
        {
            this.game = game;
        }

        public InterfaceSlider(Infiniminer.InfiniminerGame gameInstance, Infiniminer.PropertyBag pb)
        {
            this.game = game;
            _P = pb;
        }

        public void setValue(float newVal)
        {
            if (integers)
                value = (int)Math.Round((double)newVal);
            else
                value = newVal;
        }

        public float getPercent()
        {
            return (value - minVal) / (maxVal - minVal);
        }

        public override void OnMouseDown(MouseButton button, int x, int y)
        {
            if (size.Contains(x, y))
            {
                sliding = true;
                Update(x, y);
            }
        }

        public void Update()
        {
            if (game.Mouse.IsButtonDown(MouseButtons.Left))
                Update(game.Mouse.X, game.Mouse.Y);
            else
                sliding = false;
        }

        public void Update(int x, int y)
        {
            if (sliding)
            {
                if (!game.Mouse.IsButtonDown(MouseButtons.Left))
                    sliding = false;
                else
                {
                    if (x < size.X + size.Height)
                        value = minVal;
                    else if (x > size.X + size.Width - size.Height)
                        value = maxVal;
                    else
                    {
                        int xMouse = x - size.X - size.Height;
                        int xMax = size.Width - 2 * size.Height;
                        float sliderPercent = (float)xMouse / (float)xMax;
                        if (integers)
                            value = (int)Math.Round((sliderPercent * (maxVal - minVal)) + minVal);
                        else
                            value = sliderPercent * (maxVal - minVal) + minVal;
                        if (value < minVal)
                            value = minVal;
                        else if (value > maxVal)
                            value = maxVal;
                    }
                }
            }
        }

        public override void Render(RenderContext renderContext)
        {
            Update();

            if (visible&&size.Width>0&&size.Height>0)
            {
                Color4 drawColour = new Color4(1f, 1f, 1f, 1f);

                if (!enabled)
                {
                    drawColour = new Color4(.5f, .5f, .5f, 1f);
                }
                //Draw end boxes
                renderContext.Renderer2D.FillRectangle( new Rectangle(size.X, size.Y, size.Height, size.Height), drawColour);
                renderContext.Renderer2D.FillRectangle( new Rectangle(size.X + size.Width - size.Height, size.Y, size.Height, size.Height), drawColour);
                
                //Draw line
                float sliderPercent = getPercent();
                int sliderPartialWidth = size.Height / 4;
                int midHeight = (int)(size.Height/2)-1;
                int actualWidth = size.Width - 2 * size.Height;
                int actualPosition = (int)(sliderPercent * actualWidth);
                renderContext.Renderer2D.FillRectangle( new Rectangle(size.X, size.Y + midHeight, size.Width, 1), drawColour);

                //Draw slider
                renderContext.Renderer2D.FillRectangle( new Rectangle(size.X + size.Height + actualPosition - sliderPartialWidth, size.Y + midHeight - sliderPartialWidth, size.Height / 2, size.Height / 2), drawColour);
                if (text != "")
                {
                    //Draw text
                    renderContext.Renderer2D.DrawString(Fonts.UiFont, text, new Vector2(size.X, size.Y - 36), drawColour);
                }
                //Draw amount
                renderContext.Renderer2D.DrawString(Fonts.UiFont, (((float)(int)(value * 10)) / 10).ToString(), new Vector2(size.X, size.Y - 20), drawColour); 
           
            }
        }
    }
}
