using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEditor;
using System;
public enum PhaseIn { AcceptingInput, Occupied }
public class InputAcceptor : MonoBehaviour
{
    /*
      * This is a script that you attach to any object, and requires 3 textObjects
      *  - (0) Canvas
      *      - Purpose: Holds the next 4 objects within it as it direct children
      *  - (1) ShownText object: 
      *      - Purpose: Shows the text to the player in a default color
      *                      also used for getting the correct size for the rect transform
      *      - Requires: TextMeshPro Text, Content Size Fitter 
      *  - (2) HiddenInput Text
      *      - Purpose: Places letters like the Input Text but without the html colors, making
      *                      the caret able to follow each letter
      *      - Requires: TextMeshPro Text
      *  - (3) Input Text
      *      - Purpose: Places letters like the Input Text but without the html colors, making
      *                      the caret able to follow each letter
      *      - Requires: TextMeshPro Text
      *  - (4) Caret Image
      *      - Purpose: Used to show where the next inpute will go
      *      - Requires: Raw Image, no texture needed just scale
      * 
      *  
      * 
      */

    public static InputAcceptor current; // reference to script, called by InputAcceptor.current
    public event Action GotCorrectWord;

    // the toggle for whether the player can type with it
    [Tooltip("Should this input manager be allowed to accept inputs?")]
    public bool shouldAcceptInput = false;

    // the enum for whether the player can use bonuses
    public PhaseIn currentPhaseIn = PhaseIn.AcceptingInput;

    // the current word we are trying to guess, should put this in another script
    [Tooltip("What is the current word this input manager should be looking out for?")]
    public string targetWord;

    // this should be removed when a manager is created
    [TextArea]
    public string startingWord = "Hello\n\tHello Again!";


    // the ints for keeping track where we are on the input
    public int characterOn = 0, // current character we are on in the whole text
        characterColorInputIsOn = 0, // current character the Input Text is on, since we have the 
        characterLimit = 499; // character limit for how many characters can be inputted, needs to exist before a crash

    public int characterOnTotal = 0, // the total amount of characters inputted
        characterColorInputTotal = 0; // total amount of characters in the Input Text, its *24

    // moving up and down to remember itself

    // int to remeber where we are horizontally
    public int horizontalCharacterOnRemember = 0;
    // bool to know if we are suppose to remeber the old one
    public bool isOffSetOnHorizontalPosition = false;

    // current inputed text without the colors, but not in the TextMeshPro text
    [TextArea]
    public string currentText = "";

    // delay time in between each repeat for removing using the functions
    public float delayTimePerCharacterRemove = 0.5f;

    // caret y offset for the caret
    public float caretYIncreasePercent = 1.15f;

    // these are for the left and right arrow movements
    public float keyHoldTime = 0.0f; // current amount of time the player has held left/right
    public float initialDelay = 0.5f;  // Initial delay before rapid movement starts
    public float repeatRate = 0.05f;   // Interval between movements during rapid repeat


    // things to add to the game manager:
    // color hex code for the three colros
    public string colorWordCorrectString = "#1AFF00", colorWordWrongString = "#FF000B", colorWordOutOfBoundsString = "#E68C2C"; // can be word or hex color

    // color for selecting within the inspector, can change with script and hex code
    public Color colorWordCorrect, colorWordWrong, colorWordOutOfBounds; // can be word or hex color


    // Input Text component 
    public TMP_Text textComponent;

    public GameObject caretObject;
    // position of the caret
    public RectTransform caretTransform;


    // the respective rect transform and textmeshpro text from each of the objects
    public RectTransform shownRect, inputRect, hiddenInputRect;
    public TextMeshProUGUI shownText, inputText, hiddenInputText;

    // size fitter of the shown text
    public ContentSizeFitter sizeFitter;

    private void Awake()
    {
        current = this;
    }
    public void Start()
    {
       // SetTargetWord(startingWord);
    }

    // called to change the word to a differnt one/give it once in the first place
    public void SetTargetWord(string targetWord)
    {
       // print("In set target word");
        ChangeWord(ref targetWord);
    }

    public void ChangeInputState(bool stateToChangeTo)
    {
        if (stateToChangeTo)
        {
            ActivateInput();
        }
        else
        {
            DeactivateInput();
        }
    }

    // used to remove the player from inpputing into this, IE: pause menu
    public void DeactivateInput()
    {
        //print("deactive input");
        shouldAcceptInput = false;
    }

    // used to activate/reactive the input, IE: unpausing, starting the game, becoming the target enemy
    public void ActivateInput()
    {
        //print("activeate input");
        shouldAcceptInput = true;
    }

    #region powerUpCallFunctions
    // these three are the powerups the player can call/called when player loses
    public void CallAutoComplete()
    {
        AutoComplete();
    }

    public void CallRemoveWrongCharacters()
    {
        RemoveAllWrongCharacters();
    }

    public void CallFillInCharacters()
    {
        FillInText();
    }

