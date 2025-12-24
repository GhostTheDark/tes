using LiteNetLib.Utils;

namespace RustLike.Server.Packets
{
    /// <summary>
    /// Interface base para todos os pacotes de rede
    /// </summary>
    public interface IPacket
    {
        /// <summary>
        /// Tipo do pacote
        /// </summary>
        PacketType Type { get; }
        
        /// <summary>
        /// Serializa o pacote para o writer
        /// </summary>
        void Serialize(NetDataWriter writer);
        
        /// <summary>
        /// Desserializa o pacote do reader
        /// </summary>
        void Deserialize(NetDataReader reader);
    }
}
