// ===== Enemies/Types/PatrolEnemy.cs =====
using UnityEngine;
using System.Collections.Generic;
using Enemies.Core;

namespace Enemies.Types
{
    /// <summary>
    /// Враг, который патрулирует территорию и атакует игрока при обнаружении
    /// </summary>
    public class PatrolEnemy : EnemyBase
    {
        [Header("Patrol Settings")]
        [SerializeField] private PatrolType patrolType = PatrolType.Waypoints;
        [SerializeField] private Transform[] patrolPoints;
        [SerializeField] private float patrolRadius = 5f;
        [SerializeField] private float waitTimeAtPoint = 2f;
        [SerializeField] private float chaseSpeed = 3f;

        [Header("AI Settings")]
        [SerializeField] private float stoppingDistance = 0.5f;
        [SerializeField] private float returnToPatrolDelay = 3f;

        // Состояния AI
        private enum AIState
        {
            Idle,
            Patrolling,
            Chasing,
            Attacking,
            Returning
        }

        private AIState currentState = AIState.Idle;
        private int currentPatrolIndex = 0;
        private Vector2 currentPatrolTarget;
        private float waitTimer = 0f;
        private float lostTargetTimer = 0f;
        private Vector2 lastPatrolPosition;

        // Патрулирование
        public enum PatrolType
        {
            Waypoints,      // Точки патрулирования
            Random,         // Случайные точки в радиусе
            BackAndForth    // Туда-сюда между точками
        }

        protected override void InitializeEnemy()
        {
            // Инициализация патрулирования
            if (patrolType == PatrolType.Waypoints && (patrolPoints == null || patrolPoints.Length == 0))
            {
                Debug.LogWarning($"[PatrolEnemy] No patrol points set for {gameObject.name}. Switching to Random patrol.");
                patrolType = PatrolType.Random;
            }

            // Начинаем с патрулирования
            SetState(AIState.Patrolling);
            SelectNextPatrolPoint();

            // Подписываемся на события
            OnTargetDetected += HandleTargetDetected;
            OnTargetLost += HandleTargetLost;
        }

        protected override void UpdateBehavior()
        {
            // Обновляем состояние AI
            switch (currentState)
            {
                case AIState.Idle:
                    HandleIdleState();
                    break;

                case AIState.Patrolling:
                    HandlePatrolState();
                    break;

                case AIState.Chasing:
                    HandleChaseState();
                    break;

                case AIState.Attacking:
                    HandleAttackState();
                    break;

                case AIState.Returning:
                    HandleReturningState();
                    break;
            }
        }

        protected override void HandleMovement()
        {
            if (currentState == AIState.Idle || currentState == AIState.Attacking)
            {
                rb.velocity = Vector2.zero;
                return;
            }

            Vector2 targetPosition = Vector2.zero;
            float speed = moveSpeed;

            switch (currentState)
            {
                case AIState.Patrolling:
                case AIState.Returning:
                    targetPosition = currentPatrolTarget;
                    speed = moveSpeed;
                    break;

                case AIState.Chasing:
                    if (target != null)
                    {
                        targetPosition = target.position;
                        speed = chaseSpeed;
                    }
                    break;
            }

            // Движение к цели
            Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
            rb.velocity = direction * speed;

            // Поворот спрайта
            if (spriteRenderer != null && Mathf.Abs(rb.velocity.x) > 0.1f)
            {
                spriteRenderer.flipX = rb.velocity.x < 0;
            }
        }

        #region State Handlers

        private void HandleIdleState()
        {
            waitTimer -= Time.deltaTime;

            if (waitTimer <= 0)
            {
                SetState(AIState.Patrolling);
                SelectNextPatrolPoint();
            }
        }

        private void HandlePatrolState()
        {
            float distanceToPoint = Vector2.Distance(transform.position, currentPatrolTarget);

            if (distanceToPoint < stoppingDistance)
            {
                // Достигли точки патрулирования
                SetState(AIState.Idle);
                waitTimer = waitTimeAtPoint;
            }
        }

        private void HandleChaseState()
        {
            if (target == null)
            {
                SetState(AIState.Returning);
                return;
            }

            float distanceToTarget = Vector2.Distance(transform.position, target.position);

            // Проверяем дистанцию для атаки
            if (distanceToTarget <= attackRange)
            {
                SetState(AIState.Attacking);
                AttackTarget();
            }

            // Проверяем видимость цели
            if (!HasLineOfSight(target))
            {
                lostTargetTimer += Time.deltaTime;

                if (lostTargetTimer > returnToPatrolDelay)
                {
                    target = null;
                    SetState(AIState.Returning);
                }
            }
            else
            {
                lostTargetTimer = 0f;
            }
        }