    public void CallFillInNextCharacter()
    {
        // this one adds the next character needed, used if they don't know where | is
        AddSingleCharacterToInputText();
    }
    #endregion powerUpCallFunctions

    // returns true if we are in the AcceptingInput phase, else false
    bool CheckIfAllowingInput()
    {
        // this is only used by the script itself with powerups
        if (currentPhaseIn != PhaseIn.AcceptingInput)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    // called when pressing enter if after a letter is inputted?
    public bool CheckIfWordsAreSame()
    {
        if (currentText == targetWord && shouldAcceptInput)
        {
            print("Checking if words are the same");
            print(shouldAcceptInput);
            WordsAreSame();
            return true;
        }
        else
        {
            //print($"Incorrect: First word: \"{currentText}\", Second word: \"{targetWord}\"");
            return false;
        }
    }

    public void WordsAreSame()
    {
        print($"Incorrect: First word: \"{currentText}\", Second word: \"{targetWord}\"");
        DeactivateInput();
        GotCorrectWord(); // action event to call things listening in for this

        print("Found");
    }

    // to be called whenever you want to start accpeting inputs
    public void ChangePhaseToAcceptingInputs(float timeDelay = 0, bool shouldFrameDelay = false)
    {
        StartCoroutine(ChangePhaseToAcceptingInputsCoroutine(timeDelay, shouldFrameDelay));
    }
    public IEnumerator ChangePhaseToAcceptingInputsCoroutine(float timeDelay, bool shouldFrameDelay = false)
    {
        if (shouldFrameDelay)
        {
            // makes it wait for 1 more frame than the actual time
            yield return null;
        }
        if (timeDelay < 0)
        {
            timeDelay = 0;
        }
        yield return new WaitForSeconds(timeDelay);
        currentPhaseIn = PhaseIn.AcceptingInput;
    }

    // to be called whenever you want to stop accpeting inputs
    public void ChangePhaseToOccupiedInputs(float timeDelay = 0)
    {
        StartCoroutine(ChangePhaseToOccupiedInputsCoroutine(timeDelay));
    }
    public IEnumerator ChangePhaseToOccupiedInputsCoroutine(float timeDelay)
    {
        yield return new WaitForSeconds(timeDelay);
        currentPhaseIn = PhaseIn.Occupied;
    }


    #region PowerUpAutoComplete
    // removes all wrong charactersa and fills in the rest
    // returns whether or not it was able to complete it
    public void AutoComplete()
    {
        if (!CheckIfAllowingInput())
        {
            return;
        }
        // gets the time needed to call the next one
        float timeToWait = RemoveAllWrongCharacters();

        StartCoroutine(CallFillInText(timeToWait));
        return;
    }


    // called from autocomplete because we need to wait for the wrong characters ot be removed before calling this
    IEnumerator CallFillInText(float timeDelay)
    {
        yield return new WaitForSeconds(timeDelay);
        FillInText();
    }

    IEnumerator CallRemoveWrongCharacters(float timeDelay)
    {
        yield return new WaitForSeconds(timeDelay);
        RemoveAllWrongCharacters();
    }
    #endregion PowerUpAutoComplete

    #region PowerUpAddCharacter
    // this fill in the remaining text even if the current characters are wrong
    public float FillInText()
    {

        if (!CheckIfAllowingInput())
        {
            return 0;
        }

        // safeguard incase there is no text to fill in at this point
        if (currentText.Length >= targetWord.Length)
        {
            return 0;
        }
        ResetCaretToLastInput();
        ChangePhaseToOccupiedInputs();

        int placeToStart = currentText.Length;

        for (int i = placeToStart; i < targetWord.Length; i++)
        {
            StartCoroutine(DelayAddition((i - placeToStart) * delayTimePerCharacterRemove));
        }
        ChangePhaseToAcceptingInputs((targetWord.Length - placeToStart - 1) * delayTimePerCharacterRemove);
        // returns the longest wait time 
        return (targetWord.Length - placeToStart) * delayTimePerCharacterRemove;

    }
    IEnumerator DelayAddition(float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        AddSingleCharacterToInputText();
    }

    public void AddSingleCharacterToInputText()
    {

        // safeguard
        if (currentText.Length >= targetWord.Length)
        {
            return;
        }
        char characterToAdd = targetWord[currentText.Length];
        switch (characterToAdd)
        {
            case '\n':
                HandleNewLineCharacter();
                break;
            case '\t':
                HandleTabCharacter();
                break;
            default:
                HandleStandardCharacter(characterToAdd);
                break;

        }
    }
    #endregion PowerUpAddCharacter

    #region RemoveAllWrongCharactersLogic
    public float RemoveAllWrongCharacters()
    {
        if (!CheckIfAllowingInput())
        {
            return 0;
        }
        // safeguard for no characters to remove or should currently even be allowed to do this
        if (currentText.Length == 0)
        {
            return 0;
        }
        ResetCaretToLastInput();
        ChangePhaseToOccupiedInputs();

        int lengthToGoTo;

        lengthToGoTo = currentText.Length;

        int incorrectFirstCharacter = currentText.Length;

        // goes until it finds an incorrect character
        for (int i = 0; i < lengthToGoTo; i++)
        {
            // if the currentText has more letters than the target word then we know
            // otherwise its the first letter
            if (i >= targetWord.Length || currentText[i] != targetWord[i])
            {
                // saves which number the wrong one was first found at
                incorrectFirstCharacter = i;
                break;
            }
        }


        // removes each letter with a delay in between each one so looks manual
        for (int i = currentText.Length; i > incorrectFirstCharacter; i--)
        {
            // this is all called at the same time causes the delay to be multiplied
            StartCoroutine(DelayRemoval((currentText.Length - i) * delayTimePerCharacterRemove));
            print((currentText.Length - i) * delayTimePerCharacterRemove);
        }
        print((currentText.Length - incorrectFirstCharacter - 1) * delayTimePerCharacterRemove);
        ChangePhaseToAcceptingInputs((currentText.Length - incorrectFirstCharacter - 1) * delayTimePerCharacterRemove);
        // returns the longest wait time
        return (currentText.Length - incorrectFirstCharacter) * delayTimePerCharacterRemove;
    }

    // removes a character from the currentText after a delay 
    IEnumerator DelayRemoval(float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        RemoveLetterFromInputText();
    }
    #endregion RemoveAllWrongCharactersLogic


    // just used to check if the user has inputted anything
    void Update()
    {
        if (Input.anyKey && shouldAcceptInput && CheckIfAllowingInput())
        {
            HandleInputLogic();
        }
    }

    public void StartGame()
    {
        ChangeWord(ref startingWord);
    }

    void SetColor()
    {
        colorWordCorrectString = colorWordCorrect.GetHashCode().ToString();
        colorWordWrongString = colorWordWrong.GetHashCode().ToString();
        colorWordOutOfBoundsString = colorWordOutOfBounds.GetHashCode().ToString();
    }

    #region ArrowInputs
    void HandleArrowInputs()
    {
        HandleLeftArrowInput();
        HandleRightArrowInput();
        HandleUpArrowInput();
        HandleDownArrowInput();
    }

    void HandleLeftArrowInput()
    {
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            // Handle initial key press or holding
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                // Immediate movement on first press
                keyHoldTime = 0.0f;

                if (Input.GetKey(KeyCode.LeftControl))
                {
                    print("Moving All to the left");
                    ResetCaretToFirstInput();
                }
                else
                {
                    print("Moving left");
                    MoveSelectionLeft();
                }
            }
            else
            {
                // Key is being held down, check if enough time has passed for repeat
                keyHoldTime += Time.deltaTime;

                if (keyHoldTime >= initialDelay)
                {
                    // Start repeating after the initial delay
                    if (keyHoldTime >= initialDelay + repeatRate)
                    {
                        keyHoldTime = initialDelay; // Reset time for next repeat

                        if (Input.GetKey(KeyCode.LeftControl))
                        {
                            print("Moving All to the left");
                            ResetCaretToFirstInput();
                        }
                        else
                        {
                            print("Moving left");
                            MoveSelectionLeft();
                        }
                    }
                }
            }
        }
        else if (Input.GetKeyUp(KeyCode.LeftArrow))
        {
            // Reset the timing if the key is released
            keyHoldTime = 0.0f;
        }

    }
    void HandleRightArrowInput()
    {
        if (Input.GetKey(KeyCode.RightArrow))
        {
            // Handle initial key press or holding
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                // Immediate movement on first press
                keyHoldTime = 0.0f;

                if (Input.GetKey(KeyCode.LeftControl))
                {
                    print("Moving All to the right");
                    ResetCaretToLastInput();
                }
                else
                {
                    print("Moving right");
                    MoveSelectionRight();
                }
            }
            else
            {
                // Key is being held down, check if enough time has passed for repeat
                keyHoldTime += Time.deltaTime;

                if (keyHoldTime >= initialDelay)
                {
                    // Start repeating after the initial delay
                    if (keyHoldTime >= initialDelay + repeatRate)
                    {
                        keyHoldTime = initialDelay; // Reset time for next repeat

                        if (Input.GetKey(KeyCode.LeftControl))
                        {
                            print("Moving All to the right");
                            ResetCaretToLastInput();
                        }
                        else
                        {
                            print("Moving right");
                            MoveSelectionRight();
                        }
                    }
                }
            }
        }
        else if (Input.GetKeyUp(KeyCode.RightArrow))
        {
            // Reset the timing if the key is released
            keyHoldTime = 0.0f;
        }
    }

    void HandleUpArrowInput()
    {

        if (Input.GetKey(KeyCode.UpArrow))
        {
            // Handle initial key press or holding
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                // Immediate movement on first press
                keyHoldTime = 0.0f;

                MoveSelectionUp();
            }
            else
            {
                // Key is being held down, check if enough time has passed for repeat
                keyHoldTime += Time.deltaTime;

                if (keyHoldTime >= initialDelay)
                {
                    // Start repeating after the initial delay
                    if (keyHoldTime >= initialDelay + repeatRate)
                    {
                        keyHoldTime = initialDelay; // Reset time for next repeat

                        MoveSelectionUp();
                    }
                }
            }
        }
        else if (Input.GetKeyUp(KeyCode.UpArrow))
        {
            // Reset the timing if the key is released
            keyHoldTime = 0.0f;
        }
    }
    void HandleDownArrowInput()
    {

        if (Input.GetKey(KeyCode.DownArrow))
        {
            // Handle initial key press or holding
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                // Immediate movement on first press
                keyHoldTime = 0.0f;

                MoveSelectionDown();
            }
            else
            {
                // Key is being held down, check if enough time has passed for repeat
                keyHoldTime += Time.deltaTime;

                if (keyHoldTime >= initialDelay)
                {
                    // Start repeating after the initial delay
                    if (keyHoldTime >= initialDelay + repeatRate)
                    {
                        keyHoldTime = initialDelay; // Reset time for next repeat

                        MoveSelectionDown();
                    }
                }
            }
        }
        else if (Input.GetKeyUp(KeyCode.DownArrow))
        {
            // Reset the timing if the key is released
            keyHoldTime = 0.0f;
        }
    }

    #endregion ArrowInputs

    void HandleInputLogic()
    {

        // this moves the caret and position depending on which arrow was pressed (maybe add more keybinds to it)
        HandleArrowInputs();

        // this foreach loop doesn't trigger off mouse inputs
        foreach (char characterInputted in Input.inputString)
        {
            // backspace, delete character
            if (characterInputted == '\b') // has backspace/delete been pressed?
            {
                HandleBackSpaceCharacter();
            }
            // exit if we already hit the character cap, have to do after backspace since its also a character
            else if (currentText.Length >= characterLimit)
            {
                return;
            }
            // input a new line
            else if ((characterInputted == '\n') || (characterInputted == '\r')) // enter/return
            {
                HandleStandardCharacter('\n');
            }
            // its any other standard character that appears on screen
            else // its a normal character
            {
                HandleStandardCharacter(characterInputted);
            }
        }

        // safeguard if we alrerady hit the cap
        if (currentText.Length >= characterLimit)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            HandleStandardCharacter('\t');
        }
        CheckIfWordsAreSame();
        //if (Input.GetKeyDown(KeyCode.LeftAlt))
        //{
        //    print("Check if words are same");
        //    CheckIfWordsAreSame();
        //}

        //if (Input.GetKeyDown(KeyCode.RightAlt))
        //{
        //    print("Removing all wrong characters");
        //    RemoveAllWrongCharacters();
        //}
        //if (Input.GetKeyDown(KeyCode.RightControl))
        //{
        //    print("Filling remaining text");
        //    FillInText();
        //}
        //if (Input.GetKeyDown(KeyCode.LeftControl))
        //{
        //    print("AutoCompleting");
        //    AutoComplete();
        //}
    }

    void HandleBackSpaceCharacter()
    {
        RemoveLetterFromInputText();
        UpdateText();
    }

    void HandleNewLineCharacter(bool hasBeenHereBefore = false)
    {
        HandleStandardCharacter('\n');
    }

    void HandleTabCharacter(bool hasBeenHereBefore = false)
    {

        HandleStandardCharacter('\t');
    }

    // handles any character that is visual when typing and spaces (no enters or tabs)
    // adds the character to the current text, input text, and hidden text
    void HandleStandardCharacter(char characterInputted, bool hasBeenHereBefore = false)
    {
        // mark is a highlight, the base text color is the color of the text object

        // if we are currently not on the max char

        // has the player already added too many characters?, if so make it wrong
        if (characterOn >= targetWord.Length)
        {
            // splits the text into two with the 24 letters being excluded
            string holder = textComponent.text;
            textComponent.text = holder.Substring(0, characterColorInputIsOn)
                + $"<mark={colorWordOutOfBoundsString}aa>{characterInputted}</mark>";
        }
        // if the current letter is the next one we need
        else if (characterInputted == targetWord[characterOn]) // no -1 since c isn't apart of current text yet making the current length the correct one
        {
            // splits the text into two with the 24 letters being excluded
            string holder = textComponent.text;
            textComponent.text = holder.Substring(0, characterColorInputIsOn)
                + $"<color={colorWordCorrectString}>{characterInputted}</color>";
        }
        else // current letter is wrong
        {
            // splits the text into two with the 24 letters being excluded
            string holder = textComponent.text;
            textComponent.text = holder.Substring(0, characterColorInputIsOn)
                + $"<color={colorWordWrongString}>{characterInputted}</color>";
        }

        string holderCur = currentText;

        // deletes a letters off current text
        currentText = holderCur.Substring(0, characterOn)
            + characterInputted
            + holderCur.Substring(characterOn, holderCur.Length - characterOn);

        characterColorInputIsOn += $"<color={colorWordWrongString}>{characterInputted}</color>".Length;
        characterColorInputTotal += $"<color={colorWordWrongString}>{characterInputted}</color>".Length;
        characterOn++;
        characterOnTotal++;

        UpdateText();

        if (!hasBeenHereBefore)
        {
            UpdateCharacterColorings();
        }
    }

    void UpdateCharacterColorings()
    {
        int characterOnHolder = characterOn;
        int characterColorOnHolder = characterColorInputIsOn;
        int characterOnHolderTotal = characterOnTotal;
        int characterColorOnHolderTotal = characterColorInputTotal;
        string currentTextHolder = currentText;
        for (int i = characterOn; i < currentTextHolder.Length; i++)
        {
            if (currentTextHolder[i] == '\b') // has backspace/delete been pressed?
            {
                HandleBackSpaceCharacter();
            }
            else if ((currentTextHolder[i] == '\n') || (currentTextHolder[i] == '\r')) // enter/return
            {
                HandleStandardCharacter('\n', true);
            }
            else if ((currentTextHolder[i] == '\t'))
            {
                HandleStandardCharacter('\t', true);
            }
            else // its a normal character
            {
                HandleStandardCharacter(currentTextHolder[i], true);
            }
        }
        // resets the things back to what they were
        characterOn = characterOnHolder;
        characterColorInputIsOn = characterColorOnHolder;
        characterOnTotal = characterOnHolderTotal;
        characterColorInputTotal = characterColorOnHolderTotal;

        currentText = currentTextHolder;
        UpdateText();
    }


    void RemoveLetterFromInputText(bool hasBeenHereBefore = false)
    {
        // colors: #1AFF00 green, #FF000B red
        // this takes into account the fact that the textComponent will have
        // the color tags in it making subtraction be -24 since <color=#111111></color> is 24
        // -1 because length is 1 more than array starting count

        // safeguard
        if (characterOn == 0)
        {
            return;
        }


        // splits the text into two with the 24 letters being excluded
        string holder = textComponent.text;
        print($"{holder.Substring(0, characterColorInputIsOn - 1 - 23)}");
        textComponent.text = holder.Substring(0, characterColorInputIsOn - 1 - 23)
            + holder.Substring(characterColorInputIsOn, holder.Length - characterColorInputIsOn);
        characterColorInputIsOn -= 24;
        characterColorInputTotal -= 24;


        string holderCur = currentText;
        // deletes a letters off current text
        currentText = holderCur.Substring(0, characterOn - 1)
            + holderCur.Substring(characterOn, holderCur.Length - characterOn);

        characterOn--;
        characterOnTotal--;
        // horizontalCharacterOnRemember--;
        UpdateText();
        UpdateCharacterColorings();
    }

    void ReplaceParseCharacters(ref string textToAdjustParse)
    {
        // Unity attmpts to print out \n as \, n as characters instead of as the escape character
        // so the text we input as just "\n" is now "\\n", just remove the extra \ for all escape characters
        textToAdjustParse = textToAdjustParse.Replace("\\n", "\n");
        textToAdjustParse = textToAdjustParse.Replace("\\r", "\r");
        textToAdjustParse = textToAdjustParse.Replace("\\t", "\t");
    }
    public void FormatContentSizeFitter()
    {
        StartCoroutine(FormatContentSizeFitterDelay());
    }
    IEnumerator FormatContentSizeFitterDelay()
    {
        // to be called after changing the shownText's text

        //caretObject.SetActive(false);
        sizeFitter.enabled = true; // turns back on the fitter which shrinks/expands to perfectly fit the text (Doesn't do anything for leading or trailing spaces
        shownText.ForceMeshUpdate(); // forces the shownText text to update its text/content fitter to be able to tinker with its new values

        yield return new WaitForSeconds(0.05f); ; // null can be used to wait for 1 second instead of called new WaitForSeconds(0.01f);
        //caretObject.SetActive(true);
        sizeFitter.enabled = false;

        // this adjusts the inputed text's rect transform to be the same size as the shown rects
        inputRect.anchorMin = shownRect.anchorMin;
        inputRect.anchorMax = shownRect.anchorMax;
        inputRect.anchoredPosition = shownRect.anchoredPosition;
        inputRect.sizeDelta = shownRect.sizeDelta;

        // adjusts the hidden's rect so it matches the inputs
        hiddenInputRect.anchorMin = shownRect.anchorMin;
        hiddenInputRect.anchorMax = shownRect.anchorMax;
        hiddenInputRect.anchoredPosition = shownRect.anchoredPosition;
        hiddenInputRect.sizeDelta = shownRect.sizeDelta;

        UpdateCaretPositionToSpecificCharacter();
    }

    public void ChangeWord(ref string wordToChangeTo)
    {
        textComponent.text = "";
        currentText = "";
        characterOn = 0;
        characterOnTotal = 0;
        characterColorInputIsOn = 0;
        characterColorInputTotal = 0;
        // updates the shown word, resets the input text, formats both rects and the caret itself
        ReplaceParseCharacters(ref wordToChangeTo);

        // updates the word
        targetWord = shownText.text = wordToChangeTo;
        //print($"Target word: {targetWord}ShownText is now: {shownText.text}");
        characterLimit = targetWord.Length * 2;

        // updates the rects so the word fits and can be typed correctly
        FormatContentSizeFitter();
        // updates the caret on the input text so it starts at the first letter again
        UpdateText();
    }

    // called after text is changed 
    void UpdateText()
    {
        hiddenInputText.text = currentText + ' ';
        SetHorizontalPosition();
        UpdateCaretPositionToSpecificCharacter();
    }

    void UpdateCaretPosition()
    {
        hiddenInputText.ForceMeshUpdate();
        // Check if there are characters in the text
        if (hiddenInputText.text.Length > 0 && hiddenInputText.textInfo.characterInfo.Length >= hiddenInputText.text.Length)
        {
            // Get the last character info
            TMP_CharacterInfo charInfo = hiddenInputText.textInfo.characterInfo[hiddenInputText.text.Length - 1];

            // Get the bottom right position of the last character
            Vector2 charPosition = new Vector2(charInfo.origin, (charInfo.bottomLeft.y + charInfo.topRight.y) / 2);


            //  print($"character: {hiddenInputText.text[hiddenInputText.text.Length - 1]} posx:{charInfo.origin}, posy: {(charInfo.bottomLeft.y + charInfo.topRight.y) / 2}");
            // Adjust the caretTransform position to follow the last character
            caretTransform.localPosition = charPosition + shownRect.anchoredPosition;
        }

    }

    void UpdateCaretPositionToSpecificCharacter()
    {
        hiddenInputText.ForceMeshUpdate();

        if (hiddenInputText.text.Length > 0 && characterOn >= 0 && hiddenInputText.textInfo.characterInfo.Length > characterOn)
        {
            // Get the specific character's character info
            TMP_CharacterInfo charInfo = hiddenInputText.textInfo.characterInfo[characterOn];

            // Update the caret's position
            Vector2 charPosition = new Vector2(charInfo.origin, (charInfo.baseLine + 10));
            caretTransform.localPosition = charPosition + shownRect.anchoredPosition;

        }
        caretObject.SetActive(true);
    }


    // puts the caret at the specific number
    // the charOn number responds to where the caret should delete next, -1 is the startin  since there is no letter to delete there
    public void UpdateCaretPositionToSpecificCharacter(int charToGoTo)
    {

        hiddenInputText.ForceMeshUpdate();
        // Check if there are characters in the text
        if (hiddenInputText.text.Length > 0 && charToGoTo >= -1 && hiddenInputText.textInfo.characterInfo.Length >= charToGoTo)
        {
            // Get the specific character's character info
            TMP_CharacterInfo charInfo = hiddenInputText.textInfo.characterInfo[charToGoTo + 1];

            // Get the bottom right position of the last character
            Vector2 charPosition = new Vector2(charInfo.origin, (charInfo.bottomLeft.y + charInfo.topRight.y) / 2);


            //  print($"character: {hiddenInputText.text[hiddenInputText.text.Length - 1]} posx:{charInfo.origin}, posy: {(charInfo.bottomLeft.y + charInfo.topRight.y) / 2}");
            // Adjust the caretTransform position to follow the last character
            caretTransform.localPosition = charPosition;
        }
        caretObject.SetActive(true);
    }

    // moves the next character to the left
    void MoveSelectionLeft()
    {
        // safeguard
        if (characterOn <= 0) // otherwill be if charOn is greater than current text
        {
            return;
        }
        characterColorInputIsOn -= 24;
        characterOn--;
        isOffSetOnHorizontalPosition = false;

        SetHorizontalPosition();
        UpdateCaretPositionToSpecificCharacter();
    }

    void MoveSelectionRight()
    {
        // safeguard
        if (characterOn >= currentText.Length) // otherwill be if charOn is greater than current text
        {
            return;
        }

        characterColorInputIsOn += 24;

        characterOn++;
        isOffSetOnHorizontalPosition = false;
        SetHorizontalPosition();
        UpdateCaretPositionToSpecificCharacter();
    }

    public void SetHorizontalPosition()
    {
        // sets the horizontalCharacterOnRemember whenever we move left or right to set it
        //safe guard
        if (characterOn < 0)
        {
            return;
        }
       // print($"{hiddenInputText.text}, {hiddenInputText.text.Length}, {characterOn}");
        // Ensure the text mesh is fully updated
        hiddenInputText.ForceMeshUpdate();
        TMP_TextInfo textInfo = hiddenInputText.textInfo;
        TMP_CharacterInfo currentCharInfo = textInfo.characterInfo[characterOn];

        int lineNumber = currentCharInfo.lineNumber;

        // Get the TMP_LineInfo for the current and above lines
        TMP_LineInfo currentLineInfo = textInfo.lineInfo[lineNumber];
        int e = CountTabs(hiddenInputText.text.Substring(currentLineInfo.firstCharacterIndex, characterOn - currentLineInfo.firstCharacterIndex));
       // print($"Set hori:{e}");
        horizontalCharacterOnRemember = characterOn - currentLineInfo.firstCharacterIndex + e;

    }

    // moves it all the way to the right to the last character 
    public void ResetCaretToLastInput()
    {
        characterOn = characterOnTotal;
        characterColorInputIsOn = characterColorInputTotal;
        UpdateCaretPositionToSpecificCharacter();
    }

    // moves it all the way to the left before the first letter
    public void ResetCaretToFirstInput()
    {
        characterOn = 0;
        characterColorInputIsOn = 0;
        UpdateCaretPositionToSpecificCharacter();
    }

    void MoveSelectionUp()
    {
        MoveCaretUp();
        characterColorInputIsOn = characterOn * 24;
        UpdateCaretPositionToSpecificCharacter();
    }

    void MoveSelectionDown()
    {
        MoveCaretDown();
        characterColorInputIsOn = characterOn * 24;
        UpdateCaretPositionToSpecificCharacter();
    }



    // has to account for the tab stops
    // a    a tabs stop at certain places, a set amount of spaces
    //aaaa  a
    int CountTabs(string text)
    {
        int currentTabStopSpacing = 3;
        int tabCount = 0;
        foreach (char c in text)
        {
            if (c == '\t')
            {
                tabCount += currentTabStopSpacing;
                currentTabStopSpacing = 3;
            }
            else
            {
                currentTabStopSpacing--;
            }

            // resets after dropping below 0
            if (currentTabStopSpacing < 0)
            {
                currentTabStopSpacing = 3;
            }
        }

        return tabCount;
    }

    void MoveCaretUp()
    {
        // Safeguard if there are no characters to move up from
        if (characterOn == 0)
        {
            return;
        }

        TMP_TextInfo textInfo = hiddenInputText.textInfo;
        TMP_CharacterInfo currentCharInfo = textInfo.characterInfo[characterOn];

        // Safeguard if we are on the first line
        if (currentCharInfo.lineNumber <= 0)
        {
            return;
        }

        int lineNumber = currentCharInfo.lineNumber;

        // Get the TMP_LineInfo for the current and above lines
        TMP_LineInfo currentLineInfo = textInfo.lineInfo[lineNumber];
        TMP_LineInfo lineInfoAbove = textInfo.lineInfo[lineNumber - 1];

        // Get the first character index of the current line
        int firstCharacterIndexInCurrentLine = currentLineInfo.firstCharacterIndex;

        // Calculate the character's position within the current line
        int characterIndexWithinLine = characterOn - firstCharacterIndexInCurrentLine;

        // Adjust the index considering tabs in the current line
        string currentLineText = hiddenInputText.text.Substring(currentLineInfo.firstCharacterIndex, characterIndexWithinLine);

        int tabCountIncrease = CountTabs(currentLineText);

        // increases index as tabs are still 1 character but take 1-4 spaces
        characterIndexWithinLine += tabCountIncrease;
        print(characterIndexWithinLine);
        if (isOffSetOnHorizontalPosition)
        {
            characterIndexWithinLine = horizontalCharacterOnRemember;

        }
        // Calculate the target character index on the line above, the offset on the line
        int characterToGoToOnAboveLine = 0;



        // Adjust for tabs in the line above
        string aboveLineText = hiddenInputText.text.Substring(lineInfoAbove.firstCharacterIndex, lineInfoAbove.lastCharacterIndex - lineInfoAbove.firstCharacterIndex);

        int currentTabStopSpacing = 4;
        int totalTabStopSpacings = 0;
        int lengthToStopAt = 0;
        int i = 0;
        // while we havn't gotten past the characters position on the line and havn't gone through each letter
        // gets the tabs through the above line

        while (lengthToStopAt < characterIndexWithinLine && i < lineInfoAbove.characterCount - 1)
        {

            if (aboveLineText[i] == '\t')
            {

                // increase x position depending on current length of a tab
                lengthToStopAt += currentTabStopSpacing;
                // keeps track of current amount of tab only x changes
                totalTabStopSpacings += currentTabStopSpacing;

                // resets the count since we hit a tab stop
                currentTabStopSpacing = 4;
                if (lengthToStopAt - characterIndexWithinLine >= 2)
                {
                    characterToGoToOnAboveLine = i;
                }
                else
                {
                    characterToGoToOnAboveLine = i + 1;
                }
                // using this incase we stop within a tab and need to remeber
                isOffSetOnHorizontalPosition = true;
            }
            else
            {
                lengthToStopAt++;
                currentTabStopSpacing--;
                characterToGoToOnAboveLine = i + 1;
                isOffSetOnHorizontalPosition = false;
            }

            i++;

            // resets after dropping below 0
            if (currentTabStopSpacing <= 0)
            {
                currentTabStopSpacing = 4;
            }

        }

        // Ensure the target index is within the bounds of the above line
        if (horizontalCharacterOnRemember >= lineInfoAbove.characterCount - 1 + totalTabStopSpacings)
        {
            // sets the horizontalPosition trigger
            isOffSetOnHorizontalPosition = true;
            characterToGoToOnAboveLine = lineInfoAbove.characterCount - 1;
        }
        // Update characterOn to the new position
        // if we are suppose to just go to positon, then set it
        characterOn = lineInfoAbove.firstCharacterIndex + characterToGoToOnAboveLine;

        // Update caret position
        UpdateCaretPositionToSpecificCharacter();
    }

    void MoveCaretDown()
    {
        int lineNumber = 0;// 
        TMP_TextInfo textInfo = hiddenInputText.textInfo;

        // checks if we aren't even on a character yet (only applies to the inital start)
        if (characterOn != 0)
        {
            TMP_CharacterInfo currentCharInfo = textInfo.characterInfo[characterOn];
            lineNumber = currentCharInfo.lineNumber;
        }

        // Safeguard if we are on the last line, line count starts at 1, lineNumber starts at 0
        if (lineNumber == textInfo.lineCount - 1)
        {
            return;
        }

        // Get the TMP_LineInfo for the current and above lines
        TMP_LineInfo currentLineInfo = textInfo.lineInfo[lineNumber];
        TMP_LineInfo lineInfoAbove = textInfo.lineInfo[lineNumber + 1];

        // Get the first character index of the current line
        int firstCharacterIndexInCurrentLine = currentLineInfo.firstCharacterIndex;

        // Calculate the character's position within the current line
        int characterIndexWithinLine = characterOn - firstCharacterIndexInCurrentLine;

        // Adjust the index considering tabs in the current line
        string currentLineText = hiddenInputText.text.Substring(currentLineInfo.firstCharacterIndex, characterIndexWithinLine);

        int tabCountIncrease = CountTabs(currentLineText);

        // increases index as tabs are still 1 character but take 1-4 spaces
        characterIndexWithinLine += tabCountIncrease;

        // resets the horiztonal target to the orignal x position
        if (isOffSetOnHorizontalPosition)
        {
            characterIndexWithinLine = horizontalCharacterOnRemember;
        }
        // Calculate the target character index on the line above, the offset on the line
        int characterToGoToOnAboveLine = 0;



        // Adjust for tabs in the line above
        string aboveLineText = hiddenInputText.text.Substring(lineInfoAbove.firstCharacterIndex, lineInfoAbove.lastCharacterIndex - lineInfoAbove.firstCharacterIndex);

        int currentTabStopSpacing = 4;
        int totalTabStopSpacings = 0;
        int lengthToStopAt = 0;
        int i = 0;
        // while we havn't gotten past the characters position on the line and havn't gone through each letter
        // gets the tabs through the above line
        while (lengthToStopAt < characterIndexWithinLine && i < lineInfoAbove.characterCount - 1)
        {

            if (aboveLineText[i] == '\t')
            {

                //  print($"Added froim tab {currentTabStopSpacing}");
                lengthToStopAt += currentTabStopSpacing;
                totalTabStopSpacings += currentTabStopSpacing;
                // print($"Increased by: {currentTabStopSpacing}");
                currentTabStopSpacing = 4;
                //print(characterToGoToOnAboveLine);
                if (lengthToStopAt - characterIndexWithinLine >= 2)
                {
                    //  print("In top if set ");
                    //  print($"lengthToStopAt: {lengthToStopAt}, characterIndexWithinLine: {characterIndexWithinLine}");

                    // move forwards liek normal
                    characterToGoToOnAboveLine = i;
                }
                else
                {
                    // we should move backwards
                    //print("In bot if set i  + 1");
                    // print($"lengthToStopAt: {lengthToStopAt}, characterIndexWithinLine: {characterIndexWithinLine}");
                    characterToGoToOnAboveLine = i + 1;
                }
                // print(characterToGoToOnAboveLine);
                // using this incase we stop within a tab and need to remeber
                isOffSetOnHorizontalPosition = true;
            }
            else
            {
                //  print("Added 1");
                lengthToStopAt++;
                // print($"Increased by: 1");
                currentTabStopSpacing--;
                characterToGoToOnAboveLine = i + 1;
                isOffSetOnHorizontalPosition = false;
            }

            i++;

            // resets after dropping below 0
            if (currentTabStopSpacing <= 0)
            {
                currentTabStopSpacing = 4;
            }
        }
        // Ensure the target index is within the bounds of the above line
        if (horizontalCharacterOnRemember >= lineInfoAbove.characterCount - 1 + totalTabStopSpacings)
        {

            // sets the horizontalPosition trigger
            isOffSetOnHorizontalPosition = true;
            characterToGoToOnAboveLine = lineInfoAbove.characterCount - 1;
        }


        // Update characterOn to the new position
        // if we are suppose to just go to positon, then set it
        characterOn = lineInfoAbove.firstCharacterIndex + characterToGoToOnAboveLine;

        // Update caret position
        UpdateCaretPositionToSpecificCharacter();
    }

}
