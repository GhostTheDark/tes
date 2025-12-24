using LiteNetLib.Utils;

namespace RustLike.Server.Packets
{
    /// <summary>
    /// Pacote de estado de um jogador - Servidor -> Cliente
    /// </summary>
    public class PlayerStatePacket : IPacket
    {
        public PacketType Type => PacketType.PlayerState;
        
        public int PlayerId { get; set; }
        public string PlayerName { get; set; } = string.Empty;
        
        // Posição
        public float PosX { get; set; }
        public float PosY { get; set; }
        public float PosZ { get; set; }
        
        // Rotação
        public float RotX { get; set; }
        public float RotY { get; set; }
        public float RotZ { get; set; }
        
        // Velocidade
        public float VelX { get; set; }
        public float VelY { get; set; }
        public float VelZ { get; set; }
        
        // Estado
        public bool IsGrounded { get; set; }
        public bool IsSprinting { get; set; }
        public bool IsCrouching { get; set; }
        
        // Timestamp
        public long ServerTimestamp { get; set; }
        public int Tick { get; set; }
        
        public PlayerStatePacket() { }
        
        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)Type);
            writer.Put(PlayerId);
            writer.Put(PlayerName);
            writer.Put(PosX);
            writer.Put(PosY);
            writer.Put(PosZ);
            writer.Put(RotX);
            writer.Put(RotY);
            writer.Put(RotZ);
            writer.Put(VelX);
            writer.Put(VelY);
            writer.Put(VelZ);
            writer.Put(IsGrounded);
            writer.Put(IsSprinting);
            writer.Put(IsCrouching);
            writer.Put(ServerTimestamp);
            writer.Put(Tick);
        }
        
        public void Deserialize(NetDataReader reader)
        {
            PlayerId = reader.GetInt();
            PlayerName = reader.GetString();
            PosX = reader.GetFloat();
            PosY = reader.GetFloat();
            PosZ = reader.GetFloat();
            RotX = reader.GetFloat();
            RotY = reader.GetFloat();
            RotZ = reader.GetFloat();
            VelX = reader.GetFloat();
            VelY = reader.GetFloat();
            VelZ = reader.GetFloat();
            IsGrounded = reader.GetBool();
            IsSprinting = reader.GetBool();
            IsCrouching = reader.GetBool();
            ServerTimestamp = reader.GetLong();
            Tick = reader.GetInt();
        }
    }
}
