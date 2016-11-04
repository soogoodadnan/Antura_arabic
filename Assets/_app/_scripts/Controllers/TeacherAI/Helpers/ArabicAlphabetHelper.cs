﻿using UnityEngine;
using System.Collections.Generic;
using System;
using ArabicSupport;

namespace EA4S
{
    public static class ArabicAlphabetHelper
    {

        public static List<string> LetterExceptions = new List<string>() { "0627", "062F", "0630", "0631", "0632", "0648", "0623" };

        /// <summary>
        /// Prepares the string for display (say fro mArabic into TMPro Text
        /// </summary>
        /// <returns>The string for display.</returns>
        /// <param name="">.</param>
        public static string PrepareStringForDisplay(string str)
        {
            return GenericUtilities.ReverseText(ArabicFixer.Fix(str));
        }

        /// <summary>
        /// Return single letter string start from unicode hexa code.
        /// </summary>
        /// <param name="_hexaCode"></param>
        /// <returns></returns>
        public static string GetLetterFromUnicode(string _hexaCode)
        {
            int unicode = int.Parse(_hexaCode, System.Globalization.NumberStyles.HexNumber);
            char character = (char)unicode;
            string text = character.ToString();
            return text;
        }

        /// <summary>
        /// Get char hexa code.
        /// </summary>
        /// <param name="_c"></param>
        /// <param name="_forConversion"></param>
        /// <returns></returns>
        public static string GetHexaUnicodeFromChar(char _c, bool _forConversion = false)
        {
            return string.Format("{1}{0:X4}", Convert.ToUInt16(_c), _forConversion ? "/U" : string.Empty);
        }

        /// <summary>
        /// Returns the list of letters found in a word string
        /// </summary>
        /// <param name="_word"></param>
        /// <param name="_vocabulary"></param>
        /// <param name="_revertOrder">Return in list position 0 most right letter in input string and last the most left.</param>
        /// <returns></returns>
        public static List<string> LetterDataList(string _word, List<Db.LetterData> _vocabulary, bool _revertOrder = false)
        {
            List<string> returnList = new List<string>();

            char[] chars = _word.ToCharArray();
            if (_revertOrder)
            {
                Array.Reverse(chars);
            }

            for (int i = 0; i < chars.Length; i++)
            {
                char ch = chars[i];
                string unicodeString = GetHexaUnicodeFromChar(ch);
                Db.LetterData letterData =  _vocabulary.Find(l => l.Isolated_Unicode == unicodeString);
                if (letterData != null)
                    returnList.Add(letterData.Id);
            }

            return returnList;
        }


        /// <summary>
        /// Return list of letter data for any letter of param word.
        /// </summary>
        /// <param name="_word"></param>
        /// <param name="_vocabulary"></param>
        /// <param name="_revertOrder">Return in list position 0 most right letter in input string and last the most left.</param>
        /// <returns></returns>
        public static List<LL_LetterData> LetterDataListFromWord(string _word, List<LL_LetterData> _vocabulary, bool _revertOrder = false)
        {
            List<LL_LetterData> returnList = new List<LL_LetterData>();

            char[] chars = _word.ToCharArray();
            if (_revertOrder)
                Array.Reverse(chars);
            for (int i = 0; i < chars.Length; i++) {
                char ch = chars[i];
                string unicodeString = GetHexaUnicodeFromChar(ch);
                LL_LetterData let = _vocabulary.Find(l => l.Data.Isolated_Unicode == unicodeString);
                if (let != null)
                    returnList.Add(let);
            }
            return returnList;
        }

        /// <summary>
        /// Return last field.
        /// </summary>
        /// <param name="_word"></param>
        /// <param name="_vocabulary"></param>
        /// <param name="_revertOrder"></param>
        /// <returns></returns>
        public static string ParseWord(string _word, List<LL_LetterData> _vocabulary, bool _revertOrder = false)
        {
            string returnString = string.Empty;
            bool exceptionActive = false;
            List<LL_LetterData> letters = LetterDataListFromWord(_word, _vocabulary);
            if (letters.Count == 1)
                return returnString = ArabicAlphabetHelper.GetLetterFromUnicode(letters[0].Data.Isolated_Unicode);
            for (int i = 0; i < letters.Count; i++) {
                LL_LetterData let = letters[i];

                /// Exceptions
                if (exceptionActive) {
                    if (i == letters.Count - 1)
                        returnString += ArabicAlphabetHelper.GetLetterFromUnicode(let.Data.Isolated_Unicode);
                    else
                        returnString += ArabicAlphabetHelper.GetLetterFromUnicode(let.Data.Initial_Unicode);
                    exceptionActive = false;
                    continue;
                }
                if (LetterExceptions.Contains(let.Data.Isolated_Unicode))
                    exceptionActive = true;
                /// end Exceptions

                if (let != null) {
                    if (i == 0) {
                        returnString += ArabicAlphabetHelper.GetLetterFromUnicode(let.Data.Initial_Unicode);
                        continue;
                    } else if (i == letters.Count - 1) {
                        returnString += ArabicAlphabetHelper.GetLetterFromUnicode(let.Data.Final_Unicode);
                        continue;
                    } else {
                        returnString += ArabicAlphabetHelper.GetLetterFromUnicode(let.Data.Medial_Unicode);
                        continue;
                    }
                } else {
                    returnString += string.Format("{0}{2}{1}", "<color=red>", "</color>", ArabicAlphabetHelper.GetLetterFromUnicode(let.Data.Isolated_Unicode));
                }
            }
            return returnString;
        }
    }
}