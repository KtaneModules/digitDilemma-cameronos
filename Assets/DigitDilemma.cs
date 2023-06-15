using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;
using Math = ExMath;

public class DigitDilemma : MonoBehaviour {

  public KMBombInfo Bomb;
  public KMBombModule Module;
  public KMBombInfo BombInfo;
  public KMAudio Audio;
  public KMSelectable[] Buttons;
  public TextMesh[] DisplayTexts;

  //Keypad mats
  public Material KeypadCorrect;
  public Material KeypadOff;
  public Material KeypadWrong;
  public Material KeypadYellow;
  public Material StageGreen;
  public Material StageRed;
  public Material DisplayBack;
  public Renderer[] Keypads;
  public Renderer[] StageIdents;

  //Changing the keypads
  private int buttonNumber;
  private int materialNum;
  private int failButton;
  private int[] buttonSequence;
  private int buttonSequenceIndex = 0;
  string[] buttonLabels = { "Button 1", "Button 2", "Button 3", "Button 4" };
  //Num display
  private int stage = 0;
  private string targetNumber;
  private float typingSpeed = 0.15f;
  private bool numberDisplayed = false;
  private bool isTyping = false;
  private bool moduleButtonSolve = false;

   static int ModuleIdCounter = 1;
   int ModuleId;
   private bool ModuleSolved;

   void Awake () {
      ModuleId = ModuleIdCounter++;
      GetComponent<KMBombModule>().OnActivate += Activate;
      /*
      foreach (KMSelectable object in keypad) {
          object.OnInteract += delegate () { keypadPress(object); return false; };
      }
      */

      // Keypad buttons
      Buttons[0].OnInteract += delegate () { interactPress(1); return false; };
      Buttons[1].OnInteract += delegate () { interactPress(2); return false; };
      Buttons[2].OnInteract += delegate () { interactPress(3); return false; };
      Buttons[3].OnInteract += delegate () { interactPress(4); return false; };

      // Interaction PUNCHES!!!
      Buttons[0].AddInteractionPunch();
      Buttons[1].AddInteractionPunch();
      Buttons[2].AddInteractionPunch();
      Buttons[3].AddInteractionPunch();
    }

    void interactPress(int buttonIndex)
    {
    if(!moduleButtonSolve)
    {
      if(!isTyping)
      {
      int buttonSoundIndex = buttonIndex - 1;
      Audio.PlaySoundAtTransform("ButtonPress", Buttons[buttonSoundIndex].transform);
        if (stage == 0)
        {
            Debug.LogFormat("[Digit Dilemma " + "#" + ModuleId + "] Number displayed is: " + targetNumber + ".", ModuleId);
            materialNum = buttonIndex;
            CalculateButtonSequence();
            Debug.LogFormat("[Digit Dilemma " + "#" + ModuleId + "] The correct button number is: " + buttonNumber + ".", ModuleId);
            stage++;
            StartCoroutine(TryTypeNumber());
        }
        else if (stage == 1)
        {
            if (buttonIndex == buttonNumber)
            {
                materialNum = buttonIndex;
                GenerateNumber();
                CalculateButtonSequence();
                Debug.LogFormat("[Digit Dilemma " + "#" + ModuleId + "] Correct button #" + buttonIndex + " was pressed. Continuing to stage 2.", ModuleId);
                StartCoroutine(TypeNumber());
                StartCoroutine(ResetKeypadColors());
                StartCoroutine(GreenKeypadFlash());
                StageIdents[2].material = StageGreen;
                stage++;
                StageLogging();
                Audio.PlaySoundAtTransform("StageContinue", DisplayTexts[0].transform);
            }
            else
            {
                Audio.PlaySoundAtTransform("Strike", DisplayTexts[0].transform);
                Debug.LogFormat("[Digit Dilemma " + "#" + ModuleId + "] Strike! Button #" + failButton + " was pressed. Restarting stages.", ModuleId);
                RestartStages();
                Strike();
                return; // Exit the method to prevent further execution
            }
        }
        else if (stage == 2)
        {
          if (buttonIndex == buttonNumber)
          {
            materialNum = buttonIndex;
            Debug.LogFormat("[Digit Dilemma " + "#" + ModuleId + "] Correct button #" + buttonIndex + " was pressed. Continuing to stage 3.", ModuleId);
            StartCoroutine(TypeNumber());
            StartCoroutine(ResetKeypadColors());
            StartCoroutine(GreenKeypadFlash());
            StageIdents[1].material = StageGreen;
            stage++;
            StageLogging();
            Audio.PlaySoundAtTransform("StageContinue", DisplayTexts[0].transform);
          }
          else
          {
              Audio.PlaySoundAtTransform("Strike", DisplayTexts[0].transform);
              Debug.LogFormat("[Digit Dilemma " + "#" + ModuleId + "] Strike! Button #" + failButton + " was pressed. Restarting stages.", ModuleId);
              RestartStages();
              Strike();
              return;
          }
    }
    else if (stage == 3)
    {
      if (buttonIndex == buttonNumber)
      {
        materialNum = buttonIndex;
        Debug.LogFormat("[Digit Dilemma " + "#" + ModuleId + "] Module solved!", ModuleId);
        Solve();
        Audio.PlaySoundAtTransform("Solve", DisplayTexts[0].transform);
        //Make all keypads flash, tabs green, ensure continuity
        DisplayTexts[0].text = "DEFUSED";
        StartGlitchEffect();
        DisplayTexts[0].color = Color.green;
        StageIdents[0].material = StageGreen;
        StageIdents[1].material = StageGreen;
        StageIdents[2].material = StageGreen;
        Keypads[0].material = KeypadCorrect;
        Keypads[1].material = KeypadCorrect;
        Keypads[2].material = KeypadCorrect;
        Keypads[3].material = KeypadCorrect;
        moduleButtonSolve = true;
        Keypads[materialNum].material = KeypadCorrect;
      }
      else
      {
          Audio.PlaySoundAtTransform("Strike", DisplayTexts[0].transform);
          Debug.LogFormat("[Digit Dilemma " + "#" + ModuleId + "] Strike! Button #" + failButton + " was pressed. Restarting stages.", ModuleId);
          RestartStages();
          Strike();
          return; // Exit the method to prevent further execution
      }
}
}
else
{
  //Do nothing. Cant
}
}
else
{
  //Do nothing. Cant like usual.
}
  }

