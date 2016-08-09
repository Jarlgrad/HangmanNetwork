using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangman
{
    public class Game
    {
        public string KeyWord { get; set; }
        public List<char> HiddenWord { get; set; }
        public List<char> WrongGuesses { get; set; }

        public void StartGame( List<Player> players)
        {
           List<GameWords> dictionary= GetDictionary(); //Todo: ska bara köras en gång
            PlayGame(dictionary, players);
        }

        private void PlayGame(List<GameWords> dictionary, List<Player> players)
        {
            int playRounds = 0;
            do
            {

            KeyWord = SetKeyWord(dictionary);
            HiddenWord = new List<char>();
            WrongGuesses = new List<char>();
            SetHiddenWord(KeyWord);
            Play(players);
                playRounds++;
            } while (playRounds < 3);
        }

        private string SetKeyWord(List<GameWords>dictionary)
        {

            // Todo: Vid Nytt spel Ska denna metod ta fram ett nytt ord! 

            Random rand = new Random();

            var keyword = dictionary.ElementAt(rand.Next(0, dictionary.Count));



            return keyword.Word;  
        }

        private List<GameWords> GetDictionary()
        {
            // Måste sätta ny directory!! 
            StreamReader readFile = new StreamReader(@"C:\Users\Administrator\Documents\Visual Studio 2015\Projects\Hangman\Hangman\Hangman\Ordlista.txt");
            var tmpWords = readFile.ReadLine();

            GameWords words = new GameWords();
            var tmpdictionary = JsonConvert.DeserializeObject<List<GameWords>>(tmpWords);

            return tmpdictionary;
        }

        public void SetHiddenWord(string keyWord)
        {
            for (int i = 0; i < keyWord.Length; i++)
            {
                HiddenWord.Add('_');

            
            }
        }

        public bool PlayerAction(char c)
        {
            bool guess = false;
            for (int i = 0; i < KeyWord.Length; i++)
            {
                if (KeyWord[i] == c)
                {
                    guess = true;
                    HiddenWord[i] = c;
                }
            }
            return guess;
        }

        public void Play(List<Player> players)
        {
            string s = "";
            int i = 0;
            var sb = "";

            do
            {
                foreach (var item in players)
                {

                    char guess = Guess(item);
                    var IsCorrect = PlayerAction(guess);

                    if (IsCorrect)
                    {
                        Console.WriteLine($"{item.PlayerName} gissade rätt!");
                    }
                    else
                    {
                        Console.WriteLine($"{item.PlayerName} gissade fel!");
                        WrongGuesses.Add(guess);
                    }

                    var strb = new StringBuilder();
                    foreach (var letter in HiddenWord)
                    {
                        strb.Append(letter);
                    }
                    sb = strb.ToString();

                    if (sb == KeyWord)
                    {
                        item.WonGame = true;
                        item.Wins++;
                        s = item.PlayerName;
                        i = item.Wins;
                    }
                    if (item.WonGame)
                        break;
                }


            } while (sb != KeyWord && WrongGuesses.Count < 10);  //Todo:Lägg till möjligheter att ändra antal gissningar vid nytt spel. 
            Console.WriteLine($"Game Over {s} won the game ({i})");
            Console.ReadKey();
        }

        private void DrawGame()
        {
            foreach (var item in HiddenWord)
            {
                Console.Write(item);
            }
        }

        private char Guess(Player item)
        {
            // TODO: Lägg till att man kan gissa på hela ordet
            string guess;
            var falseInput = true;
            do
            {
                Console.Clear();
                DrawGame();
                Console.WriteLine("");
                Console.WriteLine($"{item.PlayerName}'s tur. Vilken bokstav vill du gissa på ?");

                guess = Console.ReadLine();

                if (guess.Length != 1)
                {
                    Console.WriteLine("Du får bara gissa på en bokstav");
                }
                // Todo: Jämför med bokstäver som redan är gissade - Isåfall får man Fel. 
                //else if (guess == HiddenWord.FirstOrDefault(c => c.(guess)).ToString())
                //{
                //    Console.WriteLine("Du har redan gissat på denna bokstav");
                //}
                else
                {
                    falseInput = false;
                }

            } while (falseInput);
            return guess[0];
        }
    }
}
