using Newtonsoft.Json.Linq;
using NBitcoin;
using System.Net.Http.Json;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace SeedPhraseArranger
{
    class Program
    {
        static string KnownBitcoinAddress;
        static readonly HttpClient httpClient = new HttpClient();
        static int counter = 0;

        static async Task Main(string[] args)
        {
            string[] seedWords = new string[12];
            Console.WriteLine("Enter your 12-word seed phrase (one word at a time then press ENTER):");

            for (int i = 0; i < 12; i++)
            {
                Console.Write($"Enter word {i + 1}: ");
                seedWords[i] = Console.ReadLine();
            }

            Console.Write("Enter the known Bitcoin address: ");
            KnownBitcoinAddress = Console.ReadLine();

            Console.WriteLine("Starting to check permutations...");

            await GenerateAndCheckPermutations(seedWords.ToList(), 0);
        }
        static async Task GenerateAndCheckPermutations(List<string> seedWords, int position)
        {
            if (position == seedWords.Count - 1)
            {
                counter++;
                string seedPhrase = string.Join(" ", seedWords);

                if (counter % 100 == 0)
                {
                    Console.WriteLine($"Checked {counter} permutations so far...");
                }

                bool checksumValid = CheckChecksum(seedPhrase);
                bool addressMatches = await CheckAddress(seedPhrase);

                if (checksumValid && addressMatches)
                {
                    Console.WriteLine($"Found valid arrangement: {seedPhrase}");
                    Environment.Exit(0);
                }

                return;
            }

            for (int i = position; i < seedWords.Count; i++)
            {
                (seedWords[position], seedWords[i]) = (seedWords[i], seedWords[position]);

                await GenerateAndCheckPermutations(new List<string>(seedWords), position + 1);

                (seedWords[position], seedWords[i]) = (seedWords[i], seedWords[position]);
            }
        }
        static bool CheckChecksum(string seedPhrase)
        {
            try
            {
                Mnemonic mnemonic = new Mnemonic(seedPhrase);
                return mnemonic.IsValidChecksum;
            }
            catch
            {
                return false;
            }
        }
        static async Task<bool> CheckAddress(string seedPhrase)
        {
            Mnemonic mnemonic = new Mnemonic(seedPhrase);
            ExtKey hdRoot = mnemonic.DeriveExtKey();
            BitcoinAddress address = hdRoot.Derive(new KeyPath("m/44'/0'/0'/0/0")).PrivateKey.PubKey.GetAddress(ScriptPubKeyType.Legacy, Network.Main);
            return KnownBitcoinAddress == address.ToString();
        }
    }
}