  private Coroutine glitchCoroutine;
  private float durationInSeconds = 2f;
  private float glitchInterval = 0.005f;
  private string originalText = "DEFUSED";
  private void StartGlitchEffect()
  {
      if (glitchCoroutine != null)
          StopCoroutine(glitchCoroutine);

      glitchCoroutine = StartCoroutine(DoGlitchEffect());
  }

  private IEnumerator DoGlitchEffect()
  {
      float startTime = Time.time;

      while (Time.time - startTime < durationInSeconds)
      {
          char[] glitchedText = originalText.ToCharArray();
          for (int i = 0; i < glitchedText.Length; i++)
          {
              glitchedText[i] = GetRandomCharacter();
              DisplayTexts[0].text = new string(glitchedText);
              yield return new WaitForSeconds(glitchInterval);
          }

          yield return null;
      }
      DisplayTexts[0].text = originalText;
  }

  private char GetRandomCharacter()
  {
      string characters = "0123456789!@#$%^&*()";
      int index = UnityEngine.Random.Range(0, characters.Length);
      return characters[index];
  }

void StageLogging()
{
  GenerateNumber();
  Debug.LogFormat("[Digit Dilemma " + "#" + ModuleId + "] Number displayed is: " + targetNumber + ".", ModuleId);
  CalculateButtonSequence();
  Debug.LogFormat("[Digit Dilemma " + "#" + ModuleId + "] The correct button number is: " + buttonNumber + ".", ModuleId);
}

void RestartStages()
{
  numberDisplayed = false;
  stage = 0;
  DisplayTexts[0].text = "";
  StartCoroutine(FlashStagesRed());
  ResetKeypadColors();
}

      IEnumerator ResetKeypadColors()
      {
          Keypads[0].material = KeypadOff;
          Keypads[1].material = KeypadOff;
          Keypads[2].material = KeypadOff;
          Keypads[3].material = KeypadOff;
          yield return null;
      }

private float flashDuration = 0.5f;
private float flashInterval = 0.15f; // Time interval between flashes

IEnumerator GreenKeypadFlash()
{
    int numberOfFlashes = Mathf.CeilToInt(flashDuration / flashInterval);
    for (int i = 0; i < numberOfFlashes; i++)
    {
        switch (materialNum)
        {
            case 1:
                Keypads[0].material = KeypadCorrect;
                break;
            case 2:
                Keypads[1].material = KeypadCorrect;
                break;
            case 3:
                Keypads[2].material = KeypadCorrect;
                break;
            case 4:
                Keypads[3].material = KeypadCorrect;
                break;
            default:
                break;
        }

        yield return new WaitForSeconds(flashInterval);

        switch (materialNum)
        {
            case 1: // Assuming button numbers start from 0
                Keypads[0].material = KeypadOff;
                break;
            case 2:
                Keypads[1].material = KeypadOff;
                break;
            case 3:
                Keypads[2].material = KeypadOff;
                break;
            case 4:
                Keypads[3].material = KeypadOff;
                break;
            default:
                // Handle any other button number here
                break;
        }

        yield return new WaitForSeconds(flashInterval);
    }
}

      IEnumerator TryTypeNumber()
      {
          if (!numberDisplayed)
          {
              StartCoroutine(FlashButtonYellow());
              numberDisplayed = true;
              StartCoroutine(TypeNumber());
          }
          else
          {
              StartCoroutine(ResetKeypadColors());
              StartCoroutine(GreenKeypadFlash());
          }
          yield return new WaitForSeconds(1f);
      }

