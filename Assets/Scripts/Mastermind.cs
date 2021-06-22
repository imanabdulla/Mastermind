using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class Mastermind : MonoBehaviour
{
    class Digit { public int pos; public int val; public int count; }
    const int NumOfDigitsOfGuessedNumber = 4;
    [SerializeField] Text playerText, aiText;
    [SerializeField] InputField playerInput;
    [SerializeField] string guessedNumber;
    [SerializeField] string correctNumber;
    [SerializeField] bool startGuessing, firstTime = true;
    [SerializeField] List<string> allPermutations = new List<string>();
    [SerializeField] List<string> prunedSet = new List<string>();
    int trialsNumber = 0;
    bool startSolving;
    //================================================================================
    void Start()
    {
        GameManager.instance.OnGameStart += MakeIntroBeforeGameplay;
        allPermutations = GeneratePermutations(NumOfDigitsOfGuessedNumber);
        foreach (var n in allPermutations)
        {
            prunedSet.Add(n);
        }
    }

    void Update()
    {
        if (startGuessing)
            Invoke("GuessNumber", 1);
        else if (startSolving)
            Invoke("DisplayResult", 1);
    }

    void MakeIntroBeforeGameplay() => StartCoroutine("GameplayIntro");
    IEnumerator GameplayIntro()
    {
        playerText.text = "Hello dear!\nLet's play Mastermind board game together!";
        yield return new WaitForSeconds(2f);
        playerText.text = "";
        aiText.text = "Cool!\nLet's Start";
        yield return new WaitForSeconds(2f);
        playerText.text = "Let me first think of a number!\n mmmmmmm";
        aiText.text = "";
        yield return new WaitForSeconds(2f);
        playerInput.gameObject.SetActive(true);
    }
    public void FinishGameplayIntro()
    {
        playerInput.gameObject.SetActive(false);
        playerText.text = "Now, guess a number of 4 digits.\nThe possible digits are\n{0, 1, 2, 3, 4, 5, 6, 7, 8, 9}";
        correctNumber = playerInput.text;
        startGuessing = true;
    }


    void GuessNumber()
    {
        if (firstTime)
        {
            guessedNumber = MakeGuess(allPermutations, true);
            firstTime = false;
        }
        else
        {
            guessedNumber = MakeGuess(prunedSet, true);
        }

        aiText.text = guessedNumber.ToString();
        trialsNumber++;
        startGuessing = false;
        startSolving = true;
    }
    void DisplayResult()
    {
        if (GetBullsCows(guessedNumber, correctNumber)["Bulls"] == 4 || prunedSet.Count <= 1)
        {
            playerText.text = "Awesome! Your Number " + guessedNumber + " is right.\nCongratulations!";
            startSolving = false;
            startGuessing = false;

            Invoke("Restart", 5);

        }
        else
        {
            playerText.text = "Bulls = " + GetBullsCows(guessedNumber, correctNumber)["Bulls"] + "\nCows = " + GetBullsCows(guessedNumber, correctNumber)["Cows"];
            prunedSet = PruneSet(prunedSet, guessedNumber, correctNumber);
            startSolving = false;
            startGuessing = true;
        }
    }
    void Restart() { GameManager.instance.RestartGame(); }

    /*
     * generate all possible permutations of r digits formed from {0, 1, 2, 3, 4, 5, 6, 7, 8, 9}
     */
    List<string> GeneratePermutations(int r)
    {
        List<string> allPermutations = new List<string>();
        bool isUnique;

        //Form larger number from all permutations
        string maxNumber = "9";
        for (int i = 1; i < r; i++) maxNumber += 9;

        //Form all possible permutations
        for (int i = 0; i < int.Parse(maxNumber); i++)
        {
            // form the number to be like (0000, 0001, 0002,...., 0023,... etc)
            var uniqueNumber = i.ToString("D4");

            //check if the number has dublicate digits, don't add it to allPermutations list
            isUnique = true;
            for (int j = 0; j < r - 1; j++)
            {
                for (int k = j + 1; k < r; k++)
                    if (uniqueNumber[j] == uniqueNumber[k]) isUnique = false;
            }

            //only add the numbers which don't have duplicate digits
            if (isUnique) allPermutations.Add(uniqueNumber);
        }
        return allPermutations;
    }

    /*
     * Get bulls and cows counts of the guessedNumber
     */
    Dictionary<string, int> GetBullsCows(string guessedNumber, string correctNumber)
    {
        List<int> guessedDigits = new List<int>();
        List<int> correctDigits = new List<int>();
        int bulls = 0, cows = 0;

        foreach (var digit in guessedNumber) guessedDigits.Add(digit);

        foreach (var digit in correctNumber) correctDigits.Add(digit);

        for (int i = 0; i < guessedDigits.Count; i++)
        {
            if (correctDigits.IndexOf(guessedDigits[i]) != -1)
            {
                //same digit and same position
                if (correctDigits.IndexOf(guessedDigits[i]) == i) bulls++;
                //same digit but in different position
                else cows++;
            }
        }
        return new Dictionary<string, int>() { ["Bulls"] = bulls, ["Cows"] = cows };
    }

    /*
     * elimenate some numbers of the set and keep only the numbers which has the same result of the
     * guessedNumber result
     */
    List<string> PruneSet(List<string> set, string guessedNumber, string correctNumber)
    {
        var bullsCows_guess = GetBullsCows(guessedNumber, correctNumber);
        List<string> prunedSet = new List<string>();
        set.ForEach(delegate (string number)
        {
            var bullsCows_prune = GetBullsCows(number, guessedNumber);
            if (bullsCows_prune["Bulls"] == bullsCows_guess["Bulls"] && bullsCows_prune["Cows"] == bullsCows_guess["Cows"])
                prunedSet.Add(number);
        });
        return prunedSet;
    }

    Digit SearchInDigits(List<Digit> digits, int pos, int val)
    {
        for (int i = 0; i < digits.Count; i++)
        {
            if (digits[i].pos == pos && digits[i].val == val)
                return digits[i];
        }
        return null;
    }

    /*
     * initialize digit object of each digit of each number in the set
     */
    List<Digit> GetSortedDigits(List<string> set)
    {
        List<Digit> digits = new List<Digit>();
        set.ForEach(delegate (string number)
        {
            for (int pos = 0; pos < NumOfDigitsOfGuessedNumber; pos++)
            {
                //'12' - '0' >>> get the int of 12  Like '12' = 12
                Digit d = SearchInDigits(digits, pos, number[pos] - '0');
                if (d != null)
                    d.count++;
                else
                    digits.Add(new Digit() { pos = pos, val = number[pos] - '0', count = 1 });
            }
        });
        List<Digit> sortedDigits = digits.OrderBy(o => o.count).ToList();
        return sortedDigits;
    }

    /*
     * Find unique guesss 
     */
    List<string> FindUniqueGuesses(List<string> set)
    {
        if (set.Count == 1) return set;

        var bullsCowsCounts = new List<Dictionary<string, int>>();
        var uniqueSet = new List<string>();

        for (int i = 0; i < allPermutations.Count; i++)
        {
            set.ForEach(delegate (string number)
            {
                var result = GetBullsCows(allPermutations[i], number);
                bullsCowsCounts.Add(result);
            });


            var matches = bullsCowsCounts.Count * -1;

            bullsCowsCounts.ForEach(delegate (Dictionary<string, int> res)
            {
                bullsCowsCounts.ForEach(delegate (Dictionary<string, int> r)
                {
                    if (res["Bulls"] == r["Bulls"] && res["Cows"] == r["Cows"])
                        matches++;
                });
            });

            if (matches == 0)
                uniqueSet.Add(allPermutations[i]);

            bullsCowsCounts.Clear();
        }
        return uniqueSet;
    }

    /*
     * Make a guess
     */
    string MakeGuess(List<string> set, bool repeats)
    {
        if (set.Count == 1) return set[0];
        if (set.Count < 100)
        {
            var unique = FindUniqueGuesses(set);
            if (unique.Count > 0) return unique[0];
            else
            {
                return string.Empty;
            }
        }

        var sortedDigits = GetSortedDigits(set);
        string guess = string.Empty;

        var i = 0;
        if (repeats) i++;

        while (guess.Length != NumOfDigitsOfGuessedNumber)
        {
            if (guess.IndexOf((char)sortedDigits[i].val) == -1)
                guess += sortedDigits[i].val;
            i++;
        }

        return guess;
    }
}









