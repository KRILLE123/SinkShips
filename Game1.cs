using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;


namespace SkänkaSkeppV3
{
    public class Player
    {
        public int identifier { get; set; }
        public int[,] shipState { get; set; }
        public int tries { get; set; }
        public int numOfShips { get; set; }
    }

    public class Listener
    {
        Thread t = null;

        public Listener()
        {
            t = new Thread(ThreadProc);
            t.Start();
        }

        public void ThreadProc()
        {
            while (true)
            {
                Thread.Sleep(1000);
                Game1.ServerRequest("listener", 1, 1);
            }
        }

        public void ResetClient()
        {
            t = new Thread(ResetThread);
            t.Start();
        }
        public void ResetThread()
        {
            Thread.Sleep(2500);
            Game1.ResetGame();
        }

        public void EndThread()
        {
            t.Abort();
        }
    }

    public class ResetThread
    {
        Thread t = null;

        public ResetThread()
        {

            t = new Thread(ResetClient);
            t.Start();

            Thread.Sleep(2500);
            Game1.ResetGame();
        }

        public void ResetClient()
        {
            Thread.Sleep(2500);
            Game1.ResetGame();
            t.Abort();
        }

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
        private SpriteFont font;


        private MouseState oldState;
        int xGrid = 8;
        int yGrid = 4;
        int maxTries = 5;
        static int state = 1;
        Listener listener;

        static List<Player> Players = new List<Player>();
        Color red = new Color(150, 0, 0, 100);
        Color green = new Color(0, 100, 0, 100);


        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            _graphics.IsFullScreen = false;

            _graphics.ApplyChanges();
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            Players.Add(new Player() { identifier = -1, numOfShips = 0, shipState = new int[8, 5], tries = 0 });
        }


        public static void ResetGame()
        {
            Players.Clear();
            state = 1;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            ServerRequest("start", 1, 1);
            listener = new Listener();
            base.Initialize();

        }

        int GetCorrectSide(int i, int j)
        {
            if (Players[0].identifier == 1)
            {
                if (Players[0].numOfShips < 3 && Players[0].shipState[i, j] == 0 && i < (xGrid / 2))
                {
                    return 1;
                }
                else if (Players[0].numOfShips == 3 && i >= xGrid / 2)
                {
                    return 2;
                }

            }
            else if (Players[0].identifier == 2)
            {
                if (Players[0].numOfShips < 3 && Players[0].shipState[i, j] == 0 && i >= (xGrid / 2))
                {
                    return 1;
                }
                else if (Players[0].numOfShips == 3 && i < xGrid / 2)
                {
                    return 2;
                }
            }
            return 0;
        }


        public static void ServerRequest(string action, int x, int y)
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

                    if (action == "start")
                    {
                        Players[0].identifier = int.Parse(data);
                    }
                    else if (data == "start")
                    {
                        Players[0].numOfShips = 3;
                        state = 2;
                    }
                    else if (data == "hit")
                    {
                        Players[0].tries += 1;
                        Players[0].shipState[x, y] = 2;
                    }
                    else if (data == "miss")
                    {
                        Players[0].tries += 1;
                        Players[0].shipState[x, y] = 3;
                    }
                    else if (data.Contains("/"))
                    {
                        string[] dataSplit = data.Split("/");

                        if (dataSplit[3] == "hit")
                        {
                            Players[0].shipState[int.Parse(dataSplit[1]), int.Parse(dataSplit[2])] = 2;
                        }
                        else if (dataSplit[3] == "miss")
                        {
                            Players[0].shipState[int.Parse(dataSplit[1]), int.Parse(dataSplit[2])] = 3;
                        }
                    } else if (data == "win")
                    {
                        state = 3;
                        ResetClient();
                    } else if (data == "lost")
                    {
                        state = 4;
                    } else if (data == "draw")
                    {
                        state = 5;
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

        protected override void UnloadContent()
        {
            listener.EndThread();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            battle_shipTexture = Content.Load<Texture2D>("battle_ship");
            battle_ship_destroyedTexture = Content.Load<Texture2D>("battle_ship_destroyed");
            line = Content.Load<Texture2D>("line");
            wrong = Content.Load<Texture2D>("wrong");
            background = Content.Load<Texture2D>("background");
            font = Content.Load<SpriteFont>("American Captain");


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
                            int isRightSide = GetCorrectSide(i, j);

                            Debug.WriteLine(state);
                            if (state == 1)
                            {
                                if (isRightSide == 1)
                                {
                                    if (Players[0].shipState[i, j] == 0)
                                    {
                                        Players[0].numOfShips++;
                                        Players[0].shipState[i, j] = 1;
                                        ServerRequest("add", i, j);
                                    }
                                }
                            }
                            else if (state == 2 && maxTries > Players[0].tries && isRightSide == 2)
                            {
                                ServerRequest("search", i, j);
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
            Debug.WriteLine(state);


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
                    int isRightSide = GetCorrectSide(i, j);
                    Vector2 fixedPosWon = font.MeasureString("You won!");
                    Vector2 fixedPosLost = font.MeasureString("You lost");
                    Vector2 fixedPosDraw = font.MeasureString("It's a draw");


                    _spriteBatch.DrawString(font, "Lives left: " + (maxTries - Players[0].tries), new Vector2(50, 410), Color.White);

                    if(state == 3)
                    {
                        _spriteBatch.DrawString(font, "You won!",new Vector2(420 - (fixedPosWon.X/2), 410), Color.White);
                    } else if (state == 4)
                    {
                        _spriteBatch.DrawString(font, "You lost!", new Vector2(420 - (fixedPosLost.X/2), 410), Color.White);
                    } else if (state == 5)
                    {
                        _spriteBatch.DrawString(font, "It's a draw!", new Vector2(420 - (fixedPosDraw.X/2), 410), Color.White);
                    }


                    if (Players[0].shipState[i, j] == 2)
                    {
                        _spriteBatch.Draw(battle_ship_destroyedTexture, new Vector2(i * 100 + 5, j * 100), null, Color.White, 0.0f, new Vector2(1.5f), new Vector2(0.2f), SpriteEffects.None, 1.0f);
                    }
                    else if (Players[0].shipState[i, j] == 3)
                    {
                        _spriteBatch.Draw(wrong, new Vector2(i * 100 + 5, j * 100), null, Color.White, 0.0f, new Vector2(1.5f), new Vector2(0.2f), SpriteEffects.None, 1.0f);
                    }
                    else if (Players[0].shipState[i, j] == 1)
                    {
                        _spriteBatch.Draw(battle_shipTexture, new Vector2(i * 100 + 5, j * 100), null, Color.White, 0.0f, new Vector2(1.5f), new Vector2(0.2f), SpriteEffects.None, 1.0f);
                    }
                    else
                    {
                        if (isRightSide == 2 && Players[0].shipState[i, j] == 0)
                        {
                            _spriteBatch.Draw(line, new Rectangle(i * 100 + 5, j * 100, 90, 90), green);
                        }
                        else if (isRightSide == 1 && Players[0].shipState[i, j] == 0)
                        {
                            _spriteBatch.Draw(line, new Rectangle(i * 100 + 5, j * 100, 90, 90), green);
                        }
                        else if (isRightSide == 0 && Players[0].shipState[i, j] == 0)

                        {
                            _spriteBatch.Draw(line, new Rectangle(i * 100 + 5, j * 100, 90, 90), red);
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
