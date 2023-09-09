This console application is doing the following!

- Asks the user to input each word of a 12-word seed phrase, one at a time.
- Asks the user to enter a known Bitcoin address that is derived from the original (but unordered) 12-word seed phrase.
- Generates all permutations of the entered seed phrase and checks each one against the known Bitcoin address, along with validating the checksum.
- Outputs the correct ordering of the seed phrase that matches the provided Bitcoin address if it finds a match.

  Worth your attention is that there is two methods named the same
  CheckAddress()
  the first one is commented out, that only checks the first derived address of the seed phrase.

  the second that is not commented out, that checks the first 50 derived addresses of the seed phrase.
