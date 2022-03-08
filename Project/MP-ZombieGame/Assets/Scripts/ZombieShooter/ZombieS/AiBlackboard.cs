using System.Collections.Generic;

namespace ZombieGame
{
    public static class AiBlackboard
    {
        private static PlayerController[] players = null;

        public static PlayerController[] GetPlayers()
        {
            VerifyPlayers();
            return players;
        }

        private static void VerifyPlayers()
        {
            if (players == null || players.Length == 0) return;

            var removes = new List<int>();
            for (int i = 0; i < players.Length; i++)
                if (players[i] == null) removes.Add(i);

            foreach (var id in removes) DeregisterPlayer(id);
        }

        public static bool PlayerRegistered(PlayerController player)
        {
            if (players == null)
            {
                return false;
            }
            else
            {
                for (int i = 0; i < players.Length; i++) if (players[i] == player) return true;
                return false;
            }
        }

        public static void RegisterPlayer(PlayerController player)
        {
            if (PlayerRegistered(player)) return;

            if (players == null)
            {
                players = new PlayerController[] { player };
            }
            else
            {
                var old = players;
                players = new PlayerController[players.Length + 1];

                for (int i = 0; i < old.Length; i++) players[i] = old[i];
                players[old.Length] = player;
            }
        }

        public static void DeregisterPlayer(PlayerController player)
        {
            if (PlayerRegistered(player)) return;

            if (players != null)
            {
                var old = players;
                players = new PlayerController[players.Length - 1];

                for (int j = 0, i = 0; i < old.Length; i++)
                {
                    if (old[i] == player) continue;
                    players[j] = old[i];
                    j++;
                }
            }
        }

        public static void DeregisterPlayer(int index)
        {
            if (players != null)
            {
                var old = players;
                players = new PlayerController[players.Length - 1];

                for (int j = 0, i = 0; i < old.Length; i++)
                {
                    if (i == index) continue;
                    players[j] = old[i];
                    j++;
                }
            }
        }
    }
}