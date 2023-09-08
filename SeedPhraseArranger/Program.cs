using Newtonsoft.Json.Linq;
using NBitcoin;
using System.Net.Http.Json;
using System.Net.Http;

namespace SeedPhraseArranger;
class Program
{
    static string KnownPublicKey = "THE ADDRESS THAT IS CREATED BY THE SEED PHRASE"; // CHANGE THIS
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
            bool addressValid = await CheckAddressAgainstNode(seedPhrase);
            bool publicKeyMatches = CheckPublicKey(seedPhrase);

            if (checksumValid && addressValid && publicKeyMatches)
            {
                Console.WriteLine($"Found valid arrangement: {seedPhrase}");
                Environment.Exit(0);
            }

            return;
        }

        for (int i = position; i < seedWords.Count; i++)
        {
            // Swap
            (seedWords[position], seedWords[i]) = (seedWords[i], seedWords[position]);

            await GenerateAndCheckPermutations(new List<string>(seedWords), position + 1);

            // Backtrack
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

    static async Task<bool> CheckAddressAgainstNode(string seedPhrase)
    {
        string rpcUrl = "http://CLEARNET.ADDRESS.TO.A.NODE:8332"; // CHANGE THIS
        string rpcUsername = "RPC USERNAME"; // CHANGE THIS
        string rpcPassword = "RPC PASSWORD"; // CHANGE THIS

        Mnemonic mnemonic = new Mnemonic(seedPhrase);
        ExtKey hdRoot = mnemonic.DeriveExtKey();
        BitcoinAddress address = hdRoot.Derive(new KeyPath("m/44'/0'/0'/0/0")).PrivateKey.PubKey.GetAddress(ScriptPubKeyType.Legacy, Network.Main);

        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($"{rpcUsername}:{rpcPassword}")));

        var payload = new
        {
            jsonrpc = "2.0",
            id = 1,
            method = "validateaddress",
            @params = new[] { address.ToString() }
        };

        var response = await httpClient.PostAsJsonAsync(rpcUrl, payload);
        var content = await response.Content.ReadAsStringAsync();
        JObject json = JObject.Parse(content);
        return (bool)json["result"]["isvalid"];
    }

    static bool CheckPublicKey(string seedPhrase)
    {
        Mnemonic mnemonic = new Mnemonic(seedPhrase);
        ExtKey hdRoot = mnemonic.DeriveExtKey();
        PubKey publicKey = hdRoot.Derive(new KeyPath("m/44'/0'/0'/0/0")).PrivateKey.PubKey; // CHANGE THIS IF NEEDED
        return publicKey.ToString() == KnownPublicKey;
    }
}