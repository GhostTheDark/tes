using LiteNetLib.Utils;

namespace RustLike.Server.Packets
{
    /// <summary>
    /// Pacote de handshake inicial - Cliente -> Servidor
    /// </summary>
    public class HandshakePacket : IPacket
    {
        public PacketType Type => PacketType.Handshake;
        
        public string PlayerName { get; set; } = string.Empty;
        public string ClientVersion { get; set; } = string.Empty;
        public long ClientTimestamp { get; set; }
        
        public HandshakePacket() { }
        
        public HandshakePacket(string playerName, string clientVersion, long timestamp)
        {
            PlayerName = playerName;
            ClientVersion = clientVersion;
            ClientTimestamp = timestamp;
        }
        
        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)Type);
            writer.Put(PlayerName);
            writer.Put(ClientVersion);
            writer.Put(ClientTimestamp);
        }
        
        public void Deserialize(NetDataReader reader)
        {
            PlayerName = reader.GetString();
            ClientVersion = reader.GetString();
            ClientTimestamp = reader.GetLong();
        }
    }
    
    /// <summary>
    /// Resposta do handshake - Servidor -> Cliente
    /// </summary>
    public class HandshakeResponsePacket : IPacket
    {
        public PacketType Type => PacketType.HandshakeResponse;
        
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int AssignedPlayerId { get; set; }
        public long ServerTimestamp { get; set; }
        
        public HandshakeResponsePacket() { }
        
        public HandshakeResponsePacket(bool success, string message, int playerId, long timestamp)
        {
            Success = success;
            Message = message;
            AssignedPlayerId = playerId;
            ServerTimestamp = timestamp;
        }
        
        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)Type);
            writer.Put(Success);
            writer.Put(Message);
            writer.Put(AssignedPlayerId);
            writer.Put(ServerTimestamp);
        }
        
        public void Deserialize(NetDataReader reader)
        {
            Success = reader.GetBool();
            Message = reader.GetString();
            AssignedPlayerId = reader.GetInt();
            ServerTimestamp = reader.GetLong();
        }
    }
}
