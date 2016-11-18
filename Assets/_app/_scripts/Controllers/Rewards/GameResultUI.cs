﻿// Author: Daniele Giardini - http://www.demigiant.com
// Created: 2016/11/18

using UnityEngine;

namespace EA4S
{
    public class GameResultUI : MonoBehaviour
    {
        public EndgameResultPanel EndgameResultPanel;

        static GameResultUI I;
        const string ResourcesPath = "Prefabs/UI/GameResultUI";

        #region Unity + Init

        static void Init()
        {
            if (I != null) return;

            I = Instantiate(Resources.Load<GameResultUI>(ResourcesPath));
        }

        void Awake()
        {
            I = this;
        }

        void OnDestroy()
        {
            if (I == this) I = null;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Never use this directly! Use the <code>Minigames Interface</code> instead.
        /// </summary>
        public static void HideEndgameResult()
        {
            if (I == null) return;

            I.EndgameResultPanel.Show(false);
        }

        /// <summary>
        /// Never use this directly! Use the <code>Minigames Interface</code> instead.
        /// </summary>
        public static void ShowEndgameResult(int _numStars)
        {
            Init();
            I.EndgameResultPanel.Show(true, _numStars);
        }

        #endregion
    }
}