Você é um engenheiro sênior de redes e jogos multiplayer, especialista em Unity 6, C#, arquitetura cliente/servidor, jogos survival estilo Rust e LiteNetLib (versão mais recente – 23/12/2025).
Seu objetivo é CRIAR UM SISTEMA COMPLETO, funcional e sem erros, composto por:
🖥️ Servidor dedicado C# (.NET Console App)
🎮 Cliente Unity 6
🔌 Comunicação via LiteNetLib
🧠 Arquitetura 100% autoritativa no servidor
💾 Sistema de save/load persistente
🚀 Baixo lag, sincronização suave, sem desync
🧩 VISÃO GERAL DO SISTEMA
Arquitetura
Servidor roda fora da Unity
Cliente Unity apenas:
Envia input
Renderiza
Servidor:
Simula mundo
Valida ações
Sincroniza estado
Salva dados
🖥️ PARTE 1 – SERVIDOR DEDICADO (C#)
🔹 Tecnologia
.NET 9 ou superior
LiteNetLib (última versão estável)
Console Application
Tickrate fixo (ex: 30 ou 60)
📂 ESTRUTURA DE PASTAS (SERVIDOR)
Crie exatamente essa estrutura:
Copiar código

RustLike.Server/
│
├── Program.cs
├── ServerConfig.cs
│
├── Core/
│   ├── ServerBootstrap.cs
│   ├── GameLoop.cs
│   ├── TickSystem.cs
│
├── Network/
│   ├── NetworkServer.cs
│   ├── NetworkPeer.cs
│   ├── PacketHandler.cs
│   ├── PacketSerializer.cs
│
├── Packets/
│   ├── IPacket.cs
│   ├── PacketType.cs
│   ├── HandshakePacket.cs
│   ├── PlayerInputPacket.cs
│   ├── PlayerStatePacket.cs
│   ├── WorldStatePacket.cs
│
├── World/
│   ├── WorldManager.cs
│   ├── PlayerEntity.cs
│   ├── PlayerManager.cs
│
├── Persistence/
│   ├── SaveSystem.cs
│   ├── PlayerSaveData.cs
│
└── Utils/
    ├── TimeUtils.cs
    ├── Logger.cs
🔹 REGRAS OBRIGATÓRIAS DO SERVIDOR
Servidor é a autoridade absoluta
Cliente nunca altera estado diretamente
Todo input passa por validação
Sistema de tick fixo
Serialização manual (sem reflection)
Nunca usar UnityEngine no servidor
Nenhuma dependência visual
Logs claros e detalhados
🔹 FUNCIONALIDADES DO SERVIDOR
Implemente:
Sistema de conexão/desconexão
Handshake inicial
Spawn de player
Processamento de input
Atualização de posição
Broadcast de estado
Save automático por intervalo
Load ao conectar
Anti-spam de pacotes
Controle de latency (RTT)
🎮 PARTE 2 – CLIENTE UNITY 6
🔹 Versão
Unity 6
Projeto 3D Core
Input System novo
📂 ESTRUTURA DE PASTAS (UNITY)
Copiar código

Assets/
│
├── Scripts/
│   ├── Network/
│   │   ├── ClientNetworkManager.cs
│   │   ├── ClientPacketHandler.cs
│   │
│   ├── Player/
│   │   ├── PlayerController.cs
│   │   ├── PlayerView.cs
│   │
│   ├── UI/
│   │   ├── ConnectUI.cs
│   │
│   └── World/
│       ├── WorldStateApplier.cs
│
├── Scenes/
│   ├── ConnectScene.unity
│   ├── GameplayScene.unity
🎬 CENA 1 – CONNECT SCENE
UI obrigatória:
InputField (IP)
InputField (Porta)
Botão Play
Fluxo:
Usuário digita IP
Clica Play
Cliente conecta ao servidor
Handshake bem-sucedido
Carrega GameplayScene
🎮 CENA 2 – GAMEPLAY SCENE
Deve conter:
Player local
Players remotos
Câmera
Loop de envio de input
Aplicação do estado recebido
🔁 SINCRONIZAÇÃO DE REDE
Cliente → Servidor
Input (WASD, mouse)
Frequência limitada
Timestamp
Servidor → Cliente
Estado do player
Interpolação
Snapshot system
💾 SISTEMA DE SAVE / LOAD
Servidor deve salvar:
Posição do player
Último login
ID do jogador
Formato:
JSON ou binário
Um arquivo por jogador
⚠️ REGRAS IMPORTANTES
Resolver conflitos de namespace (ex: DisconnectReason)
Nunca duplicar enums
Usar namespaces claros
Código limpo e comentado
Sem warnings CS8618 / CS0104
Código compilável sem ajustes manuais
📌 RESULTADO FINAL ESPERADO
Ao final, você deve entregar:
Todos os arquivos do servidor
Todos os scripts do cliente Unity
Explicação de como rodar:
Servidor
Cliente
Explicação do fluxo de rede
Nenhum erro de build
Sincronização suave
Sistema pronto para evoluir para:
Combate
Inventário
Construção
Mundo persistente
🎯 OBJETIVO FINAL
Criar a base perfeita de um jogo estilo Rust, com:
Servidor dedicado real
Código limpo
Arquitetura profissional
Pronto para escalar
🚨 IMPORTANTE
Não pule arquivos.
Não simplifique.
Não omita código crítico.
Não use pseudocódigo.
Tudo deve estar completo, funcional e organizado.
