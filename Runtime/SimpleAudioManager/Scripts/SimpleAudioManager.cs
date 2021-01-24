using System.Collections.Generic;
using UnityEngine.Audio;
using UnityEngine;

namespace DB.SimpleFramework.SimpleAudioManager {

    public class SimpleAudioManager : MonoBehaviour {

        private const int DEFAULT_SOURCE_POOL_SIZE = 10;

        private const string AUDIO_MANAGER_NAME = "[Simple AudioManager]";
        private const string AUDIO_POOL_NAME = "[Simple AudioPool]";
        private const string AUDIO_SOURCE_NAME = "[Simple AudioSource]";

        private const string AUDIO_MIXER_NAME = "SimpleAudioMixer";
        private const string AUDIO_MIXER_MASTER_VOLUME_KEY = "MasterVolume";

        private static GameObject audioManager;
        private static GameObject audioPool;

        private static List<SimpleAudioSource> activeSources = new List<SimpleAudioSource>();
        private static Queue<SimpleAudioSource> sourcePool = new Queue<SimpleAudioSource>();

        public static bool IsMuted { get; private set; }
        public static bool IsPaused { get; private set; }

        public static AudioMixerGroup MixerMasterGroup { get; private set; }
        public static float MasterVolume { get; private set; }

        private static AudioMixer mixer;
        private static int sourcePoolSize;
        private static bool usePooling => sourcePoolSize > 0;

        /// <summary>
        /// Manually initialize the SimpleAudioManager, enables the usage of custom parameters.
        /// </summary>
        /// <param name="parent">The parent transform the SimpleAudioManager will child itself to.</param>
        /// <param name="poolSize">Override the default pool size for SimpleAudioSources, set to 0 to disable pooling completely.</param>
        public static void Initialize(Transform parent = null, int poolSize = DEFAULT_SOURCE_POOL_SIZE) {
            if (audioManager) { return; }

            audioManager = new GameObject(AUDIO_MANAGER_NAME);
            audioManager.AddComponent<SimpleAudioManager>();
            audioManager.transform.SetParent(parent);

            mixer = Resources.Load<AudioMixer>(AUDIO_MIXER_NAME);
            MixerMasterGroup = mixer.FindMatchingGroups("Master")[0];
            SetMasterVolume(1f);

            sourcePoolSize = poolSize;
            if (usePooling) {
                audioPool = new GameObject(AUDIO_POOL_NAME);
                audioPool.transform.SetParent(audioManager.transform);
                FillSourcePool(sourcePoolSize);
            }

            DontDestroyOnLoad(audioManager);
        }

        private void Update() {
            HandleActiveSources();
        }

        /// <summary>
        /// Plays an audio clip with custom settings.
        /// </summary>
        /// <returns>Returns the created SimpleAudioSource GameObject.</returns>
        public static SimpleAudioSource Play(SimpleAudioSettings settings, Vector3 position = default) {
            if (!settings.IsValid) { return null; }

            Initialize();

            SimpleAudioSource source = GetNewSource();
            source.name = AUDIO_SOURCE_NAME.Insert(AUDIO_SOURCE_NAME.Length - 1, $" - {settings.Clip.name}");
            source.Initialize(settings, position);

            RegisterSource(source);
            return source;
        }
        
        /// <summary>
        /// Plays a three dimensional audio clip.
        /// </summary>
        /// <returns>Returns the created SimpleAudioSource GameObject.</returns>
        public static SimpleAudioSource Play(AudioClip clip, float volume = 1f, float pitch = 1f, Vector3 position = default) {
            SimpleAudioSettings settings = new SimpleAudioSettings(clip) {
                Volume = volume,
                PitchMin = pitch,
                PitchMax = pitch,
            };
            return Play(settings, position);
        }

        /// <summary>
        /// Plays a two dimensional audio clip.
        /// </summary>
        /// <returns>Returns the created SimpleAudioSource GameObject.</returns>
        public static SimpleAudioSource Play2D(AudioClip clip, float volume = 1f, float pitch = 1f, Vector3 position = default) {
            SimpleAudioSettings settings = new SimpleAudioSettings(clip) {
                Volume = volume,
                PitchMin = pitch,
                PitchMax = pitch,
                SpatialMode = SpatialMode.TwoDimensional,
            };
            return Play(settings, position);
        }

        /// <summary>
        /// Sets the global mute state. Affects all current and new SimpleAudioSources.
        /// </summary>
        public static void SetMuteAll(bool state) {
            IsMuted = state;
            foreach (SimpleAudioSource source in activeSources) {
                source.SetMute(state);
            }
        }

        /// <summary>
        /// Sets the global pause state. Affects all current and new SimpleAudioSources.
        /// </summary>
        public static void SetPauseAll(bool state) {
            IsPaused = state;
            foreach (SimpleAudioSource source in activeSources) {
                source.SetPause(state);
            }
        }

        /// <summary>
        /// Sets the master volume for all SimpleAudioSources.
        /// </summary>
        public static void SetMasterVolume(float volume) {
            float value01 = Mathf.Max(0.0001f, Mathf.Clamp01(volume));
            float valueDb = Mathf.Log10(value01) * 20;
            mixer.SetFloat(AUDIO_MIXER_MASTER_VOLUME_KEY, valueDb);
            MasterVolume = value01;
        }

        private static void FillSourcePool(int amount) {
            for (int i = 0; i < amount; i++) {
                SimpleAudioSource source = new GameObject(AUDIO_SOURCE_NAME).AddComponent<SimpleAudioSource>();
                source.transform.SetParent(audioPool.transform);
                source.gameObject.SetActive(false);
                sourcePool.Enqueue(source);
            }
        }

        private static SimpleAudioSource GetNewSource() {
            SimpleAudioSource source;
            if (!usePooling || sourcePool.Count <= 0) {
                source = new GameObject().AddComponent<SimpleAudioSource>();
            } else {
                source = sourcePool.Dequeue();
                source.gameObject.SetActive(true);
            }
            source.transform.SetParent(audioManager.transform);
            return source;
        }

        private static void RegisterSource(SimpleAudioSource source) {
            activeSources.Add(source);
        }

        private static void UnregisterSource(SimpleAudioSource source) {
            activeSources.Remove(source);
            source.Deinitialize();
            if (usePooling) {
                source.transform.SetParent(audioPool.transform);
                source.gameObject.SetActive(false);
                source.name = AUDIO_SOURCE_NAME;
                sourcePool.Enqueue(source);
            } else {
                Destroy(source.gameObject);
            }
        }

        private void HandleActiveSources() {
            for (int i = activeSources.Count - 1; i >= 0; i--) {
                SimpleAudioSource source = activeSources[i];
                if (!source) {
                    activeSources.Remove(source);
                    continue;
                }

                source.Tick();
                if (source.IsFinished) {
                    UnregisterSource(source);
                }
            }
        }
    }
}
