using System;
using System.Text;

namespace RustlikeServer.Network
{
    /// <summary>
    /// Cliente solicita ataque
    /// </summary>
    [Serializable]
    public class AttackRequestPacket
    {
        public int VictimId { get; set; }
        public int WeaponItemId { get; set; }
        public float HitPosX { get; set; }
        public float HitPosY { get; set; }
        public float HitPosZ { get; set; }
        public bool IsHeadshot { get; set; }

        public byte[] Serialize()
        {
            byte[] data = new byte[22];
            BitConverter.GetBytes(VictimId).CopyTo(data, 0);
            BitConverter.GetBytes(WeaponItemId).CopyTo(data, 4);
            BitConverter.GetBytes(HitPosX).CopyTo(data, 8);
            BitConverter.GetBytes(HitPosY).CopyTo(data, 12);
            BitConverter.GetBytes(HitPosZ).CopyTo(data, 16);
            data[20] = IsHeadshot ? (byte)1 : (byte)0;
            return data;
        }

        public static AttackRequestPacket Deserialize(byte[] data)
        {
            return new AttackRequestPacket
            {
                VictimId = BitConverter.ToInt32(data, 0),
                WeaponItemId = BitConverter.ToInt32(data, 4),
                HitPosX = BitConverter.ToSingle(data, 8),
                HitPosY = BitConverter.ToSingle(data, 12),
                HitPosZ = BitConverter.ToSingle(data, 16),
                IsHeadshot = data[20] == 1
            };
        }
    }

    /// <summary>
    /// Servidor confirma ataque (para atacante)
    /// </summary>
    [Serializable]
    public class AttackConfirmPacket
    {
        public bool Success { get; set; }
        public int VictimId { get; set; }
        public float Damage { get; set; }
        public bool WasHeadshot { get; set; }
        public bool WasKill { get; set; }
        public string Message { get; set; }

        public byte[] Serialize()
        {
            byte[] messageBytes = Encoding.UTF8.GetBytes(Message ?? "");
            byte[] data = new byte[14 + messageBytes.Length];
            
            data[0] = Success ? (byte)1 : (byte)0;
            BitConverter.GetBytes(VictimId).CopyTo(data, 1);
            BitConverter.GetBytes(Damage).CopyTo(data, 5);
            data[9] = WasHeadshot ? (byte)1 : (byte)0;
            data[10] = WasKill ? (byte)1 : (byte)0;
            BitConverter.GetBytes(messageBytes.Length).CopyTo(data, 11);
            messageBytes.CopyTo(data, 15);
            
            return data;
        }

        public static AttackConfirmPacket Deserialize(byte[] data)
        {
            int messageLength = BitConverter.ToInt32(data, 11);
            return new AttackConfirmPacket
            {
                Success = data[0] == 1,
                VictimId = BitConverter.ToInt32(data, 1),
                Damage = BitConverter.ToSingle(data, 5),
                WasHeadshot = data[9] == 1,
                WasKill = data[10] == 1,
                Message = messageLength > 0 ? Encoding.UTF8.GetString(data, 15, messageLength) : ""
            };
        }
    }

    /// <summary>
    /// Broadcast: jogador foi atingido
    /// </summary>
    [Serializable]
    public class PlayerHitPacket
    {
        public int VictimId { get; set; }
        public int AttackerId { get; set; }
        public float Damage { get; set; }
        public bool WasHeadshot { get; set; }
        public float HitPosX { get; set; }
        public float HitPosY { get; set; }
        public float HitPosZ { get; set; }

        public byte[] Serialize()
        {
            byte[] data = new byte[22];
            BitConverter.GetBytes(VictimId).CopyTo(data, 0);
            BitConverter.GetBytes(AttackerId).CopyTo(data, 4);
            BitConverter.GetBytes(Damage).CopyTo(data, 8);
            data[12] = WasHeadshot ? (byte)1 : (byte)0;
            BitConverter.GetBytes(HitPosX).CopyTo(data, 13);
            BitConverter.GetBytes(HitPosY).CopyTo(data, 17);
            BitConverter.GetBytes(HitPosZ).CopyTo(data, 21);
            return data;
        }

        public static PlayerHitPacket Deserialize(byte[] data)
        {
            return new PlayerHitPacket
            {
                VictimId = BitConverter.ToInt32(data, 0),
                AttackerId = BitConverter.ToInt32(data, 4),
                Damage = BitConverter.ToSingle(data, 8),
                WasHeadshot = data[12] == 1,
                HitPosX = BitConverter.ToSingle(data, 13),
                HitPosY = BitConverter.ToSingle(data, 17),
                HitPosZ = BitConverter.ToSingle(data, 21)
            };
        }
    }

