using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using LibreLancer;
using LibreLancer.Graphics;
using LibreLancer.Graphics.Text;

namespace Infiniminer
{
    public class InterfaceEngine
    {
        InfiniminerGame gameInstance;
        PropertyBag _P;
        Rectangle drawRect;

        Texture2D texCrosshairs, texHelp;
        Texture2D texRadarBackground, texRadarForeground, texRadarPlayerSame, texRadarPlayerAbove, texRadarPlayerBelow, texRadarPlayerPing, texRadarNorth;
        Texture2D texToolRadarRed, texToolRadarBlue, texToolRadarGold, texToolRadarDiamond, texToolRadarLED, texToolRadarPointer, texToolRadarFlash;
        Texture2D texToolDetonatorDownRed, texToolDetonatorUpRed, texToolDetonatorDownBlue, texToolDetonatorUpBlue;
        Texture2D texToolBuild, texToolBuildCharge, texToolBuildBlast, texToolBuildSmoke;

        Dictionary<BlockType, Texture2D> blockIcons = new Dictionary<BlockType, Texture2D>();

        public InterfaceEngine(InfiniminerGame gameInstance)
        {
            this.gameInstance = gameInstance;

            // Load textures.
            texCrosshairs = gameInstance.Content.LoadTexture("ui/tex_ui_crosshair.png");
            texRadarBackground = gameInstance.Content.LoadTexture("ui/tex_radar_background.png");
            texRadarForeground = gameInstance.Content.LoadTexture("ui/tex_radar_foreground.png");
            texRadarPlayerSame = gameInstance.Content.LoadTexture("ui/tex_radar_player_same.png");
            texRadarPlayerAbove = gameInstance.Content.LoadTexture("ui/tex_radar_player_above.png");
            texRadarPlayerBelow = gameInstance.Content.LoadTexture("ui/tex_radar_player_below.png");
            texRadarPlayerPing = gameInstance.Content.LoadTexture("ui/tex_radar_player_ping.png");
            texRadarNorth = gameInstance.Content.LoadTexture("ui/tex_radar_north.png");
            texHelp = gameInstance.Content.LoadTexture("menus/tex_menu_help.png");

            texToolRadarRed = gameInstance.Content.LoadTexture("tools/tex_tool_radar_red.png");
            texToolRadarBlue = gameInstance.Content.LoadTexture("tools/tex_tool_radar_blue.png");
            texToolRadarGold = gameInstance.Content.LoadTexture("tools/tex_tool_radar_screen_gold.png");
            texToolRadarDiamond = gameInstance.Content.LoadTexture("tools/tex_tool_radar_screen_diamond.png");
            texToolRadarLED = gameInstance.Content.LoadTexture("tools/tex_tool_radar_led.png");
            texToolRadarPointer = gameInstance.Content.LoadTexture("tools/tex_tool_radar_pointer.png");
            texToolRadarFlash = gameInstance.Content.LoadTexture("tools/tex_tool_radar_flash.png");

            texToolBuild = gameInstance.Content.LoadTexture("tools/tex_tool_build.png");
            texToolBuildCharge = gameInstance.Content.LoadTexture("tools/tex_tool_build_charge.png");
            texToolBuildBlast = gameInstance.Content.LoadTexture("tools/tex_tool_build_blast.png");
            texToolBuildSmoke = gameInstance.Content.LoadTexture("tools/tex_tool_build_smoke.png");

            texToolDetonatorDownRed = gameInstance.Content.LoadTexture("tools/tex_tool_detonator_down_red.png");
            texToolDetonatorUpRed = gameInstance.Content.LoadTexture("tools/tex_tool_detonator_up_red.png");
            texToolDetonatorDownBlue = gameInstance.Content.LoadTexture("tools/tex_tool_detonator_down_blue.png");
            texToolDetonatorUpBlue = gameInstance.Content.LoadTexture("tools/tex_tool_detonator_up_blue.png");

            drawRect = new Rectangle(gameInstance.RenderContext.CurrentViewport.Width / 2 - 1024 / 2,
                                     gameInstance.RenderContext.CurrentViewport.Height / 2 - 768 / 2,
                                     1024,
                                     1024);

            // Load icons.
            blockIcons[BlockType.BankBlue] = gameInstance.Content.LoadTexture("icons/tex_icon_bank_blue.png");
            blockIcons[BlockType.BankRed] = gameInstance.Content.LoadTexture("icons/tex_icon_bank_red.png");
            blockIcons[BlockType.Explosive] = gameInstance.Content.LoadTexture("icons/tex_icon_explosive.png");
            blockIcons[BlockType.Jump] = gameInstance.Content.LoadTexture("icons/tex_icon_jump.png");
            blockIcons[BlockType.Ladder] = gameInstance.Content.LoadTexture("icons/tex_icon_ladder.png");
            blockIcons[BlockType.SolidBlue] = gameInstance.Content.LoadTexture("icons/tex_icon_solid_blue.png");
            blockIcons[BlockType.SolidRed] = gameInstance.Content.LoadTexture("icons/tex_icon_solid_red.png");
            blockIcons[BlockType.Shock] = gameInstance.Content.LoadTexture("icons/tex_icon_spikes.png");
            blockIcons[BlockType.TransBlue] = gameInstance.Content.LoadTexture("icons/tex_icon_translucent_blue.png");
            blockIcons[BlockType.TransRed] = gameInstance.Content.LoadTexture("icons/tex_icon_translucent_red.png");
            blockIcons[BlockType.BeaconRed] = gameInstance.Content.LoadTexture("icons/tex_icon_beacon.png");
            blockIcons[BlockType.BeaconBlue] = gameInstance.Content.LoadTexture("icons/tex_icon_beacon.png");
            blockIcons[BlockType.Road] = gameInstance.Content.LoadTexture("icons/tex_icon_road.png");
            blockIcons[BlockType.None] = gameInstance.Content.LoadTexture("icons/tex_icon_deconstruction.png");
        }

