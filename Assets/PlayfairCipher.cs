using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayfairCipher 
{
    private readonly char[] _alphabet = {'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z'};
    private readonly char[] _omittedLetterPriority = {'Q', 'J', 'I'};
    
    public string Cipher(string key, string plainText)
    {
        PrepareKeySquare(key.ToUpper(), out char[][] keySquare);
        return EncryptText(keySquare, plainText);
    }

    public string Decipher(string key, string encryptedText)
    {
        PrepareKeySquare(key.ToUpper(), out char[][] keySquare);
        return DecryptText(keySquare, encryptedText);
    }

    #region PrepareAlphabet

    public void PrepareAlphabet(out List<char> alphabet, ref string key)
    {
        CloneAlphabet(out alphabet);
        ExtractKeyFromAlphabet(alphabet, ref key);
        OmitALetterFromAlphabet(alphabet, _omittedLetterPriority);
        Print(alphabet);
    }

    private void CloneAlphabet(out List<char> alphabet)
    {
        char[] charArray = new char[_alphabet.Length];
        _alphabet.CopyTo(charArray, 0);
        alphabet = new List<char>(charArray);
    }

    private void ExtractKeyFromAlphabet(List<char> alphabet, ref string key)
    {
        List<char> tempKey = new List<char>(key.ToUpper()); 
        for(int i = 0; i < tempKey.Count; i++)
        {
            if (!TryRemove(alphabet, tempKey[i]))
            {
                tempKey.RemoveAt(i);
                i--;
            }
        }
        key = new string(tempKey.ToArray());
        Debug.Log("tempkey is " + key);
    }

    private void OmitALetterFromAlphabet(List<char> alphabet, char[] omittedLetterPriority = null)
    {
        if (omittedLetterPriority != null)
        {
            foreach (var letter in omittedLetterPriority)
            {
                if (TryRemove(alphabet, letter))
                {
                    Debug.Log(letter + " is omitted");
                    return;
                }
            }
        }

        //İf priority array is not long enough, then remove a random one
        var randomIndex = Random.Range(0, alphabet.Count);
        Debug.Log(alphabet[randomIndex] + " is omitted");
        alphabet.RemoveAt(randomIndex);
    }

    private static void Print(List<char> alphabet)
    {
        string str = new string(alphabet.ToArray());
        Debug.Log(str);
    }

    #endregion

    #region PrepareKeySquare

    public void PrepareKeySquare(string key, out char[][] keySquare)
    {
        PrepareAlphabet(out List<char> alphabet, ref key);
        AllocateKeySquare(out keySquare);
        FillKeySquare(alphabet, key, keySquare);
        Print(keySquare);
    }
    
    private static void AllocateKeySquare(out char[][] keySquare)
    {
        keySquare = new char[5][];
        keySquare[0] = new char[5];
        keySquare[1] = new char[5];
        keySquare[2] = new char[5];
        keySquare[3] = new char[5];
        keySquare[4] = new char[5];
    }
    
    private void FillKeySquare(List<char> alphabet, string key, char[][] keySquare)
    {
        int elementIndex = 0;
        while (elementIndex < key.Length)
        {
            keySquare[elementIndex / 5][elementIndex % 5] = key[elementIndex];
            elementIndex++;
        }

        while (elementIndex < 25)
        {
            keySquare[elementIndex / 5][elementIndex % 5] = alphabet[elementIndex - key.Length];
            elementIndex++;
        }
    }
    
    private void Print(char[][] keySquare)
    {
        for (int i = 0; i < 5; i++)
        {
            string str = "";
            for (int j = 0; j < 5; j++)
            {
                str += keySquare[i][j] + " ";
            }
            Debug.Log(str);
        }
    }

    #endregion

    #region EncryptText

    public string EncryptText(char[][] keySquare, string plainText)
    {
        string tempPlaintext = plainText.Replace(" ", "").ToUpper();
        LookForAdjacentRepetitiveLetters(ref tempPlaintext);
        EnsureEvenLetterCount(ref tempPlaintext);
        return Encrypt(keySquare, SplitStringInToSegments(tempPlaintext, 2));
    }

    public void LookForAdjacentRepetitiveLetters(ref string str)
    {
        char prevLetter = '\0';
        for (int i = 0; i < str.Length; i++)
        {
            if (prevLetter == str[i])
            {
                str = str.Insert(i, new string(GetRandomDifferentLetter(str[i]), 1));
                Debug.Log("Adjacent repetitive letter found on the string an separated with a random letter. Last form of the string is : " + str);
            }

            prevLetter = str[i];
        }
    }

    private char GetRandomDifferentLetter(char refLetter)
    {
        var letter = refLetter;
        while (letter == refLetter)
        {
            letter = _alphabet[Random.Range(0, _alphabet.Length)];
        }

        return letter;
    }

    public void EnsureEvenLetterCount(ref string str)
    {
        if (str.Length % 2 == 1)
        {
            str += "Z";
            Debug.Log("Plain text has odd number of letter, z added: " + str);
        }
    }

    private string Encrypt(char[][] keySquare, string[] letterCouples)
    {
        string encryptedText = "";
        foreach (var couple in letterCouples)
        {
            encryptedText += DetermineEncryptedLetterCouple(keySquare, couple[0], couple[1]);
        }
        Debug.Log(encryptedText);
        return encryptedText;
    }

    private string DetermineEncryptedLetterCouple(char[][] keySquare, char c, char c1)
    {
        LetterPosition cPos = FindLetterLocation(keySquare, c);
        LetterPosition c1Pos = FindLetterLocation(keySquare, c1);
        if (cPos.column == c1Pos.column)
        {
            cPos.row += 1;
            if (cPos.row > 5) cPos.row -= 5;
            c1Pos.row += 1;
            if (c1Pos.row > 5) c1Pos.row -= 5;
        }
        else if(cPos.row == c1Pos.row)
        {
            cPos.column += 1;
            if (cPos.column > 5) cPos.column -= 5;
            c1Pos.column += 1;
            if (c1Pos.column > 5) c1Pos.column -= 5;
        }
        else
        {
            var tempCol = cPos.column;
            cPos.column = c1Pos.column;
            c1Pos.column = tempCol;
        }

        return keySquare[cPos.row - 1][cPos.column - 1] + "" + keySquare[c1Pos.row - 1][c1Pos.column - 1];
    }
    
    #endregion

    #region DecryptText

    public string DecryptText(char[][] keySquare, string encryptedText)
    {
        return Decrypt(keySquare, SplitStringInToSegments(encryptedText, 2));
    }
    
    private string Decrypt(char[][] keySquare, string[] letterCouples)
    {
        string plainText = "";
        foreach (var couple in letterCouples)
        {
            plainText += DetermineDecryptedLetterCouple(keySquare, couple[0], couple[1]);
        }
        Debug.Log(plainText);
        return plainText;
    }

    private string DetermineDecryptedLetterCouple(char[][] keySquare, char c, char c1)
    {
        LetterPosition cPos = FindLetterLocation(keySquare, c);
        LetterPosition c1Pos = FindLetterLocation(keySquare, c1);
        if (cPos.column == c1Pos.column)
        {
            cPos.row -= 1;
            if (cPos.row < 1) cPos.row += 5;
            c1Pos.row -= 1;
            if (c1Pos.row < 1) c1Pos.row += 5;
        }
        else if(cPos.row == c1Pos.row)
        {
            cPos.column -= 1;
            if (cPos.column < 1) cPos.column += 5;
            c1Pos.column -= 1;
            if (c1Pos.column < 1) c1Pos.column += 5;
        }
        else
        {
            var tempCol = cPos.column;
            cPos.column = c1Pos.column;
            c1Pos.column = tempCol;
        }

        return keySquare[cPos.row - 1][cPos.column - 1] + "" + keySquare[c1Pos.row - 1][c1Pos.column - 1];
    }


    #endregion

    #region Utility
    
    private bool TryRemove(List<char> alphabet, char letter)
    {
        int index = alphabet.FindIndex(c => c == letter);
        if (index >= 0)
        {
            alphabet.RemoveAt(index);
            return true;
        }

        return false;
    }

    public string[] SplitStringInToSegments(string str, int segmentLength)
    {
        string[] strSegments = new string[Mathf.CeilToInt(str.Length/segmentLength)];
        string debugString = "";
        for (int i = 0; i < str.Length - (segmentLength - 1); i += segmentLength)
        {
            var segment = str.Substring(i, segmentLength);
            strSegments[i / segmentLength] += segment;
            debugString += segment + " + ";
        }

        if (debugString.Length >= 3)
        {
            debugString = debugString.Substring(0, debugString.Length - 3);
            Debug.Log(debugString);
        }
        return strSegments;
    }

    public LetterPosition FindLetterLocation(char[][] keySquare, char c)
    {
        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                if(keySquare[i][j] == c) return new LetterPosition(j  + 1, i + 1);
            }
        }

        return new LetterPosition(-1, -1);
    }
    
    #endregion

    public struct LetterPosition
    {
        public int column;
        public int row;

        public LetterPosition(int column, int row)
        {
            this.column = column;
            this.row = row;
        }
    }
}
