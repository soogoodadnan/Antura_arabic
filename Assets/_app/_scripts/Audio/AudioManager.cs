﻿using UnityEngine;
using System.Collections.Generic;
using EA4S.Db;
using DG.DeAudio;

namespace EA4S
{
    /// <summary>
    /// Handles audio requests throughout the application
    /// </summary>
    public class AudioManager : MonoBehaviour, ISerializationCallbackReceiver
    {
        public static AudioManager I;

        List<AudioSourceWrapper> playingAudio = new List<AudioSourceWrapper>();

        DeAudioGroup musicGroup;
        DeAudioGroup wordsLettersPhrasesGroup;
        DeAudioGroup keeperGroup;
        DeAudioGroup sfxGroup;

        System.Action OnDialogueEnded;
        bool hasToNotifyEndDialogue = false;

        bool musicEnabled = true;
        AudioClip customMusic;
        Music currentMusic;
        public bool MusicEnabled
        {
            get
            {
                return musicEnabled;
            }

            set
            {
                if (musicEnabled == value)
                    return;

                musicEnabled = value;

                if (musicEnabled && (currentMusic != Music.Silence))
                {
                    if (musicGroup != null)
                    {
                        musicGroup.Resume();

                        bool hasToReset = false;

                        if (musicGroup.sources == null)
                            hasToReset = true;
                        else
                        {
                            foreach (var s in musicGroup.sources)
                            {
                                if (s.isPlaying)
                                    goto Cont;
                            }
                            hasToReset = true;
                        }
                    Cont:
                        if (hasToReset)
                        {
                            if (currentMusic == Music.Custom)
                                musicGroup.Play(customMusic, 1, 1, true);
                            else
                                musicGroup.Play(GetAudioClip(currentMusic), 1, 1, true);
                        }

                    }
                }
                else
                {
                    if (musicGroup != null)
                        musicGroup.Pause();
                }
            }
        }

        Dictionary<string, AudioClip> audioCache = new Dictionary<string, AudioClip>();

        #region Serialized Configuration
        [SerializeField, HideInInspector]
        List<SfxConfiguration> sfxConfs = new List<SfxConfiguration>();

        [SerializeField, HideInInspector]
        List<MusicConfiguration> musicConfs = new List<MusicConfiguration>();

        bool configurationInitialized = false;
        Dictionary<Sfx, SfxConfiguration> sfxConfigurationMap = new Dictionary<Sfx, SfxConfiguration>();
        Dictionary<Music, MusicConfiguration> musicConfigurationMap = new Dictionary<Music, MusicConfiguration>();

        public void ClearConfiguration()
        {
            sfxConfs.Clear();
            musicConfs.Clear();
            sfxConfigurationMap.Clear();
            musicConfigurationMap.Clear();
            configurationInitialized = false;
        }

        public void UpdateSfxConfiguration(SfxConfiguration conf)
        {
            var id = sfxConfs.FindIndex((a) => { return a.sfx == conf.sfx; });

            if (id >= 0)
                sfxConfs.RemoveAt(id);

            sfxConfs.Add(conf);
            sfxConfigurationMap[conf.sfx] = conf;
        }

        public void UpdateMusicConfiguration(MusicConfiguration conf)
        {
            var id = musicConfs.FindIndex((a) => { return a.music == conf.music; });

            if (id >= 0)
                musicConfs.RemoveAt(id);

            musicConfs.Add(conf);
            musicConfigurationMap[conf.music] = conf;
        }

        public MusicConfiguration GetMusicConfiguration(Music music)
        {
            MusicConfiguration v;
            if (musicConfigurationMap.TryGetValue(music, out v))
                return v;
            return null;
        }

        public SfxConfiguration GetSfxConfiguration(Sfx sfx)
        {
            SfxConfiguration v;
            if (sfxConfigurationMap.TryGetValue(sfx, out v))
                return v;
            return null;
        }
        #endregion

