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
    class InterfaceTextInput : InterfaceElement
    {
        public string value = "";
        private bool partialInFocus = false;
        private bool inFocus=false;
        //Infiniminer.KeyMap keyMap;

        public InterfaceTextInput()
        {
            //keyMap = new Infiniminer.KeyMap();
        }

        public InterfaceTextInput(Infiniminer.InfiniminerGame gameInstance)
        {
            //keyMap = new Infiniminer.KeyMap();
        }

        public InterfaceTextInput(Infiniminer.InfiniminerGame gameInstance, Infiniminer.PropertyBag pb)
        {
            _P = pb;
            //keyMap = new Infiniminer.KeyMap();
        }

        public override void OnMouseDown(MouseButton button, int x, int y)
        {
            if (enabled && size.Contains(x, y))
                partialInFocus = true;
            else
                inFocus = false;
        }

        public override void OnMouseUp(MouseButton button, int x, int y)
        {
            if (enabled && partialInFocus && size.Contains(x, y))
            {
                inFocus = true;
                _P.PlaySound(Infiniminer.InfiniminerSound.ClickLow);
            }
            partialInFocus = false;
        }

        public override void OnCharEntered(string e)
        {
            base.OnCharEntered(e);
            if ((int)e[0] < 32 || (int)e[0] > 126) //From space to tilde
                return; //Do nothing

            if (inFocus)
            {
                value += e;
            }
        }

        public override void OnKeyDown(Keys key)
        {
            base.OnKeyDown(key);
            if (inFocus)
            {
                if (key == Keys.Enter)
                {
                    inFocus = false;
                    _P.PlaySound(Infiniminer.InfiniminerSound.ClickHigh);
                }
                else if (key == Keys.Backspace &&value.Length>0)
                    value = value.Substring(0, value.Length - 1);
                /*else if (keyMap.IsKeyMapped(key))
                {
                    value += keyMap.TranslateKey(key, _SM.Keyboard.IsKeyDown(Keys.LeftShift) || _SM.Keyboard.IsKeyDown(Keys.RightShift));
                }*/
            }
        }

        public override void Render(RenderContext renderContext)
        {
            if (visible && size.Width > 0 && size.Height > 0)
            {
                Color4 drawColour = new Color4(1f, 1f, 1f, 1f);

                if (!enabled)
                    drawColour = new Color4(.7f, .7f, .7f, 1f);
                else if (!inFocus)
                    drawColour = new Color4(.85f, .85f, .85f, 1f);
                

                //Draw base background
                renderContext.Renderer2D.FillRectangle(size, drawColour);

                renderContext.Renderer2D.DrawString(Fonts.UiFont, value, new Vector2(size.X + size.Width / 2 - renderContext.Renderer2D.MeasureString(Fonts.UiFont, value).X / 2, size.Y + size.Height / 2 - 8), Color4.Black);

                if (text != "")
                {
                    //Draw text
                    renderContext.Renderer2D.DrawString(Fonts.UiFont, text, new Vector2(size.X, size.Y - 20), enabled ? Color4.White : new Color4(.7f, .7f, .7f, 1f));//drawColour);
                }

                /*spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.SaveState);
                spriteBatch.Draw(shade, new Rectangle(size.X, size.Y, size.Height, size.Height), drawColour);
                spriteBatch.Draw(shade, new Rectangle(size.X + size.Width - size.Height, size.Y, size.Height, size.Height), drawColour);

                //Draw line
                float sliderPercent = getPercent();
                int sliderPartialWidth = size.Height / 4;
                int midHeight = (int)(size.Height / 2) - 1;
                int actualWidth = size.Width - 2 * size.Height;
                int actualPosition = (int)(sliderPercent * actualWidth);
                spriteBatch.Draw(shade, new Rectangle(size.X, size.Y + midHeight, size.Width, 1), drawColour);

                //Draw slider
                spriteBatch.Draw(shade, new Rectangle(size.X + size.Height + actualPosition - sliderPartialWidth, size.Y + midHeight - sliderPartialWidth, size.Height / 2, size.Height / 2), drawColour);
                
                //Draw amount
                spriteBatch.DrawString(uiFont, (((float)(int)(value * 10)) / 10).ToString(), new Vector2(size.X, size.Y - 20), drawColour);
                */
            }
        }
    }
}
