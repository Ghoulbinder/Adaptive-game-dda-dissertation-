using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Survivor_of_the_Bulge
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private Player player;
        private Enemy enemy;
        private TreesBox[] obstacles;
        private Silos[] silos;

        private enum GameState { MainMenu, GreenForestCentre, ForestTop, ForestButtom, ForestLeft, ForestRight }
        private GameState currentState;

        private Texture2D mainMenuBackground;
        private Texture2D greenForestBackground;
        private Texture2D forestTopBackground;
        private Texture2D forestButtomBackground;
        private Texture2D forestLeftBackground;
        private Texture2D forestRightBackground;
        private SpriteFont gameFont;

        private MenuState menuState;

        // Original map size
        private const int MapWidth = 1600;
        private const int MapHeight = 1600;

        // Scaling factor
        private float scaleX;
        private float scaleY;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            // Set window size to a more screen-friendly resolution (e.g., 1280x720)
            _graphics.PreferredBackBufferWidth = 1280;
            _graphics.PreferredBackBufferHeight = 720;
            _graphics.ApplyChanges();
        }

        protected override void Initialize()
        {
            currentState = GameState.MainMenu;

            // Calculate scaling factors based on the current window size
            scaleX = (float)_graphics.PreferredBackBufferWidth / MapWidth;
            scaleY = (float)_graphics.PreferredBackBufferHeight / MapHeight;

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // Load Assets
            mainMenuBackground = Content.Load<Texture2D>("Images/Maps/mmBackground");
            greenForestBackground = Content.Load<Texture2D>("Images/Maps/greenForestCentre");
            forestTopBackground = Content.Load<Texture2D>("Images/Maps/snowForestTop");
            forestButtomBackground = Content.Load<Texture2D>("Images/Maps/snowForestButtom");
            forestLeftBackground = Content.Load<Texture2D>("Images/Maps/snowForestLeft");
            forestRightBackground = Content.Load<Texture2D>("Images/Maps/snowForestRight");
            gameFont = Content.Load<SpriteFont>("Fonts/jungleFont");

            // Initialize Player
            player = new Player(
                Content.Load<Texture2D>("Images/Soldier/backWalking"),
                Content.Load<Texture2D>("Images/Soldier/frontWalking"),
                Content.Load<Texture2D>("Images/Soldier/leftWalking"),
                new Vector2(100, 100)
            );

            // Initialize Menu
            menuState = new MenuState(gameFont, mainMenuBackground);

            // Load enemy textures for all directions
            enemy = new Enemy(
                Content.Load<Texture2D>("Images/Enemy/enemyBackWalking"),
                Content.Load<Texture2D>("Images/Enemy/enemyFrontWalking"),
                Content.Load<Texture2D>("Images/Enemy/enemyLeftWalking"),
                Content.Load<Texture2D>("Images/Enemy/enemyLeftWalking"), // Flip this for right walking
                new Vector2(200, 200)
            );

            // Initialize Obstacles
            obstacles = new TreesBox[]
            {
                new TreesBox(Content.Load<Texture2D>("Images/Maps/box"), 300, 300),
                new TreesBox(Content.Load<Texture2D>("Images/Maps/box2"), 400, 400)
            };

            // Initialize Silos
            silos = new Silos[]
            {
                new Silos(Content.Load<Texture2D>("Images/Maps/box3"), 500, 500)
            };
        }

        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            switch (currentState)
            {
                case GameState.MainMenu:
                    if (menuState.Update(gameTime))
                        currentState = GameState.GreenForestCentre;
                    break;

                case GameState.GreenForestCentre:
                    player.Update(gameTime, _graphics.GraphicsDevice.Viewport);

                    // Check Collisions
                    HandleCollisions();

                    // Handle State Transitions
                    if (player.Position.Y <= 50)
                    {
                        currentState = GameState.ForestTop;
                    }
                    else if (player.Position.Y >= GraphicsDevice.Viewport.Height - 50)
                    {
                        currentState = GameState.ForestButtom;
                    }
                    else if (player.Position.X <= 50)
                    {
                        currentState = GameState.ForestLeft;
                    }
                    else if (player.Position.X >= GraphicsDevice.Viewport.Width - 50)
                    {
                        currentState = GameState.ForestRight;
                    }
                    break;

                case GameState.ForestTop:
                    player.Update(gameTime, _graphics.GraphicsDevice.Viewport);
                    if (player.Position.Y >= GraphicsDevice.Viewport.Height - 50)
                        currentState = GameState.GreenForestCentre;
                    break;

                case GameState.ForestButtom:
                    player.Update(gameTime, _graphics.GraphicsDevice.Viewport);
                    if (player.Position.Y <= 50)
                        currentState = GameState.GreenForestCentre;
                    break;

                case GameState.ForestLeft:
                    player.Update(gameTime, _graphics.GraphicsDevice.Viewport);
                    if (player.Position.X >= GraphicsDevice.Viewport.Width - 50)
                        currentState = GameState.GreenForestCentre;
                    break;

                case GameState.ForestRight:
                    player.Update(gameTime, _graphics.GraphicsDevice.Viewport);
                    if (player.Position.X <= 50)
                        currentState = GameState.GreenForestCentre;
                    break;
            }

            base.Update(gameTime);
        }

        private void HandleCollisions()
        {
            // Player vs Obstacles
            foreach (var box in obstacles)
            {
                if (player.Bounds.Intersects(box.Bounds))
                {
                    // Prevent movement through obstacles
                    player.ResolveCollision(box.Bounds);
                }
            }

            // Player vs Silos
            foreach (var silo in silos)
            {
                if (player.Bounds.Intersects(silo.Bounds))
                {
                    // Logic for silo interaction
                    Console.WriteLine("Player hit a silo!");
                }
            }

            // Player vs Enemy
            if (player.Bounds.Intersects(enemy.Bounds))
            {
                // Logic for enemy collision
                Console.WriteLine("Player hit an enemy!");
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin();

            switch (currentState)
            {
                case GameState.MainMenu:
                    _spriteBatch.Draw(mainMenuBackground, Vector2.Zero, null, Color.White, 0f, Vector2.Zero, new Vector2(scaleX, scaleY), SpriteEffects.None, 0f);
                    break;

                case GameState.GreenForestCentre:
                    _spriteBatch.Draw(greenForestBackground, Vector2.Zero, null, Color.White, 0f, Vector2.Zero, new Vector2(scaleX, scaleY), SpriteEffects.None, 0f);
                    player.Draw(_spriteBatch);
                    enemy.Draw(_spriteBatch);
                    foreach (var box in obstacles)
                        box.Draw(_spriteBatch);
                    foreach (var silo in silos)
                        silo.Draw(_spriteBatch);
                    break;

                case GameState.ForestTop:
                    _spriteBatch.Draw(forestTopBackground, Vector2.Zero, null, Color.White, 0f, Vector2.Zero, new Vector2(scaleX, scaleY), SpriteEffects.None, 0f);
                    player.Draw(_spriteBatch);
                    break;

                case GameState.ForestButtom:
                    _spriteBatch.Draw(forestButtomBackground, Vector2.Zero, null, Color.White, 0f, Vector2.Zero, new Vector2(scaleX, scaleY), SpriteEffects.None, 0f);
                    player.Draw(_spriteBatch);
                    break;

                case GameState.ForestLeft:
                    _spriteBatch.Draw(forestLeftBackground, Vector2.Zero, null, Color.White, 0f, Vector2.Zero, new Vector2(scaleX, scaleY), SpriteEffects.None, 0f);
                    player.Draw(_spriteBatch);
                    break;

                case GameState.ForestRight:
                    _spriteBatch.Draw(forestRightBackground, Vector2.Zero, null, Color.White, 0f, Vector2.Zero, new Vector2(scaleX, scaleY), SpriteEffects.None, 0f);
                    player.Draw(_spriteBatch);
                    break;
            }

            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
