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

        // Weather effects.
        private List<FallingLeaf> fallingLeaves;
        private List<SnowFlake> snowFlakes;
        private Random random = new Random();

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

            // Create the player with modular parameters.
            player = new Player(
                Content.Load<Texture2D>("Images/Soldier/backWalking"),
                Content.Load<Texture2D>("Images/Soldier/frontWalking"),
                Content.Load<Texture2D>("Images/Soldier/leftWalking"),
                bulletHorizontalTexture,
                bulletVerticalTexture,
                new Vector2(100, 100)
            );

            // Initialize the player stats.
            playerStats = new PlayerStats(100, 50, 10, 0, 1, gameFont);
            menuState = new MenuState(gameFont, mainMenuBackground);

            // Initialize maps (only GreenForestCentre will have enemies).
            InitializeMaps();

            // After maps are initialized, add a Boss to the GreenForestCentre map.
            Boss boss = new Boss(
                Content.Load<Texture2D>("Images/Enemy/enemyBackWalking"),
                Content.Load<Texture2D>("Images/Enemy/enemyFrontWalking"),
                Content.Load<Texture2D>("Images/Enemy/enemyLeftWalking"),
                Content.Load<Texture2D>("Images/Projectile/bullet"),
                Content.Load<Texture2D>("Images/Projectile/bullet2"),
                new Vector2(300, 300),
                Boss.Direction.Up,
                300,  // Boss health
                15    // Boss bullet damage
            );
            maps[GameState.GreenForestCentre].AddEnemy(boss);

            // Adjust viewport based on the largest map's background.
            var largestMap = maps[GameState.GreenForestCentre];
            _graphics.PreferredBackBufferWidth = largestMap.Background.Width;
            _graphics.PreferredBackBufferHeight = largestMap.Background.Height;
            _graphics.ApplyChanges();

            // Load weather textures and create falling particles.
            Texture2D leafTexture = Content.Load<Texture2D>("Images/Maps/tinyleaf");
            Texture2D snowTexture = Content.Load<Texture2D>("Images/Maps/snowFlake");

            int particleCount = 100;
            fallingLeaves = new List<FallingLeaf>();
            snowFlakes = new List<SnowFlake>();

            for (int i = 0; i < particleCount; i++)
            {
                fallingLeaves.Add(new FallingLeaf(leafTexture, random, _graphics.PreferredBackBufferWidth));
                snowFlakes.Add(new SnowFlake(snowTexture, random, _graphics.PreferredBackBufferWidth));
            }
        }

        private void SpawnEnemiesForMap(
            Map map,
            int enemyCount,
            Texture2D enemyBackTexture,
            Texture2D enemyFrontTexture,
            Texture2D enemyLeftTexture,
            Texture2D enemyBulletHorizontal,
            Texture2D enemyBulletVertical)
        {
            int totalFrames = 4;
            int enemyWidth = enemyLeftTexture.Width / totalFrames;
            int enemyHeight = enemyLeftTexture.Height;

            for (int i = 0; i < enemyCount; i++)
            {
                bool validPosition = false;
                int attempts = 0;
                int x = 0, y = 0;
                int maxAttempts = 50;

                while (!validPosition && attempts < maxAttempts)
                {
                    x = random.Next(0, Math.Max(1, map.Background.Width - enemyWidth));
                    y = random.Next(0, Math.Max(1, map.Background.Height - enemyHeight));
                    Rectangle candidate = new Rectangle(x, y, enemyWidth, enemyHeight);

                    validPosition = true;
                    foreach (Enemy existingEnemy in map.Enemies)
                    {
                        if (candidate.Intersects(existingEnemy.Bounds))
                        {
                            validPosition = false;
                            break;
                        }
                    }
                    attempts++;
                }

                Array directions = Enum.GetValues(typeof(Enemy.Direction));
                Enemy.Direction randomDirection = (Enemy.Direction)directions.GetValue(random.Next(directions.Length));

                int baseHealth = 50;
                int baseDamage = 5;
                int enemyHealth = (int)(baseHealth * DifficultyManager.Instance.EnemyHealthMultiplier);
                int enemyDamage = (int)(baseDamage * DifficultyManager.Instance.EnemyDamageMultiplier);

                Enemy enemy = new Enemy(
                    enemyBackTexture,
                    enemyFrontTexture,
                    enemyLeftTexture,
                    enemyBulletHorizontal,
                    enemyBulletVertical,
                    new Vector2(x, y),
                    randomDirection,
                    enemyHealth,
                    enemyDamage
                );
                map.AddEnemy(enemy);
            }
        }

        private void InitializeMaps()
        {
            // We only want enemies (and the boss) on the GreenForestCentre map.
            Texture2D enemyBulletHorizontal = Content.Load<Texture2D>("Images/Projectile/bullet");
            Texture2D enemyBulletVertical = Content.Load<Texture2D>("Images/Projectile/bullet2");

            Texture2D enemyBackTexture = Content.Load<Texture2D>("Images/Enemy/enemyBackWalking");
            Texture2D enemyFrontTexture = Content.Load<Texture2D>("Images/Enemy/enemyFrontWalking");
            Texture2D enemyLeftTexture = Content.Load<Texture2D>("Images/Enemy/enemyLeftWalking");

            // Create maps. Only GreenForestCentre will have enemies; others are empty.
            maps = new Dictionary<GameState, Map>
            {
                { GameState.GreenForestCentre, new Map(Content.Load<Texture2D>("Images/Maps/greenForestCentre2"), new List<Enemy>()) },
                { GameState.ForestTop, new Map(Content.Load<Texture2D>("Images/Maps/snowForestTop2"), new List<Enemy>()) },
                { GameState.ForestLeft, new Map(Content.Load<Texture2D>("Images/Maps/snowForestLeft2"), new List<Enemy>()) },
                { GameState.ForestButtom, new Map(Content.Load<Texture2D>("Images/Maps/snowForestButtom2"), new List<Enemy>()) },
                { GameState.ForestRight, new Map(Content.Load<Texture2D>("Images/Maps/snowForestRight2"), new List<Enemy>()) }
            };

            // Only spawn enemies for the GreenForestCentre map.
            int enemyCountPerMap = DifficultyManager.Instance.BaseEnemyCount;
            SpawnEnemiesForMap(maps[GameState.GreenForestCentre], enemyCountPerMap, enemyBackTexture, enemyFrontTexture, enemyLeftTexture, enemyBulletHorizontal, enemyBulletVertical);
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

                // Update player and UI stats.
                player.Update(gameTime, _graphics.GraphicsDevice.Viewport, currentMap.Enemies);
                playerStats.UpdateHealth(player.Health);

                // Update each enemy.
                foreach (var enemy in currentMap.Enemies)
                {
                    enemy.Update(gameTime, _graphics.GraphicsDevice.Viewport, player.Position, player);
                }
                currentMap.Enemies.RemoveAll(e => e.IsDead);

                // Update weather effects.
                foreach (var leaf in fallingLeaves)
                    leaf.Update(_graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);
                foreach (var snow in snowFlakes)
                    snow.Update(_graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);

                // Process map transitions.
                foreach (var transition in transitions)
                {
                    if (transition.From == currentState && transition.Zone.Intersects(player.Bounds))
                    {
                        if (maps.ContainsKey(transition.To))
                        {
                            var newMap = maps[transition.To];
                            currentState = transition.To;
                            player.Position = new Vector2(
                                (newMap.Background.Width - player.Bounds.Width) / 2,
                                (newMap.Background.Height - player.Bounds.Height) / 2
                            );
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
                _spriteBatch.Draw(mainMenuBackground, Vector2.Zero, Color.White);
                menuState.Draw(_spriteBatch);
            }
            else
            {
                var currentMap = maps[currentState];
                _spriteBatch.Draw(currentMap.Background, Vector2.Zero, Color.White);

                // Draw weather effects.
                foreach (var snow in snowFlakes)
                    snow.Draw(_spriteBatch);
                foreach (var leaf in fallingLeaves)
                    leaf.Draw(_spriteBatch);

                player.Draw(_spriteBatch);

                foreach (var enemy in currentMap.Enemies)
                {
                    enemy.Draw(_spriteBatch);
                }

                playerStats.Draw(_spriteBatch, new Vector2(10, 10));

                if (showGrid)
                    DrawGrid();

                if (showStats)
                    DrawDebugStats();
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
            string debugText = $"Current State: {currentState}\nPlayer Position: {player.Position}\nDifficulty Level: {DifficultyManager.Instance.Level}";
            _spriteBatch.DrawString(gameFont, debugText, new Vector2(10, 100), Color.White);
        }
    }
}
