﻿using Hangman;
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
        public Queue<VCTProtocol> GuessQueue { get; set; }
        public bool RoundOver { get; set; }
        public List<string> PlayersToClients { get; set; }
        public List<char> AllGuesses { get; set; }
        public Thread QueueThread { get; set; }


        public Game(Server myServer)
        {
            Dictionary = new List<GameWords>();
            HiddenWord = new List<char>();
            WrongGuesses = new List<char>();
            AllGuesses = new List<char>();
            GuessQueue = new Queue<VCTProtocol>();
            Dictionary = GetDictionary();
            QueueThread = new Thread(GetGuessQueue);
            QueueThread.Start();
            MyServer = myServer;
        }

        public void GetGuessQueue()
        {
            while (true)
            {
                Thread.Sleep(1000);
                if (GuessQueue.Count > 0)
                {
                    Play(GuessQueue.Dequeue());
                }
            }
        }

        public void StartGame()
        {
            AllGuesses.Clear();
            WrongGuesses.Clear();
            // Todo: ska bara köras en gång
            KeyWord = SetKeyWord();

            SetHiddenWord(KeyWord);
            var tmpStr = DrawGame();
            VCTProtocol tmpVCT = new VCTProtocol();
            tmpVCT.Message = $"Välkommen till en ny runda av Gubbhäng!!";
            tmpVCT.GameBoard = tmpStr;
            MyServer.ServerBroadcast(tmpVCT);

        }

        /// <summary>
        /// Väljer randomiserat ut ett ord ur dictionary
        /// </summary>
        /// <returns>Ord ur dictionary</returns>
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
            Console.WriteLine("Här börjar GetDictionary-metoden");
            // Måste sätta ny directory!! 
            StreamReader readFile = new StreamReader(@"C:\Users\Administrator\Documents\Visual Studio 2015\Projects\Hangman\Hangman\Hangman\Ordlista.txt");
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
            HiddenWord.Clear();

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
            Console.WriteLine("Här börjar ValidateGuess-metoden");
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
        public void Play(VCTProtocol tmpInput)
        {
            Console.WriteLine("Här börjar Play-metoden");
            var sb = "";

            //Console.Clear();
            var IsCorrect = ValidateGuess(tmpInput.Guess);

            PlayersToClients = new List<string>();
            foreach (var player in MyServer.players)
            {
                PlayersToClients.Add(player.PlayerData.Player.Name);
            }
            tmpInput.Players = PlayersToClients;

            AllGuesses.Add(tmpInput.Guess);
            tmpInput.AllGuesses = AllGuesses;
            tmpInput.GameBoard = DrawGame();


            if (IsCorrect)
            {
                tmpInput.Message = $"{tmpInput.Player.Name} gissade rätt!";
                MyServer.ServerBroadcast(tmpInput);
                tmpInput.AllGuesses.Add(tmpInput.Guess);
                Console.WriteLine("Nu har gisnningen lagts till i AllGuesses");
            }
            else
            {

                tmpInput.Message = $"{tmpInput.Player.Name} gissade fel!";
                WrongGuesses.Add(tmpInput.Guess);
                tmpInput.IncorrectGuesses = WrongGuesses;
                MyServer.ServerBroadcast(tmpInput);
                Console.WriteLine("Nu har gissningen lagts till i AllGuesses och IncorrectGuesses");
            }

            //Omvandlar listan av Char till en textsträng
            var strb = new StringBuilder();
            foreach (var letter in HiddenWord)
            {
                strb.Append(letter);
            }
            sb = strb.ToString();

            // Todo: Lägg till möjligheter att ändra antal gissningar vid nytt spel. 
            if (sb == KeyWord)
            {
                RoundOver = true;

                tmpInput.Message = $"Där satt den! Alla får 1 poäng!";
                tmpInput.RoundOver = true;
                foreach (var item in MyServer.players)
                {
                    if (item.PlayerData.Player.Name == tmpInput.Player.Name)
                    {
                        item.PlayerData.Player.Wins++;
                    }
                }
                MyServer.ServerBroadcast(tmpInput);

                StartGame();
            }
            else if (WrongGuesses.Count > 5)
            {
                RoundOver = true;

                tmpInput.Message = $"Kunde ni inte den lätta? {Environment.NewLine}Alla tappar 1 poäng!";
                foreach (var item in MyServer.players)
                {
                    item.PlayerData.Player.Wins--;
                }
                StartGame();
            }
        }

        /// <summary>
        /// Ritar spelplanen
        /// </summary>
        private string DrawGame()
        {
            Console.WriteLine("Här börjar DrawGame-metoden");

            // Todo: Sätt fler properties på DrawGame som ett protokoll. Förenklar felsökning.
            //VCTProtocol tmpVCT = new VCTProtocol();
            string tempStr = string.Empty;
            foreach (var item in HiddenWord)
            {
                tempStr += item;
            }
            //tmpVCT.GameBoard = tempStr;
            //tmpVCT.Players = PlayersToClients;
            return tempStr;
        }
    }
}
