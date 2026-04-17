using BigProject.Managers.SoundsMusicManagers;
using BigProject.Systems;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace BigProject.Managers
{
    public class SettingsManager
    {
        private SoundsManager _soundsManager;

        //Settings
        private Resolution[] resolutions;
        private List<Resolution> _filteredResolutions;
        private int _currentResolutionIndex;
        private bool _isFullscreen = true;
        private float _soundVolume;
        private float _musicVolume;

        public void Init(SoundsManager soundsManager)
        {
            _soundsManager = soundsManager;
            Assert.IsNotNull(_soundsManager, String.Format(LogStr.CRITICAL_NULL_REFERENCE, "Settings Manager", "Sounds Manager"));

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

            _soundVolume = _soundsManager.GetAudioSource3D().volume;
            _musicVolume = _soundsManager.GetAudioSource2D().volume;
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
            _soundsManager.GetAudioSource3D().volume = val;
            _soundVolume = val;
        }

        public float GetSoundVolume()
        { 
            return _soundVolume;
        }

        public void SetMusicVolume(float val)
        {
            _soundsManager.GetAudioSource2D().volume = val;
            _musicVolume = val;
        }

        public float GetMusicVolume()
        {
            return _musicVolume;
        }
    }
}