      IEnumerator FlashStagesRed()
      {
          float flashDuration = 0.2f;
          int flashCount = 3;
          for (int i = 0; i < flashCount; i++)
          {
              StageIdents[0].material = StageRed;
              StageIdents[1].material = StageRed;
              StageIdents[2].material = StageRed;
              yield return new WaitForSeconds(flashDuration);
              StageIdents[0].material = DisplayBack;
              StageIdents[1].material = DisplayBack;
              StageIdents[2].material = DisplayBack;
              yield return new WaitForSeconds(flashDuration);
          }
      }

      IEnumerator FlashButtonYellow()
      {
          float flashDuration = 0.2f;
          int flashCount = 3;
          for (int i = 0; i < flashCount; i++)
          {
              Keypads[0].material = KeypadYellow;
              Keypads[1].material = KeypadYellow;
              Keypads[2].material = KeypadYellow;
              Keypads[3].material = KeypadYellow;
              yield return new WaitForSeconds(flashDuration);
              Keypads[0].material = KeypadOff;
              Keypads[1].material = KeypadOff;
              Keypads[2].material = KeypadOff;
              Keypads[3].material = KeypadOff;
              yield return new WaitForSeconds(flashDuration);
          }
      }

      IEnumerator TypeNumber()
      {
          isTyping = true;
          for (int i = 0; i <= targetNumber.Length; i++)
          {
              Audio.PlaySoundAtTransform("Type", DisplayTexts[0].transform);
              string currentNumber = targetNumber.Substring(0, i);
              DisplayTexts[0].text = currentNumber;
              yield return new WaitForSeconds(typingSpeed);
          }
          isTyping = false;
      }


      void CalculateButtonSequence()
      {
          int solvedModuleCount = BombInfo.GetSolvedModuleNames().Count;
          int batteryCount = BombInfo.GetBatteryCount();

          int sumOfDigits = targetNumber.Sum(c => int.Parse(c.ToString()));

          if (sumOfDigits % 2 == 0)
          {
              buttonNumber = (sumOfDigits / (solvedModuleCount + 1)) % 4 + 1;
          }
          else
          {
              buttonNumber = (sumOfDigits * batteryCount) % 4 + 1;
          }

          buttonSequence = new int[] { buttonNumber };
      }

   void OnDestroy () { //Shit you need to do when the bomb ends

   }

   void Activate () { //Shit that should happen when the bomb arrives (factory)/Lights turn on

   }

   private void GenerateNumber()
   {
       targetNumber = UnityEngine.Random.Range(1000, 9999).ToString();
   }

   void Start () { //Shit
     StageIdents[0].material = DisplayBack;
     StageIdents[1].material = DisplayBack;
     StageIdents[2].material = DisplayBack;
     GenerateNumber();
   }

   void Update () { //Shit that happens at any point after initialization

   }

   void Solve () {
      GetComponent<KMBombModule>().HandlePass();
   }

   void Strike () {
      GetComponent<KMBombModule>().HandleStrike();
   }

#pragma warning disable 414
   private readonly string TwitchHelpMessage = @"Use !{0} start to start the initialize the module. Use !{0} press [1/2/3/4] to press the respective keypads.";
#pragma warning restore 414

IEnumerator ProcessTwitchCommand(string Command) {
  Command = Command.ToUpper();
  yield return null;

  switch (Command) {
    case "START":
      Buttons[0].OnInteract();
      break;
    case "PRESS 1":
      Buttons[0].OnInteract();
      break;
    case "PRESS 2":
      Buttons[1].OnInteract();
      break;
    case "PRESS 3":
      Buttons[2].OnInteract();
      break;
    case "PRESS 4":
      Buttons[3].OnInteract();
      break;
    case "PRESS ONE":
      Buttons[0].OnInteract();
      break;
    case "PRESS TWO":
      Buttons[1].OnInteract();
      break;
    case "PRESS THREE":
      Buttons[2].OnInteract();
      break;
    case "PRESS FOUR":
      Buttons[3].OnInteract();
      break;
    default:
      yield return string.Format(
          "sendtochaterror Invalid command or cannot press button currently");
      yield break;
  }
}

   IEnumerator TwitchHandleForcedSolve () {
     //Not writing all that code to figure out how to solve math. It's not a computer.
     Debug.LogFormat("[Digit Dilemma " + "#" + ModuleId + "] Module solved!", ModuleId);
     Solve();
     Audio.PlaySoundAtTransform("Solve", DisplayTexts[0].transform);
     //Make all keypads flash
     DisplayTexts[0].text = "DEFUSED";
     StartGlitchEffect();
     DisplayTexts[0].color = Color.green;
     StageIdents[0].material = StageGreen;
     StageIdents[1].material = StageGreen;
     StageIdents[2].material = StageGreen;
     Keypads[0].material = KeypadCorrect;
     Keypads[1].material = KeypadCorrect;
     Keypads[2].material = KeypadCorrect;
     Keypads[3].material = KeypadCorrect;
     yield return null;
   }
}
