using UnityEngine;

namespace Audio
{
    [RequireComponent(typeof(AudioSource))]
    public class BackgroundMusicManager : MonoBehaviour
    {
        [Header("Music Settings")]
        [SerializeField] private AudioClip backgroundMusic;
        [SerializeField] private bool playOnStart = true;
        [SerializeField] private bool loop = true;

        [Header("Volume Settings")]
        [Range(0f, 1f)]
        [SerializeField] private float musicVolume = 0.5f;
        [SerializeField] private float fadeInDuration = 2f;
        [SerializeField] private float fadeOutDuration = 1f;

        private AudioSource audioSource;
        private static BackgroundMusicManager instance;

        private void Awake()
        {
            // Простой синглтон чтобы музыка не прерывалась между сценами
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);

                SetupAudioSource();

                if (playOnStart && backgroundMusic != null)
                {
                    PlayMusic();
                }
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void SetupAudioSource()
        {
            audioSource = GetComponent<AudioSource>();
            audioSource.clip = backgroundMusic;
            audioSource.loop = loop;
            audioSource.playOnAwake = false;
            audioSource.volume = 0f; // Начинаем с 0 для fade in
        }

        public void PlayMusic()
        {
            if (backgroundMusic == null || audioSource == null) return;

            audioSource.clip = backgroundMusic;
            audioSource.Play();

            // Fade in
            StartCoroutine(FadeVolume(0f, musicVolume, fadeInDuration));
        }

        public void StopMusic()
        {
            if (audioSource == null || !audioSource.isPlaying) return;

            // Fade out затем остановить
            StartCoroutine(FadeOutAndStop());
        }

        public void PauseMusic()
        {
            if (audioSource != null)
                audioSource.Pause();
        }

        public void ResumeMusic()
        {
            if (audioSource != null)
                audioSource.UnPause();
        }

        public void SetVolume(float volume)
        {
            musicVolume = Mathf.Clamp01(volume);
            if (audioSource != null)
                audioSource.volume = musicVolume;
        }

        public void ChangeMusic(AudioClip newMusic, bool fadeTransition = true)
        {
            if (newMusic == null) return;

            if (fadeTransition && audioSource.isPlaying)
            {
                StartCoroutine(CrossfadeToNewMusic(newMusic));
            }
            else
            {
                audioSource.clip = newMusic;
                audioSource.Play();
                audioSource.volume = musicVolume;
            }
        }

        private System.Collections.IEnumerator FadeVolume(float from, float to, float duration)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                audioSource.volume = Mathf.Lerp(from, to, t);
                yield return null;
            }

            audioSource.volume = to;
        }

        private System.Collections.IEnumerator FadeOutAndStop()
        {
            yield return FadeVolume(audioSource.volume, 0f, fadeOutDuration);
            audioSource.Stop();
        }

        private System.Collections.IEnumerator CrossfadeToNewMusic(AudioClip newMusic)
        {
            // Fade out текущая музыка
            yield return FadeVolume(audioSource.volume, 0f, fadeOutDuration);

            // Меняем трек
            audioSource.Stop();
            audioSource.clip = newMusic;
            audioSource.Play();

            // Fade in новая музыка
            yield return FadeVolume(0f, musicVolume, fadeInDuration);
        }

        // Статические методы для удобного доступа
        public static void Play() => instance?.PlayMusic();
        public static void Stop() => instance?.StopMusic();
        public static void Pause() => instance?.PauseMusic();
        public static void Resume() => instance?.ResumeMusic();
        public static void SetMusicVolume(float volume) => instance?.SetVolume(volume);
    }
}