        public void RenderMessageCenter(RenderContext renderContext, string text, Vector2 pointCenter, Color4 colorText, Color4 colorBackground)
        {
            Vector2 textSize = renderContext.Renderer2D.MeasureString(Fonts.UiFont, text);
            renderContext.Renderer2D.FillRectangle(
                new Rectangle((int) (pointCenter.X - textSize.X / 2 - 10), (int) (pointCenter.Y - textSize.Y / 2 - 10),
                    (int) (textSize.X + 20), (int) (textSize.Y + 20)), colorBackground);
            renderContext.Renderer2D.DrawString(Fonts.UiFont, text, pointCenter - textSize / 2, colorText);
        }

        private static bool MessageExpired(ChatMessage msg)
        {
            return msg.timestamp <= 0;
        }

        public void Update(double elapsedSeconds)
        {
            if (_P == null)
                return;

            foreach (ChatMessage msg in _P.chatBuffer)
                msg.timestamp -= (float)elapsedSeconds;
            _P.chatBuffer.RemoveAll(MessageExpired);

            int bufferSize = 10;
            if (_P.chatFullBuffer.Count > bufferSize)
                _P.chatFullBuffer.RemoveRange(bufferSize, _P.chatFullBuffer.Count - bufferSize);

            if (_P.constructionGunAnimation > 0)
            {
                if (_P.constructionGunAnimation > elapsedSeconds)
                    _P.constructionGunAnimation -= (float)elapsedSeconds;
                else
                    _P.constructionGunAnimation = 0;
            }
            else
            {
                if (_P.constructionGunAnimation < -elapsedSeconds)
                    _P.constructionGunAnimation += (float)elapsedSeconds;
                else
                    _P.constructionGunAnimation = 0;
            }
        }

