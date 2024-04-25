using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net;
using System.IO;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using System.Threading;
using LibreLancer;
using LibreLancer.Dialogs;
using LibreLancer.Graphics;
using LibreLancer.Media;
using LiteNetLib;
using LiteNetLib.Utils;

namespace Infiniminer
{
    public class InfiniminerGame : StateMasher.StateMachine
    {
        double timeSinceLastUpdate = 0;
        string playerHandle = "Player";
        float volumeLevel = 1.0f;
        //NetBuffer msgBuffer = null;
        //Song songTitle = null;

        public bool RenderPretty = true;
        public bool DrawFrameRate = false;
        public bool InvertMouseYAxis = false;
        public bool NoSound = false;
        public float mouseSensitivity = 0.005f;
        public bool customColours = false;
        public Color4 red=Defines.IM_RED;
        public string redName = "Red";
        public Color4 blue = Defines.IM_BLUE;
        public string blueName = "Blue";

        public KeyBindHandler keyBinds;

        public bool anyPacketsReceived = false;

        public InfiniminerGame(string[] args) : base(1024, 768, false)
        {
            keyBinds = new KeyBindHandler(this);
            Title = "Infiniminer";
        }

        public void setServername(string newName)
        {
            propertyBag.serverName = newName;
        }

        public void JoinGame(IPEndPoint serverEndPoint)
        {
            anyPacketsReceived = false;
            // Clear out the map load progress indicator.
            propertyBag.mapLoadProgress = new bool[64,64];
            for (int i = 0; i < 64; i++)
                for (int j=0; j<64; j++)
                    propertyBag.mapLoadProgress[i,j] = false;

            // Create our connect message.
            var connectBuffer = new NetDataWriter();
            connectBuffer.Put("INFINIMINER");
            connectBuffer.Put(propertyBag.playerHandle);
            connectBuffer.Put(Defines.INFINIMINER_VERSION);

            //Compression - will be ignored by regular servers
            connectBuffer.Put(true);

            // Connect to the server.
            propertyBag.netClient.Connect(serverEndPoint, connectBuffer);
        }

        public List<ServerInformation> EnumerateServers(float discoveryTime)
        {
            List<ServerInformation> serverList = new List<ServerInformation>();
            
            // Discover local servers.
            propertyBag.netClient.DiscoverLocalServers(5565);
            float timeTaken = 0;
            while (timeTaken < discoveryTime)
            {
                while (propertyBag.netClient.ReadMessage(out var peer, out var msgBuffer, out var msgType))
                {
                    if (msgType == NetEventType.ServerDiscovered)
                    {
                        bool serverFound = false;
                        ServerInformation serverInfo = new ServerInformation(peer);
                        foreach (ServerInformation si in serverList)
                            if (si.Equals(serverInfo))
                                serverFound = true;
                        if (!serverFound)
                            serverList.Add(serverInfo);
                    }
                }

                timeTaken += 0.1f;
                Thread.Sleep(100);
            }

            // Discover remote servers.
            try
            {
                /*string publicList = HttpRequest.Get("http://apps.keithholman.net/plain", null);
                foreach (string s in publicList.Split("\r\n".ToCharArray()))
                {
                    string[] args = s.Split(";".ToCharArray());
                    if (args.Length == 6)
                    {
                        IPAddress serverIp;
                        if (IPAddress.TryParse(args[1], out serverIp) && args[2] == "INFINIMINER")
                        {
                            ServerInformation serverInfo = new ServerInformation(serverIp, args[0], args[5], args[3], args[4]);
                            serverList.Add(serverInfo);
                        }
                    }
                }*/
            }
            catch (Exception)
            {
            }

            return serverList;
        }

