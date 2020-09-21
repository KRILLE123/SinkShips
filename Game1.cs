using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Diagnostics;

namespace SkänkaSkeppV3
{
    public class Game1 : Game
    {
        Texture2D battle_shipTexture;
        Texture2D line;


        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        string[,] hasHit = new string[8, 5];
        static Random random = new Random();
        static int[,] shipPos = new int[8,5];
        int ran_x = random.Next(7);
        int ran_y = random.Next(4);
        static int state = 1;
        private MouseState oldState;
        int numOfShips = 2;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            battle_shipTexture = Content.Load<Texture2D>("battle_ship");
            line = Content.Load<Texture2D>("line");

            // TODO: use this.Content to load your game content here
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            MouseState newState = Mouse.GetState();


            if (newState.LeftButton == ButtonState.Pressed && oldState.LeftButton == ButtonState.Released)
            {
                for (int i = 0; i < 8; i++)
                {
                    for (int j = 0; j < 5; j++)
                    {
                        bool insideGrid = newState.X >= i * 100 + 5 && newState.Y >= j * 100 && newState.X <= (i + 1) * 100 + 5 && newState.Y <= (j + 1) * 100;
                        if (numOfShips <= 5)
                        {
                            if (insideGrid && shipPos[i, j] != 2)
                            {
                                numOfShips += 1;
                                shipPos[i, j] += 1;
                            }
                            else { Debug.WriteLine("valde samma"); }
                        }
                        else if (state == 2 && insideGrid)
                        {
                            Debug.WriteLine("visa");
                            shipPos[i, j] += 1;
                        }
                        else { state += 1; }
                    }
                } 
            }
            oldState = newState;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin();
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    if (shipPos[i, j] == 2)
                    {
                        _spriteBatch.Draw(battle_shipTexture, new Vector2(i * 100 + 5, j * 100), null, Color.White, 0.0f, new Vector2(1.5f), new Vector2(0.2f), SpriteEffects.None, 1.0f);
                    } else
                    {
                        _spriteBatch.Draw(line, new Rectangle(i * 100 + 5, j * 100, 90, 90), Color.DarkBlue);
                    }
                }
            }
            _spriteBatch.End();


            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }
    }
}