        public void RenderRadarBlip(RenderContext renderContext, Vector3 position, Color4 color, bool ping, string text)
        {
            // Figure out the relative position for the radar blip.
            Vector3 relativePosition = position - _P.playerPosition;
            float relativeAltitude = relativePosition.Y;
            relativePosition.Y = 0;
            Matrix4x4 rotationMatrix = Matrix4x4.CreateRotationY(-_P.playerCamera.Yaw);
            relativePosition = Vector3.Transform(relativePosition, rotationMatrix) * 10;
            float relativeLength = Math.Min(relativePosition.Length(), 93);
            if (relativeLength != 0)
                relativePosition.Normalize();
            relativePosition *= relativeLength;

            // Draw the radar blip.
            if (text == "")
            {
                relativePosition.X = (int)relativePosition.X;
                relativePosition.Z = (int)relativePosition.Z;
                Texture2D texRadarSprite = texRadarPlayerSame;
                if (relativeAltitude > 2)
                    texRadarSprite = texRadarPlayerAbove;
                else if (relativeAltitude < -2)
                    texRadarSprite = texRadarPlayerBelow;
                renderContext.Renderer2D.Draw(texRadarSprite, new Vector2(10 + 99 + relativePosition.X - texRadarSprite.Width / 2, 30 + 99 + relativePosition.Z - texRadarSprite.Height / 2), color);
                if (ping)
                    renderContext.Renderer2D.Draw(texRadarPlayerPing, new Vector2(10 + 99 + relativePosition.X - texRadarPlayerPing.Width / 2, 30 + 99 + relativePosition.Z - texRadarPlayerPing.Height / 2), color);
            }

            // Render text.
            if (text != "")
            {
                relativePosition *= 0.9f;
                relativePosition.X = (int)relativePosition.X;
                relativePosition.Z = (int)relativePosition.Z;

                if (text == "NORTH.png")
                {
                    renderContext.Renderer2D.Draw(texRadarNorth, new Vector2(10 + 99 + relativePosition.X - texRadarNorth.Width / 2, 30 + 99 + relativePosition.Z - texRadarNorth.Height / 2), color);
                }
                else
                {
                    if (relativeAltitude > 2)
                        text += " ^";
                    else if (relativeAltitude < -2)
                        text += " v";
                    Vector2 textSize = renderContext.Renderer2D.MeasureString(Fonts.RadarFont, text);
                    renderContext.Renderer2D.DrawString(Fonts.RadarFont, text, new Vector2(10 + 99 + relativePosition.X - textSize.X / 2, 30 + 99 + relativePosition.Z - textSize.Y / 2), color);
                }
            }
        }

        public void RenderDetonator(RenderContext renderContext, Mouse mouse)
        {
            int screenWidth = renderContext.CurrentViewport.Width;
            int screenHeight = renderContext.CurrentViewport.Height;

            Texture2D textureToUse;
            if (mouse.IsButtonDown(MouseButtons.Left) || mouse.IsButtonDown(MouseButtons.Middle) || mouse.IsButtonDown(MouseButtons.Right))
                textureToUse = _P.playerTeam == PlayerTeam.Red ? texToolDetonatorDownRed : texToolDetonatorDownBlue;
            else
                textureToUse = _P.playerTeam == PlayerTeam.Red ? texToolDetonatorUpRed : texToolDetonatorUpBlue;

            textureToUse.SetFiltering(TextureFiltering.Nearest);
            renderContext.Renderer2D.DrawImageStretched(textureToUse, new Rectangle(screenWidth / 2 /*- 22 * 3*/, screenHeight - 77 * 3 + 14 * 3, 75 * 3, 77 * 3), Color4.White);
        }