        private void HandleAttackState()
        {
            if (target == null)
            {
                SetState(AIState.Returning);
                return;
            }

            float distanceToTarget = Vector2.Distance(transform.position, target.position);

            // Поворачиваемся к цели
            if (spriteRenderer != null)
            {
                spriteRenderer.flipX = target.position.x < transform.position.x;
            }

            // Продолжаем атаковать или возвращаемся к преследованию
            if (distanceToTarget > attackRange * 1.2f)
            {
                SetState(AIState.Chasing);
            }
            else if (!isAttacking)
            {
                AttackTarget();
            }
        }

        private void HandleReturningState()
        {
            float distanceToPatrol = Vector2.Distance(transform.position, lastPatrolPosition);

            if (distanceToPatrol < stoppingDistance)
            {
                SetState(AIState.Patrolling);
                SelectNextPatrolPoint();
            }
        }

        #endregion

        #region State Management

        private void SetState(AIState newState)
        {
            // Сохраняем последнюю позицию патрулирования
            if (currentState == AIState.Patrolling && newState != AIState.Patrolling)
            {
                lastPatrolPosition = transform.position;
            }

            currentState = newState;

            // Сброс таймеров
            if (newState == AIState.Chasing)
            {
                lostTargetTimer = 0f;
            }
        }

        #endregion

        #region Patrol Logic

        private void SelectNextPatrolPoint()
        {
            switch (patrolType)
            {
                case PatrolType.Waypoints:
                    SelectWaypointTarget();
                    break;

                case PatrolType.Random:
                    SelectRandomTarget();
                    break;

                case PatrolType.BackAndForth:
                    SelectBackAndForthTarget();
                    break;
            }
        }

        private void SelectWaypointTarget()
        {
            if (patrolPoints.Length == 0) return;

            currentPatrolTarget = patrolPoints[currentPatrolIndex].position;
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
        }

        private void SelectRandomTarget()
        {
            Vector2 randomDirection = Random.insideUnitCircle * patrolRadius;
            currentPatrolTarget = (Vector2)transform.position + randomDirection;

            // Проверяем, что точка достижима
            RaycastHit2D hit = Physics2D.Raycast(transform.position, randomDirection, randomDirection.magnitude, obstacleLayer);
            if (hit.collider != null)
            {
                currentPatrolTarget = hit.point - randomDirection.normalized * 0.5f;
            }
        }

        private void SelectBackAndForthTarget()
        {
            if (patrolPoints.Length < 2) return;

            // Простая логика туда-сюда между первыми двумя точками
            currentPatrolTarget = patrolPoints[currentPatrolIndex].position;
            currentPatrolIndex = currentPatrolIndex == 0 ? 1 : 0;
        }

        #endregion

        #region Event Handlers

        private void HandleTargetDetected(Transform detectedTarget)
        {
            if (currentState != AIState.Attacking)
            {
                SetState(AIState.Chasing);
            }
        }

        private void HandleTargetLost()
        {
            if (currentState == AIState.Chasing || currentState == AIState.Attacking)
            {
                SetState(AIState.Returning);
                currentPatrolTarget = lastPatrolPosition;
            }
        }

        #endregion

        #region Debug

        protected override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();

            // Рисуем точки патрулирования
            if (patrolType == PatrolType.Waypoints && patrolPoints != null)
            {
                Gizmos.color = Color.blue;
                for (int i = 0; i < patrolPoints.Length; i++)
                {
                    if (patrolPoints[i] != null)
                    {
                        Gizmos.DrawWireSphere(patrolPoints[i].position, 0.3f);

                        if (i < patrolPoints.Length - 1 && patrolPoints[i + 1] != null)
                        {
                            Gizmos.DrawLine(patrolPoints[i].position, patrolPoints[i + 1].position);
                        }
                    }
                }

                // Линия к последней точке
                if (patrolPoints.Length > 1 && patrolType == PatrolType.Waypoints)
                {
                    Gizmos.DrawLine(patrolPoints[patrolPoints.Length - 1].position, patrolPoints[0].position);
                }
            }
            else if (patrolType == PatrolType.Random)
            {
                // Радиус случайного патрулирования
                Gizmos.color = new Color(0, 0, 1, 0.3f);
                Gizmos.DrawWireSphere(transform.position, patrolRadius);
            }

            // Текущая цель патрулирования
            if (Application.isPlaying)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(transform.position, currentPatrolTarget);
                Gizmos.DrawWireSphere(currentPatrolTarget, 0.5f);
            }
        }

        #endregion
    }
}