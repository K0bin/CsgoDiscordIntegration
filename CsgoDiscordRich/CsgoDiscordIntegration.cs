using CSGSI;
using System;
using System.Collections.Generic;
using System.Text;

namespace CsgoDiscordRich
{
    class CsgoDiscordIntegration
    {
        private const short DefaultPort = ((short)'c') + ((short)'s') + ((short)'g') + ((short)'o') + ((short)'d') + ((short)'i') + ((short)'s') + ((short)'c') + ((short)'o') + ((short)'r') + ((short)'d');
        private GameStateListener listener;
        private Discord discord;

        private DateTime gameStart;

        public CsgoDiscordIntegration(short? port = null)
        {
            short _port;
            if (port == null)
            {
                _port = DefaultPort;
            }
            else
            {
                _port = port.Value;
            }
            listener = new GameStateListener(_port);
            listener.NewGameState += OnGameStateChanged;
            
            discord = new Discord(Secret.DiscordApplicationId);
        }

        public void Start()
        {
            discord.Start();
            listener.Start();
        }

        private void OnGameStateChanged(GameState state)
        {
            if (state?.Map?.Name == "")
            {
                gameStart = default(DateTime);
                var menuPresence = MakeMenuPresence();
                if (discord.IsReady)
                {
                    discord.UpdatePresence(in menuPresence);
                }
                return;
            }
            
            if (gameStart == default(DateTime))
            {
                gameStart = DateTime.Now;
            }

            var provider = state.Provider.Name;

            var team = state.Player.Team;
            int score1, score2;
            if (team == CSGSI.Nodes.PlayerTeam.CT)
            {
                score1 = state.Map.TeamCT.Score;
                score2 = state.Map.TeamT.Score;
            }
            else
            {
                score1 = state.Map.TeamT.Score;
                score2 = state.Map.TeamCT.Score;
            }

            var map = state.Map.Name;
            var presence = MakeInGamePresence(map, provider, score1, score2, team);
            if (discord.IsReady)
            {
                discord.UpdatePresence(in presence);
            }
        }

        private Discord.RichPresence MakeInGamePresence(string mapName, string provider, int scoreTeam1, int scoreTeam2, CSGSI.Nodes.PlayerTeam side)
        {
            var seconds = gameStart.UnixTime();
            return new Discord.RichPresence()
            {
                Instance = 1,
                LargeImageKey = "map_"+mapName+"_large",
                LargeImageText = mapName,
                SmallImageKey = side == CSGSI.Nodes.PlayerTeam.T ? "side_t" : "side_ct",
                SmallImageText = side == CSGSI.Nodes.PlayerTeam.T ? "Terrorist" : "Counter-Terrorist",
                Details = provider,
                State = $"{mapName}: {scoreTeam1}-{scoreTeam2}",
                StartTimestamp = seconds
            };
        }

        private Discord.RichPresence MakeMenuPresence()
        {
            return new Discord.RichPresence()
            {
                Instance = 1,
                State = $"Menu",
            };
        }
    }
}
