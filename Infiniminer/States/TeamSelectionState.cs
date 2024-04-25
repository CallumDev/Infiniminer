using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using StateMasher;
using LibreLancer;
using LibreLancer.Graphics;
using SharpDX.Direct2D1;
using Point = LibreLancer.Point;
using Rectangle = LibreLancer.Rectangle;

namespace Infiniminer.States
{
    public class TeamSelectionState : State
    {
        Texture2D texMenu;
        Rectangle drawRect;
        string nextState = null;
        bool canCancel = false;

        ClickRegion[] clkTeamMenu = new ClickRegion[2] {
	        new ClickRegion(new Rectangle(229,156,572,190), "red"), 
	        new ClickRegion(new Rectangle(135,424,761,181), "blue")//,
            //new ClickRegion(new Rectangle(0,0,0,0), "cancel")
        };

        public override void OnEnter(string oldState)
        {
            _SM.RelativeMouseMode = false;
            _SM.CursorKind = CursorKind.Arrow;

            texMenu = _SM.Content.LoadTexture("menus/tex_menu_team");

            drawRect = new Rectangle(_SM.Width / 2 - 1024 / 2,
                                     _SM.Height / 2 - 768 / 2,
                                     1024,
                                     1024);
            
            if (oldState == "Infiniminer.States.MainGameState")
                canCancel = true;
        }

        public override void OnLeave(string newState)
        {

        }

        public override string OnUpdate(double gameTime)
        {
            _SM.TextInputEnabled = false;
            // Do network stuff.
            (_SM as InfiniminerGame).UpdateNetwork(gameTime);

            return nextState;
        }

        public override void OnRenderAtEnter()
        {

        }

        public void QuickDrawText( string text, int y, Color4 color)
        {
            var sb = _SM.RenderContext.Renderer2D;
            sb.DrawString(Fonts.UiFont, text, new Vector2(_SM.Width / 2 - sb.MeasureString(Fonts.UiFont, text).X / 2, drawRect.Y + y), color);
        }

        public override void OnRenderAtUpdate(double gameTime)
        {
            int redTeamCount = 0, blueTeamCount = 0;
            foreach (Player p in _P.playerList.Values)
            {
                if (p.Team == PlayerTeam.Red)
                    redTeamCount += 1;
                else if (p.Team == PlayerTeam.Blue)
                    blueTeamCount += 1;
            }
            var sb = _SM.RenderContext.Renderer2D;
            sb.DrawImageStretched(texMenu, drawRect, Color4.White);
            QuickDrawText( "" + redTeamCount + " PLAYERS", 360, _P.red);//Defines.IM_RED);
            QuickDrawText("" + blueTeamCount + " PLAYERS", 620, _P.blue);//Defines.IM_BLUE);
        }

        public override void OnKeyDown(Keys key)
        {
            if (key == Keys.Escape && canCancel)
                nextState = "Infiniminer.States.MainGameState";
        }

        public override void OnKeyUp(Keys key)
        {

        }

        public override void OnMouseDown(MouseButton button, int x, int y)
        {
            x -= drawRect.X;
            y -= drawRect.Y;
            switch (ClickRegion.HitTest(clkTeamMenu, new Point(x, y)))
            {
                case "red":
                    if (_P.playerTeam == PlayerTeam.Red && canCancel)
                        nextState = "Infiniminer.States.MainGameState";
                    else
                    {
                        _P.SetPlayerTeam(PlayerTeam.Red);
                        nextState = "Infiniminer.States.ClassSelectionState";
                    }
                    _P.PlaySound(InfiniminerSound.ClickHigh);
                    break;
                case "blue":
                    if (_P.playerTeam == PlayerTeam.Blue && canCancel)
                        nextState = "Infiniminer.States.MainGameState";
                    else
                    {
                        _P.SetPlayerTeam(PlayerTeam.Blue);
                        nextState = "Infiniminer.States.ClassSelectionState";
                    }
                    _P.PlaySound(InfiniminerSound.ClickHigh);
                    break;
                case "cancel":
                    if (canCancel)
                    {
                        nextState = "Infiniminer.States.MainGameState";
                        _P.PlaySound(InfiniminerSound.ClickHigh);
                    }
                    break;
            }
        }

        public override void OnMouseUp(MouseButton button, int x, int y)
        {

        }

        public override void OnMouseScroll(int scrollDelta)
        {

        }
    }
}