        public void RenderProspectron(RenderContext renderContext)
        {
            int screenWidth = renderContext.CurrentViewport.Width;
            int screenHeight = renderContext.CurrentViewport.Height;
            
            texToolRadarRed.SetFiltering(TextureFiltering.Nearest);
            texToolRadarBlue.SetFiltering(TextureFiltering.Nearest);

            int drawX = screenWidth / 2 - 32 * 3;
            int drawY = screenHeight - 102 * 3;

            renderContext.Renderer2D.DrawImageStretched(_P.playerTeam == PlayerTeam.Red ? texToolRadarRed : texToolRadarBlue, new Rectangle(drawX, drawY, 70 * 3, 102 * 3), Color4.White);

            if (_P.radarValue > 0)
                renderContext.Renderer2D.DrawImageStretched(texToolRadarLED, new Rectangle(drawX, drawY, 70 * 3, 102 * 3), Color4.White);
            if (_P.radarValue == 200)
                renderContext.Renderer2D.DrawImageStretched(texToolRadarGold, new Rectangle(drawX, drawY, 70 * 3, 102 * 3), Color4.White);
            if (_P.radarValue == 1000)
                renderContext.Renderer2D.DrawImageStretched(texToolRadarDiamond, new Rectangle(drawX, drawY, 70 * 3, 102 * 3), Color4.White);
            if (_P.playerToolCooldown > 0.2f)
                renderContext.Renderer2D.DrawImageStretched(texToolRadarFlash, new Rectangle(drawX, drawY, 70 * 3, 102 * 3), Color4.White);

            int pointerOffset = (int)(30 - _P.radarDistance) / 2;  // ranges from 0 to 15 inclusive
            if (_P.radarDistance == 30)
                pointerOffset = 15;
            renderContext.Renderer2D.DrawImageStretched(texToolRadarPointer, new Rectangle(drawX + 54 * 3, drawY + 20 * 3 + pointerOffset * 3, 4 * 3, 5 * 3), Color4.White);
        }

        public void RenderConstructionGun(RenderContext renderContext, BlockType blockType)
        {
            int screenWidth = renderContext.CurrentViewport.Width;
            int screenHeight = renderContext.CurrentViewport.Height;
            blockIcons[blockType].SetFiltering(TextureFiltering.Nearest);

            int drawX = screenWidth / 2 - 60 * 3;
            int drawY = screenHeight - 91 * 3;

            Texture2D gunSprite = texToolBuild;
            if (_P.constructionGunAnimation < -0.001)
                gunSprite = texToolBuildCharge;
            else if (_P.constructionGunAnimation > 0.3)
                gunSprite = texToolBuildBlast;
            else if (_P.constructionGunAnimation > 0.001)
                gunSprite = texToolBuildSmoke;
            gunSprite.SetFiltering(TextureFiltering.Nearest);

            renderContext.Renderer2D.DrawImageStretched(gunSprite, new Rectangle(drawX, drawY, 120 * 3, 126 * 3), Color4.White);
            renderContext.Renderer2D.DrawImageStretched(blockIcons[blockType], new Rectangle(drawX + 37 * 3, drawY + 50 * 3, 117, 63), Color4.White);
        }

        public void drawChat(List<ChatMessage>messages, RenderContext renderContext)
        {
            int newlines = 0;
            for (int i = 0; i < messages.Count; i++)
            {
                Color4 chatColor = Color4.White;
                if (messages[i].type == ChatMessageType.SayRedTeam)
                    chatColor = _P.red;// Defines.IM_RED;
                if (messages[i].type == ChatMessageType.SayBlueTeam)
                    chatColor = _P.blue;// Defines.IM_BLUE;

                int y = renderContext.CurrentViewport.Height - 114;
                newlines += messages[i].newlines;
                y -= 16 * newlines;
                //y -= 16 * i;
                renderContext.Renderer2D.DrawStringBaseline(Fonts.UiFont.Name, Fonts.UiFont.Size, messages[i].message, 20, y, chatColor,
            false, new OptionalColor(Color4.Black));
            }
        }