        public void UpdateNetwork(double elapsed)
        {
            // Update the server with our status.
            timeSinceLastUpdate += elapsed;
            if (timeSinceLastUpdate > 0.05)
            {
                timeSinceLastUpdate = 0;
                if (CurrentStateType == "Infiniminer.States.MainGameState")
                    propertyBag.SendPlayerUpdate();
            }

            // Recieve messages from the server.
            while (propertyBag.netClient.ReadMessage(out _, out var msgBuffer, out var msgType))
            {
                switch (msgType)
                {
                    case NetEventType.Disconnected:
                        ChangeState("Infiniminer.States.ServerBrowserState");
                        break;
                    case NetEventType.Approved:
                        anyPacketsReceived = true;
                        break;
                    case NetEventType.Rejected:
                        {
                            anyPacketsReceived = false;
                            try
                            {
                                string[] reason = msgBuffer.GetString().Split(";".ToCharArray());
                                if (reason.Length < 2 || reason[0] == "VER")
                                    CrashWindow.Run("Infiniminer", "Connection Failed",
                                        "Error: client/server version incompability!\r\nServer: " +
                                        msgBuffer.GetString() + "\r\nClient: " + Defines.INFINIMINER_VERSION);
                                else
                                    CrashWindow.Run("Infiniminer", "Error: you are banned from this server!",
                                        "Banned!");
                            }
                            catch { }
                            ChangeState("Infiniminer.States.ServerBrowserState");
                        }
                        break;

                    case NetEventType.Packet:
                        {
                            try
                            {
                                InfiniminerMessage dataType = (InfiniminerMessage)msgBuffer.GetByte();
                                switch (dataType)
                                {
                                    case InfiniminerMessage.BlockBulkTransfer:
                                        {
                                            anyPacketsReceived = true;

                                            try
                                            {
                                                //This is either the compression flag or the x coordiante
                                                byte isCompressed = msgBuffer.GetByte();
                                                byte x;
                                                byte y;

                                                //255 was used because it exceeds the map size - of course, bytes won't work anyway if map sizes are allowed to be this big, so this method is a non-issue
                                                if (isCompressed == 255)
                                                {
                                                    var compressed = msgBuffer.GetRemainingBytes();
                                                    var compressedstream = new System.IO.MemoryStream(compressed);
                                                    var decompresser = new System.IO.Compression.GZipStream(compressedstream, System.IO.Compression.CompressionMode.Decompress);

                                                    x = (byte)decompresser.ReadByte();
                                                    y = (byte)decompresser.ReadByte();
                                                    propertyBag.mapLoadProgress[x, y] = true;
                                                    for (byte dy = 0; dy < 16; dy++)
                                                        for (byte z = 0; z < 64; z++)
                                                        {
                                                            BlockType blockType = (BlockType)decompresser.ReadByte();
                                                            if (blockType != BlockType.None)
                                                                propertyBag.blockEngine.downloadList[x, y + dy, z] = blockType;
                                                        }
                                                }
                                                else
                                                {
                                                    x = isCompressed;
                                                    y = msgBuffer.GetByte();
                                                    propertyBag.mapLoadProgress[x, y] = true;
                                                    for (byte dy = 0; dy < 16; dy++)
                                                        for (byte z = 0; z < 64; z++)
                                                        {
                                                            BlockType blockType = (BlockType)msgBuffer.GetByte();
                                                            if (blockType != BlockType.None)
                                                                propertyBag.blockEngine.downloadList[x, y + dy, z] = blockType;
                                                        }
                                                }
                                                bool downloadComplete = true;
                                                for (x = 0; x < 64; x++)
                                                    for (y = 0; y < 64; y += 16)
                                                        if (propertyBag.mapLoadProgress[x, y] == false)
                                                        {
                                                            downloadComplete = false;
                                                            break;
                                                        }
                                                if (downloadComplete)
                                                {
                                                    ChangeState("Infiniminer.States.TeamSelectionState");
                                                    if (!NoSound)
                                                        Audio.Music.Stop(3);
                                                    propertyBag.blockEngine.DownloadComplete();
                                                }
                                            }
                                            catch (Exception e)
                                            {
                                                Console.OpenStandardError();
                                                Console.Error.WriteLine(e.Message);
                                                Console.Error.WriteLine(e.StackTrace);
                                                Console.Error.Close();
                                            }
                                        }
                                        break;

                                    case InfiniminerMessage.SetBeacon:
                                        {
                                            Vector3 position = msgBuffer.GetVector3();
                                            string text = msgBuffer.GetString();
                                            PlayerTeam team = (PlayerTeam)msgBuffer.GetByte();

                                            if (text == "")
                                            {
                                                if (propertyBag.beaconList.ContainsKey(position))
                                                    propertyBag.beaconList.Remove(position);
                                            }
                                            else
                                            {
                                                Beacon newBeacon = new Beacon();
                                                newBeacon.ID = text;
                                                newBeacon.Team = team;
                                                propertyBag.beaconList.Add(position, newBeacon);
                                            }
                                        }
                                        break;

                                    case InfiniminerMessage.TriggerConstructionGunAnimation:
                                        {
                                            propertyBag.constructionGunAnimation = msgBuffer.GetFloat();
                                            if (propertyBag.constructionGunAnimation <= -0.1)
                                                propertyBag.PlaySound(InfiniminerSound.RadarSwitch);
                                        }
                                        break;

                                    case InfiniminerMessage.ResourceUpdate:
                                        {
                                            // ore, cash, weight, max ore, max weight, team ore, red cash, blue cash, all uint
                                            propertyBag.playerOre = msgBuffer.GetUInt();
                                            propertyBag.playerCash = msgBuffer.GetUInt();
                                            propertyBag.playerWeight = msgBuffer.GetUInt();
                                            propertyBag.playerOreMax = msgBuffer.GetUInt();
                                            propertyBag.playerWeightMax = msgBuffer.GetUInt();
                                            propertyBag.teamOre = msgBuffer.GetUInt();
                                            propertyBag.teamRedCash = msgBuffer.GetUInt();
                                            propertyBag.teamBlueCash = msgBuffer.GetUInt();
                                        }
                                        break;

                                    case InfiniminerMessage.BlockSet:
                                        {
                                            // x, y, z, type, all bytes
                                            byte x = msgBuffer.GetByte();
                                            byte y = msgBuffer.GetByte();
                                            byte z = msgBuffer.GetByte();
                                            BlockType blockType = (BlockType)msgBuffer.GetByte();
                                            if (blockType == BlockType.None)
                                            {
                                                if (propertyBag.blockEngine.BlockAtPoint(new Vector3(x, y, z)) != BlockType.None)
                                                    propertyBag.blockEngine.RemoveBlock(x, y, z);
                                            }
                                            else
                                            {
                                                if (propertyBag.blockEngine.BlockAtPoint(new Vector3(x, y, z)) != BlockType.None)
                                                    propertyBag.blockEngine.RemoveBlock(x, y, z);
                                                propertyBag.blockEngine.AddBlock(x, y, z, blockType);
                                                CheckForStandingInLava();
                                            }
                                        }
                                        break;

                                    case InfiniminerMessage.TriggerExplosion:
                                        {
                                            Vector3 blockPos = msgBuffer.GetVector3();

                                            // Play the explosion sound.
                                            propertyBag.PlaySound(InfiniminerSound.Explosion, blockPos);

                                            // Create some particles.
                                            propertyBag.particleEngine.CreateExplosionDebris(blockPos);

                                            // Figure out what the effect is.
                                            float distFromExplosive = (blockPos + 0.5f * Vector3.One - propertyBag.playerPosition).Length();
                                            if (distFromExplosive < 3)
                                                propertyBag.KillPlayer(Defines.deathByExpl);//"WAS KILLED IN AN EXPLOSION!");
                                            else if (distFromExplosive < 8)
                                            {
                                                // If we're not in explosion mode, turn it on with the minimum ammount of shakiness.
                                                if (propertyBag.screenEffect != ScreenEffect.Explosion)
                                                {
                                                    propertyBag.screenEffect = ScreenEffect.Explosion;
                                                    propertyBag.screenEffectCounter = 2;
                                                }
                                                // If this bomb would result in a bigger shake, use its value.
                                                propertyBag.screenEffectCounter = Math.Min(propertyBag.screenEffectCounter, (distFromExplosive - 2) / 5);
                                            }
                                        }
                                        break;

                                    case InfiniminerMessage.PlayerSetTeam:
                                        {
                                            uint playerId = msgBuffer.GetUInt();
                                            if (propertyBag.playerList.ContainsKey(playerId))
                                            {
                                                Player player = propertyBag.playerList[playerId];
                                                player.Team = (PlayerTeam)msgBuffer.GetByte();
                                            }
                                        }
                                        break;

                                    case InfiniminerMessage.PlayerJoined:
                                        {
                                            uint playerId = msgBuffer.GetUInt();
                                            string playerName = msgBuffer.GetString();
                                            bool thisIsMe = msgBuffer.GetBool();
                                            bool playerAlive = msgBuffer.GetBool();
                                            propertyBag.playerList[playerId] = new Player(null, (Game)this);
                                            propertyBag.playerList[playerId].Handle = playerName;
                                            propertyBag.playerList[playerId].ID = playerId;
                                            propertyBag.playerList[playerId].Alive = playerAlive;
                                            propertyBag.playerList[playerId].AltColours = customColours;
                                            propertyBag.playerList[playerId].redTeam = red;
                                            propertyBag.playerList[playerId].blueTeam = blue;
                                            if (thisIsMe)
                                                propertyBag.playerMyId = playerId;
                                        }
                                        break;

                                    case InfiniminerMessage.PlayerLeft:
                                        {
                                            uint playerId = msgBuffer.GetUInt();
                                            if (propertyBag.playerList.ContainsKey(playerId))
                                                propertyBag.playerList.Remove(playerId);
                                        }
                                        break;

                                    case InfiniminerMessage.PlayerDead:
                                        {
                                            uint playerId = msgBuffer.GetUInt();
                                            if (propertyBag.playerList.ContainsKey(playerId))
                                            {
                                                Player player = propertyBag.playerList[playerId];
                                                player.Alive = false;
                                                propertyBag.particleEngine.CreateBloodSplatter(player.Position, player.Team == PlayerTeam.Red ? Color4.Red : Color4.Blue);
                                                if (playerId != propertyBag.playerMyId)
                                                    propertyBag.PlaySound(InfiniminerSound.Death, player.Position);
                                            }
                                        }
                                        break;

                                    case InfiniminerMessage.PlayerAlive:
                                        {
                                            uint playerId = msgBuffer.GetUInt();
                                            if (propertyBag.playerList.ContainsKey(playerId))
                                            {
                                                Player player = propertyBag.playerList[playerId];
                                                player.Alive = true;
                                            }
                                        }
                                        break;

                                    case InfiniminerMessage.PlayerUpdate:
                                        {
                                            uint playerId = msgBuffer.GetUInt();
                                            if (propertyBag.playerList.ContainsKey(playerId))
                                            {
                                                Player player = propertyBag.playerList[playerId];
                                                player.UpdatePosition(msgBuffer.GetVector3(), TotalTime);
                                                player.Heading = msgBuffer.GetVector3();
                                                player.Tool = (PlayerTools)msgBuffer.GetByte();
                                                player.UsingTool = msgBuffer.GetBool();
                                                player.Score = (uint)(msgBuffer.GetUShort() * 100);
                                            }
                                        }
                                        break;

                                    case InfiniminerMessage.GameOver:
                                        {
                                            propertyBag.teamWinners = (PlayerTeam)msgBuffer.GetByte();
                                        }
                                        break;

                                    case InfiniminerMessage.ChatMessage:
                                        {
                                            ChatMessageType chatType = (ChatMessageType)msgBuffer.GetByte();
                                            string chatString = Defines.Sanitize(msgBuffer.GetString());
                                            //Time to break it up into multiple lines
                                            propertyBag.addChatMessage(chatString, chatType, 10);
                                        }
                                        break;

                                    case InfiniminerMessage.PlayerPing:
                                        {
                                            uint playerId = (uint)msgBuffer.GetInt();
                                            if (propertyBag.playerList.ContainsKey(playerId))
                                            {
                                                if (propertyBag.playerList[playerId].Team == propertyBag.playerTeam)
                                                {
                                                    propertyBag.playerList[playerId].Ping = 1;
                                                    propertyBag.PlaySound(InfiniminerSound.Ping);
                                                }
                                            }
                                        }
                                        break;

                                    case InfiniminerMessage.PlaySound:
                                        {
                                            InfiniminerSound sound = (InfiniminerSound)msgBuffer.GetByte();
                                            bool hasPosition = msgBuffer.GetBool();
                                            if (hasPosition)
                                            {
                                                Vector3 soundPosition = msgBuffer.GetVector3();
                                                propertyBag.PlaySound(sound, soundPosition);
                                            }
                                            else
                                                propertyBag.PlaySound(sound);
                                        }
                                        break;
                                }
                            }
                            catch { } //Error in a received message
                        }
                        break;
                }
            }

            // Make sure our network thread actually gets to run.
            Thread.Sleep(1);
        }

