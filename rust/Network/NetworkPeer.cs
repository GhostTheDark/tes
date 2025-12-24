using LiteNetLib;
using RustLike.Server.World;

namespace RustLike.Server.Network
{
    /// <summary>
    /// Representa uma conexão de cliente no servidor
    /// </summary>
    public class NetworkPeer
    {
        public NetPeer Peer { get; }
        public PlayerEntity? Player { get; set; }
        
        public bool IsAuthenticated { get; set; }
        public long LastPingTime { get; set; }
        public int Ping { get; set; }
        
        // Controle de taxa de pacotes
        public float LastInputTime { get; set; }
        public int InputPacketCount { get; set; }
        
        public NetworkPeer(NetPeer peer)
        {
            Peer = peer;
            IsAuthenticated = false;
            LastPingTime = 0;
            Ping = 0;
        }
        
        /// <summary>
        /// Verifica se pode processar input (anti-spam)
        /// </summary>
        public bool CanProcessInput(float currentTime)
        {
            if (currentTime - LastInputTime < ServerConfig.INPUT_COOLDOWN)
            {
                return false;
            }
            
            LastInputTime = currentTime;
            return true;
        }
        
        /// <summary>
        /// Atualiza contagem de ping
        /// </summary>
        public void UpdatePing(long currentTime)
        {
            if (LastPingTime > 0)
            {
                Ping = (int)(currentTime - LastPingTime);
            }
        }
    }
}
