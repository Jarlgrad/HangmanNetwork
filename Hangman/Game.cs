using Hangman;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Hangman
{
    public class Game
    {
        public string KeyWord { get; set; }
        public List<char> HiddenWord { get; set; }
        public List<char> WrongGuesses { get; set; }
        public List<GameWords> Dictionary { get; set; }
        public Server MyServer { get; set; }
        public Queue<char> GuessQueue { get; set; }
        public bool RoundOver { get; set; }


        public Game(Server myServer)
        {
            Dictionary = new List<GameWords>();
            HiddenWord = new List<char>();
            WrongGuesses = new List<char>();
            GuessQueue = new Queue<char>();
            Dictionary = GetDictionary();
            KeyWord = SetKeyWord();
            MyServer = myServer;
            Thread clientThread = new Thread(GetGuessQueue);
            clientThread.Start();

        }

        public void GetGuessQueue()
        {
            do
            {
                Thread.Sleep(700);
                if (GuessQueue.Count > 0)
                {
                    Play(GuessQueue.Dequeue());
                }
            } while (!RoundOver);

        }

        public void StartGame(List<PlayerHandler> players)
        {
            // Todo: ska bara köras en gång
            PlayGame(players);
        }

        private void PlayGame(List<PlayerHandler> players)
        {
            int playRounds = 0;
            do
            {
                SetHiddenWord(KeyWord);
                playRounds++;
            } while (playRounds < 3);
        }

        private string SetKeyWord()
        {
            // Todo: Vid Nytt spel Ska denna metod ta fram ett nytt ord! 
            Random rand = new Random();
            var keyword = Dictionary.ElementAt(rand.Next(0, Dictionary.Count));
            return keyword.Word;
        }

        /// <summary>
        /// Hämtar ordlistan från en Json-fil och sätter ihop det i en lista av gamewords
        /// </summary>
        /// <returns>List<GameWords></returns>
        private List<GameWords> GetDictionary()
        {
            // Måste sätta ny directory!! 
            StreamReader readFile = new StreamReader(@"C:\Users\Administrator\Source\Repos\HangmanNetwork\Hangman\Ordlista.txt");
            var tmpWords = readFile.ReadLine();

            GameWords words = new GameWords();
            var tmpdictionary = JsonConvert.DeserializeObject<List<GameWords>>(tmpWords);
            return tmpdictionary;
        }


        /// <summary>
        ///  Ritar upp keywordet i censurerad version vilket ska rítas upp för spelaren
        /// </summary>
        /// <param name="keyWord"></param>
        public void SetHiddenWord(string keyWord)
        {
            for (int i = 0; i < keyWord.Length; i++)
            {
                HiddenWord.Add('_');
            }
        }

        /// <summary>
        /// Tar emot en spelares gissning och kollar om den stämmer, sätter isåfall in de rätta bokstäverna i hiddenword
        /// </summary>
        /// <param name="c">Spelarens gissning</param>
        /// <returns>HiddenWord med de rätt gissade bokstäverna</returns>
        public bool ValidateGuess(char c)
        {
            bool guessIsCorrect = false;
            for (int i = 0; i < KeyWord.Length; i++)
            {
                if (KeyWord[i] == c)
                {
                    guessIsCorrect = true;
                    HiddenWord[i] = c;
                }
            }
            return guessIsCorrect;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="players"></param>
        public void Play(char a)
        {
            //s=testvariabel
            string s = "";
            int i = 0;
            var sb = "";

            Console.Clear();
            var IsCorrect = ValidateGuess(a);

            if (IsCorrect)
            {
                MyServer.Broadcast($"this.player gissade rätt!");
            }
            else
            {
                MyServer.Broadcast($"this.player gissade fel!");
                WrongGuesses.Add(a);
            }

            //Omvandlar listan av Char till en textsträng
            var strb = new StringBuilder();
            foreach (var letter in HiddenWord)
            {
                strb.Append(letter);
            }
            sb = strb.ToString();

            //if (sb == KeyWord)
            //{
            //    item.WonGame = true;
            //    item.Wins++;
            //    s = item.Name;
            //    i = item.Wins;
            //}
            //if (item.WonGame)
            //    break;
            MyServer.Broadcast(DrawGame());

            if (sb != KeyWord && WrongGuesses.Count < 10)
                RoundOver = true;

            // Todo: Lägg till möjligheter att ändra antal gissningar vid nytt spel. 
            MyServer.Broadcast($"Game Over {s} won the game ({i})");

        }

        /// <summary>
        /// Ritar spelplanen
        /// </summary>
        private string DrawGame()
        {
            string tempStr = string.Empty;
            foreach (var item in HiddenWord)
            {
                tempStr += item;
            }
            return tempStr;

        }

        /// <summary>
        /// Säger att det är spelarens tur, tar emot en gissning och felcheckar denna
        /// </summary>
        /// <param name="item">Tar emot en player</param>
        /// <returns>Spelarens gissning</returns>
        private char Guess(string guess)
        {
            // Todo: Lägg till att man kan gissa på hela ordet

            var falseInput = true;
            do
            {
                MyServer.Broadcast("");
                //MyServer.Broadcast($"Player.Name's tur. Vilken bokstav vill du gissa på ?");



                if (guess.Length != 1)
                {
                    //Todo: Broadcasta till spelaren som gissade. 
                    Console.WriteLine("Du får bara gissa på en bokstav");
                }
                //Todo: Jämför med bokstäver som redan är gissade - Isåfall får man Fel.
                //else if (guess == HiddenWord.FirstOrDefault(c => c.ToString().Equals(guess)).ToString())
                //{
                //    Console.WriteLine("Du har redan gissat på denna bokstav");
                //}
                else
                {
                    falseInput = false;
                }

            } while (falseInput);
            var IsCorrect = ValidateGuess(guess[0]);
            DrawGame();
            return guess[0];
        }
    }
}
