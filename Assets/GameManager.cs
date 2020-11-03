using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public GameObject entry;
    public GameObject result;
    public Text keyText;
    public Text textArea;
    public Letter[] letters;
    public TextMeshProUGUI resultText;
    public Button encryptBtn;
    public Button decryptBtn;
    public Button copyTextBtn;
    public Text letterOne;
    public Text letterTwo;

    [Header("Animation Params")] 
    public float letterAppearDelay;
    public Color convertedLetterColor;
    public Color letterColor;
    public Color squareColor;
    public float colorSpeed = 0.5f;
    public float delayBetweenColorStages = 0.5f;
    public float delayBeforeDeColoring = 2;
    public float delayBeforeNextCouple = 1;
    
    
    PlayfairCipher _playfairCipher = new PlayfairCipher();
    List<LetterColorProcess> _firstLetterColor = new List<LetterColorProcess>();
    List<LetterColorProcess> _secondLetterColor = new List<LetterColorProcess>();
    List<LetterColorProcess> _thirdLetterColor = new List<LetterColorProcess>();
    List<LetterColorProcess> _letterDeColor = new List<LetterColorProcess>();

    private void Awake()
    {
        encryptBtn?.onClick.AddListener(Encrypt);
        decryptBtn?.onClick.AddListener(Decrypt);
        copyTextBtn?.onClick.AddListener(() => GUIUtility.systemCopyBuffer = resultText.text);
    }

    private void Encrypt()
    {
        OnStartResult();
        _playfairCipher.PrepareKeySquare(keyText.text.ToUpper(), out char[][] keySquare);
        DressLetters(keySquare);
        StartCoroutine(EncryptFlowHandler(keySquare));
    }
    
    private void Decrypt()
    {
        OnStartResult();
        _playfairCipher.PrepareKeySquare(keyText.text.ToUpper(), out char[][] keySquare);
        DressLetters(keySquare);
        StartCoroutine(DecryptFlowHandler(keySquare));
    }

    private void OnStartResult()
    {
        entry.SetActive(false);
        result.SetActive(true);
    }

    private void DressLetters(char[][] keySquare)
    {
        for (int i = 0; i < 25; i++)
        {
            letters[i].letterText.text = "" + keySquare[i / 5][i % 5];
        }
    }

    private IEnumerator EncryptFlowHandler(char[][] keySquare)
    {
        int counter = 0;
        while (counter < 25)
        {
            yield return new WaitForSeconds(letterAppearDelay);
            letters[counter].Appear();
            counter++;
        }
        
        yield return new WaitForSeconds(letterAppearDelay);

        string tempPlaintext = textArea.text.Replace(" ", "").ToUpper();
        _playfairCipher.LookForAdjacentRepetitiveLetters(ref tempPlaintext);
        _playfairCipher.EnsureEvenLetterCount(ref tempPlaintext);
        
        var segments = _playfairCipher.SplitStringInToSegments(tempPlaintext, 2);
        for (int i = 0; i < segments.Length; i++)
        {
            string s = DetermineEncryptedLetterCouple(keySquare, segments[i][0], segments[i][1]);
            yield return LetterColorRoutine(s, i == segments.Length - 1);
        }
        
        copyTextBtn?.gameObject.SetActive(true);
        letterOne.text = "";
        letterTwo.text = "";
    }
    
    private IEnumerator DecryptFlowHandler(char[][] keySquare)
    {
        int counter = 0;
        while (counter < 25)
        {
            yield return new WaitForSeconds(letterAppearDelay);
            letters[counter].Appear();
            counter++;
        }
        
        yield return new WaitForSeconds(letterAppearDelay);

        string tempPlaintext = textArea.text.Replace(" ", "").ToUpper();

        var segments = _playfairCipher.SplitStringInToSegments(tempPlaintext, 2);
        for (int i = 0; i < segments.Length; i++)
        {
            string s = DetermineDecryptedLetterCouple(keySquare, segments[i][0], segments[i][1]);
            yield return LetterColorRoutine(s, i == segments.Length - 1);
        }
        
        copyTextBtn?.gameObject.SetActive(true);
        letterOne.text = "";
        letterTwo.text = "";
    }
    
    private string DetermineEncryptedLetterCouple(char[][] keySquare, char c, char c1)
    {
        letterOne.text = "" + c;
        letterTwo.text = "" + c1;
        PlayfairCipher.LetterPosition cPos = _playfairCipher.FindLetterLocation(keySquare, c);
        PlayfairCipher.LetterPosition c1Pos = _playfairCipher.FindLetterLocation(keySquare, c1);
        _firstLetterColor.Add(new LetterColorProcess(letters[((cPos.row - 1) * 5) + (cPos.column - 1)], letterColor));
        _firstLetterColor.Add(new LetterColorProcess(letters[((c1Pos.row - 1) * 5) + (c1Pos.column - 1)], letterColor));
        
        if (cPos.column == c1Pos.column)
        {
            Debug.Log(c + c1 + " in the same column");
            cPos.row += 1;
            if (cPos.row > 5) cPos.row -= 5;
            c1Pos.row += 1;
            if (c1Pos.row > 5) c1Pos.row -= 5;
        }
        else if(cPos.row == c1Pos.row)
        {
            Debug.Log(c + c1 + " in the same row");
            cPos.column += 1;
            if (cPos.column > 5) cPos.column -= 5;
            c1Pos.column += 1;
            if (c1Pos.column > 5) c1Pos.column -= 5;
        }
        else
        {
            Debug.Log(c + c1 + " in the different columns and rows");

            int minColumn = cPos.column;
            int maxColumn = c1Pos.column;
            
            if (minColumn > maxColumn)
            {
                var temp = minColumn;
                minColumn = maxColumn;
                maxColumn = temp;
            }
            
            int minRow = cPos.row;
            int maxRow = c1Pos.row;
            
            if (minRow > maxRow)
            {
                var temp = minRow;
                minRow = maxRow;
                maxRow = temp;
            }

            for (int i = minRow; i <= maxRow; i++)
            {
                for (int j = minColumn; j <= maxColumn; j++)
                {
                    if ((i == cPos.row && j == cPos.column) || (i == c1Pos.row && j == c1Pos.column))
                    {
                        Debug.Log("square " + letters[((i - 1) * 5) + (j - 1)].letterText.text + " skipped");
                        continue;
                    }
                    _secondLetterColor.Add(new LetterColorProcess(letters[((i - 1) * 5) + (j - 1)], squareColor));
                }
            }
            
            var tempCol = cPos.column;
            cPos.column = c1Pos.column;
            c1Pos.column = tempCol;
        }
        _thirdLetterColor.Add(new LetterColorProcess(letters[((cPos.row - 1) * 5) + (cPos.column - 1)], convertedLetterColor));
        _thirdLetterColor.Add(new LetterColorProcess(letters[((c1Pos.row - 1) * 5) + (c1Pos.column - 1)], convertedLetterColor));

        return keySquare[cPos.row - 1][cPos.column - 1] + "" + keySquare[c1Pos.row - 1][c1Pos.column - 1];
    }
    
    private string DetermineDecryptedLetterCouple(char[][] keySquare, char c, char c1)
    {
        letterOne.text = "" + c;
        letterTwo.text = "" + c1;
        PlayfairCipher.LetterPosition cPos = _playfairCipher.FindLetterLocation(keySquare, c);
        PlayfairCipher.LetterPosition c1Pos = _playfairCipher.FindLetterLocation(keySquare, c1);
        _firstLetterColor.Add(new LetterColorProcess(letters[((cPos.row - 1) * 5) + (cPos.column - 1)], letterColor));
        _firstLetterColor.Add(new LetterColorProcess(letters[((c1Pos.row - 1) * 5) + (c1Pos.column - 1)], letterColor));
        
        if (cPos.column == c1Pos.column)
        {
            Debug.Log(c + c1 + " in the same column");
            cPos.row -= 1;
            if (cPos.row < 1) cPos.row += 5;
            c1Pos.row -= 1;
            if (c1Pos.row < 1) c1Pos.row += 5;
        }
        else if(cPos.row == c1Pos.row)
        {
            Debug.Log(c + c1 + " in the same row");
            cPos.column -= 1;
            if (cPos.column < 1) cPos.column += 5;
            c1Pos.column -= 1;
            if (c1Pos.column < 1) c1Pos.column += 5;
        }
        else
        {
            Debug.Log(c + c1 + " in the different columns and rows");

            int minColumn = cPos.column;
            int maxColumn = c1Pos.column;
            
            if (minColumn > maxColumn)
            {
                var temp = minColumn;
                minColumn = maxColumn;
                maxColumn = temp;
            }
            
            int minRow = cPos.row;
            int maxRow = c1Pos.row;
            
            if (minRow > maxRow)
            {
                var temp = minRow;
                minRow = maxRow;
                maxRow = temp;
            }

            for (int i = minRow; i <= maxRow; i++)
            {
                for (int j = minColumn; j <= maxColumn; j++)
                {
                    if ((i == cPos.row && j == cPos.column) || (i == c1Pos.row && j == c1Pos.column))
                    {
                        Debug.Log("square " + letters[((i - 1) * 5) + (j - 1)].letterText.text + " skipped");
                        continue;
                    }
                    _secondLetterColor.Add(new LetterColorProcess(letters[((i - 1) * 5) + (j - 1)], squareColor));
                }
            }
            
            var tempCol = cPos.column;
            cPos.column = c1Pos.column;
            c1Pos.column = tempCol;
        }
        _thirdLetterColor.Add(new LetterColorProcess(letters[((cPos.row - 1) * 5) + (cPos.column - 1)], convertedLetterColor));
        _thirdLetterColor.Add(new LetterColorProcess(letters[((c1Pos.row - 1) * 5) + (c1Pos.column - 1)], convertedLetterColor));

        return keySquare[cPos.row - 1][cPos.column - 1] + "" + keySquare[c1Pos.row - 1][c1Pos.column - 1];
    }


    private IEnumerator LetterColorRoutine(string s, bool isLast = false)
    {
        while (_firstLetterColor.Count > 0)
        {
            for (int i = 0; i < _firstLetterColor.Count; i++)
            {
                var letter = _firstLetterColor[i];
                letter = ColorALetter(letter);
                if (letter.currentColor == letter.TargetColor)
                {
                    _firstLetterColor.RemoveAt(i);
                    i--;
                    
                    _letterDeColor.Add(new LetterColorProcess(letter.Letter, Color.white));
                }
                else
                {
                    _firstLetterColor[i] = letter;
                }
            }

            yield return null;
        }
        
        yield return new WaitForSeconds(delayBetweenColorStages);
        
        while (_secondLetterColor.Count > 0)
        {
            for (int i = 0; i < _secondLetterColor.Count; i++)
            {
                var letter = _secondLetterColor[i];
                letter = ColorALetter(letter);
                if (letter.currentColor == letter.TargetColor)
                {
                    _secondLetterColor.RemoveAt(i);
                    i--;
                    
                    _letterDeColor.Add(new LetterColorProcess(letter.Letter, Color.white));
                }
                else
                {
                    _secondLetterColor[i] = letter;
                }
            }

            yield return null;
        }
        
        yield return new WaitForSeconds(delayBetweenColorStages);
        
        while (_thirdLetterColor.Count > 0)
        {
            for (int i = 0; i < _thirdLetterColor.Count; i++)
            {
                var letter = _thirdLetterColor[i];
                letter = ColorALetter(letter);
                if (letter.currentColor == letter.TargetColor)
                {
                    _thirdLetterColor.RemoveAt(i);
                    i--;
                    
                    _letterDeColor.Add(new LetterColorProcess(letter.Letter, Color.white));
                }
                else
                {
                    _thirdLetterColor[i] = letter;
                }
            }

            yield return null;
        }
        
        resultText.text += s;
        yield return new WaitForSeconds(delayBeforeDeColoring);
        
        while (_letterDeColor.Count > 0)
        {
            for (int i = 0; i < _letterDeColor.Count; i++)
            {
                var letter = _letterDeColor[i];
                letter = ColorALetter(letter);
                if (letter.currentColor == letter.TargetColor)
                {
                    _letterDeColor.RemoveAt(i);
                    letter.Letter.letterBg.color = Color.white;
                    i--;
                }
                else
                {
                    _letterDeColor[i] = letter;
                }
            }

            yield return null;
        }

        if(!isLast) yield return new WaitForSeconds(delayBeforeNextCouple);
    }

    private LetterColorProcess ColorALetter(LetterColorProcess coloredLetter)
    {
        Color currentColor = coloredLetter.currentColor;
        Color offsetColor = new Color(0, 0, 0, 0);
        
        var value = colorSpeed * Time.deltaTime * coloredLetter.Direction.x;
        currentColor.r += value;
        offsetColor.r += value;
        if (currentColor.r * coloredLetter.Direction.x > coloredLetter.TargetColor.r * coloredLetter.Direction.x)
        {
            var offset = coloredLetter.TargetColor.r - currentColor.r;
            offsetColor.r += offset;
            currentColor.r += offset;
        }
        
        value = colorSpeed * Time.deltaTime * coloredLetter.Direction.y;
        currentColor.g += value;
        offsetColor.g += value;
        if (currentColor.g * coloredLetter.Direction.y > coloredLetter.TargetColor.g * coloredLetter.Direction.y)
        {
            var offset = coloredLetter.TargetColor.g - currentColor.g;
            offsetColor.g += offset;
            currentColor.g += offset;
        }
        
        value = colorSpeed * Time.deltaTime * coloredLetter.Direction.z;
        currentColor.b += value;
        offsetColor.b += value;
        if (currentColor.b * coloredLetter.Direction.z > coloredLetter.TargetColor.b * coloredLetter.Direction.z)
        {
            var offset = coloredLetter.TargetColor.b - currentColor.b;
            offsetColor.b += offset;
            currentColor.b += offset;
        }

        var tempColor = coloredLetter.Letter.letterBg.color;
        tempColor += offsetColor;
        coloredLetter.Letter.letterBg.color = tempColor;
        
        coloredLetter.currentColor = currentColor;
        return coloredLetter;
    }

    private struct LetterColorProcess
    {
        public readonly Letter Letter;
        public readonly Color TargetColor;
        public Color currentColor;

        public readonly Vector3 Direction;
        
        public LetterColorProcess(Letter letter, Color targetColor)
        {
            Letter = letter;
            TargetColor = targetColor;
            currentColor = letter.letterBg.color;

            float x, y, z;
            var diff = targetColor.r - currentColor.r;
            if (diff > 0) x = 1;
            else if (diff < 0) x = -1;
            else x = 0;
            
            diff = targetColor.g - currentColor.g;
            if (diff > 0) y = 1;
            else if (diff < 0) y = -1;
            else y = 0;
            
            diff = targetColor.b - currentColor.b;
            if (diff > 0) z = 1;
            else if (diff < 0) z = -1;
            else z = 0;
            
            Direction = new Vector3(x, y, z);
        }
    }
}
