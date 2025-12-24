namespace RustLike.Server.Packets
{
    /// <summary>
    /// Tipos de pacotes de rede
    /// Usar byte para otimizar tamanho
    /// </summary>
    public enum PacketType : byte
    {
        // Handshake e Conexão (0-9)
        Handshake = 0,
        HandshakeResponse = 1,
        Disconnect = 2,
        Ping = 3,
        Pong = 4,
        
        // Input do Cliente (10-19)
        PlayerInput = 10,
        
        // Estado do Servidor (20-29)
        PlayerState = 20,
        WorldState = 21,
        PlayerSpawn = 22,
        PlayerDespawn = 23,
        
        // Sistema de Save (30-39)
        SaveRequest = 30,
        SaveComplete = 31,
        
        // Chat e Comunicação (40-49)
        ChatMessage = 40,
        
        // Gameplay (50+)
        DamageEvent = 50,
        ItemPickup = 51,
        BuildingPlace = 52
    }
}
