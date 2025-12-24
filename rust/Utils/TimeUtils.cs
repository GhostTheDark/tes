using System;
using System.Diagnostics;

namespace RustLike.Server.Utils
{
    /// <summary>
    /// Utilitários para trabalhar com tempo
    /// </summary>
    public static class TimeUtils
    {
        private static readonly Stopwatch _stopwatch = Stopwatch.StartNew();
        
        /// <summary>
        /// Tempo desde o início do servidor em segundos
        /// </summary>
        public static float Time => (float)_stopwatch.Elapsed.TotalSeconds;
        
        /// <summary>
        /// Tempo desde o início do servidor em milissegundos
        /// </summary>
        public static long TimeMs => _stopwatch.ElapsedMilliseconds;
        
        /// <summary>
        /// Timestamp atual em Unix
        /// </summary>
        public static long UnixTimestamp => DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        
        /// <summary>
        /// DateTime atual UTC
        /// </summary>
        public static DateTime UtcNow => DateTime.UtcNow;
    }
}
