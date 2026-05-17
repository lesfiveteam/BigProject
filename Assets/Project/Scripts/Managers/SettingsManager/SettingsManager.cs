using BigProject.Managers.SoundsMusicManagers;
using BigProject.Settings;
using BigProject.Systems;
using BigProject.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BigProject.Managers
{
    public enum ScreenFreq
    {
        VSync,
        Limit_60 = 60,
        Limit_120 = 120,
        Limit_144 = 144,
        Auto,
        Infinity
    }

    public class SettingsManager : ISavable, IDisposable
    {
        private SoundsManager _soundsManager;
        private MusicManager _musicManager;
        private SavesManager _savesManager;
        private GlobalConfig _config;
        private ScreenFreqConfig _freqConfig;

        //Settings
        private Resolution[] resolutions;
        private List<Resolution> _filteredResolutions;
        private int _currentResolutionIndex;
        private bool _isFullscreen = true;
        private float _soundVolume = 1f;
        private float _musicVolume = 1f;
        private DataToSave _dataToSave;

        private const float SFX_MASTER_MIN_MIN = -10f;
        private const float SFX_MASTER_MIN_MAX_DELTA = 90f;
        private const float SFX_MASTER_POW_FACTOR = 0.25f;
        private const string STD_FREQ_MODE = "VSync";

        public string CurrentFreqMode { get; private set; } = STD_FREQ_MODE;

        [Serializable]
        private class DataToSave
        {
            public string freqMode;
            public float musicVolume;
            public float soundVolume;
        }

        public string Key => "GameSettings";

        public object SavingData
        {
            get
            {
                CreateDTO();
                return _dataToSave;
            }
        }

        public void OnSaved(bool _) => _dataToSave = null;

        public void OnLoad()
        {
            if (_dataToSave == null)
            {
                return;
            }

            SetSoundVolume(_dataToSave.soundVolume);
            SetMusicVolume(_dataToSave.musicVolume);
            SetScreenFreq(_dataToSave.freqMode);
            _dataToSave = null;
        }

        public SettingsManager()
        {
            Application.quitting += OnQuitting;
        }

        public void Dispose()
        {
            Application.quitting -= OnQuitting;
        }

        public void Init(SoundsManager soundsManager, MusicManager musicManager, SavesManager savesManager, GlobalConfig config, ScreenFreqConfig freqConfig)
        {
            _soundsManager = soundsManager;
            _musicManager = musicManager;
            _savesManager = savesManager;
            _config = config;
            _freqConfig = freqConfig;
            ExceptionUtilities.ThrowIfNull(_soundsManager, string.Format(LogStr.CRITICAL_NULL_REFERENCE, "SettingsManager", "SoundsManager"));
            ExceptionUtilities.ThrowIfNull(_musicManager, string.Format(LogStr.CRITICAL_NULL_REFERENCE, "SettingsManager", "MusicManager"));
            ExceptionUtilities.ThrowIfNull(_savesManager, string.Format(LogStr.CRITICAL_NULL_REFERENCE, "SettingsManager", "SavesManager"));
            ExceptionUtilities.ThrowIfNull(_config, string.Format(LogStr.CRITICAL_NULL_REFERENCE, "SettingsManager", "GlobalConfig"));
            ExceptionUtilities.ThrowIfNull(_freqConfig, string.Format(LogStr.CRITICAL_NULL_REFERENCE, "SettingsManager", "ScreenConfig"));

            resolutions = Screen.resolutions;
            _filteredResolutions = new List<Resolution>();

            float currentRefreshRate = (float)Screen.currentResolution.refreshRateRatio.value;

            for (int i = 0; i < resolutions.Length; i++)
            {
                float refreshRate = (float)resolutions[i].refreshRateRatio.value;

                if (refreshRate == currentRefreshRate || float.IsNaN(refreshRate))
                {
                    _filteredResolutions.Add(resolutions[i]);
                }
            }

            SetMusicVolume(_musicVolume);
            _ = SetMixerOnLoad();
            SetScreenFreq(STD_FREQ_MODE);
        }

        public void SetIsFullscreen(bool isFullscreen)
        { 
            _isFullscreen = isFullscreen;
        }

        public bool IsFullscreen()
        {
            return _isFullscreen; 
        }

        public void SetCurrentResolutionIndex(int id)
        {
            _currentResolutionIndex = id;    
        }

        public void SetScreenFreq(string freqModeName)
        {
            int targetFPS = -1;
            int vSync = 1;

            ScreenFreqConfig.Mode mode = _freqConfig.GetMode(freqModeName);

            if (mode != null)
            {
                CurrentFreqMode = mode.Name;
                targetFPS = mode.Freq;
                vSync = mode.VSync;
            }
            else
            {
                CurrentFreqMode = STD_FREQ_MODE;
            }

            if (targetFPS == 0 && vSync == 0)
            {
                targetFPS = (int)Screen.currentResolution.refreshRateRatio.value;
            }

            QualitySettings.vSyncCount = vSync;
            Application.targetFrameRate = targetFPS;
        }

        public List<Resolution> GetPossibleResolutions()
        { 
            return _filteredResolutions;
        }

        public List<string> GetPossibleScreenFreqs()
        {
            List<string> freqs = new();

            foreach (ScreenFreqConfig.Mode mode in _freqConfig)
            {
                freqs.Add(mode.Name);
            }

            return freqs;
        }

        public int GetChosenResolutionIndex()
        { 
            return _currentResolutionIndex;
        }

        public void SetSoundVolume(float val)
        {
            _soundVolume = val;
            float minMasterVolume = SFX_MASTER_MIN_MIN - SFX_MASTER_MIN_MAX_DELTA * (1f - Mathf.Pow(_soundVolume, SFX_MASTER_POW_FACTOR));
            float masterVolume = Mathf.Lerp(minMasterVolume, 0f, _soundVolume);
            _soundsManager.GetMixer().audioMixer.SetFloat("MasterVolume", masterVolume);
        }

        public float GetSoundVolume()
        { 
            return _soundVolume;
        }

        public void SetMusicVolume(float val)
        {
            _musicVolume = val;
            _musicManager.SetVolume(val);
        }

        public float GetMusicVolume()
        {
            return _musicVolume;
        }

        private async Awaitable SetMixerOnLoad()
        {
            await Awaitable.NextFrameAsync();

            if (_soundsManager != null)
            {
                SetSoundVolume(_soundVolume);
            }
        }

        private void CreateDTO()
        {
            if (_dataToSave == null)
            {
                _dataToSave = new();
            }

            _dataToSave.musicVolume = GetMusicVolume();
            _dataToSave.soundVolume = GetSoundVolume();
            _dataToSave.freqMode = CurrentFreqMode;
        }

        private void OnQuitting()
        {
            Application.quitting -= OnQuitting;

            if (_savesManager != null)
            {
                _savesManager.AddToSave(_config.GameSettingsName, this);
            }
        }
    }
}
