using System;

namespace RustLike.Server.Persistence
{
    /// <summary>
    /// Dados salvos do jogador
    /// </summary>
    [Serializable]
    public class PlayerSaveData
    {
        public int PlayerId { get; set; }
        public string PlayerName { get; set; } = string.Empty;
        public long SteamId { get; set; }
        
        // Posição
        public float PositionX { get; set; }
        public float PositionY { get; set; }
        public float PositionZ { get; set; }
        
        // Rotação
        public float RotationX { get; set; }
        public float RotationY { get; set; }
        public float RotationZ { get; set; }
        
        // Estatísticas
        public float Health { get; set; }
        public float Stamina { get; set; }
        
        // Timestamps
        public long LastSaveTimestamp { get; set; }
        public long TotalPlayTimeSeconds { get; set; }
        
        // Inventário (futuro)
        public string[] InventoryItems { get; set; } = Array.Empty<string>();
        
        // Construções (futuro)
        public int[] OwnedStructures { get; set; } = Array.Empty<int>();
        
        public PlayerSaveData() { }
        
        /// <summary>
        /// Cria save data a partir de uma entidade de jogador
        /// </summary>
        public static PlayerSaveData FromPlayerEntity(World.PlayerEntity player)
        {
            return new PlayerSaveData
            {
                PlayerId = player.Id,
                PlayerName = player.Name,
                SteamId = player.SteamId,
                PositionX = player.PositionX,
                PositionY = player.PositionY,
                PositionZ = player.PositionZ,
                RotationX = player.RotationX,
                RotationY = player.RotationY,
                RotationZ = player.RotationZ,
                Health = player.Health,
                Stamina = player.Stamina,
                LastSaveTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };
        }
        
        /// <summary>
        /// Aplica save data a uma entidade de jogador
        /// </summary>
        public void ApplyToPlayerEntity(World.PlayerEntity player)
        {
            player.PositionX = PositionX;
            player.PositionY = PositionY;
            player.PositionZ = PositionZ;
            player.RotationX = RotationX;
            player.RotationY = RotationY;
            player.RotationZ = RotationZ;
            player.Health = Health;
            player.Stamina = Stamina;
        }
    }
}
