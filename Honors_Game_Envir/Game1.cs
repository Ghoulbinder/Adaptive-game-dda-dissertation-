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
        private PlayerStats playerStats;
        private GameState currentState;

        private Texture2D mainMenuBackground;
        private SpriteFont gameFont;

        private MenuState menuState;
        private const int TileSize = 25;

        private bool showGrid = false;
        private bool showStats = false;
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

            Texture2D bulletHorizontalTexture = Content.Load<Texture2D>("Images/Projectile/bullet");
            Texture2D bulletVerticalTexture = Content.Load<Texture2D>("Images/Projectile/bullet2");

            player = new Player(
                Content.Load<Texture2D>("Images/Soldier/backWalking"),
                Content.Load<Texture2D>("Images/Soldier/frontWalking"),
                Content.Load<Texture2D>("Images/Soldier/leftWalking"),
                bulletHorizontalTexture,
                bulletVerticalTexture,
                new Vector2(100, 100)
            );

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
            Texture2D enemyBulletHorizontal = Content.Load<Texture2D>("Images/Projectile/bullet");
            Texture2D enemyBulletVertical = Content.Load<Texture2D>("Images/Projectile/bullet2");

            maps = new Dictionary<GameState, Map>
    {
        {
            GameState.GreenForestCentre,
            new Map(Content.Load<Texture2D>("Images/Maps/greenForestCentre2"),
                new List<Enemy>
                {
                    new Enemy(
                        Content.Load<Texture2D>("Images/Enemy/enemyBackWalking"),
                        Content.Load<Texture2D>("Images/Enemy/enemyFrontWalking"),
                        Content.Load<Texture2D>("Images/Enemy/enemyLeftWalking"),
                        enemyBulletHorizontal,
                        enemyBulletVertical,
                        new Vector2(300, 300),
                        Enemy.Direction.Up,
                        50,
                        5
                    )
                })
        },
        {
            GameState.ForestTop,
            new Map(Content.Load<Texture2D>("Images/Maps/snowForestTop2"),
                new List<Enemy>
                {
                    new Enemy(
                        Content.Load<Texture2D>("Images/Enemy/enemyBackWalking"),
                        Content.Load<Texture2D>("Images/Enemy/enemyFrontWalking"),
                        Content.Load<Texture2D>("Images/Enemy/enemyLeftWalking"),
                        enemyBulletHorizontal,
                        enemyBulletVertical,
                        new Vector2(400, 200),
                        Enemy.Direction.Left,
                        70,
                        10
                    )
                })
        },
        {
            GameState.ForestLeft, // ✅ Fix: Ensure this exists
            new Map(Content.Load<Texture2D>("Images/Maps/snowForestLeft2"),
                new List<Enemy>
                {
                    new Enemy(
                        Content.Load<Texture2D>("Images/Enemy/enemyBackWalking"),
                        Content.Load<Texture2D>("Images/Enemy/enemyFrontWalking"),
                        Content.Load<Texture2D>("Images/Enemy/enemyLeftWalking"),
                        enemyBulletHorizontal,
                        enemyBulletVertical,
                        new Vector2(250, 350),
                        Enemy.Direction.Right,
                        60,
                        8
                    )
                })
        },
        {
            GameState.ForestButtom, // ✅ Added missing map
            new Map(Content.Load<Texture2D>("Images/Maps/snowForestButtom2"),
                new List<Enemy>
                {
                    new Enemy(
                        Content.Load<Texture2D>("Images/Enemy/enemyBackWalking"),
                        Content.Load<Texture2D>("Images/Enemy/enemyFrontWalking"),
                        Content.Load<Texture2D>("Images/Enemy/enemyLeftWalking"),
                        enemyBulletHorizontal,
                        enemyBulletVertical,
                        new Vector2(350, 400),
                        Enemy.Direction.Down,
                        55,
                        7
                    )
                })
        },
        {
            GameState.ForestRight, // ✅ Added missing map
            new Map(Content.Load<Texture2D>("Images/Maps/snowForestRight2"),
                new List<Enemy>
                {
                    new Enemy(
                        Content.Load<Texture2D>("Images/Enemy/enemyBackWalking"),
                        Content.Load<Texture2D>("Images/Enemy/enemyFrontWalking"),
                        Content.Load<Texture2D>("Images/Enemy/enemyLeftWalking"),
                        enemyBulletHorizontal,
                        enemyBulletVertical,
                        new Vector2(500, 300),
                        Enemy.Direction.Left,
                        65,
                        9
                    )
                })
        }
    };
        }


        protected override void Update(GameTime gameTime)
        {
            var currentKeyboardState = Keyboard.GetState();

            if (currentKeyboardState.IsKeyDown(Keys.Escape))
                Exit();

            if (currentKeyboardState.IsKeyDown(Keys.G) && previousKeyboardState.IsKeyUp(Keys.G))
                showGrid = !showGrid;

            if (currentKeyboardState.IsKeyDown(Keys.Tab) && previousKeyboardState.IsKeyUp(Keys.Tab))
                showStats = !showStats;

            if (currentState == GameState.MainMenu && currentKeyboardState.IsKeyDown(Keys.Enter))
                currentState = GameState.GreenForestCentre;

            if (currentState != GameState.MainMenu)
            {
                var currentMap = maps[currentState];

                player.Update(gameTime, _graphics.GraphicsDevice.Viewport, currentMap.Enemies);

                foreach (var enemy in currentMap.Enemies)
                {
                    enemy.Update(gameTime, _graphics.GraphicsDevice.Viewport, player.Position, player);
                }

                foreach (var transition in transitions)
                {
                    if (transition.From == currentState && transition.Zone.Intersects(player.Bounds))
                    {
                        if (maps.ContainsKey(transition.To)) // ✅ Fix: Ensure map exists before switching
                        {
                            currentState = transition.To;
                            player.Position = new Vector2(100, 100);
                        }
                        break;
                    }
                }
            }

            previousKeyboardState = currentKeyboardState;
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

                // Draw menu state (e.g., menu options)
                menuState.Draw(_spriteBatch);
            }
            else
            {
                // Draw the current map's background
                var currentMap = maps[currentState];
                _spriteBatch.Draw(currentMap.Background, Vector2.Zero, Color.White);

                // Draw the player
                player.Draw(_spriteBatch);

                // Draw all enemies
                foreach (var enemy in currentMap.Enemies)
                {
                    enemy.Draw(_spriteBatch);
                }

                // Draw player stats in the top-left corner
                playerStats.Draw(_spriteBatch, new Vector2(10, 10));

                // Optionally draw a grid for debugging purposes
                if (showGrid)
                {
                    DrawGrid();
                }

                // Show debugging stats or additional information if toggled
                if (showStats)
                {
                    DrawDebugStats();
                }
            }

            _spriteBatch.End();

            base.Draw(gameTime);
        }
        private void DrawGrid()
        {
            Texture2D gridTexture = new Texture2D(GraphicsDevice, 1, 1);
            gridTexture.SetData(new[] { Color.Gray });

            for (int x = 0; x < _graphics.PreferredBackBufferWidth; x += TileSize)
            {
                _spriteBatch.Draw(gridTexture, new Rectangle(x, 0, 1, _graphics.PreferredBackBufferHeight), Color.Gray);
            }

            for (int y = 0; y < _graphics.PreferredBackBufferHeight; y += TileSize)
            {
                _spriteBatch.Draw(gridTexture, new Rectangle(0, y, _graphics.PreferredBackBufferWidth, 1), Color.Gray);
            }
        }
        private void DrawDebugStats()
        {
            string debugText = $"Current State: {currentState}\nPlayer Position: {player.Position}";
            _spriteBatch.DrawString(gameFont, debugText, new Vector2(10, 100), Color.White);
        }

    }
}
