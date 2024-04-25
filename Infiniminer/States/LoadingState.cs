using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using LibreLancer;
using LibreLancer.Graphics;
using StateMasher;
using Rectangle = LibreLancer.Rectangle;

namespace Infiniminer.States
{
    public class LoadingState : State
    {
        Texture2D texMenu;
        Rectangle drawRect;
        string nextState = null;
        string[] currentHint;
        
        static string[] HINTS = new string[19]
        {
            "Engineers can build bank blocks near ore veins for\nMiners to quickly fill the team's supplies.",
            "Sappers can use TNT to excavate around gold and\ndiamonds, as it does not destroy them.",
            "Gold occurs in veins that can run dozens of blocks\nin length; follow the veins!",
            "You can paste a server name or IP into the direct\nconnect field by using Ctrl-V.",
            "The Engineer's jump blocks cost as much as a ladder\nblock but are far more efficient for going up.",
            "Beacon blocks are shown on your teammates' radar.\nUse them to mark important locations.",
            "Build force fields to keep the enemy out of your tunnels.",
            "Shock blocks will kill anyone who touches their underside.",
            "Combine jump blocks and shock blocks to make deadly traps!",
            "The Prospectron 3000 can detect gold and diamonds through walls.\nLet a prospector guide you when digging.",
            "Miners can dig much faster than the other classes!\nUse them to quickly mine out an area.",
            "Engineers can build force fields of the other team's color.\nUse this ability to create bridges only accessible to your team.",
            "Movement speed is doubled on road blocks.\nUse them to cover horizontal distances quickly.",
            "Return gold and diamonds to the surface to collect loot for your team!",
            "Press Q to quickly signal your teammates.",
            "All constructions require metal ore.\nDig for some or take it from your team's banks.",
            "Banks are indestructible - use them as walls even sappers can't pass!",
            "Don't have a scroll wheel?\nPress R to cycle through block types for the construction gun.",
            "You can set your name and adjust your screen resolution\nby using the settings scree or the config files.",
        };

        public override void OnEnter(string oldState)
        {
            _SM.RelativeMouseMode = false;
            _SM.CursorKind = CursorKind.None;

            texMenu = _SM.Content.LoadTexture("menus/tex_menu_loading.png");

            drawRect = new Rectangle(_SM.Width / 2 - 1024 / 2,
                                     _SM.Height / 2 - 768 / 2,
                                     1024,
                                     1024);
            

            // Pick a random hint.
            Random randGen = new Random();
            currentHint = HINTS[randGen.Next(0, HINTS.Length)].Split("\n".ToCharArray());
        }

        public override void OnLeave(string newState)
        {

        }

        public override string OnUpdate(double gameTime)
        {
            // Do network stuff.
            (_SM as InfiniminerGame).UpdateNetwork(gameTime);

            return nextState;
        }

        public override void OnRenderAtEnter()
        {

        }

        public override void OnRenderAtUpdate(double gameTime)
        {
            uint dataPacketsRecieved = 0;
            for (int x = 0; x < 64; x++)
                for (int y = 0; y < 64; y+=16)
                    if (_P.mapLoadProgress[x, y])
                        dataPacketsRecieved += 1;
            string progressText = "Connecting...";
            if ((_SM as InfiniminerGame).anyPacketsReceived)
                progressText = String.Format("{0:00}% LOADED", dataPacketsRecieved / 256.0f * 100);

            var spriteBatch = _SM.RenderContext.Renderer2D;
            spriteBatch.DrawImageStretched(texMenu, drawRect, Color4.White);
            spriteBatch.DrawString(Fonts.UiFont, progressText, new Vector2(((int)(_SM.Width / 2 - spriteBatch.MeasureString(Fonts.UiFont, progressText).X / 2)), drawRect.Y + 430), Color4.White);
            for (int i=0; i<currentHint.Length; i++)
                spriteBatch.DrawString(Fonts.UiFont, currentHint[i], new Vector2(((int)(_SM.Width / 2 - spriteBatch.MeasureString(Fonts.UiFont, currentHint[i]).X / 2)), drawRect.Y + 600+25*i), Color4.White);
        }

        public override void OnKeyDown(Keys key)
        {
            if (key == Keys.Escape)
            {
                _P.netClient.Disconnect("Client disconnected.");
                nextState = "Infiniminer.States.ServerBrowserState";
            }
        }

        public override void OnKeyUp(Keys key)
        {

        }

        public override void OnMouseDown(MouseButton button, int x, int y)
        {

        }

        public override void OnMouseUp(MouseButton button, int x, int y)
        {

        }

        public override void OnMouseScroll(int scrollDelta)
        {

        }
    }
}