        public void Render(RenderContext renderContext)
        {
            // If we don't have _P, grab it from the current gameInstance.
            // We can't do this in the constructor because we are created in the property bag's constructor!
            if (_P == null)
                _P = gameInstance.propertyBag;

            // Draw the UI.
            
            // Draw the crosshair.
            var vp = renderContext.CurrentViewport;
            renderContext.Renderer2D.DrawImageStretched(texCrosshairs, new Rectangle(vp.Width / 2 - texCrosshairs.Width / 2,
                                                            vp.Height / 2 - texCrosshairs.Height / 2,
                                                            texCrosshairs.Width,
                                                            texCrosshairs.Height), Color4.White);

            // If equipped, draw the tool.
            switch (_P.playerTools[_P.playerToolSelected])
            {
                case PlayerTools.Detonator:
                    RenderDetonator(renderContext, gameInstance.Mouse);
                    break;

                case PlayerTools.ProspectingRadar:
                    RenderProspectron(renderContext);
                    break;

                case PlayerTools.ConstructionGun:
                    RenderConstructionGun(renderContext, _P.playerBlocks[_P.playerBlockSelected]);
                    break;

                case PlayerTools.DeconstructionGun:
                    RenderConstructionGun(renderContext, BlockType.None);
                    break;

                default:
                    {
                        // Draw info about what we have equipped.
                        PlayerTools currentTool = _P.playerTools[_P.playerToolSelected];
                        BlockType currentBlock = _P.playerBlocks[_P.playerBlockSelected];
                        string equipment = currentTool.ToString();
                        if (currentTool == PlayerTools.ConstructionGun)
                            equipment += " - " + currentBlock.ToString() + " (" + BlockInformation.GetCost(currentBlock) + ")";
                        RenderMessageCenter(renderContext, equipment, new Vector2(renderContext.CurrentViewport.Width / 2f, renderContext.CurrentViewport.Height - 20), Color4.White, Color4.Black);
                    }
                    break;
            }

            if (gameInstance.DrawFrameRate)
                RenderMessageCenter(renderContext, String.Format("FPS: {0:000}", gameInstance.RenderFrequency), new Vector2(60, renderContext.CurrentViewport.Height - 20), Color4.Gray, Color4.Black);

            // Show the altimeter.
            int altitude = (int)(_P.playerPosition.Y - 64 + Defines.GROUND_LEVEL);
            RenderMessageCenter(renderContext, String.Format("ALTITUDE: {0:00}", altitude), new Vector2(renderContext.CurrentViewport.Width - 90, renderContext.CurrentViewport.Height - 20), altitude >= 0 ? Color4.Gray : Defines.IM_RED, Color4.Black);

            // Draw bank instructions.
            if (_P.AtBankTerminal())
                RenderMessageCenter(renderContext, "8: DEPOSIT 50 ORE  9: WITHDRAW 50 ORE", new Vector2(renderContext.CurrentViewport.Width / 2, renderContext.CurrentViewport.Height / 2 + 60), Color4.White, Color4.Black);

            // Are they trying to change class when they cannot?
            //if (_SM.Keyboard.IsKeyDown(Keys.M) && _P.playerPosition.Y <= 64 - Defines.GROUND_LEVEL && _P.chatMode == ChatMessageType.None)
            //    RenderMessageCenter(renderContext, "YOU CANNOT CHANGE YOUR CLASS BELOW THE SURFACE", new Vector2(renderContext.CurrentViewport.Width / 2, renderContext.CurrentViewport.Height / 2 + 90), Color4.White, Color.Black);

            // Draw the text-based information panel.
            int textStart = (gameInstance.Width - 1024) / 2;
            renderContext.Renderer2D.FillRectangle(new Rectangle(0, 0, gameInstance.Width, 20), Color4.Black);
            renderContext.Renderer2D.DrawString(Fonts.UiFont, "ORE: " + _P.playerOre + "/" + _P.playerOreMax, new Vector2(textStart + 3, 2), Color4.White);
            renderContext.Renderer2D.DrawString(Fonts.UiFont, "LOOT: $" + _P.playerCash, new Vector2(textStart + 170, 2), Color4.White);
            renderContext.Renderer2D.DrawString(Fonts.UiFont, "WEIGHT: " + _P.playerWeight + "/" + _P.playerWeightMax, new Vector2(textStart + 340, 2), Color4.White);
            renderContext.Renderer2D.DrawString(Fonts.UiFont, "TEAM ORE: " + _P.teamOre, new Vector2(textStart + 515, 2), Color4.White);
            renderContext.Renderer2D.DrawString(Fonts.UiFont, _P.redName + ": $" + _P.teamRedCash, new Vector2(textStart + 700, 2), _P.red);// Defines.IM_RED);
            renderContext.Renderer2D.DrawString(Fonts.UiFont, _P.blueName + ": $" + _P.teamBlueCash, new Vector2(textStart + 860, 2), _P.blue);// Defines.IM_BLUE);

            // Draw player information.
            if ((gameInstance.Keyboard.IsKeyDown(Keys.Tab) && _P.screenEffect == ScreenEffect.None) || _P.teamWinners != PlayerTeam.None)
            {
                renderContext.Renderer2D.FillRectangle( new Rectangle(0, 0, renderContext.CurrentViewport.Width, renderContext.CurrentViewport.Height), new Color4(0,0,0, 0.7f));

                //Server name
                RenderMessageCenter(renderContext, _P.serverName, new Vector2(renderContext.CurrentViewport.Width / 2, 32), _P.playerTeam == PlayerTeam.Blue ? _P.blue : _P.red, Color4.Black);//Defines.IM_BLUE : Defines.IM_RED, Color.Black);
                
                if (_P.teamWinners != PlayerTeam.None)
                {
                    string teamName = _P.teamWinners == PlayerTeam.Red ? "RED" : "BLUE";
                    Color4 teamColor = _P.teamWinners == PlayerTeam.Red ? _P.red : _P.blue;//Defines.IM_RED : Defines.IM_BLUE;
                    string gameOverMessage = "GAME OVER - " + teamName + " TEAM WINS!";
                    RenderMessageCenter(renderContext, gameOverMessage, new Vector2(renderContext.CurrentViewport.Width / 2, 150), teamColor, new Color4(0, 0, 0, 0));
                }

                int drawY = 200;
                foreach (Player p in _P.playerList.Values)
                {
                    if (p.Team != PlayerTeam.Red)
                        continue;
                    RenderMessageCenter(renderContext, p.Handle + " ( $" + p.Score + " )", new Vector2(renderContext.CurrentViewport.Width / 4, drawY), _P.red, new Color4(0, 0, 0, 0));//Defines.IM_RED
                    drawY += 35;
                }
                drawY = 200;
                foreach (Player p in _P.playerList.Values)
                {
                    if (p.Team != PlayerTeam.Blue)
                        continue;
                    RenderMessageCenter(renderContext, p.Handle + " ( $" + p.Score + " )", new Vector2(renderContext.CurrentViewport.Width * 3 / 4, drawY), _P.blue, new Color4(0, 0, 0, 0)); //Defines.IM_BLUE
                    drawY += 35;
                }
            }

            // Draw the chat buffer.
            if (_P.chatMode == ChatMessageType.SayAll)
            {
                renderContext.Renderer2D.DrawString(Fonts.UiFont, "ALL> " + _P.chatEntryBuffer, new Vector2(22, renderContext.CurrentViewport.Height - 98), Color4.Black);
                renderContext.Renderer2D.DrawString(Fonts.UiFont, "ALL> " + _P.chatEntryBuffer, new Vector2(20, renderContext.CurrentViewport.Height - 100), Color4.White);
            }
            else if (_P.chatMode == ChatMessageType.SayBlueTeam || _P.chatMode == ChatMessageType.SayRedTeam)
            {
                renderContext.Renderer2D.DrawString(Fonts.UiFont, "TEAM> " + _P.chatEntryBuffer, new Vector2(22, renderContext.CurrentViewport.Height - 98), Color4.Black);
                renderContext.Renderer2D.DrawString(Fonts.UiFont, "TEAM> " + _P.chatEntryBuffer, new Vector2(20, renderContext.CurrentViewport.Height - 100), Color4.White);
            }
            if (_P.chatMode != ChatMessageType.None)
            {
                drawChat(_P.chatFullBuffer,renderContext);
                for (int i = 0; i < _P.chatFullBuffer.Count; i++)
                {
                    Color4 chatColor = Color4.White;
                    chatColor = _P.chatFullBuffer[i].type == ChatMessageType.SayAll ? Color4.White : _P.chatFullBuffer[i].type == ChatMessageType.SayRedTeam ? Color4.Red : Color4.Blue;
                    
                    renderContext.Renderer2D.DrawString(Fonts.UiFont, _P.chatFullBuffer[i].message, new Vector2(22, renderContext.CurrentViewport.Height - 114 - 16 * i), Color4.Black);
                    renderContext.Renderer2D.DrawString(Fonts.UiFont, _P.chatFullBuffer[i].message, new Vector2(20, renderContext.CurrentViewport.Height - 116 - 16 * i), chatColor);
                }
            }
            else
            {
                drawChat(_P.chatBuffer,renderContext);
            }

            // Draw the player radar.
            renderContext.Renderer2D.Draw(texRadarBackground, new Vector2(10, 30), Color4.White);
            foreach (Player p in _P.playerList.Values)
                if (p.Team == _P.playerTeam && p.Alive)
                    RenderRadarBlip(renderContext, p.ID == _P.playerMyId ? _P.playerPosition : p.Position, p.Team == PlayerTeam.Red ? _P.red : _P.blue, p.Ping > 0, ""); //Defines.IM_RED : Defines.IM_BLUE, p.Ping > 0, "");
            foreach (KeyValuePair<Vector3, Beacon> bPair in _P.beaconList)
                if (bPair.Value.Team == _P.playerTeam)
                    RenderRadarBlip(renderContext, bPair.Key, Color4.White, false, bPair.Value.ID);
            RenderRadarBlip(renderContext, new Vector3(100000, 0, 32), Color4.White, false, "NORTH");

            renderContext.Renderer2D.Draw(texRadarForeground, new Vector2(10, 30), Color4.White);

            // Draw escape message.
            if (gameInstance.Keyboard.IsKeyDown(Keys.Escape))
            {
                RenderMessageCenter( renderContext, "PRESS Y TO CONFIRM THAT YOU WANT TO QUIT.", new Vector2(renderContext.CurrentViewport.Width / 2, renderContext.CurrentViewport.Height / 2 + 30), Color4.White, Color4.Black);
                RenderMessageCenter( renderContext, "PRESS K TO COMMIT PIXELCIDE.", new Vector2(renderContext.CurrentViewport.Width / 2, renderContext.CurrentViewport.Height / 2 + 80), Color4.White, Color4.Black);
            }

            // Draw the current screen effect.
            if (_P.screenEffect == ScreenEffect.Death)
            {
                Color4 drawColor = new Color4(1 - (float)_P.screenEffectCounter * 0.5f, 0f, 0f, 1f);
                renderContext.Renderer2D.FillRectangle( new Rectangle(0, 0, renderContext.CurrentViewport.Width, renderContext.CurrentViewport.Height), drawColor);
                if (_P.screenEffectCounter >= 2)
                    RenderMessageCenter(renderContext, "You have died. Click to respawn.", new Vector2(renderContext.CurrentViewport.Width / 2, renderContext.CurrentViewport.Height / 2), Color4.White, Color4.Black);
            }
            if (_P.screenEffect == ScreenEffect.Teleport || _P.screenEffect == ScreenEffect.Explosion)
            {
                Color4 drawColor = new Color4(1, 1, 1, 1 - (float)_P.screenEffectCounter * 0.5f);
                renderContext.Renderer2D.FillRectangle( new Rectangle(0, 0, renderContext.CurrentViewport.Width, renderContext.CurrentViewport.Height), drawColor);
                if (_P.screenEffectCounter > 2)
                    _P.screenEffect = ScreenEffect.None;
            }
            if (_P.screenEffect == ScreenEffect.Fall)
            {
                Color4 drawColor = new Color4(1, 0, 0, 1 - (float)_P.screenEffectCounter * 0.5f);
                renderContext.Renderer2D.FillRectangle( new Rectangle(0, 0, renderContext.CurrentViewport.Width, renderContext.CurrentViewport.Height), drawColor);
                if (_P.screenEffectCounter > 2)
                    _P.screenEffect = ScreenEffect.None;
            }

            // Draw the help screen.
            if (gameInstance.Keyboard.IsKeyDown(Keys.F1))
            {
                renderContext.Renderer2D.FillRectangle( new Rectangle(0, 0, renderContext.CurrentViewport.Width, renderContext.CurrentViewport.Height), Color4.Black);
                renderContext.Renderer2D.DrawImageStretched(texHelp, drawRect, Color4.White);
            }

        }
    }
}
