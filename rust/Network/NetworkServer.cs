using System;
using LiteNetLib;
using LiteNetLib.Utils;
using RustLike.Server.Packets;
using RustLike.Server.Persistence;
using RustLike.Server.Utils;
using RustLike.Server.World;
using System.Collections.Concurrent;

namespace RustLike.Server.Network
{
    /// <summary>
    /// Servidor de rede principal usando LiteNetLib
    /// </summary>
    public class NetworkServer : INetEventListener
    {
        private NetManager? _netManager;
        private readonly ConcurrentDictionary<int, NetworkPeer> _peers = new();
        
        private WorldManager? _worldManager;
        private PacketHandler? _packetHandler;
        
        public SaveSystem SaveSystem { get; private set; }
        public bool IsRunning { get; private set; }
        
        public NetworkServer()
        {
            SaveSystem = new SaveSystem();
        }
        
        /// <summary>
        /// Inicializa e inicia o servidor
        /// </summary>
        public void Start(WorldManager worldManager)
        {
            _worldManager = worldManager;
            _packetHandler = new PacketHandler(this, worldManager);
            
            _netManager = new NetManager(this)
            {
                BroadcastReceiveEnabled = true,
                UnconnectedMessagesEnabled = true,
                UpdateTime = 15,
                DisconnectTimeout = ServerConfig.DISCONNECT_TIMEOUT,
                PingInterval = ServerConfig.PING_INTERVAL
            };
            
            bool started = _netManager.Start(ServerConfig.PORT);
            
            if (started)
            {
                IsRunning = true;
                Logger.Info($"===========================================");
                Logger.Info($"🚀 Servidor iniciado em {ServerConfig.HOST}:{ServerConfig.PORT}");
                Logger.Info($"📊 Max Players: {ServerConfig.MAX_PLAYERS}");
                Logger.Info($"⚡ Tick Rate: {ServerConfig.TICK_RATE} Hz");
                Logger.Info($"💾 Save Path: {ServerConfig.SAVE_PATH}");
                Logger.Info($"===========================================");
            }
            else
            {
                Logger.Error("❌ Falha ao iniciar servidor!");
                throw new System.Exception("Falha ao iniciar servidor de rede");
            }
        }
        
        /// <summary>
        /// Para o servidor
        /// </summary>
        public void Stop()
        {
            if (_netManager != null)
            {
                // Salvar todos os jogadores antes de desligar
                if (_worldManager != null)
                {
                    SaveSystem.SaveAllPlayers(_worldManager.PlayerManager);
                }
                
                _netManager.Stop();
                IsRunning = false;
                Logger.Info("Servidor parado");
            }
        }
        
        /// <summary>
        /// Atualiza o servidor (deve ser chamado regularmente)
        /// </summary>
        public void PollEvents()
        {
            _netManager?.PollEvents();
        }
        
        /// <summary>
        /// Envia dados para todos os clientes conectados
        /// </summary>
        public void SendToAll(NetDataWriter writer, DeliveryMethod deliveryMethod)
        {
            foreach (var peer in _peers.Values)
            {
                if (peer.IsAuthenticated)
                {
                    peer.Peer.Send(writer, deliveryMethod);
                }
            }
        }
        
        /// <summary>
        /// Envia dados para todos exceto um cliente específico
        /// </summary>
        public void SendToAllExcept(int excludePlayerId, NetDataWriter writer, DeliveryMethod deliveryMethod)
        {
            foreach (var peer in _peers.Values)
            {
                if (peer.IsAuthenticated && peer.Player != null && peer.Player.Id != excludePlayerId)
                {
                    peer.Peer.Send(writer, deliveryMethod);
                }
            }
        }
        
        /// <summary>
        /// Faz broadcast do estado de todos os jogadores
        /// </summary>
        public void BroadcastPlayerStates()
        {
            if (_worldManager == null) return;
            
            foreach (var player in _worldManager.PlayerManager.GetConnectedPlayers())
            {
                var statePacket = new PlayerStatePacket
                {
                    PlayerId = player.Id,
                    PlayerName = player.Name,
                    PosX = player.PositionX,
                    PosY = player.PositionY,
                    PosZ = player.PositionZ,
                    RotX = player.RotationX,
                    RotY = player.RotationY,
                    RotZ = player.RotationZ,
                    VelX = player.VelocityX,
                    VelY = player.VelocityY,
                    VelZ = player.VelocityZ,
                    IsGrounded = player.IsGrounded,
                    IsSprinting = player.IsSprinting,
                    IsCrouching = player.IsCrouching,
                    ServerTimestamp = TimeUtils.UnixTimestamp,
                    Tick = _worldManager.CurrentTick
                };
                
                var writer = PacketSerializer.SerializeToWriter(statePacket);
                SendToAll(writer, DeliveryMethod.Sequenced);
            }
        }
        
