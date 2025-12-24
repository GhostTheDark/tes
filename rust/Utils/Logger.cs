using System;

namespace RustLike.Server.Utils
{
    /// <summary>
    /// Sistema de logging do servidor
    /// </summary>
    public static class Logger
    {
        public enum LogLevel
        {
            Debug,
            Info,
            Warning,
            Error
        }
        
        private static LogLevel _minLevel = LogLevel.Debug;
        
        public static void SetMinLevel(LogLevel level)
        {
            _minLevel = level;
        }
        
        public static void Debug(string message)
        {
            Log(LogLevel.Debug, message);
        }
        
        public static void Info(string message)
        {
            Log(LogLevel.Info, message);
        }
        
        public static void Warning(string message)
        {
            Log(LogLevel.Warning, message);
        }
        
        public static void Error(string message)
        {
            Log(LogLevel.Error, message);
        }
        
        public static void Error(string message, Exception ex)
        {
            Log(LogLevel.Error, $"{message}\n{ex}");
        }
        
        private static void Log(LogLevel level, string message)
        {
            if (level < _minLevel) return;
            
            var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            var levelStr = level.ToString().ToUpper().PadRight(7);
            var color = GetColor(level);
            
            Console.ForegroundColor = color;
            Console.WriteLine($"[{timestamp}] [{levelStr}] {message}");
            Console.ResetColor();
        }
        
        private static ConsoleColor GetColor(LogLevel level)
        {
            return level switch
            {
                LogLevel.Debug => ConsoleColor.Gray,
                LogLevel.Info => ConsoleColor.White,
                LogLevel.Warning => ConsoleColor.Yellow,
                LogLevel.Error => ConsoleColor.Red,
                _ => ConsoleColor.White
            };
        }
    }
}
