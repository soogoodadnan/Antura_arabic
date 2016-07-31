﻿using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using ModularFramework.Helpers;

namespace EA4S
{
    public class AssessmentManager : MonoBehaviour
    {

        public List<Color> Colors;
        public List<ColorSet> AvailableColors = new List<ColorSet>();

        public List<AssessmentObject> Draws;
        public List<AssessmentObject> Words;

        public AssessmentObject startObj, endObj;
        public PopupTmp Popup;

        public struct ColorSet {
            public Color Color;
            public bool Available;
        }

        void Start() {
            AppManager.Instance.InitDataAI();
            Colors.Shuffle();
            foreach (Color c in Colors) {
                AvailableColors.Add(new ColorSet() { Color = c, Available = true });
            }
            List<ILivingLetterData> newDatas = new List<ILivingLetterData>(); // list to be shuffled
            for (int i = 0; i < Draws.Count; i++) {
                ILivingLetterData newData = AppManager.Instance.Teacher.GimmeAGoodWordData();
                newDatas.Add(newData);
                Draws[i].Init(newData, false);
                Draws[i].InjectManager(this);
            }
            newDatas.Shuffle();
            for (int i = 0; i < newDatas.Count; i++) {
                Words[i].Init(newDatas[i], true);
                Words[i].InjectManager(this);
            }


            WidgetSubtitles.I.DisplaySentence("assessment_start_A1", 2, true);
        }

        public void OnReleaseOnWord(AssessmentObject _objDrag, AssessmentObject _objDrop) {
            foreach (var obj in Draws.FindAll(o => o.Color == _objDrag.Color && o != _objDrag && o != _objDrop)) {
                obj.HideCyrcle(0.5f);
            }
            foreach (var obj in Words.FindAll(o => o.Color == _objDrag.Color && o != _objDrop)) {
                obj.HideCyrcle(0.5f);
            }
            if (Draws.FindAll(o => !o.IsLocked).Count == 0)
                CalculateResult();
        }

        void CalculateResult() {
            int rightCounter = 0;
            foreach (var c in Colors) {
                ILivingLetterData d, w;
                d = Draws.Find(o => o.Color == c).data;
                w = Words.Find(o => o.Color == c).data;
                if (d.Key == w.Key) {
                    rightCounter++;
                }
            }

            //Debug.LogFormat("Result : {0}/{1}", rightCounter, Draws.Count);
            Popup.Show(true, string.Format("Result : {0}/{1}", rightCounter, Draws.Count));
        }


        public void UnlockObjects(Color _color) {
            foreach (var obj in Draws.FindAll(o => o.Color == _color && o.IsLocked)) {
                obj.IsLocked = false;
                obj.HideCyrcle(0.5f);
            }
            foreach (var obj in Words.FindAll(o => o.Color == _color && o.IsLocked)) {
                obj.IsLocked = false;
                obj.HideCyrcle(0.5f);
            }
            ReleaseColor(_color);
        }

        #region colors
        public Color GetAvailableColor() {
            AvailableColors.Shuffle();
            int index = AvailableColors.FindIndex(c => c.Available == true);
            AvailableColors[index] = new ColorSet() { Color = AvailableColors[index].Color, Available = false };
            return AvailableColors[index].Color;
        }

        public void ReleaseColor(Color _color) {
            int index = AvailableColors.FindIndex(c => c.Color == _color);
            AvailableColors[index] = new ColorSet() { Color = AvailableColors[index].Color, Available = true };
        }
        #endregion
    }

}