using System.Collections;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
public class PlayerController : MonoBehaviour, IRestartGameElement
{
    public enum TPunchType
    {
        RIGHT_HAND=0,
        LEFT_HAND,
        KICK
    }
    public Camera m_Camera;
    CharacterController m_CharacterController;
    Animator m_Animator;
    Vector3 m_StartPosition;
    Quaternion m_StartRotation;
    public float m_RunSpeed;
    public float m_WalkSpeed;
    float m_VerticalSpeed = 0.0f;
    public Transform m_LookAt;
    [Range(0.0f, 1.0f)] public float m_RotationLerpPct = 0.8f;
    public float m_DampTime = 0.2f;
    Checkpoint m_CurrentCheckpoint;

    [Header("UI")]
   
    public int m_maxLife;
    public int coins = 0;


    [Header("Jump")]
    public KeyCode m_Keyjump= KeyCode.Space;
    public float m_jumpSpeed=6.0f;
    public float m_DoubleJumpSpeed=8.0f;
    public float m_TripleJumpSpeed=10.0f;
    public float m_KilljumpSpeed = 4.0f;
    public float m_MaxAngleToKillGoomba = 30.0f;
    public float m_MaxTimeToComboJump = 0.5f; 
    int m_CurrentJumpId = 0;
    float m_LastJumpTime;

    [Header ("Punch")]
    public float m_MaxTimeToComboPunch=0.8f;
    int m_CurrentPunchId;
    float m_LastPunchTime;
    public GameObject m_RightHandPunchCollider;
    public GameObject m_LeftHandPunchCollider;
    public GameObject m_KickCollider;

    [Header ("Elevator")]
    public float m_MaxAngleToAttachElevator = 30.0f;
    Collider m_ElevatorCollider;

    [Header ("Input")]
    public int m_PunchMouseButton=0;

    
    [Header("Bridge")]
    public float m_BridgeHitForce = 10.0f;

    [Header("Audio")]
    public AudioSource m_footRightStepAudio;
    public AudioSource m_footLeftStepAudio;
    
    [Header("GoombaHit")]
    public float m_TimeHit = 1.0f;
    bool m_HitRecived = false;

    CoinsController m_CoinsController=new CoinsController();
    LifeController m_LifeController=new LifeController();


    private void Awake()
    {
        m_CharacterController=GetComponent<CharacterController>();
        m_Animator=GetComponent<Animator>();
    }
    void Start()
    {
        m_LastPunchTime=-m_MaxTimeToComboPunch;
        m_RightHandPunchCollider.SetActive(false);
        m_LeftHandPunchCollider.SetActive(false);
        m_KickCollider.SetActive(false);
        m_StartPosition=transform.position;
        m_StartRotation=transform.rotation;
        //GameManager.GetGameManager().AddRestartGameElement(this);
    }
    void Update()
    {

        m_TimeHit += Time.deltaTime;
        Vector3 l_Right = m_Camera.transform.right;
        Vector3 l_Forward = m_Camera.transform.forward;
        Vector3 l_Movement = Vector3.zero;

        l_Right.y = 0;
        l_Right.Normalize();
        l_Forward.y = 0;
        l_Forward.Normalize();

        if (Input.GetKey(KeyCode.D))
            l_Movement = l_Right;
        else if (Input.GetKey(KeyCode.A))
            l_Movement = -l_Right;
        if (Input.GetKey(KeyCode.W))
            l_Movement += l_Forward;
        else if (Input.GetKey(KeyCode.S))
            l_Movement -= l_Forward;

        l_Movement.Normalize();

        float l_SpeedAnimatorValue = 0.5f;
        float l_Speed = m_WalkSpeed;
        if (Input.GetKey(KeyCode.LeftShift))
        {
            l_Speed = m_RunSpeed;
            l_SpeedAnimatorValue = 1.0f;
        }
        if (l_Movement.sqrMagnitude == 0.0f)
            m_Animator.SetFloat("Speed", 0.0f, m_DampTime, Time.deltaTime);
        else
        {
            m_Animator.SetFloat("Speed", l_SpeedAnimatorValue, m_DampTime, Time.deltaTime);
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(l_Movement), m_RotationLerpPct);
        }

        bool l_IsGrounded = m_CharacterController.isGrounded;
        if (l_IsGrounded)
        {
            float l_DiffJumpTime = Time.time - m_LastJumpTime;
            if (m_CurrentJumpId > 0 && l_DiffJumpTime >= m_MaxTimeToComboJump)
            {
                m_CurrentJumpId = 0;
                m_Animator.SetInteger("JumpId", m_CurrentJumpId);
            }
        }
        if (Input.GetKey(m_Keyjump))
        {
            if (CanJump())
                Jump();
        }

