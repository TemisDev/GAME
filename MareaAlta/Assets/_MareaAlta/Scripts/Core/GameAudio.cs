using UnityEngine;

namespace MareaAlta.Core
{
    public sealed class GameAudio : MonoBehaviour
    {
        public static GameAudio Instance { get; private set; }

        [Header("Música")]
        [SerializeField] private AudioClip ambientMusic;
        [SerializeField, Range(0f, 1f)] private float musicVolume = 0.3f;

        [Header("Efectos")]
        [SerializeField] private AudioClip pickupClip;
        [SerializeField] private AudioClip energyClip;
        [SerializeField] private AudioClip errorClip;
        [SerializeField] private AudioClip turbineClip;
        [SerializeField] private AudioClip gateClip;
        [SerializeField] private AudioClip victoryClip;
        [SerializeField] private AudioClip uiClip;
        [SerializeField, Range(0f, 1f)] private float effectsVolume = 0.75f;

        private AudioSource musicSource;
        private AudioSource effectsSource;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            AudioSource[] sources = GetComponents<AudioSource>();
            musicSource = sources.Length > 0 ? sources[0] : gameObject.AddComponent<AudioSource>();
            effectsSource = sources.Length > 1 ? sources[1] : gameObject.AddComponent<AudioSource>();

            musicSource.loop = true;
            musicSource.playOnAwake = false;
            musicSource.volume = musicVolume;
            effectsSource.playOnAwake = false;
            effectsSource.volume = effectsVolume;

            if (ambientMusic != null)
            {
                musicSource.clip = ambientMusic;
                musicSource.Play();
            }
        }

        public void Configure(
            AudioClip music,
            AudioClip pickup,
            AudioClip energy,
            AudioClip error,
            AudioClip turbine,
            AudioClip gate,
            AudioClip victory,
            AudioClip ui)
        {
            ambientMusic = music;
            pickupClip = pickup;
            energyClip = energy;
            errorClip = error;
            turbineClip = turbine;
            gateClip = gate;
            victoryClip = victory;
            uiClip = ui;
        }

        public void PlayPickup() => Play(pickupClip, 0.65f);
        public void PlayEnergyCreated() => Play(energyClip);
        public void PlayError() => Play(errorClip, 0.65f);
        public void PlayTurbine() => Play(turbineClip);
        public void PlayGate() => Play(gateClip);
        public void PlayVictory() => Play(victoryClip);
        public void PlayUi() => Play(uiClip, 0.6f);

        private void Play(AudioClip clip, float volumeScale = 1f)
        {
            if (clip != null && effectsSource != null)
                effectsSource.PlayOneShot(clip, volumeScale);
        }
    }
}
