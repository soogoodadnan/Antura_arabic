﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ModularFramework.Core;
using ModularFramework.Helpers;
using EA4S;

namespace Balloons
{
    public class WordPromptController : MonoBehaviour
    {
        public LetterPromptController[] letterPrompts;

        [HideInInspector]
        public List<LetterPromptController> IdleLetterPrompts
        {
            get { return new List<LetterPromptController>(letterPrompts).FindAll(prompt => prompt.isActiveAndEnabled && prompt.State == LetterPromptController.PromptState.IDLE); }
        }


        public void DisplayWord(List<LetterData> wordLetters)
        {
            for (int i = 0; i < wordLetters.Count; i++)
            {
                letterPrompts[i].gameObject.SetActive(true);
                letterPrompts[i].Init(wordLetters[i]);
            }
        }

        public void Reset()
        {
            foreach (var prompt in letterPrompts)
            {
                prompt.State = LetterPromptController.PromptState.IDLE;
                prompt.gameObject.SetActive(false);
            }
        }
    }
}