        private void CheckForStandingInLava()
        {
            // Copied from TryToMoveTo; responsible for checking if lava has flowed over us.

            Vector3 movePosition = propertyBag.playerPosition;
            Vector3 midBodyPoint = movePosition + new Vector3(0, -0.7f, 0);
            Vector3 lowerBodyPoint = movePosition + new Vector3(0, -1.4f, 0);
            BlockType lowerBlock = propertyBag.blockEngine.BlockAtPoint(lowerBodyPoint);
            BlockType midBlock = propertyBag.blockEngine.BlockAtPoint(midBodyPoint);
            BlockType upperBlock = propertyBag.blockEngine.BlockAtPoint(movePosition);
            if (upperBlock == BlockType.Lava || lowerBlock == BlockType.Lava || midBlock == BlockType.Lava)
            {
                propertyBag.KillPlayer(Defines.deathByLava);
            }
        }

        protected override void Load()
        {
            base.Load();
            FLLog.Info("Game", "Reading Config");
            //Now moving to DatafileWriter only since it can read and write
            DatafileWriter dataFile = new DatafileWriter("client.config.txt");
            //if (dataFile.Data.ContainsKey("width"))
              //  Width = int.Parse(dataFile.Data["width"], System.Globalization.CultureInfo.InvariantCulture);
            //if (dataFile.Data.ContainsKey("height"))
              //  Height = int.Parse(dataFile.Data["height"], System.Globalization.CultureInfo.InvariantCulture);
            //if (dataFile.Data.ContainsKey("fullscreen"))
              //  graphicsDeviceManager.IsFullScreen = bool.Parse(dataFile.Data["fullscreen"]);
            if (dataFile.Data.ContainsKey("handle"))
                playerHandle = dataFile.Data["handle"];
            if (dataFile.Data.ContainsKey("showfps"))
                DrawFrameRate = bool.Parse(dataFile.Data["showfps"]);
            if (dataFile.Data.ContainsKey("yinvert"))
                InvertMouseYAxis = bool.Parse(dataFile.Data["yinvert"]);
            if (dataFile.Data.ContainsKey("nosound"))
                NoSound = bool.Parse(dataFile.Data["nosound"]);
            if (dataFile.Data.ContainsKey("pretty"))
                RenderPretty = bool.Parse(dataFile.Data["pretty"]);
            if (dataFile.Data.ContainsKey("volume"))
                volumeLevel = Math.Max(0,Math.Min(1,float.Parse(dataFile.Data["volume"], System.Globalization.CultureInfo.InvariantCulture)));
            if (dataFile.Data.ContainsKey("sensitivity"))
                mouseSensitivity=Math.Max(0.001f,Math.Min(0.05f,float.Parse(dataFile.Data["sensitivity"], System.Globalization.CultureInfo.InvariantCulture)/1000f));
            if (dataFile.Data.ContainsKey("red_name"))
                redName = dataFile.Data["red_name"].Trim();
            if (dataFile.Data.ContainsKey("blue_name"))
                blueName = dataFile.Data["blue_name"].Trim();


            if (dataFile.Data.ContainsKey("red"))
            {
                Color4 temp = new Color4();
                string[] data = dataFile.Data["red"].Split(',');
                try
                {
                    temp.R = byte.Parse(data[0].Trim());
                    temp.G = byte.Parse(data[1].Trim());
                    temp.B = byte.Parse(data[2].Trim());
                    temp.A = (byte)255;
                }
                catch {
                    Console.WriteLine("Invalid colour values for red");
                }
                if (temp.A != 0)
                {
                    red = temp;
                    customColours = true;
                }
            }

            if (dataFile.Data.ContainsKey("blue"))
            {
                Color4 temp = new Color4();
                string[] data = dataFile.Data["blue"].Split(',');
                try
                {
                    temp.R = byte.Parse(data[0].Trim());
                    temp.G = byte.Parse(data[1].Trim());
                    temp.B = byte.Parse(data[2].Trim());
                    temp.A = (byte)255;
                }
                catch {
                    Console.WriteLine("Invalid colour values for blue");
                }
                if (temp.A != 0)
                {
                    blue = temp;
                    customColours = true;
                }
            }

            //Now to read the key bindings
            if (!File.Exists("keymap.txt"))
            {
                FileStream temp = File.Create("keymap.txt");
                temp.Close();
                Console.WriteLine("Keymap file does not exist, creating.");
            }
            dataFile = new DatafileWriter("keymap.txt");
            bool anyChanged = false;
            foreach (string key in dataFile.Data.Keys)
            {
                try
                {
                    Buttons button = (Buttons)Enum.Parse(typeof(Buttons),dataFile.Data[key],true);
                    if (Enum.IsDefined(typeof(Buttons), button))
                    {
                        if (keyBinds.BindKey(button, key, true))
                        {
                            anyChanged = true;
                        }
                    }
                    else
                    {
                        Console.WriteLine("Enum not defined for " + dataFile.Data[key] + ".");
                    }
                } catch { }
            }

            //If no keys are bound in this manner then create the default set
            if (!anyChanged)
            {
                keyBinds.CreateDefaultSet();
                keyBinds.SaveBinds(dataFile, "keymap.txt");
                Console.WriteLine("Creating default keymap...");
            }

            Effect.Context = RenderContext;
            LoadContent();
        }

