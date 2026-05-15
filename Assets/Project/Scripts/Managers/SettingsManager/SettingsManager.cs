using BigProject.Managers.SoundsMusicManagers;
using BigProject.Settings;
using BigProject.Systems;
using BigProject.Utilities;
using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;

namespace BigProject.Managers
{
    public class SettingsManager : ISavable, IDisposable
    {
        private SoundsManager _soundsManager;
        private MusicManager _musicManager;
        private SavesManager _savesManager;
        private GlobalConfig _config;

        //Settings
        private Resolution[] resolutions;
        private List<Resolution> _filteredResolutions;
        private int _currentResolutionIndex;
        private bool _isFullscreen = true;
        private float _soundVolume;
        private float _musicVolume;
        private DataToSave _dataToSave;

        [Serializable]
        private class DataToSave
        {
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

        public void Init(SoundsManager soundsManager, MusicManager musicManager, SavesManager savesManager, GlobalConfig config)
        {
            _soundsManager = soundsManager;
            _musicManager = musicManager;
            _savesManager = savesManager;
            _config = config;
            ExceptionUtilities.ThrowIfNull(_soundsManager, string.Format(LogStr.CRITICAL_NULL_REFERENCE, "SettingsManager", "SoundsManager"));
            ExceptionUtilities.ThrowIfNull(_musicManager, string.Format(LogStr.CRITICAL_NULL_REFERENCE, "SettingsManager", "MusicManager"));
            ExceptionUtilities.ThrowIfNull(_savesManager, string.Format(LogStr.CRITICAL_NULL_REFERENCE, "SettingsManager", "SavesManager"));
            ExceptionUtilities.ThrowIfNull(_config, string.Format(LogStr.CRITICAL_NULL_REFERENCE, "SettingsManager", "GlobalConfig"));

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

            _soundsManager.GetMixer().audioMixer.GetFloat("MasterVolume", out _soundVolume);
            _soundVolume = math.remap(-100, 0, 0, 1, _soundVolume);

            _musicVolume = _musicManager.GetAudioSources()[0].volume;
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

        public List<Resolution> GetPossibleResolutions()
        { 
            return _filteredResolutions;
        }

        public int GetChosenResolutionIndex()
        { 
            return _currentResolutionIndex;
        }

        public void SetSoundVolume(float val)
        {
            float _newVal = math.remap(0, 1, -100, 0, val);
            _soundsManager.GetMixer().audioMixer.SetFloat("MasterVolume", _newVal);
            _soundVolume = val;
        }

        public float GetSoundVolume()
        { 
            return _soundVolume;
        }

        public void SetMusicVolume(float val)
        {
            foreach (AudioSource audioSource in _musicManager.GetAudioSources())
                audioSource.volume = val;
            _musicVolume = val;
        }

        public float GetMusicVolume()
        {
            return _musicVolume;
        }

        private void CreateDTO()
        {
            if (_dataToSave == null)
            {
                _dataToSave = new();
            }

            _dataToSave.musicVolume = GetMusicVolume();
            _dataToSave.soundVolume = GetSoundVolume();
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
