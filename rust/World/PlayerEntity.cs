using System;

namespace RustLike.Server.World
{
    /// <summary>
    /// Representa um jogador no mundo do servidor
    /// </summary>
    public class PlayerEntity
    {
        // Identificação
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public long SteamId { get; set; } // Para futuro
        
        // Posição e Rotação
        public float PositionX { get; set; }
        public float PositionY { get; set; }
        public float PositionZ { get; set; }
        
        public float RotationX { get; set; }
        public float RotationY { get; set; }
        public float RotationZ { get; set; }
        
        // Velocidade
        public float VelocityX { get; set; }
        public float VelocityY { get; set; }
        public float VelocityZ { get; set; }
        
        // Estado
        public bool IsGrounded { get; set; } = true;
        public bool IsSprinting { get; set; }
        public bool IsCrouching { get; set; }
        public bool IsJumping { get; set; }
        
        // Estatísticas
        public float Health { get; set; } = 100f;
        public float MaxHealth { get; set; } = 100f;
        public float Stamina { get; set; } = 100f;
        public float MaxStamina { get; set; } = 100f;
        
        // Timestamps
        public long LastInputTimestamp { get; set; }
        public long LastUpdateTimestamp { get; set; }
        public long ConnectedTimestamp { get; set; }
        public long LastSaveTimestamp { get; set; }
        
        // Controle de Input
        public int LastInputSequence { get; set; }
        public float LastInputTime { get; set; }
        
        // Flags
        public bool IsAlive { get; set; } = true;
        public bool IsConnected { get; set; } = true;
        
        public PlayerEntity()
        {
            ConnectedTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }
        
        /// <summary>
        /// Processa input do jogador
        /// </summary>
        public void ProcessInput(float moveX, float moveZ, float rotationY, bool isJumping, bool isSprinting, bool isCrouching, float deltaTime)
        {
            // Atualizar flags de estado
            IsSprinting = isSprinting;
            IsCrouching = isCrouching;
            IsJumping = isJumping;
            
            // Calcular velocidade de movimento
            float speed = ServerConfig.PLAYER_SPEED;
            if (isSprinting) speed *= 1.5f;
            if (isCrouching) speed *= 0.5f;
            
            // Normalizar vetor de movimento
            float magnitude = MathF.Sqrt(moveX * moveX + moveZ * moveZ);
            if (magnitude > 1f)
            {
                moveX /= magnitude;
                moveZ /= magnitude;
            }
            
            // Calcular movimento baseado na rotação
            float radians = RotationY * MathF.PI / 180f;
            float cos = MathF.Cos(radians);
            float sin = MathF.Sin(radians);
            
            float worldMoveX = moveX * cos - moveZ * sin;
            float worldMoveZ = moveX * sin + moveZ * cos;
            
            // Aplicar velocidade
            VelocityX = worldMoveX * speed;
            VelocityZ = worldMoveZ * speed;
            
            // Atualizar posição
            PositionX += VelocityX * deltaTime;
            PositionZ += VelocityZ * deltaTime;
            
            // Atualizar rotação
            RotationY = rotationY;
            
            // Garantir que Y não fique negativo (chão temporário)
            if (PositionY < 0f)
            {
                PositionY = 0f;
                IsGrounded = true;
            }
            
            LastUpdateTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }
        
        /// <summary>
        /// Valida se o movimento é válido (anti-cheat básico)
        /// </summary>
        public bool ValidateMovement(float newX, float newY, float newZ)
        {
            float distance = MathF.Sqrt(
                MathF.Pow(newX - PositionX, 2) +
                MathF.Pow(newY - PositionY, 2) +
                MathF.Pow(newZ - PositionZ, 2)
            );
            
            return distance <= ServerConfig.MAX_MOVE_DISTANCE_PER_TICK;
        }
        
        /// <summary>
        /// Spawn em posição inicial
        /// </summary>
        public void SpawnAtPosition(float x, float y, float z)
        {
            PositionX = x;
            PositionY = y;
            PositionZ = z;
            RotationX = 0f;
            RotationY = 0f;
            RotationZ = 0f;
            VelocityX = 0f;
            VelocityY = 0f;
            VelocityZ = 0f;
            IsAlive = true;
            IsGrounded = true;
        }
    }
}
