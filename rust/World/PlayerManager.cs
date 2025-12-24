using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using RustLike.Server.Utils;

namespace RustLike.Server.World
{
    /// <summary>
    /// Gerencia todos os jogadores conectados no servidor
    /// </summary>
    public class PlayerManager
    {
        private readonly ConcurrentDictionary<int, PlayerEntity> _players = new();
        private int _nextPlayerId = 1;
        
        /// <summary>
        /// Adiciona um novo jogador
        /// </summary>
        public PlayerEntity AddPlayer(string playerName)
        {
            int playerId = _nextPlayerId++;
            
            var player = new PlayerEntity
            {
                Id = playerId,
                Name = playerName,
                IsConnected = true
            };
            
            // Spawn em posição inicial (pode ser randomizado)
            player.SpawnAtPosition(0f, 1f, 0f);
            
            if (_players.TryAdd(playerId, player))
            {
                Logger.Info($"Jogador adicionado: {playerName} (ID: {playerId})");
                return player;
            }
            
            Logger.Error($"Falha ao adicionar jogador: {playerName}");
            throw new System.Exception("Falha ao adicionar jogador");
        }
        
        /// <summary>
        /// Remove um jogador
        /// </summary>
        public void RemovePlayer(int playerId)
        {
            if (_players.TryRemove(playerId, out var player))
            {
                player.IsConnected = false;
                Logger.Info($"Jogador removido: {player.Name} (ID: {playerId})");
            }
        }
        
        /// <summary>
        /// Obtém um jogador por ID
        /// </summary>
        public PlayerEntity? GetPlayer(int playerId)
        {
            _players.TryGetValue(playerId, out var player);
            return player;
        }
        
        /// <summary>
        /// Verifica se jogador existe
        /// </summary>
        public bool HasPlayer(int playerId)
        {
            return _players.ContainsKey(playerId);
        }
        
        /// <summary>
        /// Obtém todos os jogadores
        /// </summary>
        public IEnumerable<PlayerEntity> GetAllPlayers()
        {
            return _players.Values;
        }
        
        /// <summary>
        /// Obtém todos os jogadores conectados
        /// </summary>
        public IEnumerable<PlayerEntity> GetConnectedPlayers()
        {
            return _players.Values.Where(p => p.IsConnected);
        }
        
        /// <summary>
        /// Obtém contagem de jogadores
        /// </summary>
        public int GetPlayerCount()
        {
            return _players.Count;
        }
        
        /// <summary>
        /// Atualiza todos os jogadores (chamado a cada tick)
        /// </summary>
        public void UpdatePlayers(float deltaTime)
        {
            foreach (var player in _players.Values)
            {
                if (!player.IsConnected) continue;
                
                // Aqui pode adicionar lógica de física, gravidade, etc
                // Por enquanto, apenas aplicar gravidade simples
                if (!player.IsGrounded)
                {
                    player.VelocityY -= 9.81f * deltaTime;
                    player.PositionY += player.VelocityY * deltaTime;
                    
                    if (player.PositionY <= 0f)
                    {
                        player.PositionY = 0f;
                        player.VelocityY = 0f;
                        player.IsGrounded = true;
                    }
                }
            }
        }
        
        /// <summary>
        /// Limpa todos os jogadores
        /// </summary>
        public void Clear()
        {
            _players.Clear();
            _nextPlayerId = 1;
            Logger.Info("Todos os jogadores foram removidos");
        }
    }
}
