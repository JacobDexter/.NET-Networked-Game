using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using NetworkedGame.Library;

namespace NetworkedGame
{
    public class NetworkedGame : Game
    {
        //create client
        Client _client;

        //Player Data
        private List<Texture2D> PlayerSprites = new List<Texture2D>();
        private List<Vector2> PlayerPositions = new List<Vector2>();
        private float PlayerSpeed = 150.0f;
        public int localServerIndex = -1;

        private PlayerPositionPacket _playerPositionPacket;

        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        public NetworkedGame()
        {
            _client = new Client(this);

            //Connect to server
            if (_client.Connect("127.0.0.1", 4444))
            {
                _client.Run();
            }

            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = false;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            _client.Login(0.0f, 0.0f);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here
            var kstate = Keyboard.GetState();

            if (kstate.IsKeyDown(Keys.W) || kstate.IsKeyDown(Keys.Up))
            {
                PlayerPositions[localServerIndex] = new Vector2(PlayerPositions[localServerIndex].X, PlayerPositions[localServerIndex].Y - (PlayerSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds));
                _playerPositionPacket = new PlayerPositionPacket(localServerIndex, (float)PlayerPositions[localServerIndex].X, (float)PlayerPositions[localServerIndex].Y);
                _client.UDPSendPacket(_playerPositionPacket);
            }

            if (kstate.IsKeyDown(Keys.S) || kstate.IsKeyDown(Keys.Down))
            {
                PlayerPositions[localServerIndex] = new Vector2(PlayerPositions[localServerIndex].X, PlayerPositions[localServerIndex].Y + (PlayerSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds));
                _playerPositionPacket = new PlayerPositionPacket(localServerIndex, (float)PlayerPositions[localServerIndex].X, (float)PlayerPositions[localServerIndex].Y);
                _client.UDPSendPacket(_playerPositionPacket);
            }

            if (kstate.IsKeyDown(Keys.A) || kstate.IsKeyDown(Keys.Left))
            {
                PlayerPositions[localServerIndex] = new Vector2(PlayerPositions[localServerIndex].X - (PlayerSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds), (PlayerPositions[localServerIndex].Y));
                _playerPositionPacket = new PlayerPositionPacket(localServerIndex, (float)PlayerPositions[localServerIndex].X, (float)PlayerPositions[localServerIndex].Y);
                _client.UDPSendPacket(_playerPositionPacket);
            }

            if (kstate.IsKeyDown(Keys.D) || kstate.IsKeyDown(Keys.Right))
            {
                PlayerPositions[localServerIndex] = new Vector2(PlayerPositions[localServerIndex].X + (PlayerSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds), (PlayerPositions[localServerIndex].Y));
                _playerPositionPacket = new PlayerPositionPacket(localServerIndex, (float)PlayerPositions[localServerIndex].X, (float)PlayerPositions[localServerIndex].Y);
                _client.UDPSendPacket(_playerPositionPacket);
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.White);

            // TODO: Add your drawing code here
            _spriteBatch.Begin();

            if(PlayerSprites.Count > 0)
            {
                for (int i = 0; i < PlayerSprites.Count; i++)
                {
                    _spriteBatch.Draw(PlayerSprites[i], PlayerPositions[i], Color.White);
                }
            }

            _spriteBatch.End();

            base.Draw(gameTime);
        }

        public void InitialiseNewPlayer(float x, float y)
        {
            PlayerPositions.Add(new Vector2(x, y));
            PlayerSprites.Add(Content.Load<Texture2D>("playersprite"));
        }

        public void LoadExistingPlayers(int count, float[] xValues, float[] yValues)
        {
            for(int i = 0; i < count; i++)
            {
                PlayerPositions.Add(new Vector2(xValues[i], yValues[i]));
                PlayerSprites.Add(Content.Load<Texture2D>("playersprite"));
            }
        }

        public void ChangePlayerPosition(int index, float x, float y)
        {
            PlayerPositions[index] = new Vector2(x, y);
        }
    }
}
