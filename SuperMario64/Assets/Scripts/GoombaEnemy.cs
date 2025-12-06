using System.Collections;
using System.Collections.Generic;
    using Unity.VisualScripting;
    using UnityEngine;
    using UnityEngine.AI;

    public class GoombaEnemy : MonoBehaviour, IRestartGameElement
    {
        public enum TStates
        {
            PATROL=0,
            ALERT,
            ATTACK,
            HIT,
            DIE
        }
        public TStates m_state;
        private TStates m_previousState;
        NavMeshAgent m_NavMeshAgent;
        public Transform m_target;
        Vector3 m_StartPosition;
        Quaternion m_StartRotation;
        CharacterController m_CharacterController;
        
        [Header("DistanceChase")]
        public float m_MaxDistanceToAttack = 15f;

        [Header("Patrol")]
        public List<Transform> m_PatrolPosition;
        public int m_currentPatrolPos;

        [Header("Sight")]
        public float m_EyesHeight = 0.5f;
        public float m_SightAngle = 60f;
        public LayerMask m_SightLayerMask;

        [Header("Ears")]
        public float m_MaxEarDistance = 3f;

        [Header("Alert")]
        public float m_AlertRotateSpeed = 90f;
        float m_AlertTimer;
        public float m_AlertMaxTime = 3f;


        [Header("Hit")]
        public float m_pushForce = 100.0f;
        private float m_HitDuration = 0.5f;
        private float m_HitTimer = 0f;
        public ParticleSystem m_HitParticles;
        public ParticleSystem m_DieParticles;

        [Header("Dead")]
        public List<MeshRenderer> m_MeshesRend;
        public List<GameObject> m_dropObject;
        public float m_DropChance = 0.9f;

        private void Awake()
        {
            m_NavMeshAgent=GetComponent<NavMeshAgent>();
            m_CharacterController = GetComponent<CharacterController>();
        }
        private void Start()
        {
            GameManager.GetGameManager().AddRestartGameElement(this);
            m_StartPosition = transform.position;
            m_StartRotation = transform.rotation;  
            m_target = GameManager.GetGameManager().GetPlayer().transform;
            SetPatrolState();
        }
        private void Update()
        {
            switch (m_state) 
            {
                case TStates.PATROL:
                    UpdatePatrolState(); 
                    break;
                case TStates.ALERT:
                    UpdateAlertState();
                    break;
                case TStates.ATTACK:
                    UpdateAttackState(); 
                    break;
                case TStates.HIT:
                    UpdateHitState();
                    break;
                case TStates.DIE:
                    UpdateDieState();
                    break;
            }
        }
        void SetPatrolState()
        {
            ChangeState(TStates.PATROL); 
            m_currentPatrolPos = 0;
            MoveToNextPatrolPosition();
        }
        void UpdatePatrolState()
        {
            if (!m_NavMeshAgent.pathPending && m_NavMeshAgent.remainingDistance <= m_NavMeshAgent.stoppingDistance)
            {
                m_NavMeshAgent.isStopped=true;
                MoveToNextPatrolPosition();
                m_NavMeshAgent.isStopped=false;
            }
            if(SeePlayer() || HearsPlayer())
                SetAlertState();
                
        }
        void SetAlertState()
        {
            ChangeState(TStates.ALERT);
            m_AlertTimer=0.0f;
            m_NavMeshAgent.isStopped = true;
            m_NavMeshAgent.ResetPath();

        }
        void UpdateAlertState()
        {
            Vector3 l_PlayerPosition = m_target.position;
            FaceTarget(l_PlayerPosition); 
            
            m_AlertTimer += Time.deltaTime;

            if (SeePlayer())
            {
            SetAttackState();
            }
            else if (m_AlertTimer >= m_AlertMaxTime)
            {
                SetPatrolState();
            }
        }
        void SetAttackState()
        {
            ChangeState(TStates.ATTACK);
            m_NavMeshAgent.isStopped = false;
        }
        void UpdateAttackState()
        {
            if (m_target == null) return;
            Vector3 l_PlayerPosition = m_target.transform.position;
            float l_Distance = Vector3.Distance(transform.position, l_PlayerPosition);

            if (l_Distance > m_MaxDistanceToAttack)
            {
                SetPatrolState();
                return;
            }
            if (!SeePlayer() && !HearsPlayer())
            {
                SetAlertState();
                return;
            }

            if (l_Distance <= m_MaxDistanceToAttack)
            {
                m_NavMeshAgent.isStopped = false; 
                FaceTarget(l_PlayerPosition);     
                m_NavMeshAgent.SetDestination(l_PlayerPosition);
            }

        }
        
        void SetHitState()
        {
            ChangeState(TStates.HIT);
            m_HitTimer = 0.0f;
            m_NavMeshAgent.isStopped = true;
            if (m_HitParticles != null) m_HitParticles.Play();
        }
        void UpdateHitState()
        {
            m_HitTimer += Time.deltaTime;

            if (m_HitTimer >= m_HitDuration)
            {
                SetPreviousState();
            }
        }
        void SetPreviousState()
        {
            ChangeState(m_previousState);
            if (m_state != TStates.ALERT && m_state != TStates.DIE)
            {
                m_NavMeshAgent.isStopped = false;
                if (m_state == TStates.PATROL && m_PatrolPosition != null && m_PatrolPosition.Count > 0)
                {
                    m_NavMeshAgent.SetDestination(m_PatrolPosition[m_currentPatrolPos].position);
                }
            }
        }
    void ChangeState(TStates newState)
        {
            if (m_state != newState)
            {
                m_previousState = m_state;
                m_state = newState;
            }
        }
        void SetDieState()
        {
            ChangeState(TStates.DIE);
            m_NavMeshAgent.isStopped = true;
            m_NavMeshAgent.ResetPath();
        }
        void UpdateDieState()
        {
            Kill();
        }

        void MoveToNextPatrolPosition()
        {
            Vector3 l_Destination=m_PatrolPosition[m_currentPatrolPos].position;
            m_NavMeshAgent.destination = l_Destination;
            ++m_currentPatrolPos;
            if (m_currentPatrolPos>=m_PatrolPosition.Count)
            {
                m_currentPatrolPos = 0;
            }
        }
        void FaceTarget(Vector3 targetPos)
        {
            Vector3 direction = (targetPos - transform.position).normalized;
            direction.y = 0;
            if (direction!=Vector3.zero)
            {
                Quaternion lookPlayer = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookPlayer, Time.deltaTime * 5f);
            }
        }
        
        bool SeePlayer()
        {
            Vector3 l_PlayerPosition = GameManager.GetGameManager().GetPlayer().transform.position;
            Vector3 l_direction = l_PlayerPosition - transform.position;
            float l_Distance = l_direction.magnitude;
            //l_direction.Normalize();
            l_direction/=l_Distance;//Es lo mismo que normalizarlo
            float l_DotValue= Vector3.Dot(l_direction,transform.forward);
            if (l_DotValue>=Mathf.Cos(m_SightAngle*0.5f*Mathf.Deg2Rad))
            {
                Ray l_Ray = new Ray(transform.position+Vector3.up*m_EyesHeight,l_direction);
                //float l_Distance=Vector3.Distance(l_PlayerPosition, transform.position);
                if (Physics.Raycast(l_Ray,l_Distance,m_SightLayerMask.value))
                {
                    return true;
                }
            }
            return false;
        }

        bool HearsPlayer()
        {
            Vector3 l_PlayerPosition = GameManager.GetGameManager().GetPlayer().transform.position;
            float l_Distance = Vector3.Distance(l_PlayerPosition, transform.position);
            return l_Distance < m_MaxEarDistance;
        }
        public void RestartGame()
        {
            m_CharacterController.enabled = false;
            transform.position = m_StartPosition;
            transform.rotation = m_StartRotation;
            m_CharacterController.enabled=true;
            gameObject.SetActive(true);
        }
        public void Kill()
        {
            gameObject.SetActive(false);
        }

    
}
