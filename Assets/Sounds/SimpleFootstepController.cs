using UnityEngine;

namespace Player.Audio
{
    [RequireComponent(typeof(AudioSource))]
    public class SimpleFootstepController : MonoBehaviour
    {
        [Header("Footstep Sound")]
        [SerializeField] private AudioClip footstepSound;

        [Header("Settings")]
        [SerializeField] private float stepInterval = 0.4f; // Время между шагами
        [SerializeField] private float baseVolume = 0.7f;
        [Range(0.8f, 1.2f)]
        [SerializeField] private float pitchMin = 0.9f;
        [Range(0.8f, 1.2f)]
        [SerializeField] private float pitchMax = 1.1f;

        [Header("References")]
        [SerializeField] private PlayerMovement playerMovement;

        private AudioSource audioSource;
        private float stepTimer;
        private bool wasMoving = false;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            audioSource.volume = baseVolume;

            // Находим PlayerMovement если не назначен
            if (playerMovement == null)
            {
                playerMovement = GetComponentInParent<PlayerMovement>();
                if (playerMovement == null)
                {
                    playerMovement = transform.parent?.GetComponent<PlayerMovement>();
                }
            }
        }

        private void Update()
        {
            if (playerMovement == null || footstepSound == null) return;

            // Получаем скорость движения
            Rigidbody2D rb = playerMovement.GetComponent<Rigidbody2D>();
            if (rb == null) return;

            bool isMoving = rb.velocity.magnitude > 0.1f;

            // Проверяем начало движения
            if (isMoving && !wasMoving)
            {
                // Сразу проигрываем звук при начале движения
                PlayFootstep();
                stepTimer = stepInterval;
            }

            // Если двигаемся
            if (isMoving)
            {
                stepTimer -= Time.deltaTime;

                if (stepTimer <= 0f)
                {
                    PlayFootstep();

                    // Интервал зависит от скорости движения
                    float speedFactor = Mathf.Clamp(rb.velocity.magnitude / 3f, 0.5f, 2f);
                    stepTimer = stepInterval / speedFactor;
                }
            }
            else
            {
                // Сброс таймера когда стоим
                stepTimer = 0f;
            }

            wasMoving = isMoving;
        }

        private void PlayFootstep()
        {
            if (footstepSound == null || audioSource == null) return;

            // Рандомный pitch для вариативности
            audioSource.pitch = Random.Range(pitchMin, pitchMax);

            // Проигрываем звук
            audioSource.PlayOneShot(footstepSound, baseVolume);
        }

        // Метод для Animation Events (если будешь использовать)
        public void OnFootstep()
        {
            PlayFootstep();
        }

        // Публичные методы для настройки
        public void SetStepInterval(float interval)
        {
            stepInterval = Mathf.Max(0.1f, interval);
        }

        public void SetVolume(float volume)
        {
            baseVolume = Mathf.Clamp01(volume);
            if (audioSource != null)
                audioSource.volume = baseVolume;
        }
    }
}