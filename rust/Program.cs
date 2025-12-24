using RustLike.Server.Core;
using RustLike.Server.Utils;
using System;

namespace RustLike.Server
{
    /// <summary>
    /// Ponto de entrada do servidor
    /// </summary>
    class Program
    {
        private static ServerBootstrap? _bootstrap;
        
        static void Main(string[] args)
        {
            // Configurar console
            Console.Title = "RustLike Server";
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            
            // Configurar handlers de sinal
            Console.CancelKeyPress += OnCancelKeyPress;
            AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
            
            try
            {
                // Inicializar e rodar servidor
                _bootstrap = new ServerBootstrap();
                _bootstrap.Initialize();
            }
            catch (Exception ex)
            {
                Logger.Error("Erro fatal", ex);
                Console.WriteLine("\nPressione qualquer tecla para sair...");
                Console.ReadKey();
            }
        }
        
        /// <summary>
        /// Trata Ctrl+C
        /// </summary>
        private static void OnCancelKeyPress(object? sender, ConsoleCancelEventArgs e)
        {
            e.Cancel = true;
            Logger.Info("\n🛑 Sinal de interrupção recebido (Ctrl+C)");
            Shutdown();
        }
        
        /// <summary>
        /// Trata saída do processo
        /// </summary>
        private static void OnProcessExit(object? sender, EventArgs e)
        {
            Logger.Info("🛑 Processo sendo encerrado");
            Shutdown();
        }
        
        /// <summary>
        /// Encerra o servidor
        /// </summary>
        private static void Shutdown()
        {
            if (_bootstrap != null)
            {
                _bootstrap.Shutdown();
                _bootstrap = null;
            }
        }
    }
}