        protected override void Update(double gameTime)
        {
            base.Update(gameTime);
            Audio.UpdateAsync().Wait();
        }

        protected override void Cleanup()
        {
            propertyBag.netClient.Disconnect("Client exiting.");
        }

        public void ResetPropertyBag()
        {
            if (propertyBag != null)
                propertyBag.netClient.Disconnect("");

            propertyBag = new Infiniminer.PropertyBag(this);
            propertyBag.playerHandle = playerHandle;
            propertyBag.volumeLevel = volumeLevel;
            propertyBag.mouseSensitivity = mouseSensitivity;
            propertyBag.keyBinds = keyBinds;
            propertyBag.blue = blue;
            propertyBag.red = red;
            propertyBag.blueName = blueName;
            propertyBag.redName = redName;
        }

        void LoadContent()
        {
            FLLog.Info("Game", "Loading Content");

            Content.AddFontFile("VT323-Regular.ttf");
            // Initialize the property bag.
            ResetPropertyBag();

            FLLog.Info("Game", "Starting");
            // Set the initial state to team selection
            ChangeState("Infiniminer.States.TitleState");

            // Play the title music.
            if (!NoSound)
            {
                Audio.Music.Volume = propertyBag.volumeLevel;
                var stream = File.OpenRead(Content.GetPath("song_title.mp3"));
                Audio.Music.Play(stream, 0, 0, true);
            }
        }
    }
}