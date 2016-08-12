using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangman
{
    public class VCTProtocol
    {
        public string Version { get; set; }
        public Player Player { get; set; }
        public char Guess { get; set; }
        public string Message { get; set; }
        public List<char> AllGuesses { get; set; }
        public List<char> IncorrectGuesses { get; set; }

    }
}
