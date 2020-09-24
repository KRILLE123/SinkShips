using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SkänkaSkeppV3
{
    public class Player
    {
        public int identifier { get; set; }
        public int[,] shipState { get; set; }
        public int tries { get; set; }
        public int numOfShips { get; set; }
    }

    public class Game1 : Game
    {
        Texture2D battle_shipTexture;
        Texture2D line;
        Texture2D wrong;
        Texture2D battle_ship_destroyedTexture;
        Texture2D background;


        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private MouseState oldState;
        int xGrid = 8;
        int yGrid = 5;
        int maxTries = 10;
        static int state = 1;

        static List<Player> Players = new List<Player>();
        Color boxColor1 = new Color(150,0,0, 100);
        Color boxColor2 = new Color(0, 100, 0, 100);


        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            _graphics.IsFullScreen = false; 

            _graphics.ApplyChanges();
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            Players.Add(new Player() { identifier = -1, numOfShips = 0, shipState = new int[8,5], tries = 0});
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            StartClient("start",1,1);
            base.Initialize();
        }

        public static void StartClient(string action, int x, int y)
        {
            byte[] bytes = new byte[1024];

            try
            {
                // Connect to a Remote server  
                // Get Host IP Address that is used to establish a connection  
                // In this case, we get one IP address of localhost that is IP : 127.0.0.1  
                // If a host has multiple addresses, you will get a list of addresses  
                IPHostEntry host = Dns.GetHostEntry("localhost");
                IPAddress ipAddress = host.AddressList[0];
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, 11005);

                // Create a TCP/IP  socket.    
                Socket sender = new Socket(ipAddress.AddressFamily,
                    SocketType.Stream, ProtocolType.Tcp);

                // Connect the socket to the remote endpoint. Catch any errors.    
                try
                {
                    // Connect to Remote EndPoint  
                    sender.Connect(remoteEP);

                    Console.WriteLine("Socket connected to {0}", sender.RemoteEndPoint.ToString());

                    byte[] msg = Encoding.ASCII.GetBytes(Players[0].identifier.ToString() + "/" + x.ToString() + "/" + y.ToString() + "/" + action);
  

                    // Send the data through the socket.    
                    int bytesSent = sender.Send(msg);

                    // Receive the response from the remote device.    
                    int bytesRec = sender.Receive(bytes);
                    string data = Encoding.ASCII.GetString(bytes, 0, bytesRec);

                    Debug.WriteLine(data);

                    if (action == "start")
                    {
                        Players[0].identifier = int.Parse(data);
                    } 
                    else if (data == "start:Search")
                    {
                        Players[0].numOfShips = 5;
                        state = 2;
                    } 
                    else if(data == "hit")
                    {
                        Players[0].shipState[x, y] = 2;
                    } 
                    else if(data == "miss")
                    {
                        Players[0].shipState[x, y] = 3;
                    }
                    sender.Shutdown(SocketShutdown.Both);
                    sender.Close();

                }
                catch (ArgumentNullException ane)
                {
                    Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
                }
                catch (SocketException se)
                {
                    Console.WriteLine("SocketException : {0}", se.ToString());
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unexpected exception : {0}", e.ToString());
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

    protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            battle_shipTexture = Content.Load<Texture2D>("battle_ship");
            battle_ship_destroyedTexture = Content.Load<Texture2D>("battle_ship_destroyed");
            line = Content.Load<Texture2D>("line");
            wrong = Content.Load<Texture2D>("wrong");
            background = Content.Load<Texture2D>("background");


            // TODO: use this.Content to load your game content here
        }

        void Game_Control()
        {
            MouseState newState = Mouse.GetState();

            if (newState.LeftButton == ButtonState.Pressed && oldState.LeftButton == ButtonState.Released)
            {
                for (int i = 0; i < xGrid; i++)
                {
                    for (int j = 0; j < yGrid; j++)
                    {
                        bool insideGrid = newState.X >= i * 100 + 5 && newState.Y >= j * 100 && newState.X <= (i + 1) * 100 + 5 && newState.Y <= (j + 1) * 100;
                        if (insideGrid)
                        {
                            if (Players[0].numOfShips < 5)
                            {
                                if (Players[0].shipState[i, j] == 0 && i < (xGrid/2))
                                {
                                    Players[0].numOfShips++;
                                    StartClient("add", i, j);
                                }
                            }
                            else if (state == 2 && maxTries > Players[0].tries && i >= (xGrid/2))
                            {
                                StartClient("search", i, j);
                                Players[0].tries += 1;
                            }
                            else if (maxTries == Players[0].tries)
                            {
                                Debug.WriteLine("du förlorade");
                            }
                        }
                    }
                }
            }
            oldState = newState;

        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            Game_Control();


            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Blue);

            _spriteBatch.Begin();
            _spriteBatch.Draw(background, new Vector2(0, 0), null, Color.White, 0.0f, new Vector2(0.5f), new Vector2(1f), SpriteEffects.None, 1.0f);

            for (int i = 0; i < xGrid; i++)
            {
                for (int j = 0; j < yGrid; j++)
                {
                    if (Players[0].shipState[i, j] == 2)
                    {
                        _spriteBatch.Draw(battle_ship_destroyedTexture, new Vector2(i * 100 + 5, j * 100), null, Color.White, 0.0f, new Vector2(1.5f), new Vector2(0.2f), SpriteEffects.None, 1.0f);
                    } else if (Players[0].shipState[i,j] == 3)
                    {
                        _spriteBatch.Draw(wrong, new Vector2(i * 100 + 5, j * 100), null, Color.White, 0.0f, new Vector2(1.5f), new Vector2(0.2f), SpriteEffects.None, 1.0f);

                    }
                    else
                    {
                        if (state == 2)
                        {
                            if (i < (xGrid / 2))
                            {
                                _spriteBatch.Draw(line, new Rectangle(i * 100 + 5, j * 100, 90, 90), boxColor2);
                            }
                            else if (i >= (xGrid / 2))
                            {
                                _spriteBatch.Draw(line, new Rectangle(i * 100 + 5, j * 100, 90, 90), boxColor1);

                            }
                        } else if (state == 1)
                        {
                            if (i < (xGrid / 2))
                            {
                                _spriteBatch.Draw(line, new Rectangle(i * 100 + 5, j * 100, 90, 90), boxColor1);
                            }
                            else if (i >= (xGrid / 2))
                            {
                                _spriteBatch.Draw(line, new Rectangle(i * 100 + 5, j * 100, 90, 90), boxColor2);

                            }
                        }
                    }
                }
            }

            _spriteBatch.End();


            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }
    }
}
