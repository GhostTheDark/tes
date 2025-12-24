using RustLike.Server.Utils;
using System;
using System.Diagnostics;

namespace RustLike.Server.Core
{
    /// <summary>
    /// Sistema de tick com taxa fixa
    /// </summary>
    public class TickSystem
    {
        private readonly Stopwatch _stopwatch;
        private double _accumulator;
        private int _tickCount;
        private float _lastStatsTime;
        private int _ticksLastSecond;
        
        public int CurrentTick => _tickCount;
        public int ActualTickRate { get; private set; }
        
        public TickSystem()
        {
            _stopwatch = Stopwatch.StartNew();
            _accumulator = 0;
            _tickCount = 0;
            _lastStatsTime = 0;
            _ticksLastSecond = 0;
            ActualTickRate = 0;
        }
        
        /// <summary>
        /// Atualiza o sistema de tick
        /// Retorna o número de ticks que devem ser processados
        /// </summary>
        public int Update()
        {
            double deltaTime = _stopwatch.Elapsed.TotalSeconds;
            _stopwatch.Restart();
            
            _accumulator += deltaTime;
            
            int ticksToProcess = 0;
            
            // Processar ticks acumulados
            while (_accumulator >= ServerConfig.TICK_DELTA)
            {
                _accumulator -= ServerConfig.TICK_DELTA;
                ticksToProcess++;
                _tickCount++;
                _ticksLastSecond++;
                
                // Limite de ticks por frame para evitar spiral of death
                if (ticksToProcess >= 5)
                {
                    Logger.Warning($"Sistema de tick atrasado! Pulando {(int)(_accumulator / ServerConfig.TICK_DELTA)} ticks");
                    _accumulator = 0;
                    break;
                }
            }
            
            // Atualizar estatísticas a cada segundo
            float currentTime = TimeUtils.Time;
            if (currentTime - _lastStatsTime >= 1f)
            {
                ActualTickRate = _ticksLastSecond;
                _ticksLastSecond = 0;
                _lastStatsTime = currentTime;
            }
            
            return ticksToProcess;
        }
        
        /// <summary>
        /// Obtém o alpha de interpolação para renderização suave
        /// </summary>
        public float GetInterpolationAlpha()
        {
            return (float)(_accumulator / ServerConfig.TICK_DELTA);
        }
    }
}
