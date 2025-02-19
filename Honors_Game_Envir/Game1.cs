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
        ForestLeft,
        ForestButtom,
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

        private MenuState mainMenu;
        private PauseMenu pauseMenu;
        private const int TileSize = 25;

        private KeyboardState previousKeyboardState;

        private List<Transition> transitions;
        private Dictionary<GameState, Map> maps;

        // Weather effects.
        private List<FallingLeaf> fallingLeaves;
        private List<SnowFlake> snowFlakes;
        private Random random = new Random();

        private bool isPaused = false;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = false;
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

            // Load bullet textures.
            Texture2D bulletTexture1 = Content.Load<Texture2D>("Images/Projectile/bullet");
            Texture2D bulletTexture2 = Content.Load<Texture2D>("Images/Projectile/bullet2");

            // Load player textures (using walk textures for idle and separate attack textures).
            Texture2D walkUp = Content.Load<Texture2D>("Player_Ranged/PlayerRangeWalking/PlayerRangeWalkingUp");
            Texture2D walkDown = Content.Load<Texture2D>("Player_Ranged/PlayerRangeWalking/PlayerRangeWalkingDown");
            Texture2D walkLeft = Content.Load<Texture2D>("Player_Ranged/PlayerRangeWalking/PlayerRangeWalkingLeft");
            Texture2D walkRight = Content.Load<Texture2D>("Player_Ranged/PlayerRangeWalking/PlayerRangeWalkingRight");

            Texture2D attackUp = Content.Load<Texture2D>("Player_Ranged/PlayerRangeAttack/PlayerRangeAttackUp");
            Texture2D attackDown = Content.Load<Texture2D>("Player_Ranged/PlayerRangeAttack/PlayerRangeAttackDown");
            Texture2D attackLeft = Content.Load<Texture2D>("Player_Ranged/PlayerRangeAttack/PlayerRangeAttackLeft");
            Texture2D attackRight = Content.Load<Texture2D>("Player_Ranged/PlayerRangeAttack/PlayerRangeAttackRight");

            // Create player stats and instantiate the player.
            PlayerStats stats = new PlayerStats(100, 3, 50, 1.0f, 200f, 0, 1, gameFont);
            player = new Player(
                walkUp, walkDown, walkLeft, walkRight,
                attackUp, attackDown, attackLeft, attackRight,
                bulletTexture1, bulletTexture2,
                new Vector2(100, 100),
                stats
            );
            player.Scale = 0.3f;
            playerStats = stats;

            mainMenu = new MenuState(gameFont, mainMenuBackground);
            pauseMenu = new PauseMenu(gameFont, CreatePanelTexture());

            InitializeMaps();

            // --- Boss Spawning ---

            // GreenBoss on GreenForestCentre.
            if (!maps[GameState.GreenForestCentre].BossSpawned)
            {
                Vector2 bossPos = new Vector2(
                    (maps[GameState.GreenForestCentre].Background.Width - 256) / 2,
                    (maps[GameState.GreenForestCentre].Background.Height - 256) / 2
                );
                GreenBoss greenBoss = new GreenBoss(
                    Content.Load<Texture2D>("Images/Enemy/enemyBackWalking"),
                    Content.Load<Texture2D>("Images/Enemy/enemyFrontWalking"),
                    Content.Load<Texture2D>("Images/Enemy/enemyLeftWalking"),
                    bulletTexture1, bulletTexture2,
                    bossPos,
                    Boss.Direction.Up,
                    300,
                    15
                );
                maps[GameState.GreenForestCentre].AddEnemy(greenBoss);
                maps[GameState.GreenForestCentre].SetBossSpawned();
            }

            // ButterflyBoss on ForestTop.
            if (!maps[GameState.ForestTop].BossSpawned)
            {
                Vector2 bossPos = new Vector2(
                    (maps[GameState.ForestTop].Background.Width - 256) / 2,
                    (maps[GameState.ForestTop].Background.Height - 256) / 2
                );
                // For ButterflyBoss, we use its attack and walking textures (no idle).
                Texture2D bossAttack = Content.Load<Texture2D>("Butterfly_Boss/ButterflyBossAttack/ButterflyBossDown");
                Texture2D bossWalking = Content.Load<Texture2D>("Butterfly_Boss/ButterflyBossWalking/ButterflyBossWalkingDown");
                // Load butterfly boss bullet textures.
                Texture2D butterflyBulletHorizontal = Content.Load<Texture2D>("Images/Projectile/butterfly_attack");
                Texture2D butterflyBulletVertical = Content.Load<Texture2D>("Images/Projectile/butterfly_attack2");
                ButterflyBoss butterflyBoss = new ButterflyBoss(
                    bossAttack,
                    bossWalking,
                    butterflyBulletHorizontal, butterflyBulletVertical,
                    bossPos,
                    Boss.Direction.Up,
                    300,
                    15
                );
                maps[GameState.ForestTop].AddEnemy(butterflyBoss);
                maps[GameState.ForestTop].SetBossSpawned();
            }

            // DragonBoss on ForestRight.
            if (!maps[GameState.ForestRight].BossSpawned)
            {
                Vector2 bossPos = new Vector2(
                    (maps[GameState.ForestRight].Background.Width - 256) / 2,
                    (maps[GameState.ForestRight].Background.Height - 256) / 2
                );
                Texture2D dragonIdle = Content.Load<Texture2D>("Dragon_Boss/DragoBossIdle/DragoBossIdleDown");
                Texture2D dragonAttack = Content.Load<Texture2D>("Dragon_Boss/DragoBossAttack/DragoBossAttackDown");
                Texture2D dragonWalking = Content.Load<Texture2D>("Dragon_Boss/DragoBossWalking/DragoBossWalkingDown");
                // Load dragon boss bullet textures.
                Texture2D dragonBulletHorizontal = Content.Load<Texture2D>("Images/Projectile/Dragon_Fireball");
                Texture2D dragonBulletVertical = Content.Load<Texture2D>("Images/Projectile/Dragon_Fireball2");
                DragonBoss dragonBoss = new DragonBoss(
                    dragonIdle,
                    dragonAttack,
                    dragonWalking,
                    dragonBulletHorizontal,
                    dragonBulletVertical,
                    bossPos,
                    Boss.Direction.Up,
                    300,
                    15
                );
                maps[GameState.ForestRight].AddEnemy(dragonBoss);
                maps[GameState.ForestRight].SetBossSpawned();
            }

            // OgreBoss on ForestButtom.
            if (!maps[GameState.ForestButtom].BossSpawned)
            {
                Vector2 bossPos = new Vector2(
                    (maps[GameState.ForestButtom].Background.Width - 256) / 2,
                    (maps[GameState.ForestButtom].Background.Height - 256) / 2
                );
                Texture2D ogreIdleUp = Content.Load<Texture2D>("Ogre_Boss/OgreBossIdle/OgreBossIdleUp");
                Texture2D ogreIdleDown = Content.Load<Texture2D>("Ogre_Boss/OgreBossIdle/OgreBossIdleDown");
                Texture2D ogreIdleLeft = Content.Load<Texture2D>("Ogre_Boss/OgreBossIdle/OgreBossIdleLeft");
                Texture2D ogreIdleRight = Content.Load<Texture2D>("Ogre_Boss/OgreBossIdle/OgreBossIdleRight");

                Texture2D ogreAttackUp = Content.Load<Texture2D>("Ogre_Boss/OgreBossAttack/OgreBossAttackUp");
                Texture2D ogreAttackDown = Content.Load<Texture2D>("Ogre_Boss/OgreBossAttack/OgreBossAttackDown");
                Texture2D ogreAttackLeft = Content.Load<Texture2D>("Ogre_Boss/OgreBossAttack/OgreBossAttackLeft");
                Texture2D ogreAttackRight = Content.Load<Texture2D>("Ogre_Boss/OgreBossAttack/OgreBossAttackRight");

                Texture2D ogreWalkingUp = Content.Load<Texture2D>("Ogre_Boss/OgreBossWalking/OgreBossWalkingUp");
                Texture2D ogreWalkingDown = Content.Load<Texture2D>("Ogre_Boss/OgreBossWalking/OgreBossWalkingDown");
                Texture2D ogreWalkingLeft = Content.Load<Texture2D>("Ogre_Boss/OgreBossWalking/OgreBossWalkingLeft");
                Texture2D ogreWalkingRight = Content.Load<Texture2D>("Ogre_Boss/OgreBossWalking/OgreBossWalkingRight");

                OgreBoss ogreBoss = new OgreBoss(
                    ogreIdleUp, ogreIdleDown, ogreIdleLeft, ogreIdleRight,
                    ogreAttackUp, ogreAttackDown, ogreAttackLeft, ogreAttackRight,
                    ogreWalkingUp, ogreWalkingDown, ogreWalkingLeft, ogreWalkingRight,
                    bulletTexture1, bulletTexture2,
                    bossPos,
                    Boss.Direction.Up,
                    300,
                    15
                );
                maps[GameState.ForestButtom].AddEnemy(ogreBoss);
                maps[GameState.ForestButtom].SetBossSpawned();
            }


            // SpiderBoss on ForestLeft.
            if (!maps[GameState.ForestLeft].BossSpawned)
            {
                Vector2 bossPos = new Vector2(
                    (maps[GameState.ForestLeft].Background.Width - 256) / 2,
                    (maps[GameState.ForestLeft].Background.Height - 256) / 2
                );
                Texture2D spiderIdle = Content.Load<Texture2D>("Spider_Boss/SpiderBossIdle/SpiderBossIdleDown");
                Texture2D spiderAttack = Content.Load<Texture2D>("Spider_Boss/SpiderBossAttack/SpiderBossAttackDown");
                Texture2D spiderWalking = Content.Load<Texture2D>("Spider_Boss/SpiderBossWalking/SpiderBossWalkingDown");
                Texture2D spiderBulletHorizontal = Content.Load<Texture2D>("Images/Projectile/spider_attack");
                Texture2D spiderBulletVertical = Content.Load<Texture2D>("Images/Projectile/spider_attack2");
                SpiderBoss spiderBoss = new SpiderBoss(
                    spiderIdle,
                    spiderAttack,
                    spiderWalking,
                    spiderBulletHorizontal, spiderBulletVertical,
                    bossPos,
                    Boss.Direction.Up,
                    300,
                    15
                );
                maps[GameState.ForestLeft].AddEnemy(spiderBoss);
                maps[GameState.ForestLeft].SetBossSpawned();
            }

            // Adjust viewport based on the GreenForestCentre background.
            var largestMap = maps[GameState.GreenForestCentre];
            _graphics.PreferredBackBufferWidth = largestMap.Background.Width;
            _graphics.PreferredBackBufferHeight = largestMap.Background.Height;
            _graphics.ApplyChanges();

            // Load weather textures.
            Texture2D leafTexture = Content.Load<Texture2D>("Images/Maps/tinyleaf");
            Texture2D snowTexture = Content.Load<Texture2D>("Images/Maps/snowFlake");
            int particleCount = 20;
            fallingLeaves = new List<FallingLeaf>();
            snowFlakes = new List<SnowFlake>();
            for (int i = 0; i < particleCount; i++)
            {
                fallingLeaves.Add(new FallingLeaf(leafTexture, random, _graphics.PreferredBackBufferWidth));
                snowFlakes.Add(new SnowFlake(snowTexture, random, _graphics.PreferredBackBufferWidth));
            }
        }

        private Texture2D CreatePanelTexture()
        {
            Texture2D panelTexture = new Texture2D(GraphicsDevice, 1, 1);
            panelTexture.SetData(new[] { Color.Gray });
            return panelTexture;
        }

        private void InitializeMaps()
        {
            maps = new Dictionary<GameState, Map>
            {
                { GameState.GreenForestCentre, new Map(Content.Load<Texture2D>("Images/Maps/greenForestCentre2"), new List<Enemy>()) },
                { GameState.ForestTop, new Map(Content.Load<Texture2D>("Images/Maps/snowForestTop2"), new List<Enemy>()) },
                { GameState.ForestLeft, new Map(Content.Load<Texture2D>("Images/Maps/snowForestLeft2"), new List<Enemy>()) },
                { GameState.ForestButtom, new Map(Content.Load<Texture2D>("Images/Maps/snowForestButtom2"), new List<Enemy>()) },
                { GameState.ForestRight, new Map(Content.Load<Texture2D>("Images/Maps/snowForestRight2"), new List<Enemy>()) }
            };

            Texture2D enemyBulletHorizontal = Content.Load<Texture2D>("Images/Projectile/bullet");
            Texture2D enemyBulletVertical = Content.Load<Texture2D>("Images/Projectile/bullet2");
            Texture2D enemyBack = Content.Load<Texture2D>("Images/Enemy/enemyBackWalking");
            Texture2D enemyFront = Content.Load<Texture2D>("Images/Enemy/enemyFrontWalking");
            Texture2D enemyLeft = Content.Load<Texture2D>("Images/Enemy/enemyLeftWalking");

            int initialEnemyCount = 2;
            SpawnEnemiesForMap(maps[GameState.GreenForestCentre], initialEnemyCount,
                               enemyBack, enemyFront, enemyLeft,
                               enemyBulletHorizontal, enemyBulletVertical);

            foreach (var map in maps.Values)
            {
                map.SetEnemySpawnParameters(enemyBack, enemyFront, enemyLeft, enemyBulletHorizontal, enemyBulletVertical);
            }
        }

        private void SpawnEnemiesForMap(Map map, int enemyCount,
                                        Texture2D enemyBack,
                                        Texture2D enemyFront,
                                        Texture2D enemyLeft,
                                        Texture2D enemyBulletHorizontal,
                                        Texture2D enemyBulletVertical)
        {
            Random rng = new Random();
            for (int i = 0; i < enemyCount; i++)
            {
                int x = rng.Next(0, map.Background.Width - 64);
                int y = rng.Next(0, map.Background.Height - 64);
                Array dirs = Enum.GetValues(typeof(Enemy.Direction));
                Enemy.Direction dir = (Enemy.Direction)dirs.GetValue(rng.Next(dirs.Length));

                int baseHealth = 50;
                int baseDamage = 5;
                int finalHealth = (int)(baseHealth * DifficultyManager.Instance.EnemyHealthMultiplier);
                int finalDamage = (int)(baseDamage * DifficultyManager.Instance.EnemyDamageMultiplier);

                Enemy e = new Enemy(
                    enemyBack,
                    enemyFront,
                    enemyLeft,
                    enemyBulletHorizontal,
                    enemyBulletVertical,
                    new Vector2(x, y),
                    dir,
                    finalHealth,
                    finalDamage
                );
                map.AddEnemy(e);
            }
        }

        protected override void Update(GameTime gameTime)
        {
            var currentKeyboardState = Keyboard.GetState();

            if (currentKeyboardState.IsKeyDown(Keys.Tab) && previousKeyboardState.IsKeyUp(Keys.Tab))
            {
                isPaused = !isPaused;
                IsMouseVisible = isPaused;
            }

            if (currentKeyboardState.IsKeyDown(Keys.Escape))
                Exit();

            if (currentState == GameState.MainMenu && currentKeyboardState.IsKeyDown(Keys.Enter))
            {
                currentState = GameState.GreenForestCentre;
            }

            if (isPaused)
            {
                pauseMenu.Update(gameTime);
            }
            else if (currentState != GameState.MainMenu)
            {
                var currentMap = maps[currentState];

                player.Update(gameTime, _graphics.GraphicsDevice.Viewport, currentMap.Enemies);
                playerStats.UpdateHealth(player.Health);

                for (int i = currentMap.Enemies.Count - 1; i >= 0; i--)
                {
                    if (currentMap.Enemies[i].IsDead)
                    {
                        currentMap.IncrementKillCount();
                        currentMap.Enemies.RemoveAt(i);
                    }
                }

                foreach (var enemy in currentMap.Enemies)
                {
                    enemy.Update(gameTime, _graphics.GraphicsDevice.Viewport, player.Position, player);
                }

                currentMap.UpdateRespawn(gameTime);

                foreach (var leaf in fallingLeaves)
                    leaf.Update(_graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);
                foreach (var snow in snowFlakes)
                    snow.Update(_graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);

                foreach (var transition in transitions)
                {
                    if (transition.From == currentState && transition.Zone.Intersects(player.Bounds))
                    {
                        if (maps.ContainsKey(transition.To))
                        {
                            var newMap = maps[transition.To];
                            currentState = transition.To;
                            // Center the player in the new map.
                            player.Position = new Vector2(newMap.Background.Width / 2f, newMap.Background.Height / 2f);
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
                mainMenu.Draw(_spriteBatch);
            }
            else
            {
                var currentMap = maps[currentState];
                _spriteBatch.Draw(currentMap.Background, Vector2.Zero, Color.White);

                foreach (var snow in snowFlakes)
                    snow.Draw(_spriteBatch);
                foreach (var leaf in fallingLeaves)
                    leaf.Draw(_spriteBatch);

                player.Draw(_spriteBatch);

                foreach (var enemy in currentMap.Enemies)
                    enemy.Draw(_spriteBatch);

                if (!isPaused)
                    playerStats.Draw(_spriteBatch, new Vector2(10, 10));
            }

            if (isPaused)
            {
                pauseMenu.Draw(_spriteBatch, playerStats);
            }

            _spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