        void Awake()
        {
            I = this;

            sfxGroup = DeAudioManager.GetAudioGroup(DeAudioGroupId.FX);
            musicGroup = DeAudioManager.GetAudioGroup(DeAudioGroupId.Music);
            wordsLettersPhrasesGroup = DeAudioManager.GetAudioGroup(DeAudioGroupId.Custom0);
            keeperGroup = DeAudioManager.GetAudioGroup(DeAudioGroupId.Custom1);

            musicEnabled = true;
        }

        public void OnAppPause(bool pauseStatus)
        {
            MusicEnabled = !pauseStatus;
        }

        #region Music
        public void ToggleMusic()
        {
            MusicEnabled = !musicEnabled;
        }

        public void PlayMusic(Music music)
        {
            currentMusic = music;
            musicGroup.Stop();

            var musicClip = GetAudioClip(music);

            if (music == Music.Silence || musicClip == null)
            {
                StopMusic();
            }
            else
            {
                if (musicEnabled)
                {
                    musicGroup.Play(musicClip, 1, 1, true);
                }
                else
                {
                    musicGroup.Stop();
                }
            }
        }

        public void StopMusic()
        {
            currentMusic = Music.Silence;
            musicGroup.Stop();
        }
        #endregion

        #region Sfx
        /// <summary>
        /// Play a soundFX
        /// </summary>
        /// <param name="sfx">Sfx.</param>
        public IAudioSource PlaySound(Sfx sfx)
        {
            AudioClip clip = GetAudioClip(sfx);
            return new AudioSourceWrapper(sfxGroup.Play(clip), sfxGroup, this);
        }

        public void StopSounds()
        {
            sfxGroup.Stop();
        }
        #endregion

        #region Letters, Words and Phrases
        public IAudioSource PlayLetter(LetterData data)
        {
            AudioClip clip = GetAudioClip(data);
            return new AudioSourceWrapper(wordsLettersPhrasesGroup.Play(clip), sfxGroup, this);
        }

        public IAudioSource PlayWord(WordData data)
        {
            AudioClip clip = GetAudioClip(data);
            return new AudioSourceWrapper(wordsLettersPhrasesGroup.Play(clip), sfxGroup, this);
        }

        public IAudioSource PlayPhrase(PhraseData data)
        {
            AudioClip clip = GetAudioClip(data);
            return new AudioSourceWrapper(wordsLettersPhrasesGroup.Play(clip), sfxGroup, this);
        }

        public void StopLettersWordsPhrases()
        {
            if (wordsLettersPhrasesGroup != null)
                wordsLettersPhrasesGroup.Stop();
        }
        #endregion

        #region Dialogue
        public IAudioSource PlayDialogue(string localizationData_id)
        {
            return PlayDialogue(LocalizationManager.GetLocalizationData(localizationData_id));
        }

        public IAudioSource PlayDialogue(Db.LocalizationDataId id)
        {
            return PlayDialogue(LocalizationManager.GetLocalizationData(id));
        }

        public IAudioSource PlayDialogue(Db.LocalizationData data, bool clearPreviousCallback = false)
        {
            if (!clearPreviousCallback && OnDialogueEnded != null)
                OnDialogueEnded();

            OnDialogueEnded = null;

            if (!string.IsNullOrEmpty(data.AudioFile))
            {
                AudioClip clip = GetAudioClip(data);
                return new AudioSourceWrapper(sfxGroup.Play(clip), keeperGroup, this);
            }
            return null;
        }

        public IAudioSource PlayDialogue(string localizationData_id, System.Action callback)
        {
            return PlayDialogue(LocalizationManager.GetLocalizationData(localizationData_id), callback);
        }

        public IAudioSource PlayDialogue(Db.LocalizationDataId id, System.Action callback)
        {
            return PlayDialogue(LocalizationManager.GetLocalizationData(id), callback);
        }

