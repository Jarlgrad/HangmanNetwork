using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangman
{
    class Program
    {
        static void Main(string[] args)
        {
            Game newGame = new Game();

            newGame.StartGame( new List<Player>
            {
                new Player { PlayerName = "Player 1" },
                new Player { PlayerName = "Player 2" }
            });
        }
    }
}
