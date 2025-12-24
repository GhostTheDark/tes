using System;
using System.IO;
using System.Text.Json;
using RustLike.Server.Utils;
using RustLike.Server.World;

namespace RustLike.Server.Persistence
{
    /// <summary>
    /// Sistema de persistência de dados
    /// </summary>
    public class SaveSystem
    {
        private readonly string _savePath;
        private float _lastAutoSaveTime;
        
        public SaveSystem()
        {
            _savePath = ServerConfig.SAVE_PATH;
            EnsureSaveDirectoryExists();
        }
        
        /// <summary>
        /// Garante que o diretório de save existe
        /// </summary>
        private void EnsureSaveDirectoryExists()
        {
            if (!Directory.Exists(_savePath))
            {
                Directory.CreateDirectory(_savePath);
                Logger.Info($"Diretório de save criado: {_savePath}");
            }
        }
        
        /// <summary>
        /// Salva dados de um jogador
        /// </summary>
        public bool SavePlayer(PlayerEntity player)
        {
            try
            {
                var saveData = PlayerSaveData.FromPlayerEntity(player);
                string fileName = GetPlayerFileName(player.Id);
                string filePath = Path.Combine(_savePath, fileName);
                
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                
                string json = JsonSerializer.Serialize(saveData, options);
                File.WriteAllText(filePath, json);
                
                player.LastSaveTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                
                Logger.Debug($"Jogador salvo: {player.Name} (ID: {player.Id})");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error($"Erro ao salvar jogador {player.Name} (ID: {player.Id})", ex);
                return false;
            }
        }
        
        /// <summary>
        /// Carrega dados de um jogador
        /// </summary>
        public PlayerSaveData? LoadPlayer(int playerId)
        {
            try
            {
                string fileName = GetPlayerFileName(playerId);
                string filePath = Path.Combine(_savePath, fileName);
                
                if (!File.Exists(filePath))
                {
                    Logger.Debug($"Save não encontrado para jogador ID: {playerId}");
                    return null;
                }
                
                string json = File.ReadAllText(filePath);
                var saveData = JsonSerializer.Deserialize<PlayerSaveData>(json);
                
                if (saveData != null)
                {
                    Logger.Info($"Save carregado: {saveData.PlayerName} (ID: {playerId})");
                }
                
                return saveData;
            }
            catch (Exception ex)
            {
                Logger.Error($"Erro ao carregar jogador ID: {playerId}", ex);
                return null;
            }
        }
        
        /// <summary>
        /// Salva todos os jogadores conectados
        /// </summary>
        public int SaveAllPlayers(PlayerManager playerManager)
        {
            int savedCount = 0;
            
            foreach (var player in playerManager.GetConnectedPlayers())
            {
                if (SavePlayer(player))
                {
                    savedCount++;
                }
            }
            
            Logger.Info($"Auto-save completo: {savedCount} jogadores salvos");
            return savedCount;
        }
        
        /// <summary>
        /// Verifica se é hora de fazer auto-save
        /// </summary>
        public bool ShouldAutoSave(float currentTime)
        {
            if (currentTime - _lastAutoSaveTime >= ServerConfig.AUTO_SAVE_INTERVAL)
            {
                _lastAutoSaveTime = currentTime;
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// Deleta save de um jogador
        /// </summary>
        public bool DeletePlayerSave(int playerId)
        {
            try
            {
                string fileName = GetPlayerFileName(playerId);
                string filePath = Path.Combine(_savePath, fileName);
                
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    Logger.Info($"Save deletado para jogador ID: {playerId}");
                    return true;
                }
                
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error($"Erro ao deletar save do jogador ID: {playerId}", ex);
                return false;
            }
        }
        
        /// <summary>
        /// Verifica se existe save para um jogador
        /// </summary>
        public bool HasSave(int playerId)
        {
            string fileName = GetPlayerFileName(playerId);
            string filePath = Path.Combine(_savePath, fileName);
            return File.Exists(filePath);
        }
        
        /// <summary>
        /// Obtém nome do arquivo de save
        /// </summary>
        private string GetPlayerFileName(int playerId)
        {
            return $"{ServerConfig.PLAYER_SAVE_PREFIX}{playerId}{ServerConfig.PLAYER_SAVE_EXTENSION}";
        }
    }
}
