﻿using UnityEngine;
using System.Collections.Generic;
using ModularFramework.Core;
using ModularFramework.Modules;
using ModularFramework.Helpers;
using Google2u;
using EA4S;

namespace EA4S
{
    public class AppManager : GameManager
    {

        #region properties, variables and constants

        #region Overrides
        new public AppSettings GameSettings = new AppSettings();

        new public static AppManager Instance
        {
            get { return GameManager.Instance as AppManager; }
        }
        #endregion

        #region TMP

        /// <summary>
        /// Tmp var to store actual gameplay word already used.
        /// </summary>
        public List<WordData> ActualGameplayWordAlreadyUsed = new List<WordData>();

        public string ActualGame = string.Empty;

        #endregion

        #region Mood
        /// <summary>
        /// False if not executed start mood eval.
        /// </summary>
        public bool StartMood = false;
        /// <summary>
        /// Start Mood value. Values 0,1,2,3,4.
        /// </summary>
        public int StartMoodEval = 0;
        /// <summary>
        /// End Mood value. Values 0,1,2,3,4.
        /// </summary>
        public int EndMoodEval = 0;
        #endregion

        public List<LetterData> Letters = new List<LetterData>();
        
        public TeacherAI Teacher;
        public Database DB;

        public const string AppVersion = "0.2.0";

        #endregion

        #region Init

        public string IExist() {
            return "AppManager Exists";
        }

        public void InitDataAI() {
            if (DB == null)
                DB = new Database();
            if (Teacher == null)
                Teacher = new TeacherAI();
        }

        protected override void GameSetup() { 
            base.GameSetup();

            AdditionalSetup();

            CachingLetterData();

            GameSettings.HighQualityGfx = true;
        }

        void AdditionalSetup() {
            // GameplayModule
            if (GetComponentInChildren<ModuleInstaller<IGameplayModule>>()) {
                IGameplayModule moduleInstance = GetComponentInChildren<ModuleInstaller<IGameplayModule>>().InstallModule();
                Modules.GameplayModule.SetupModule(moduleInstance, moduleInstance.Settings);
            }


        }

        void CachingLetterData() {
            foreach (string rowName in letters.Instance.rowNames) {
                lettersRow letRow = letters.Instance.GetRow(rowName);
                Letters.Add(new LetterData(rowName, letRow));
            }
        }

        #endregion

        #region Game Progression
        public int Stage = 2;
        public int LearningBlock = 4;
        public int PlaySession = 1;
        public int PlaySessionGameDone = 0;
        public MinigameData ActualMinigame;

        /// <summary>
        /// Give right game. Alpha version.
        /// </summary>
        public MinigameData GetMiniGameForActualPlaySession() {
            MinigameData miniGame = null;
            switch (PlaySession) {
                case 1:
                    if (PlaySessionGameDone == 0)
                        miniGame = DB.gameData.Find(g => g.Code == "fastcrowd");
                    else
                        miniGame = DB.gameData.Find(g => g.Code == "balloons");
                    break;
                case 2:
                    if (PlaySessionGameDone == 0)
                        miniGame = DB.gameData.Find(g => g.Code == "fastcrowd_words");
                    else
                        miniGame = DB.gameData.Find(g => g.Code == "dontwakeup");
                    break;
                case 3:
                    miniGame = new MinigameData("Assessment", "Assessment", "Assessment", "app_Assessment", true);
                    break;
            }
            ActualMinigame = miniGame;
            return miniGame;
        }

        /// <summary>
        /// Set result and return next scene name.
        /// </summary>
        /// <returns>return next scene name.</returns>
        public string MiniGameDone() {
            string returnString = "app_Wheel";
            if (PlaySessionGameDone > 0) {
                PlaySession++;
                PlaySessionGameDone = 0;
            } else {
                PlaySessionGameDone++;
            }
            return returnString;
        }


        #endregion

        #region settings behaviours

        public void ToggleQualitygfx() {
            GameSettings.HighQualityGfx = !GameSettings.HighQualityGfx;
            CameraGameplayController.I.EnableFX(GameSettings.HighQualityGfx);
        }

        #endregion

        #region event delegate

        public void OnMinigameStart() {
            // reset for already used word.
            ActualGameplayWordAlreadyUsed = new List<WordData>();
        }

        #endregion

    }

    /// <summary>
    /// Game Setting Extension class.
    /// </summary>
    [System.Serializable]
    public class AppSettings : GameSettings
    {
        public bool DoLogPlayerBehaviour;
        public bool HighQualityGfx;
    }

}