        public IAudioSource PlayDialogue(Db.LocalizationData data, System.Action callback, bool clearPreviousCallback = false)
        {
            if (!clearPreviousCallback && OnDialogueEnded != null)
                OnDialogueEnded();

            OnDialogueEnded = null;

            if (!string.IsNullOrEmpty(data.AudioFile))
            {
                OnDialogueEnded = callback;
                AudioClip clip = GetAudioClip(data);
                return new AudioSourceWrapper(sfxGroup.Play(clip), keeperGroup, this);
            }
            else
            {
                if (callback != null)
                    callback();
            }
            return null;
        }

        public void StopDialogue(bool clearPreviousCallback)
        {
            if (!clearPreviousCallback && OnDialogueEnded != null)
                OnDialogueEnded();

            OnDialogueEnded = null;

            keeperGroup.Stop();
        }
        #endregion

        #region Audio clip management

        public AudioClip GetAudioClip(Db.LocalizationData data)
        {
            return GetCachedResource("AudioArabic/Dialogs/" + data.AudioFile);
        }

        public AudioClip GetAudioClip(LetterData data)
        {
            return GetCachedResource("AudioArabic/Letters/" + data.Id);
        }

        public AudioClip GetAudioClip(WordData data)
        {
            return GetCachedResource("AudioArabic/Words/" + data.Id);
        }

        public AudioClip GetAudioClip(PhraseData data)
        {
            return GetCachedResource("AudioArabic/Phrases/" + data.Id);
        }

        public AudioClip GetAudioClip(Sfx sfx)
        {
            SfxConfiguration conf = GetSfxConfiguration(sfx);

            if (conf == null)
                return null;

            return conf.clips.GetRandom();
        }

        public AudioClip GetAudioClip(Music music)
        {
            MusicConfiguration conf = GetMusicConfiguration(music);

            if (conf == null)
                return null;

            return conf.clip;
        }

        AudioClip GetCachedResource(string resource)
        {
            AudioClip clip = null;

            if (audioCache.TryGetValue(resource, out clip))
                return clip;

            clip = Resources.Load(resource) as AudioClip;

            audioCache[resource] = clip;
            return clip;
        }

        public void ClearCache()
        {
            foreach (var r in audioCache)
                Resources.UnloadAsset(r.Value);
            audioCache.Clear();
        }
        #endregion


        public void Update()
        {
            for (int i = 0; i < playingAudio.Count; ++i)
            {
                var source = playingAudio[i];
                if (source.Update())
                {
                    // could be collected
                    playingAudio.RemoveAt(i--);

                    if (source.Group == keeperGroup)
                        hasToNotifyEndDialogue = true;
                }
            }

            if (hasToNotifyEndDialogue)
            {
                hasToNotifyEndDialogue = false;
                if (OnDialogueEnded != null)
                {
                    var oldCallback = OnDialogueEnded;
                    OnDialogueEnded = null;
                    oldCallback();
                }
            }
        }

        public void OnAfterDeserialize()
        {
            // Update map from serialized data
            sfxConfigurationMap.Clear();
            for (int i = 0, count = sfxConfs.Count; i < count; ++i)
                sfxConfigurationMap[sfxConfs[i].sfx] = sfxConfs[i];

            musicConfigurationMap.Clear();
            for (int i = 0, count = musicConfs.Count; i < count; ++i)
                musicConfigurationMap[musicConfs[i].music] = musicConfs[i];
        }

        public void OnBeforeSerialize()
        {

        }

        public IAudioSource PlaySound(AudioClip clip)
        {
            return new AudioSourceWrapper(sfxGroup.Play(clip), sfxGroup, this);
        }

        public IAudioSource PlayMusic(AudioClip clip)
        {
            StopMusic();
            currentMusic = Music.Custom;

            var source = musicGroup.Play(clip);

            customMusic = clip;

            return new AudioSourceWrapper(source, musicGroup, this);
        }

        /// <summary>
        /// Used by AudioSourceWrappers.
        /// </summary>
        /// <param name="source"></param>
        public void OnAudioStarted(AudioSourceWrapper source)
        {
            if (!playingAudio.Contains(source))
                playingAudio.Add(source);
        }
    }
}