        l_Movement *= l_Speed * Time.deltaTime;
        m_VerticalSpeed += Physics.gravity.y * Time.deltaTime;

        bool l_IsFalling = !m_CharacterController.isGrounded && m_VerticalSpeed < 0.0f;
        m_Animator.SetBool("IsFalling", l_IsFalling);
        m_Animator.SetBool("IsGrounded", l_IsGrounded);

        l_Movement.y = m_VerticalSpeed * Time.deltaTime;
        CollisionFlags l_CollisionFlags = m_CharacterController.Move(l_Movement);
        if ((l_CollisionFlags & CollisionFlags.CollidedBelow) != 0 && m_VerticalSpeed < 0.0f)
            if (m_VerticalSpeed < 0)
                m_VerticalSpeed = -2.0f;
            else if ((l_CollisionFlags & CollisionFlags.CollidedAbove) != 0 && m_VerticalSpeed > 0.0f)
                m_VerticalSpeed = 0.0f;

        UpdatePunch();
        UpdateTimeHit(m_HitRecived);
        if (m_LifeController.m_Life <= 0)
        {
            Kill();
        }
    }
    private void LateUpdate()
    {
        UpdateElevator();
    }
 
    void UpdatePunch()
    {
        if(CanPunch() && Input.GetMouseButtonDown(m_PunchMouseButton))
            Punch();
    }
    bool CanPunch()
    {
        return !m_Animator.IsInTransition(0) && m_Animator.GetCurrentAnimatorStateInfo(0).shortNameHash==Animator.StringToHash("Movement");
    }

   
    void Punch()
    {
        float l_DiffPunchTime=Time.time-m_LastPunchTime;
        if(l_DiffPunchTime<m_MaxTimeToComboPunch)
            m_CurrentPunchId=(m_CurrentPunchId+1)%3;
        else
            m_CurrentPunchId=0;
        m_LastPunchTime=Time.time;
        m_Animator.SetTrigger("Punch");
        m_Animator.SetInteger("PunchId", m_CurrentPunchId);
    }
    public void SetActivePunch(TPunchType PunchType, bool Active)
    {
        if(PunchType==TPunchType.RIGHT_HAND)
            m_RightHandPunchCollider.SetActive(Active); 
        else if(PunchType==TPunchType.LEFT_HAND)
            m_LeftHandPunchCollider.SetActive(Active); 
        else if(PunchType==TPunchType.KICK)
            m_KickCollider.SetActive(Active);
    }
   public void RestartGame()
    {
        if(m_CurrentCheckpoint!=null)
        {
            m_StartPosition=m_CurrentCheckpoint.m_RestartPosition.position;
            m_StartRotation=m_CurrentCheckpoint.m_RestartPosition.rotation;
            
        }
        m_CharacterController.enabled = false;
        transform.position=m_StartPosition;
        transform.rotation=m_StartRotation;
        m_CharacterController.enabled = true;
    }
    bool CanKillWithFeet(ControllerColliderHit hit)
    {
        float l_Dot = Vector3.Dot(hit.normal, Vector3.up);
        return m_VerticalSpeed < 0.0f && l_Dot > Mathf.Cos(m_MaxAngleToKillGoomba * Mathf.Deg2Rad);
    }
    bool CanJump()
    {
        return m_CharacterController.isGrounded;    
    }

    void Jump()
    {
        float l_DiffJumpTime=Time.time-m_LastJumpTime;
        if(l_DiffJumpTime<m_MaxTimeToComboJump && m_CurrentJumpId < 2)
            m_CurrentJumpId=m_CurrentJumpId+1;
        else
            m_CurrentJumpId=0;
        m_LastJumpTime=Time.time;
        m_Animator.SetTrigger("Jump");
        m_Animator.SetInteger("JumpId", m_CurrentJumpId);  

        if(m_CurrentJumpId==0)
            m_VerticalSpeed = m_jumpSpeed;
        else if(m_CurrentJumpId==1)
            m_VerticalSpeed = m_DoubleJumpSpeed;
        else if(m_CurrentJumpId==2)
            m_VerticalSpeed = m_TripleJumpSpeed;
    }
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.collider.CompareTag("Goomba"))
        {
            GoombaEnemy goomba = hit.collider.GetComponent<GoombaEnemy>();

            // Si lo matas cayendo encima
            if (CanKillWithFeet(hit))
            {
                goomba.Kill();
                JumpOverEnemy();
            }
            Debug.DrawRay(hit.point, hit.normal, Color.red, 5.0f);

        }

        else if(hit.collider.CompareTag("Bridge"))
        {
            hit.rigidbody.AddForceAtPosition(-hit.normal*m_BridgeHitForce, hit.point);
        }
        else if(hit.collider.CompareTag("Lava"))
        {
            m_LifeController.AddLife(-8);            
        }

        if(!m_CharacterController.isGrounded && hit.normal.y < 0.1f)
        {
            
        }
    }
    void JumpOverEnemy()
    {
        m_VerticalSpeed = m_KilljumpSpeed;

    }

    public void Step(AnimationEvent _AnimEvent)
    {
        if (m_Animator.GetFloat("Speed") < 0.1f)
            return;

        AudioSource l_CurrentAudioSource = null;

        if (_AnimEvent.stringParameter == "Left")
        {
            l_CurrentAudioSource = m_footLeftStepAudio;
        }
        else if (_AnimEvent.stringParameter == "Right")
        {
            l_CurrentAudioSource = m_footRightStepAudio;
        }

        AudioClip l_AudioClip = (AudioClip)_AnimEvent.objectReferenceParameter;
        l_CurrentAudioSource.clip = l_AudioClip;
        l_CurrentAudioSource.Play();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Goomba"))
        {
            GoombaEnemy goomba = other.GetComponent<GoombaEnemy>();
            if (goomba.m_state == GoombaEnemy.TStates.ATTACK ||
                    goomba.m_state == GoombaEnemy.TStates.PATROL)
            {
                if (m_TimeHit >= 1.0f)
                {
                    Vector3 l_goombaDirection = goomba.transform.forward;
                    l_goombaDirection.y = 0;
                    //Debug.Log("Golpe");
                    Hit();
                    m_TimeHit = 0;
                    StartCoroutine(PushByGoomba(l_goombaDirection, goomba.m_pushForce));
                }
            }
        }
        if (other.CompareTag("Elevator"))
        {
            if (CanAttachElevator(other))
            {
                AttachElevator(other);
            }
        }
        else if (other.CompareTag("Checkpoint"))
        {
            m_CurrentCheckpoint = other.GetComponent<Checkpoint>();
        }
        else if (other.CompareTag("Item"))
        {
            Item l_Item = other.GetComponent<Item>();
            if (l_Item.CanPick())
            {
                l_Item.Pick();
            }
        }
    }
    IEnumerator PushByGoomba(Vector3 directiontoPush, float force)
    {
        float duration = 0.5f;
        float timer = 0;
        while (timer < duration)
        {
            m_CharacterController.Move(directiontoPush * force * Time.deltaTime);
            timer += Time.deltaTime;
            yield return null;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Elevator"))
        {
            DetachElevator();
        }
    }
 
    bool CanAttachElevator(Collider ElevatorCollider) 
    {
        return Vector3.Dot(ElevatorCollider.transform.up, Vector3.up)> Mathf.Cos(m_MaxAngleToAttachElevator*Mathf.Deg2Rad);
    }
    void AttachElevator(Collider ElevatorCollider)
    {
        transform.SetParent(ElevatorCollider.transform.parent);
        m_ElevatorCollider = ElevatorCollider;
    }
    void DetachElevator()
    {
        transform.SetParent(null);
        UpdateUpElevator();
        m_ElevatorCollider = null;
    }
    void UpdateUpElevator()
    {
        Vector3 l_direction = transform.forward;
        l_direction.y = 0.0f;
        l_direction.Normalize();
        transform.rotation=Quaternion.LookRotation(l_direction,Vector3.up);
    }
    void UpdateElevator()
    {
        if(m_ElevatorCollider!=null)
            UpdateUpElevator();
    }
    public void AddCoin()
    {
        m_CoinsController.AddCoins(1);
    }
    
    public void Hit()
    {
        //Debug.Log("Quita vida");
        m_LifeController.AddLife(-1);
    }
    public void Kill()
    {
        //GameManager.GetGameManager().m_fade.FadeIn(() =>
        //{
        GameManager.GetGameManager().GameOver();
        //});
    }
    public void UpdateTimeHit(bool Hit)
    {
        if(Hit == true)
        {
            m_TimeHit = 0;
          
        }

        m_TimeHit += Time.deltaTime;

        if(m_TimeHit > 2.0f)
        {
            m_TimeHit = 2.0f;
        }

    }


}
