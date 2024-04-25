using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using StateMasher;
using LibreLancer;
using LibreLancer.Graphics;
using Point = LibreLancer.Point;
using Rectangle = LibreLancer.Rectangle;

namespace Infiniminer.States
{
    public class ServerBrowserState : State
    {
        Texture2D texMenu;
        Rectangle drawRect;
        string nextState = null;
        List<ServerInformation> serverList = new List<ServerInformation>();
        List<int> descWidths;
        bool directConnectIPEnter = false;
        string directConnectIP = "";
        //KeyMap keyMap;

        ClickRegion[] clkMenuServer = new ClickRegion[3] {
            new ClickRegion(new Rectangle(0,713,425,42), "direct"),
            new ClickRegion(new Rectangle(456,713,262,42),"settings"),
	        new ClickRegion(new Rectangle(763,713,243,42), "refresh")
        };

        public override void OnEnter(string oldState)
        {
            _SM.RelativeMouseMode = false;
            _SM.CursorKind = CursorKind.Arrow;
            (_SM as InfiniminerGame).ResetPropertyBag();
            _P = _SM.propertyBag;

            texMenu = _SM.Content.LoadTexture("menus/tex_menu_server");

            drawRect = new Rectangle(_SM.Width / 2 - 1024 / 2,
                                     _SM.Height / 2 - 768 / 2,
                                     1024,
                                     1024);

            //keyMap = new KeyMap();
            
            serverList = (_SM as InfiniminerGame).EnumerateServers(0.5f);
        }

        public override void OnLeave(string newState)
        {

        }

        public override string OnUpdate(double gameTime)
        {
            _SM.TextInputEnabled = directConnectIPEnter;
            return nextState;
        }

        public override void OnRenderAtEnter()
        {

        }

        public override void OnRenderAtUpdate(double gameTime)
        {
            descWidths = new List<int>();
            var spriteBatch = _SM.RenderContext.Renderer2D;
            spriteBatch.DrawImageStretched(texMenu, drawRect, Color4.White);

            int drawY = 80;
            foreach (ServerInformation server in serverList)
            {
                if (drawY < 660)
                {
                    int textWidth = (int)(spriteBatch.MeasureString(Fonts.UiFont, server.GetServerDesc()).X);
                    descWidths.Add(textWidth+30);
                    spriteBatch.DrawString(Fonts.UiFont, server.GetServerDesc(), new Vector2(_SM.Width / 2 - textWidth / 2, drawRect.Y + drawY), !server.lanServer && server.numPlayers == server.maxPlayers ? new Color4(0.7f, 0.7f, 0.7f, 1f) : Color4.White);
                    drawY += 25;
                }
            }

            spriteBatch.DrawString(Fonts.UiFont, Defines.INFINIMINER_VERSION, new Vector2(10, _SM.Height - 20), Color4.White);

            if (directConnectIPEnter)
                spriteBatch.DrawString(Fonts.UiFont, "ENTER IP: " + directConnectIP, new Vector2(drawRect.X + 30, drawRect.Y + 690), Color4.White);
        }
        
        public override void OnTextEntered(string e)
        {
            foreach (var c in e)
                OnCharEntered(c);
        }

        void OnCharEntered(char e)
        {
            if ((int)e < 32 || (int)e > 126) //From space to tilde
                return; //Do nothing

            //Only respond if entering an ip and control is not pressed
            if (directConnectIPEnter && !(_SM.Keyboard.IsKeyDown(Keys.LeftControl) || _SM.Keyboard.IsKeyDown(Keys.RightControl)))
            {
                directConnectIP += e;
            }
        }

        public override void OnKeyDown(Keys key)
        {
            if (directConnectIPEnter)
            {
                if (key == Keys.Escape)
                {
                    directConnectIPEnter = false;
                    directConnectIP = "";
                }

                if (key == Keys.Backspace && directConnectIP.Length > 0)
                {
                    directConnectIP = directConnectIP.Substring(0, directConnectIP.Length - 1);
                }

                if (key == Keys.Enter)
                {
                    // Try what was entered first as an IP, and then second as a host name.
                    directConnectIPEnter = false;
                    _P.PlaySound(InfiniminerSound.ClickHigh);
                    IPAddress connectIp = null;
                    if (!IPAddress.TryParse(directConnectIP, out connectIp))
                    {
                        connectIp = null;
                        try
                        {
                            IPAddress[] resolveResults = Dns.GetHostAddresses(directConnectIP);
                            for (int i = 0; i < resolveResults.Length; i++)
                                if (resolveResults[i].AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                                {
                                    connectIp = resolveResults[i];
                                    break;
                                }
                        }
                        catch (Exception)
                        {
                            // So, GetHostAddresses() might fail, but we don't really care. Just leave connectIp as null.
                        }
                    }
                    if (connectIp != null)                   
                    {
                        (_SM as InfiniminerGame).propertyBag.serverName = directConnectIP;
                        (_SM as InfiniminerGame).JoinGame(new IPEndPoint(connectIp, 5565));
                        nextState = "Infiniminer.States.LoadingState";
                    }
                    directConnectIP = "";
                }

                if (key == Keys.V && (_SM.Keyboard.IsKeyDown(Keys.LeftControl) || _SM.Keyboard.IsKeyDown(Keys.RightControl)))
                {
                    try
                    {
                        directConnectIP += _SM.GetClipboardText();
                    }
                    catch { }
                }
                /*else if (keyMap.IsKeyMapped(key))
                {
                    directConnectIP += keyMap.TranslateKey(key, false);
                }*/
            }
            else
            {
                if (key == Keys.Escape)
                {
                    nextState = "Infiniminer.States.TitleState";
                }
            }
        }

        public override void OnKeyUp(Keys key)
        {

        }

        public override void OnMouseDown(MouseButton button, int x, int y)
        {
            if (directConnectIPEnter == false)
            {
                int serverIndex = (y - drawRect.Y - 75) / 25;
                if (serverIndex >= 0 && serverIndex < serverList.Count)
                {
                    int distanceFromCenter = Math.Abs(_SM.Width / 2 - x);
                    if (distanceFromCenter < descWidths[serverIndex] / 2)
                    {
                        (_SM as InfiniminerGame).propertyBag.serverName = serverList[serverIndex].serverName;
                        (_SM as InfiniminerGame).JoinGame(serverList[serverIndex].ipEndPoint);
                        nextState = "Infiniminer.States.LoadingState";
                        _P.PlaySound(InfiniminerSound.ClickHigh);
                    }
                }

                x -= drawRect.X;
                y -= drawRect.Y;
                switch (ClickRegion.HitTest(clkMenuServer, new Point(x, y)))
                {
                    case "refresh":
                        _P.PlaySound(InfiniminerSound.ClickHigh);
                        serverList = (_SM as InfiniminerGame).EnumerateServers(0.5f);
                        break;

                    case "direct":
                        directConnectIPEnter = true;
                        _P.PlaySound(InfiniminerSound.ClickHigh);
                        break;
                    case "settings":
                        nextState = "Infiniminer.States.SettingsState";
                        _P.PlaySound(InfiniminerSound.ClickHigh);
                        break;
                }
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
