namespace RustLike.Server
{
    /// <summary>
    /// Configurações do servidor
    /// </summary>
    public static class ServerConfig
    {
        // Configurações de Rede
        public const string HOST = "0.0.0.0";
        public const int PORT = 7777;
        public const int MAX_PLAYERS = 100;
        public const string CONNECTION_KEY = "RustLikeGame2025";
        
        // Configurações de Tick
        public const int TICK_RATE = 30; // Ticks por segundo
        public const float TICK_DELTA = 1f / TICK_RATE;
        public const int TICKS_PER_SECOND = TICK_RATE;
        
        // Configurações de Sincronização
        public const int SNAPSHOT_RATE = 20; // Snapshots por segundo
        public const float SNAPSHOT_INTERVAL = 1f / SNAPSHOT_RATE;
        
        // Configurações de Input
        public const int MAX_INPUT_PER_SECOND = 60;
        public const float INPUT_COOLDOWN = 1f / MAX_INPUT_PER_SECOND;
        
        // Configurações de Save
        public const float AUTO_SAVE_INTERVAL = 300f; // 5 minutos
        public const string SAVE_PATH = "./SaveData/";
        public const string PLAYER_SAVE_PREFIX = "player_";
        public const string PLAYER_SAVE_EXTENSION = ".json";
        
        // Configurações de Gameplay
        public const float PLAYER_SPEED = 5f;
        public const float PLAYER_ROTATION_SPEED = 180f;
        
        // Limites de Validação
        public const float MAX_MOVE_DISTANCE_PER_TICK = 2f;
        public const float MAX_ROTATION_PER_TICK = 90f;
        
        // Timeout
        public const int DISCONNECT_TIMEOUT = 5000; // ms
        public const int PING_INTERVAL = 1000; // ms
    }
}
