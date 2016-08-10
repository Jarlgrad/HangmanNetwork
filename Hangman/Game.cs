﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        public Game()
        {
            Dictionary = new List<GameWords>();
            HiddenWord = new List<char>();
            WrongGuesses = new List<char>();
            Dictionary = GetDictionary();
            KeyWord = SetKeyWord();

        }

        public void StartGame(List<Player> players)
        {
            // Todo: ska bara köras en gång
            PlayGame(players);
        }

        private void PlayGame(List<Player> players)
        {
            int playRounds = 0;
            do
            {

                SetHiddenWord(KeyWord);
                Play(players);
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="players"></param>
        public void Play(List<Player> players)
        {
            //s=testvariabel
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
                        Console.WriteLine($"{item.Name} gissade rätt!");
                    }
                    else
                    {
                        Console.WriteLine($"{item.Name} gissade fel!");
                        WrongGuesses.Add(guess);
                    }

                    //Omvandlar listan av Char till en textsträng
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
                        s = item.Name;
                        i = item.Wins;
                    }
                    if (item.WonGame)
                        break;
                }


            } while (sb != KeyWord && WrongGuesses.Count < 10);  // Todo: Lägg till möjligheter att ändra antal gissningar vid nytt spel. 
            Console.WriteLine($"Game Over {s} won the game ({i})");
            Console.ReadKey();
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
        private char Guess(Player item)
        {
            // Todo: Lägg till att man kan gissa på hela ordet
            string guess;
            var falseInput = true;
            do
            {
                Thread.Sleep(2000);
                Console.Clear();
                DrawGame();
                Console.WriteLine("");
                Console.WriteLine($"{item.Name}'s tur. Vilken bokstav vill du gissa på ?");

                guess = Console.ReadLine();

                if (guess.Length != 1)
                {
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
            return guess[0];
        }
    }
}
