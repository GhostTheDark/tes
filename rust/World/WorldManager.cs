using RustLike.Server.Utils;

namespace RustLike.Server.World
{
    /// <summary>
    /// Gerencia o estado geral do mundo do jogo
    /// </summary>
    public class WorldManager
    {
        public PlayerManager PlayerManager { get; private set; }
        
        // Estado do mundo
        public int CurrentTick { get; private set; }
        public float WorldTime { get; private set; }
        
        // Configurações do mundo
        public float DayNightCycleDuration { get; set; } = 1200f; // 20 minutos
        public float CurrentDayTime { get; private set; }
        
        public WorldManager()
        {
            PlayerManager = new PlayerManager();
            CurrentTick = 0;
            WorldTime = 0f;
            CurrentDayTime = 0.5f; // Meio-dia
        }
        
        /// <summary>
        /// Atualiza o mundo (chamado a cada tick)
        /// </summary>
        public void Update(float deltaTime)
        {
            CurrentTick++;
            WorldTime += deltaTime;
            
            // Atualizar ciclo dia/noite
            CurrentDayTime += deltaTime / DayNightCycleDuration;
            if (CurrentDayTime >= 1f)
            {
                CurrentDayTime -= 1f;
            }
            
            // Atualizar jogadores
            PlayerManager.UpdatePlayers(deltaTime);
        }
        
        /// <summary>
        /// Reseta o mundo
        /// </summary>
        public void Reset()
        {
            PlayerManager.Clear();
            CurrentTick = 0;
            WorldTime = 0f;
            CurrentDayTime = 0.5f;
            Logger.Info("Mundo resetado");
        }
        
        /// <summary>
        /// Obtém informações de debug do mundo
        /// </summary>
        public string GetDebugInfo()
        {
            int playerCount = PlayerManager.GetPlayerCount();
            return $"Tick: {CurrentTick} | Players: {playerCount} | Time: {WorldTime:F1}s | DayTime: {CurrentDayTime:F2}";
        }
    }
}
