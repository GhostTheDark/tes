using LiteNetLib.Utils;
using System.Collections.Generic;

namespace RustLike.Server.Packets
{
    /// <summary>
    /// Pacote de estado completo do mundo - Servidor -> Cliente
    /// Enviado periodicamente ou quando cliente conecta
    /// </summary>
    public class WorldStatePacket : IPacket
    {
        public PacketType Type => PacketType.WorldState;
        
        public int CurrentTick { get; set; }
        public long ServerTimestamp { get; set; }
        
        // Lista de estados de jogadores
        public List<PlayerStateData> Players { get; set; } = new List<PlayerStateData>();
        
        public WorldStatePacket() { }
        
        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)Type);
            writer.Put(CurrentTick);
            writer.Put(ServerTimestamp);
            
            // Serializar jogadores
            writer.Put(Players.Count);
            foreach (var player in Players)
            {
                writer.Put(player.PlayerId);
                writer.Put(player.PlayerName);
                writer.Put(player.PosX);
                writer.Put(player.PosY);
                writer.Put(player.PosZ);
                writer.Put(player.RotY);
                writer.Put(player.IsSprinting);
                writer.Put(player.IsCrouching);
            }
        }
        
        public void Deserialize(NetDataReader reader)
        {
            CurrentTick = reader.GetInt();
            ServerTimestamp = reader.GetLong();
            
            // Desserializar jogadores
            int playerCount = reader.GetInt();
            Players.Clear();
            
            for (int i = 0; i < playerCount; i++)
            {
                var player = new PlayerStateData
                {
                    PlayerId = reader.GetInt(),
                    PlayerName = reader.GetString(),
                    PosX = reader.GetFloat(),
                    PosY = reader.GetFloat(),
                    PosZ = reader.GetFloat(),
                    RotY = reader.GetFloat(),
                    IsSprinting = reader.GetBool(),
                    IsCrouching = reader.GetBool()
                };
                Players.Add(player);
            }
        }
    }
    
    /// <summary>
    /// Dados simplificados de estado de jogador para WorldState
    /// </summary>
    public class PlayerStateData
    {
        public int PlayerId { get; set; }
        public string PlayerName { get; set; } = string.Empty;
        public float PosX { get; set; }
        public float PosY { get; set; }
        public float PosZ { get; set; }
        public float RotY { get; set; }
        public bool IsSprinting { get; set; }
        public bool IsCrouching { get; set; }
    }
}
