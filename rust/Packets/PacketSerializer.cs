using LiteNetLib.Utils;
using RustLike.Server.Utils;
using System;

namespace RustLike.Server.Packets
{
    /// <summary>
    /// Gerencia serialização e desserialização de pacotes
    /// </summary>
    public static class PacketSerializer
    {
        /// <summary>
        /// Serializa um pacote para NetDataWriter
        /// </summary>
        public static void Serialize(IPacket packet, NetDataWriter writer)
        {
            try
            {
                packet.Serialize(writer);
            }
            catch (Exception ex)
            {
                Logger.Error($"Erro ao serializar pacote {packet.Type}", ex);
                throw;
            }
        }
        
        /// <summary>
        /// Cria um NetDataWriter com o pacote serializado
        /// </summary>
        public static NetDataWriter SerializeToWriter(IPacket packet)
        {
            var writer = new NetDataWriter();
            Serialize(packet, writer);
            return writer;
        }
        
        /// <summary>
        /// Desserializa um pacote do NetDataReader
        /// Retorna null se o tipo for desconhecido
        /// </summary>
        public static IPacket? Deserialize(NetDataReader reader)
        {
            try
            {
                // Ler o tipo do pacote
                var packetType = (PacketType)reader.GetByte();
                
                // Criar instância do pacote apropriado
                IPacket? packet = CreatePacketInstance(packetType);
                
                if (packet == null)
                {
                    Logger.Warning($"Tipo de pacote desconhecido: {packetType}");
                    return null;
                }
                
                // Desserializar dados
                packet.Deserialize(reader);
                return packet;
            }
            catch (Exception ex)
            {
                Logger.Error("Erro ao desserializar pacote", ex);
                return null;
            }
        }
        
        /// <summary>
        /// Cria uma instância de pacote baseado no tipo
        /// </summary>
        private static IPacket? CreatePacketInstance(PacketType type)
        {
            return type switch
            {
                PacketType.Handshake => new HandshakePacket(),
                PacketType.HandshakeResponse => new HandshakeResponsePacket(),
                PacketType.PlayerInput => new PlayerInputPacket(),
                PacketType.PlayerState => new PlayerStatePacket(),
                PacketType.WorldState => new WorldStatePacket(),
                _ => null
            };
        }
    }
}
