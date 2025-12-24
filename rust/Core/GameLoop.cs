using RustLike.Server.Network;
using RustLike.Server.Utils;
using RustLike.Server.World;
using System.Threading;

namespace RustLike.Server.Core
{
    /// <summary>
    /// Loop principal do servidor
    /// </summary>
    public class GameLoop
    {
        private readonly NetworkServer _networkServer;
        private readonly WorldManager _worldManager;
        private readonly TickSystem _tickSystem;
        
        private bool _isRunning;
        private float _lastSnapshotTime;
        private float _lastStatsTime;
        
        public GameLoop(NetworkServer networkServer, WorldManager worldManager)
        {
            _networkServer = networkServer;
            _worldManager = worldManager;
            _tickSystem = new TickSystem();
        }
        
        /// <summary>
        /// Inicia o loop do servidor
        /// </summary>
        public void Start()
        {
            _isRunning = true;
            Logger.Info("🎮 Game loop iniciado");
            
            while (_isRunning)
            {
                Update();
                
                // Sleep pequeno para não sobrecarregar CPU
                Thread.Sleep(1);
            }
            
            Logger.Info("🎮 Game loop encerrado");
        }
        
        /// <summary>
        /// Para o loop
        /// </summary>
        public void Stop()
        {
            _isRunning = false;
        }
        
        /// <summary>
        /// Atualização principal
        /// </summary>
        private void Update()
        {
            // Atualizar eventos de rede
            _networkServer.PollEvents();
            
            // Processar ticks
            int ticksToProcess = _tickSystem.Update();
            
            for (int i = 0; i < ticksToProcess; i++)
            {
                ProcessTick();
            }
            
            // Mostrar estatísticas periodicamente
            ShowStats();
        }
        
        /// <summary>
        /// Processa um único tick do jogo
        /// </summary>
        private void ProcessTick()
        {
            // Atualizar mundo
            _worldManager.Update(ServerConfig.TICK_DELTA);
            
            // Enviar snapshots para clientes
            float currentTime = TimeUtils.Time;
            if (currentTime - _lastSnapshotTime >= ServerConfig.SNAPSHOT_INTERVAL)
            {
                _networkServer.BroadcastPlayerStates();
                _lastSnapshotTime = currentTime;
            }
            
            // Auto-save
            if (_networkServer.SaveSystem.ShouldAutoSave(currentTime))
            {
                _networkServer.SaveSystem.SaveAllPlayers(_worldManager.PlayerManager);
            }
        }
        
        /// <summary>
        /// Mostra estatísticas do servidor
        /// </summary>
        private void ShowStats()
        {
            float currentTime = TimeUtils.Time;
            
            if (currentTime - _lastStatsTime >= 10f)
            {
                int playerCount = _worldManager.PlayerManager.GetPlayerCount();
                int tickRate = _tickSystem.ActualTickRate;
                
                Logger.Info($"📊 Stats - Players: {playerCount} | Tick Rate: {tickRate} Hz | {_worldManager.GetDebugInfo()}");
                
                _lastStatsTime = currentTime;
            }
        }
    }
}
