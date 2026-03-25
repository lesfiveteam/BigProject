using Assets.Project.Scripts.Managers.SceneLoader;
using BigProject.Settings;
using BigProject.Systems;
using BigProject.Utilities;
using System;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace BigProject.Managers.CutsceneManager
{
    public class CutsceneManager : IDisposable
    {
        private PlayableDirector _director;
        private SceneLoadManager _sceneLoader;
        private CutscenesConfig _config;
        private Dictionary<string, CutsceneActor> _actors = new();
        private GameObject _cutscenePrefabs;

        public bool IsPlaying { get; private set; } = false;

        public CutsceneManager(PlayableDirector director, SceneLoadManager sceneLoader, CutscenesConfig config)
        {
            _director = director;
            _sceneLoader = sceneLoader;
            _config = config;
            ExceptionUtilities.ThrowIfNull(_director, string.Format(LogStr.CRITICAL_NULL_REFERENCE, "CutsceneManager", "PlayableDirector"));
            ExceptionUtilities.ThrowIfNull(_sceneLoader, string.Format(LogStr.CRITICAL_NULL_REFERENCE, "CutsceneManager", "SceneLoadManager"));
            ExceptionUtilities.ThrowIfNull(_config, string.Format(LogStr.CRITICAL_NULL_REFERENCE, "CutsceneManager", "CutsceneConfig"));
            _director.stopped += Stop;
            _sceneLoader.SceneLoadingStarted += ClearActors;
            _sceneLoader.SceneLoadingCompleted += FindActors;
        }

        public void Play(TimelineAsset timeline)
        {
            ExceptionUtilities.ThrowIfNull(timeline, string.Format(LogStr.CRITICAL_NULL_REFERENCE, "CutsceneManager", "TimelineAsset"));

            if (IsPlaying)
            {
                Debug.LogWarning(string.Format(LogStr.WARNING_SYSTEM, $"CutsceneManager", $"unable to play {timeline.name}, already playing clip"));
                return;
            }

            IsPlaying = true;
            GameLogManager.Info(string.Format(LogStr.INFO_SYSTEM, $"CutsceneManager", $"start playing {timeline.name}"));
            _director.playableAsset = timeline;
            AddCutscenePrefabs(timeline);
            InitTimeline(timeline);
            _director.Play();
        }

        /// <summary>
        /// For interrupt.
        /// </summary>
        public void Stop()
        {
            if (IsPlaying)
            {
                _director.Stop();
            }
        }

        public void Dispose()
        {
            _director.stopped -= Stop;
            _sceneLoader.SceneLoadingStarted -= ClearActors;
            _sceneLoader.SceneLoadingCompleted -= FindActors;
        }

        /// <summary>
        /// Automatically called when scene load complete.
        /// </summary>
        public void FindActors()
        {
            ClearActors();
            GameLogManager.Info(string.Format(LogStr.INFO_SYSTEM, $"CutsceneManager", $"searching new actors..."));
            AddActors(GameObject.FindObjectsByType<CutsceneActor>(FindObjectsSortMode.None));
        }

        private void ClearActors()
        {
            Stop();
            _actors.Clear();
        }

        private void InitTimeline(TimelineAsset timeline)
        {
            GameLogManager.Info(string.Format(LogStr.INFO_SYSTEM, $"CutsceneManager", $"start timeline initializing..."));

            foreach (TrackAsset track in timeline.GetOutputTracks())
            {
                // CinemachineTrack has actors inside.
                if (!_actors.TryGetValue(track.name, out CutsceneActor actor) && track is not CinemachineTrack)
                {
                    Debug.LogWarning(string.Format(LogStr.WARNING_SYSTEM, $"CutsceneManager", $"unable to get actor {track.name}"));
                    continue;
                }

                switch (track)
                {
                    case AnimationTrack:
                        Animator animator = actor.GetComponent<Animator>();
                        ExceptionUtilities.ThrowIfNull(animator, string.Format(LogStr.CRITICAL_NULL_REFERENCE, "CutsceneManager", "Actor AnimationTrack"));
                        _director.SetGenericBinding(track, animator);
                        break;
                    case ActivationTrack:
                        _director.SetGenericBinding(track, actor.gameObject);
                        break;
                    case CinemachineTrack cinemachineTrack:
                        CinemachineBrain brain = Camera.main.GetComponent<CinemachineBrain>();
                        ExceptionUtilities.ThrowIfNull(brain, string.Format(LogStr.CRITICAL_NULL_REFERENCE, "CutsceneManager", "Actor CinemachineBrain"));
                        _director.SetGenericBinding(track, brain);

                        // Cameras shots inside.
                        InitCameras(cinemachineTrack);
                        break;
                    case AudioTrack:
                        AudioSource audioSource = actor.GetComponent<AudioSource>();
                        ExceptionUtilities.ThrowIfNull(audioSource, string.Format(LogStr.CRITICAL_NULL_REFERENCE, "CutsceneManager", "Actor AudioSource"));
                        _director.SetGenericBinding(track, audioSource);
                        break;
                    case SignalTrack:
                        SignalReceiver signalReceiver = actor.GetComponent<SignalReceiver>();
                        ExceptionUtilities.ThrowIfNull(signalReceiver, string.Format(LogStr.CRITICAL_NULL_REFERENCE, "CutsceneManager", "Actor SignalReceiver"));
                        _director.SetGenericBinding(track, signalReceiver);
                        break;
                    default:
                        Debug.LogWarning(string.Format(LogStr.WARNING_SYSTEM, $"CutsceneManager", $"unknown actor type {track.GetType()}"));
                        break;
                }
            }
        }

        private void InitCameras(CinemachineTrack cinemachineTrack)
        {
            GameLogManager.Info(string.Format(LogStr.INFO_SYSTEM, $"CutsceneManager", $"start {cinemachineTrack.name} cameras initializing..."));

            foreach (TimelineClip clip in cinemachineTrack.GetClips())
            {
                CinemachineShot shot = clip.asset as CinemachineShot;

                if (!_actors.TryGetValue(clip.displayName, out CutsceneActor cameraActor))
                {
                    Debug.LogWarning(string.Format(LogStr.WARNING_SYSTEM, $"CutsceneManager", $"unable to find camera actor {clip.displayName}"));
                    continue;
                }

                CinemachineCamera camera = cameraActor.GetComponent<CinemachineCamera>();
                ExceptionUtilities.ThrowIfNull(camera, string.Format(LogStr.CRITICAL_NULL_REFERENCE, "CutsceneManager", $"camera actor {clip.displayName}"));
                GameLogManager.Info(string.Format(LogStr.INFO_SYSTEM, $"CutsceneManager", $"add {clip.displayName} camera"));
                _director.SetReferenceValue(shot.VirtualCamera.exposedName, camera);

                string targetActorName = cameraActor.Name.Replace(_config.CameraActorPrefix, "");

                if (_actors.TryGetValue(targetActorName, out CutsceneActor targetActor))
                {
                    GameLogManager.Info(string.Format(LogStr.INFO_SYSTEM, $"CutsceneManager", $"add {targetActorName} as camera target"));
                    camera.Follow = targetActor.transform;
                }
                else
                {
                    GameLogManager.Info(string.Format(LogStr.INFO_SYSTEM, $"CutsceneManager", $"no target {targetActorName} for camera"));
                }
            }
        }

        private void AddCutscenePrefabs(TimelineAsset timeline)
        {
            if (_config.TryGetCutscenePrefabs(timeline, out List<GameObject> prefabs))
            {
                _cutscenePrefabs = new GameObject($"Cutscene_{timeline.name}_prefabs");

                foreach (GameObject prefab in prefabs)
                {
                    GameObject.Instantiate(prefab, _cutscenePrefabs.transform);
                }

                AddPrefabsActors();
            }
        }

        private void AddPrefabsActors() => AddActors(_cutscenePrefabs.GetComponentsInChildren<CutsceneActor>());

        private void RemovePrefabsActors() => RemoveActors(_cutscenePrefabs.GetComponentsInChildren<CutsceneActor>());

        private void AddActors(CutsceneActor[] actorsArray)
        {
            if (actorsArray == null)
            {
                return;
            }

            foreach (CutsceneActor actor in actorsArray)
            {
                if (_actors.TryAdd(actor.Name, actor))
                {
                    GameLogManager.Info(string.Format(LogStr.INFO_SYSTEM, $"CutsceneManager", $"add actor {actor.name}"));
                }
                else
                {
                    Debug.LogWarning(string.Format(LogStr.WARNING_SYSTEM, $"CutsceneManager", $"try add duplicate actor {actor.name}"));
                }
            }
        }

        private void RemoveActors(CutsceneActor[] actorsArray)
        {
            if (actorsArray == null)
            {
                return;
            }

            foreach (CutsceneActor actor in actorsArray)
            {
                GameLogManager.Info(string.Format(LogStr.INFO_SYSTEM, $"CutsceneManager", $"remove actor {actor.name}"));
                _actors.Remove(actor.Name);
            }
        }

        private void Stop(PlayableDirector _)
        {
            GameLogManager.Info(string.Format(LogStr.INFO_SYSTEM, $"CutsceneManager", $"stop playing"));
            _director.playableAsset = null;
            RemovePrefabsActors();
            GameObject.Destroy(_cutscenePrefabs);
            _cutscenePrefabs = null;
            IsPlaying = false;
        }
    }
}