using LiteNetLib;
using LiteNetLib.Utils;
using RustLike.Server.Packets;
using RustLike.Server.Utils;
using RustLike.Server.World;
using System;

namespace RustLike.Server.Network
{
    /// <summary>
    /// Processa pacotes recebidos do cliente
    /// </summary>
    public class PacketHandler
    {
        private readonly NetworkServer _server;
        private readonly WorldManager _worldManager;
        
        public PacketHandler(NetworkServer server, WorldManager worldManager)
        {
            _server = server;
            _worldManager = worldManager;
        }
        
        /// <summary>
        /// Processa um pacote recebido
        /// </summary>
        public void HandlePacket(NetworkPeer networkPeer, IPacket packet)
        {
            try
            {
                switch (packet.Type)
                {
                    case PacketType.Handshake:
                        HandleHandshake(networkPeer, (HandshakePacket)packet);
                        break;
                    
                    case PacketType.PlayerInput:
                        HandlePlayerInput(networkPeer, (PlayerInputPacket)packet);
                        break;
                    
                    default:
                        Logger.Warning($"Tipo de pacote não tratado: {packet.Type}");
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Erro ao processar pacote {packet.Type}", ex);
            }
        }
        
        /// <summary>
        /// Processa handshake inicial
        /// </summary>
        private void HandleHandshake(NetworkPeer networkPeer, HandshakePacket packet)
        {
            Logger.Info($"Handshake recebido de: {packet.PlayerName}");
            
            // Validar versão do cliente (se necessário)
            if (string.IsNullOrEmpty(packet.PlayerName))
            {
                SendHandshakeResponse(networkPeer, false, "Nome do jogador inválido", -1);
                return;
            }
            
            // Verificar limite de jogadores
            if (_worldManager.PlayerManager.GetPlayerCount() >= ServerConfig.MAX_PLAYERS)
            {
                SendHandshakeResponse(networkPeer, false, "Servidor cheio", -1);
                return;
            }
            
            try
            {
                // Criar jogador
                var player = _worldManager.PlayerManager.AddPlayer(packet.PlayerName);
                networkPeer.Player = player;
                networkPeer.IsAuthenticated = true;
                
                // Tentar carregar save
                var saveData = _server.SaveSystem.LoadPlayer(player.Id);
                if (saveData != null)
                {
                    saveData.ApplyToPlayerEntity(player);
                    Logger.Info($"Save carregado para {player.Name}");
                }
                else
                {
                    Logger.Info($"Novo jogador: {player.Name}");
                }
                
                // Enviar resposta de sucesso
                SendHandshakeResponse(networkPeer, true, "Conectado com sucesso", player.Id);
                
                // Enviar estado completo do mundo
                SendWorldState(networkPeer);
                
                // Notificar outros jogadores sobre o novo jogador
                BroadcastPlayerState(player);
                
                Logger.Info($"Jogador autenticado: {player.Name} (ID: {player.Id})");
            }
            catch (Exception ex)
            {
                Logger.Error("Erro ao processar handshake", ex);
                SendHandshakeResponse(networkPeer, false, "Erro interno do servidor", -1);
            }
        }
        
        /// <summary>
        /// Envia resposta do handshake
        /// </summary>
        private void SendHandshakeResponse(NetworkPeer networkPeer, bool success, string message, int playerId)
        {
            var response = new HandshakeResponsePacket(success, message, playerId, TimeUtils.UnixTimestamp);
            var writer = PacketSerializer.SerializeToWriter(response);
            networkPeer.Peer.Send(writer, DeliveryMethod.ReliableOrdered);
        }
        
        /// <summary>
        /// Processa input do jogador
        /// </summary>
        private void HandlePlayerInput(NetworkPeer networkPeer, PlayerInputPacket packet)
        {
            if (!networkPeer.IsAuthenticated || networkPeer.Player == null)
            {
                Logger.Warning("Input recebido de peer não autenticado");
                return;
            }
            
            // Anti-spam de input
            if (!networkPeer.CanProcessInput(TimeUtils.Time))
            {
                return;
            }
            
            var player = networkPeer.Player;
            
            // Validar sequence number (prevenir replay attacks)
            if (packet.SequenceNumber <= player.LastInputSequence)
            {
                // Input antigo, ignorar
                return;
            }
            
            player.LastInputSequence = packet.SequenceNumber;
            player.LastInputTimestamp = packet.ClientTimestamp;
            
            // Processar input
            player.ProcessInput(
                packet.MoveX,
                packet.MoveZ,
                packet.RotationY,
                packet.IsJumping,
                packet.IsSprinting,
                packet.IsCrouching,
                ServerConfig.TICK_DELTA
            );
            
            // Log para debug (pode ser removido em produção)
            // Logger.Debug($"Input processado: {player.Name} - Pos: ({player.PositionX:F2}, {player.PositionY:F2}, {player.PositionZ:F2})");
        }
        
        /// <summary>
        /// Envia estado completo do mundo para um peer
        /// </summary>
        private void SendWorldState(NetworkPeer networkPeer)
        {
            var worldState = new WorldStatePacket
            {
                CurrentTick = _worldManager.CurrentTick,
                ServerTimestamp = TimeUtils.UnixTimestamp
            };
            
            // Adicionar todos os jogadores
            foreach (var player in _worldManager.PlayerManager.GetConnectedPlayers())
            {
                worldState.Players.Add(new PlayerStateData
                {
                    PlayerId = player.Id,
                    PlayerName = player.Name,
                    PosX = player.PositionX,
                    PosY = player.PositionY,
                    PosZ = player.PositionZ,
                    RotY = player.RotationY,
                    IsSprinting = player.IsSprinting,
                    IsCrouching = player.IsCrouching
                });
            }
            
            var writer = PacketSerializer.SerializeToWriter(worldState);
            networkPeer.Peer.Send(writer, DeliveryMethod.ReliableOrdered);
        }
        
        /// <summary>
        /// Faz broadcast do estado de um jogador para todos
        /// </summary>
        private void BroadcastPlayerState(PlayerEntity player)
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
            _server.SendToAll(writer, DeliveryMethod.Sequenced);
        }
    }
}