    /// <summary>
    /// Notificação de morte com informações do killer
    /// </summary>
    [Serializable]
    public class PlayerDeathDetailedPacket
    {
        public int VictimId { get; set; }
        public int KillerId { get; set; }
        public string KillerName { get; set; }
        public int WeaponItemId { get; set; }
        public bool WasHeadshot { get; set; }
        public float Distance { get; set; }

        public byte[] Serialize()
        {
            byte[] nameBytes = Encoding.UTF8.GetBytes(KillerName ?? "Unknown");
            byte[] data = new byte[18 + nameBytes.Length];
            
            BitConverter.GetBytes(VictimId).CopyTo(data, 0);
            BitConverter.GetBytes(KillerId).CopyTo(data, 4);
            BitConverter.GetBytes(nameBytes.Length).CopyTo(data, 8);
            nameBytes.CopyTo(data, 12);
            int offset = 12 + nameBytes.Length;
            BitConverter.GetBytes(WeaponItemId).CopyTo(data, offset);
            data[offset + 4] = WasHeadshot ? (byte)1 : (byte)0;
            BitConverter.GetBytes(Distance).CopyTo(data, offset + 5);
            
            return data;
        }

        public static PlayerDeathDetailedPacket Deserialize(byte[] data)
        {
            int nameLength = BitConverter.ToInt32(data, 8);
            string killerName = nameLength > 0 ? Encoding.UTF8.GetString(data, 12, nameLength) : "Unknown";
            int offset = 12 + nameLength;
            
            return new PlayerDeathDetailedPacket
            {
                VictimId = BitConverter.ToInt32(data, 0),
                KillerId = BitConverter.ToInt32(data, 4),
                KillerName = killerName,
                WeaponItemId = BitConverter.ToInt32(data, offset),
                WasHeadshot = data[offset + 4] == 1,
                Distance = BitConverter.ToSingle(data, offset + 5)
            };
        }
    }

    /// <summary>
    /// Respawn após morte
    /// </summary>
    [Serializable]
    public class RespawnPacket
    {
        public int PlayerId { get; set; }
        public float SpawnX { get; set; }
        public float SpawnY { get; set; }
        public float SpawnZ { get; set; }

        public byte[] Serialize()
        {
            byte[] data = new byte[16];
            BitConverter.GetBytes(PlayerId).CopyTo(data, 0);
            BitConverter.GetBytes(SpawnX).CopyTo(data, 4);
            BitConverter.GetBytes(SpawnY).CopyTo(data, 8);
            BitConverter.GetBytes(SpawnZ).CopyTo(data, 12);
            return data;
        }

        public static RespawnPacket Deserialize(byte[] data)
        {
            return new RespawnPacket
            {
                PlayerId = BitConverter.ToInt32(data, 0),
                SpawnX = BitConverter.ToSingle(data, 4),
                SpawnY = BitConverter.ToSingle(data, 8),
                SpawnZ = BitConverter.ToSingle(data, 12)
            };
        }
    }

    /// <summary>
    /// Cliente solicita reload de arma
    /// </summary>
    [Serializable]
    public class ReloadRequestPacket
    {
        public int WeaponItemId { get; set; }

        public byte[] Serialize()
        {
            return BitConverter.GetBytes(WeaponItemId);
        }

        public static ReloadRequestPacket Deserialize(byte[] data)
        {
            return new ReloadRequestPacket
            {
                WeaponItemId = BitConverter.ToInt32(data, 0)
            };
        }
    }

    /// <summary>
    /// Confirma reload
    /// </summary>
    [Serializable]
    public class ReloadConfirmPacket
    {
        public bool Success { get; set; }
        public int AmmoRemaining { get; set; }
        public string Message { get; set; }

        public byte[] Serialize()
        {
            byte[] messageBytes = Encoding.UTF8.GetBytes(Message ?? "");
            byte[] data = new byte[9 + messageBytes.Length];
            
            data[0] = Success ? (byte)1 : (byte)0;
            BitConverter.GetBytes(AmmoRemaining).CopyTo(data, 1);
            BitConverter.GetBytes(messageBytes.Length).CopyTo(data, 5);
            messageBytes.CopyTo(data, 9);
            
            return data;
        }

        public static ReloadConfirmPacket Deserialize(byte[] data)
        {
            int messageLength = BitConverter.ToInt32(data, 5);
            return new ReloadConfirmPacket
            {
                Success = data[0] == 1,
                AmmoRemaining = BitConverter.ToInt32(data, 1),
                Message = messageLength > 0 ? Encoding.UTF8.GetString(data, 9, messageLength) : ""
            };
        }
    }
}