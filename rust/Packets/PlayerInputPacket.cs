using LiteNetLib.Utils;

namespace RustLike.Server.Packets
{
    /// <summary>
    /// Pacote de input do jogador - Cliente -> Servidor
    /// Enviado a cada frame/tick pelo cliente
    /// </summary>
    public class PlayerInputPacket : IPacket
    {
        public PacketType Type => PacketType.PlayerInput;
        
        // Input de movimento
        public float MoveX { get; set; }
        public float MoveZ { get; set; }
        
        // Rotação (Yaw)
        public float RotationY { get; set; }
        
        // Ações
        public bool IsJumping { get; set; }
        public bool IsSprinting { get; set; }
        public bool IsCrouching { get; set; }
        
        // Timestamp para sincronização
        public long ClientTimestamp { get; set; }
        public int SequenceNumber { get; set; }
        
        public PlayerInputPacket() { }
        
        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)Type);
            writer.Put(MoveX);
            writer.Put(MoveZ);
            writer.Put(RotationY);
            writer.Put(IsJumping);
            writer.Put(IsSprinting);
            writer.Put(IsCrouching);
            writer.Put(ClientTimestamp);
            writer.Put(SequenceNumber);
        }
        
        public void Deserialize(NetDataReader reader)
        {
            MoveX = reader.GetFloat();
            MoveZ = reader.GetFloat();
            RotationY = reader.GetFloat();
            IsJumping = reader.GetBool();
            IsSprinting = reader.GetBool();
            IsCrouching = reader.GetBool();
            ClientTimestamp = reader.GetLong();
            SequenceNumber = reader.GetInt();
        }
    }
}
