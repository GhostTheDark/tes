using RustLike.Server.Network;
using RustLike.Server.Utils;
using RustLike.Server.World;
using System;

namespace RustLike.Server.Core
{
    /// <summary>
    /// Inicializa todos os sistemas do servidor
    /// </summary>
    public class ServerBootstrap
    {
        private NetworkServer? _networkServer;
        private WorldManager? _worldManager;
        private GameLoop? _gameLoop;
        
        /// <summary>
        /// Inicializa e inicia o servidor
        /// </summary>
        public void Initialize()
        {
            try
            {
                Logger.Info("===========================================");
                Logger.Info("🚀 RUST-LIKE SERVER - INICIANDO");
                Logger.Info("===========================================");
                
                // Criar instâncias
                Logger.Info("📦 Criando componentes...");
                _worldManager = new WorldManager();
                _networkServer = new NetworkServer();
                _gameLoop = new GameLoop(_networkServer, _worldManager);
                
                // Iniciar servidor de rede
                Logger.Info("🌐 Iniciando servidor de rede...");
                _networkServer.Start(_worldManager);
                
                // Iniciar game loop
                Logger.Info("🎮 Iniciando game loop...");
                Logger.Info("===========================================");
                Logger.Info("✅ SERVIDOR PRONTO!");
                Logger.Info("===========================================");
                
                _gameLoop.Start();
            }
            catch (Exception ex)
            {
                Logger.Error("❌ Erro fatal durante inicialização", ex);
                Shutdown();
                throw;
            }
        }
        
        /// <summary>
        /// Desliga o servidor graciosamente
        /// </summary>
        public void Shutdown()
        {
            Logger.Info("===========================================");
            Logger.Info("🛑 ENCERRANDO SERVIDOR...");
            Logger.Info("===========================================");
            
            try
            {
                // Parar game loop
                if (_gameLoop != null)
                {
                    Logger.Info("Parando game loop...");
                    _gameLoop.Stop();
                }
                
                // Parar servidor de rede
                if (_networkServer != null)
                {
                    Logger.Info("Parando servidor de rede...");
                    _networkServer.Stop();
                }
                
                Logger.Info("===========================================");
                Logger.Info("✅ SERVIDOR ENCERRADO COM SUCESSO");
                Logger.Info("===========================================");
            }
            catch (Exception ex)
            {
                Logger.Error("Erro durante shutdown", ex);
            }
        }
    }
}
