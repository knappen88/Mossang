using UnityEngine;

namespace Audio
{
    /// <summary>
    /// Простой проигрыватель фоновой музыки
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class SimpleMusicPlayer : MonoBehaviour
    {
        [Header("Music")]
        [SerializeField] private AudioClip musicClip;

        [Header("Settings")]
        [Range(0f, 1f)]
        [SerializeField] private float volume = 0.3f;
        [SerializeField] private bool playOnAwake = true;
        [SerializeField] private bool loop = true;

        private AudioSource audioSource;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();

            // Настройка AudioSource
            audioSource.clip = musicClip;
            audioSource.volume = volume;
            audioSource.loop = loop;
            audioSource.playOnAwake = playOnAwake;

            // Убеждаемся что это 2D звук (не 3D)
            audioSource.spatialBlend = 0f;

            if (playOnAwake && musicClip != null)
            {
                audioSource.Play();
            }
        }

        // Публичные методы для управления
        public void PlayMusic()
        {
            if (audioSource != null && musicClip != null)
                audioSource.Play();
        }

        public void StopMusic()
        {
            if (audioSource != null)
                audioSource.Stop();
        }

        public void SetVolume(float newVolume)
        {
            volume = Mathf.Clamp01(newVolume);
            if (audioSource != null)
                audioSource.volume = volume;
        }
    }
}