        #region INetEventListener Implementation
        
        public void OnPeerConnected(NetPeer peer)
        {
            Logger.Info($"🔌 Cliente conectado: {peer.Address}:{peer.Port} (ID: {peer.Id})");
            
            var networkPeer = new NetworkPeer(peer);
            _peers.TryAdd(peer.Id, networkPeer);
        }
        
        public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            Logger.Info($"🔌 Cliente desconectado: {peer.Address}:{peer.Port} - Razão: {disconnectInfo.Reason}");
            
            if (_peers.TryRemove(peer.Id, out var networkPeer))
            {
                if (networkPeer.Player != null && _worldManager != null)
                {
                    // Salvar jogador antes de remover
                    SaveSystem.SavePlayer(networkPeer.Player);
                    
                    // Remover do mundo
                    _worldManager.PlayerManager.RemovePlayer(networkPeer.Player.Id);
                    
                    Logger.Info($"Jogador removido: {networkPeer.Player.Name}");
                }
            }
        }
        
        public void OnNetworkError(System.Net.IPEndPoint endPoint, System.Net.Sockets.SocketError socketError)
        {
            Logger.Error($"Erro de rede: {endPoint} - {socketError}");
        }
        
        public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channelNumber, DeliveryMethod deliveryMethod)
        {
            if (!_peers.TryGetValue(peer.Id, out var networkPeer))
            {
                Logger.Warning($"Pacote recebido de peer desconhecido: {peer.Id}");
                return;
            }
            
            try
            {
                // Desserializar pacote
                var dataReader = new NetDataReader(reader.RawData, reader.UserDataOffset, reader.UserDataSize);
                
                // Log apenas de pacotes importantes (não input)
                if (reader.UserDataSize > 0)
                {
                    byte packetTypeByte = reader.RawData[reader.UserDataOffset];
                    
                    // Log apenas para pacotes que não são PlayerInput (tipo 10)
                    if (packetTypeByte != 10)
                    {
                        Logger.Debug($"Recebido pacote tipo {packetTypeByte} de peer {peer.Id}, tamanho: {reader.UserDataSize} bytes");
                    }
                }
                
                var packet = PacketSerializer.Deserialize(dataReader);
                
                if (packet != null && _packetHandler != null)
                {
                    _packetHandler.HandlePacket(networkPeer, packet);
                }
                else if (packet == null)
                {
                    Logger.Warning($"Falha ao desserializar pacote de peer {peer.Id}");
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Erro ao processar pacote de peer {peer.Id}", ex);
            }
            
            reader.Recycle();
        }
        
        public void OnNetworkReceiveUnconnected(System.Net.IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
        {
            // Pode ser usado para servidor browser, ping, etc
        }
        
        public void OnNetworkLatencyUpdate(NetPeer peer, int latency)
        {
            if (_peers.TryGetValue(peer.Id, out var networkPeer))
            {
                networkPeer.Ping = latency;
            }
        }
        
        public void OnConnectionRequest(ConnectionRequest request)
        {
            try
            {
                // Validar chave de conexão
                string key = request.Data.GetString();
                
                if (key == ServerConfig.CONNECTION_KEY)
                {
                    if (_peers.Count < ServerConfig.MAX_PLAYERS)
                    {
                        Logger.Info($"✅ Conexão aceita de {request.RemoteEndPoint}");
                        request.Accept();
                    }
                    else
                    {
                        Logger.Warning($"❌ Servidor cheio. Rejeitando {request.RemoteEndPoint}");
                        request.Reject();
                    }
                }
                else
                {
                    Logger.Warning($"❌ Chave inválida de {request.RemoteEndPoint}. Esperado: '{ServerConfig.CONNECTION_KEY}', Recebido: '{key}'");
                    request.Reject();
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Erro ao processar connection request: {ex.Message}");
                request.Reject();
            }
        }
        
        #endregion
    }
}