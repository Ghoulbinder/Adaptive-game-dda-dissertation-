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
        private PlayerStats playerStats; // PlayerStats instance
        private GameState currentState;

        private Texture2D mainMenuBackground;
        private SpriteFont gameFont;

        private MenuState menuState;
        private const int TileSize = 25;

        private bool showGrid = false;
        private bool showStats = false; // To toggle the display of stats
        private KeyboardState previousKeyboardState;

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
            previousKeyboardState = Keyboard.GetState();
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

            // Load bullet textures
            Texture2D bulletHorizontalTexture = Content.Load<Texture2D>("Images/Projectile/bullet");
            Texture2D bulletVerticalTexture = Content.Load<Texture2D>("Images/Projectile/bullet2");

            // Initialize the player
            player = new Player(
                Content.Load<Texture2D>("Images/Soldier/backWalking"),
                Content.Load<Texture2D>("Images/Soldier/frontWalking"),
                Content.Load<Texture2D>("Images/Soldier/leftWalking"),
                bulletHorizontalTexture,
                bulletVerticalTexture,
                new Vector2(100, 100)
            );

            // Initialize the PlayerStats
            playerStats = new PlayerStats(100, 50, 10, 0, 1, gameFont);

            menuState = new MenuState(gameFont, mainMenuBackground);

            InitializeMaps();

            var largestMap = maps[GameState.GreenForestCentre];
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
                                new Vector2(300, 300),
                                Enemy.Direction.Up),
                            new Enemy(Content.Load<Texture2D>("Images/Enemy/enemyBackWalking"),
                                Content.Load<Texture2D>("Images/Enemy/enemyFrontWalking"),
                                Content.Load<Texture2D>("Images/Enemy/enemyLeftWalking"),
                                new Vector2(600, 500),
                                Enemy.Direction.Down)
                        })
                },
                {
                    GameState.ForestTop,
                    new Map(Content.Load<Texture2D>("Images/Maps/snowForestTop2"),
                        new List<Enemy>
                        {
                            new Enemy(Content.Load<Texture2D>("Images/Enemy/enemyBackWalking"),
                                Content.Load<Texture2D>("Images/Enemy/enemyFrontWalking"),
                                Content.Load<Texture2D>("Images/Enemy/enemyLeftWalking"),
                                new Vector2(400, 200),
                                Enemy.Direction.Left),
                            new Enemy(Content.Load<Texture2D>("Images/Enemy/enemyBackWalking"),
                                Content.Load<Texture2D>("Images/Enemy/enemyFrontWalking"),
                                Content.Load<Texture2D>("Images/Enemy/enemyLeftWalking"),
                                new Vector2(700, 300),
                                Enemy.Direction.Right)
                        })
                },
                {
                    GameState.ForestButtom,
                    new Map(Content.Load<Texture2D>("Images/Maps/snowForestButtom2"),
                        new List<Enemy>
                        {
                            new Enemy(Content.Load<Texture2D>("Images/Enemy/enemyBackWalking"),
                                Content.Load<Texture2D>("Images/Enemy/enemyFrontWalking"),
                                Content.Load<Texture2D>("Images/Enemy/enemyLeftWalking"),
                                new Vector2(500, 700),
                                Enemy.Direction.Up)
                        })
                },
                {
                    GameState.ForestLeft,
                    new Map(Content.Load<Texture2D>("Images/Maps/snowForestLeft2"),
                        new List<Enemy>
                        {
                            new Enemy(Content.Load<Texture2D>("Images/Enemy/enemyBackWalking"),
                                Content.Load<Texture2D>("Images/Enemy/enemyFrontWalking"),
                                Content.Load<Texture2D>("Images/Enemy/enemyLeftWalking"),
                                new Vector2(300, 500),
                                Enemy.Direction.Right)
                        })
                },
                {
                    GameState.ForestRight,
                    new Map(Content.Load<Texture2D>("Images/Maps/snowForestRight2"),
                        new List<Enemy>
                        {
                            new Enemy(Content.Load<Texture2D>("Images/Enemy/enemyBackWalking"),
                                Content.Load<Texture2D>("Images/Enemy/enemyFrontWalking"),
                                Content.Load<Texture2D>("Images/Enemy/enemyLeftWalking"),
                                new Vector2(800, 400),
                                Enemy.Direction.Left)
                        })
                }
            };
        }

        protected override void Update(GameTime gameTime)
        {
            var currentKeyboardState = Keyboard.GetState();

            // Exit game
            if (currentKeyboardState.IsKeyDown(Keys.Escape))
                Exit();

            // Toggle grid with G
            if (currentKeyboardState.IsKeyDown(Keys.G) && previousKeyboardState.IsKeyUp(Keys.G))
                showGrid = !showGrid;

            // Toggle stats with Tab
            if (currentKeyboardState.IsKeyDown(Keys.Tab) && previousKeyboardState.IsKeyUp(Keys.Tab))
                showStats = !showStats;

            // Start the game from the main menu
            if (currentState == GameState.MainMenu && currentKeyboardState.IsKeyDown(Keys.Enter))
                currentState = GameState.GreenForestCentre;

            // Game state logic
            if (currentState != GameState.MainMenu)
            {
                var currentMap = maps[currentState];

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

            previousKeyboardState = currentKeyboardState;
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin();

            // Draw the main menu or game content
            if (currentState == GameState.MainMenu)
            {
                _spriteBatch.Draw(mainMenuBackground, Vector2.Zero, Color.White);
            }
            else
            {
                var currentMap = maps[currentState];

                float scale = (float)_graphics.PreferredBackBufferHeight / currentMap.Background.Height;
                float scaledWidth = currentMap.Background.Width * scale;
                Vector2 position = new Vector2(
                    (_graphics.PreferredBackBufferWidth - scaledWidth) / 2,
                    0
                );

                _spriteBatch.Draw(
                    currentMap.Background,
                    position,
                    null,
                    Color.White,
                    0f,
                    Vector2.Zero,
                    new Vector2(scale, scale),
                    SpriteEffects.None,
                    0f
                );

                currentMap.DrawEnemies(_spriteBatch);
            }

            // Draw the player
            player.Draw(_spriteBatch);

            // Draw stats if Tab is pressed
            if (showStats)
            {
                playerStats.Draw(_spriteBatch, new Vector2(20, 20)); // Position at top-left corner
            }

            _spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
