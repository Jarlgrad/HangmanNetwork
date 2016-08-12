using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangman
{
    public class Player
    {
        public string Name { get; set; }
        public bool WonGame { get; set; }
        public int Wins { get; set; }
    }
}
