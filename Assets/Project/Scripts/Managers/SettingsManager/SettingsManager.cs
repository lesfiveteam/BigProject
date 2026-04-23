using BigProject.Managers.SoundsMusicManagers;
using BigProject.Systems;
using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;

namespace BigProject.Managers
{
    public class SettingsManager
    {
        private SoundsManager _soundsManager;
        private MusicManager _musicManager;

        //Settings
        private Resolution[] resolutions;
        private List<Resolution> _filteredResolutions;
        private int _currentResolutionIndex;
        private bool _isFullscreen = true;
        private float _soundVolume;
        private float _musicVolume;

        public void Init(SoundsManager soundsManager, MusicManager musicManager)
        {
            _soundsManager = soundsManager;
            _musicManager = musicManager;
            Assert.IsNotNull(_soundsManager, String.Format(LogStr.CRITICAL_NULL_REFERENCE, "Settings Manager", "Sounds Manager"));
            Assert.IsNotNull(_musicManager, String.Format(LogStr.CRITICAL_NULL_REFERENCE, "Settings Manager", "Music Manager"));

            resolutions = Screen.resolutions;
            _filteredResolutions = new List<Resolution>();

            float currentRefreshRate = (float)Screen.currentResolution.refreshRateRatio.value;

            for (int i = 0; i < resolutions.Length; i++)
            {
                if ((float)resolutions[i].refreshRateRatio.value == currentRefreshRate)
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
    }
}
