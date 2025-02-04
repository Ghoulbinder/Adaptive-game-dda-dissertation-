using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Survivor_of_the_Bulge
{
    public enum GameState
    {
        MainMenu,
        GreenForestCentre,
        ForestTop,
        ForestButtom,
        ForestLeft,
        ForestRight
    }

    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private Player player;
        private GameState currentState;

        private Texture2D mainMenuBackground;
        private SpriteFont gameFont;

        private MenuState menuState;
        private const int TileSize = 25;

        private bool showGrid = false;

        private List<Transition> transitions;
        private Dictionary<GameState, Map> maps;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            _graphics.PreferredBackBufferWidth = 1600;
            _graphics.PreferredBackBufferHeight = 980;
            _graphics.ApplyChanges();
        }

        protected override void Initialize()
        {
            currentState = GameState.MainMenu;

            InitializeTransitions();
            base.Initialize();
        }

        private void InitializeTransitions()
        {
            transitions = new List<Transition>
            {
                new Transition(GameState.GreenForestCentre, GameState.ForestTop, new Rectangle(0, 0, _graphics.PreferredBackBufferWidth, TileSize * 2)),
                new Transition(GameState.ForestTop, GameState.GreenForestCentre, new Rectangle(0, _graphics.PreferredBackBufferHeight - (TileSize * 2), _graphics.PreferredBackBufferWidth, TileSize * 2)),
                new Transition(GameState.GreenForestCentre, GameState.ForestButtom, new Rectangle(0, _graphics.PreferredBackBufferHeight - (TileSize * 2), _graphics.PreferredBackBufferWidth, TileSize * 2)),
                new Transition(GameState.ForestButtom, GameState.GreenForestCentre, new Rectangle(0, 0, _graphics.PreferredBackBufferWidth, TileSize * 2)),
                new Transition(GameState.GreenForestCentre, GameState.ForestLeft, new Rectangle(0, 0, TileSize * 2, _graphics.PreferredBackBufferHeight)),
                new Transition(GameState.ForestLeft, GameState.GreenForestCentre, new Rectangle(_graphics.PreferredBackBufferWidth - (TileSize * 2), 0, TileSize * 2, _graphics.PreferredBackBufferHeight)),
                new Transition(GameState.GreenForestCentre, GameState.ForestRight, new Rectangle(_graphics.PreferredBackBufferWidth - (TileSize * 2), 0, TileSize * 2, _graphics.PreferredBackBufferHeight)),
                new Transition(GameState.ForestRight, GameState.GreenForestCentre, new Rectangle(0, 0, TileSize * 2, _graphics.PreferredBackBufferHeight))
            };
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            mainMenuBackground = Content.Load<Texture2D>("Images/Maps/mmBackground2");
            gameFont = Content.Load<SpriteFont>("Fonts/jungleFont");

            player = new Player(
                Content.Load<Texture2D>("Images/Soldier/backWalking"),
                Content.Load<Texture2D>("Images/Soldier/frontWalking"),
                Content.Load<Texture2D>("Images/Soldier/leftWalking"),
                new Vector2(100, 100)
            );

            menuState = new MenuState(gameFont, mainMenuBackground);

            InitializeMaps();

            // Dynamically adjust the window size to fit the largest map
            var largestMap = maps[GameState.GreenForestCentre]; // Replace with logic to find the largest map
            _graphics.PreferredBackBufferWidth = largestMap.Background.Width;
            _graphics.PreferredBackBufferHeight = largestMap.Background.Height;
            _graphics.ApplyChanges();
        }

        private void InitializeMaps()
        {
            maps = new Dictionary<GameState, Map>
            {
                {
                    GameState.GreenForestCentre,
                    new Map(Content.Load<Texture2D>("Images/Maps/greenForestCentre2"),
                        new List<Enemy>
                        {
                            new Enemy(Content.Load<Texture2D>("Images/Enemy/enemyBackWalking"),
                                Content.Load<Texture2D>("Images/Enemy/enemyFrontWalking"),
                                Content.Load<Texture2D>("Images/Enemy/enemyLeftWalking"),
                                Content.Load<Texture2D>("Images/Enemy/enemyLeftWalking"), // Mirror for right
                                new Vector2(300, 300),
                                Enemy.Direction.Up),
                            new Enemy(Content.Load<Texture2D>("Images/Enemy/enemyBackWalking"),
                                Content.Load<Texture2D>("Images/Enemy/enemyFrontWalking"),
                                Content.Load<Texture2D>("Images/Enemy/enemyLeftWalking"),
                                Content.Load<Texture2D>("Images/Enemy/enemyLeftWalking"), // Mirror for right
                                new Vector2(600, 500),
                                Enemy.Direction.Down)
                        })
                },
                { GameState.ForestTop, new Map(Content.Load<Texture2D>("Images/Maps/snowForestTop2"), new List<Enemy>()) },
                { GameState.ForestButtom, new Map(Content.Load<Texture2D>("Images/Maps/snowForestButtom2"), new List<Enemy>()) },
                { GameState.ForestLeft, new Map(Content.Load<Texture2D>("Images/Maps/snowForestLeft2"), new List<Enemy>()) },
                { GameState.ForestRight, new Map(Content.Load<Texture2D>("Images/Maps/snowForestRight2"), new List<Enemy>()) }
            };
        }

        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            if (Keyboard.GetState().IsKeyDown(Keys.G))
                showGrid = !showGrid;

            if (Keyboard.GetState().IsKeyDown(Keys.Enter) && currentState == GameState.MainMenu)
                currentState = GameState.GreenForestCentre;

            if (currentState != GameState.MainMenu)
            {
                var currentMap = maps[currentState];

                // Handle map transitions
                Rectangle playerHitbox = new Rectangle((int)player.Position.X, (int)player.Position.Y, TileSize, TileSize);
                foreach (var transition in transitions)
                {
                    if (transition.From == currentState && transition.Zone.Intersects(playerHitbox))
                    {
                        currentState = transition.To;
                        player.Position = new Vector2(
                            _graphics.PreferredBackBufferWidth / 2,
                            _graphics.PreferredBackBufferHeight / 2
                        );
                        break;
                    }
                }

                player.Update(gameTime, _graphics.GraphicsDevice.Viewport);
                currentMap.UpdateEnemies(gameTime, _graphics.GraphicsDevice.Viewport);
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin();

            if (currentState == GameState.MainMenu)
            {
                // Draw the main menu background
                _spriteBatch.Draw(mainMenuBackground, Vector2.Zero, Color.White);
            }
            else
            {
                // Get the current map
                var currentMap = maps[currentState];

                // Calculate the scale to fit height (980) while maintaining aspect ratio
                float scale = (float)_graphics.PreferredBackBufferHeight / currentMap.Background.Height;

                // Calculate the new width after scaling
                float scaledWidth = currentMap.Background.Width * scale;

                // Calculate X and Y offsets for centering the map
                Vector2 position = new Vector2(
                    (_graphics.PreferredBackBufferWidth - scaledWidth) / 2, // Center horizontally
                    0 // Map will fit perfectly vertically
                );

                // Draw the map background
                _spriteBatch.Draw(
                    currentMap.Background,
                    position,
                    null,
                    Color.White,
                    0f,
                    Vector2.Zero,
                    new Vector2(scale, scale), // Apply scaling to X and Y
                    SpriteEffects.None,
                    0f
                );

                // Draw enemies
                currentMap.DrawEnemies(_spriteBatch);
            }

            // Draw player
            player.Draw(_spriteBatch);

            _spriteBatch.End();
            base.Draw(gameTime);
        }



    }

    public class Map
    {
        public Texture2D Background { get; }
        private List<Enemy> Enemies;

        public Map(Texture2D background, List<Enemy> enemies)
        {
            Background = background;
            Enemies = enemies;
        }

        public void UpdateEnemies(GameTime gameTime, Viewport viewport)
        {
            foreach (var enemy in Enemies)
                enemy.Update(gameTime, viewport);
        }

        public void DrawEnemies(SpriteBatch spriteBatch)
        {
            foreach (var enemy in Enemies)
                enemy.Draw(spriteBatch);
        }
    }

    public class Transition
    {
        public GameState From { get; }
        public GameState To { get; }
        public Rectangle Zone { get; }

        public Transition(GameState from, GameState to, Rectangle zone)
        {
            From = from;
            To = to;
            Zone = zone;
        }
    }
}
