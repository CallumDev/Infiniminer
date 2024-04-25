using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using LibreLancer;
using LibreLancer.Graphics;
using StateMasher;

namespace Infiniminer.States
{
    public class TitleState : State
    {
        Texture2D texMenu;
        Rectangle drawRect;
        string nextState = null;

        public override void OnEnter(string oldState)
        {
            _SM.RelativeMouseMode = false;
            _SM.CursorKind = CursorKind.Arrow;

            texMenu = _SM.Content.LoadTexture("menus/tex_menu_title.png");

            drawRect = new Rectangle(_SM.Width / 2 - 1024 / 2,
                                     _SM.Height / 2 - 768 / 2,
                                     1024,
                                     1024);
        }

        public override void OnLeave(string newState)
        {

        }

        public override string OnUpdate(double elapsed)
        {
            _SM.TextInputEnabled = false;
            // Do network stuff.
            (_SM as InfiniminerGame).UpdateNetwork(elapsed);

            return nextState;
        }

        public override void OnRenderAtEnter()
        {

        }

        public override void OnRenderAtUpdate(double elapsed)
        {
            _SM.RenderContext.Renderer2D.DrawImageStretched(texMenu, drawRect, Color4.White);
        }

        public override void OnKeyDown(Keys key)
        {
            if (key == Keys.Escape)
            {
                _SM.Exit();
            }
        }

        public override void OnKeyUp(Keys key)
        {

        }

        public override void OnMouseDown(MouseButton button, int x, int y)
        {
            nextState = "Infiniminer.States.ServerBrowserState";
            _P.PlaySound(InfiniminerSound.ClickHigh);
        }

        public override void OnMouseUp(MouseButton button, int x, int y)
        {

        }

        public override void OnMouseScroll(int scrollDelta)
        {

        }
    }
}
