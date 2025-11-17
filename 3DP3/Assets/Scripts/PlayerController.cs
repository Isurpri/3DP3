using UnityEngine;
using UnityEngine.Rendering;

public class PlayerController : MonoBehaviour , IRestartGame
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
    public float m_WalkSpeed;
    public float m_RunSpeed;
    float m_VerticalSpeed = 0.0f;
    public Transform m_LookAt;
    [Range(0.0f, 1.0f)] public float m_RotationLerp = 0.1f;
    public float m_DampTime = 0.2f;
    Vector3 m_StartPosition;
    Quaternion m_StartRotation;

    [Header("Punch")]
    public float m_MaxTimeToComboPunch = 0.8f;
    int m_CurrentPunchId;
    float m_LastPunchTime;
    public GameObject m_RightPunchColl;
    public GameObject m_LeftPunchColl;
    public GameObject m_KickColl;

    [Header("Input")] 
    public int m_PunchMouseButton=0;
    private void Awake()
    {
        m_CharacterController = GetComponent<CharacterController>();
        m_Animator = GetComponent<Animator>();
        m_LastPunchTime =-m_MaxTimeToComboPunch;
        
    }

    private void Start()
    {
        m_RightPunchColl.SetActive(false);
        m_LeftPunchColl.SetActive(false);
        m_KickColl.SetActive(false);
        m_StartPosition= transform.position;
        m_StartRotation= transform.rotation;
        //GameManager.getGameManager().AddRestartGameElement(This)
    }

    void Update()
    {
        Vector3 l_Right = m_Camera.transform.right; 
        Vector3 l_Forward = m_Camera.transform.forward;
        Vector3 l_Movment = Vector3.zero;

        l_Right.y = 0;
        l_Right.Normalize();
        l_Forward.y=0;
        l_Forward.Normalize();

        if(Input.GetKey(KeyCode.D))
            l_Movment=l_Right;
        else if(Input.GetKey(KeyCode.A))
            l_Movment=-l_Right;

        if (Input.GetKey(KeyCode.W))
            l_Movment += l_Forward;
        else if (Input.GetKey(KeyCode.S))
            l_Movment -= l_Forward;

        l_Movment.Normalize();

        float l_speedAnimatorValue = 0.5f;
        float l_Speed = m_WalkSpeed;
        if (Input.GetKey(KeyCode.LeftShift))
        {
            l_Speed=m_RunSpeed;
            l_speedAnimatorValue = 1.0f;
        }
        if (l_Movment.sqrMagnitude == 0.0f)
            m_Animator.SetFloat("Speed", 0.0f,m_DampTime,Time.deltaTime);
        else
        {
            m_Animator.SetFloat("Speed", l_speedAnimatorValue, m_DampTime, Time.deltaTime);
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(l_Movment), m_RotationLerp);
        }

        l_Movment*=l_Speed*Time.deltaTime;
        m_VerticalSpeed += Physics.gravity.y * Time.deltaTime;
        l_Movment.y=m_VerticalSpeed*Time.deltaTime;
        CollisionFlags l_CollisionFlag = m_CharacterController.Move(l_Movment);
        if((l_CollisionFlag & CollisionFlags.CollidedBelow)!=0)
            m_VerticalSpeed = 0.0f;
        else if((l_CollisionFlag & CollisionFlags.CollidedAbove)!=0 && m_VerticalSpeed>0.0f)
            m_VerticalSpeed=0.0f;

        UpdatePunch();
    }

    void UpdatePunch()
    {
        if (CanPunch() && Input.GetMouseButtonDown(m_PunchMouseButton))
            Punch();
    }
    bool CanPunch()
    {
        return !m_Animator.IsInTransition(0) && m_Animator.GetCurrentAnimatorStateInfo(0).shortNameHash==Animator.StringToHash("Movement");
    }

    void Punch()
    {
        float l_DiffPunchTime = Time.time-m_LastPunchTime;
        if (l_DiffPunchTime < m_MaxTimeToComboPunch)
        {
            m_CurrentPunchId = (m_CurrentPunchId + 1) % 3;
        }
        else 
        {
            m_CurrentPunchId=0;
        }
        m_LastPunchTime=Time.time;
        m_Animator.SetTrigger("Punch");
        m_Animator.SetInteger("PunchID", m_CurrentPunchId);
    }

    public void SetActivePunch(TPunchType PunchType, bool Active)
    {
        if (PunchType == TPunchType.RIGHT_HAND)
            m_RightPunchColl.SetActive(Active);
        else if(PunchType == TPunchType.LEFT_HAND)
            m_LeftPunchColl.SetActive(Active);
        else if (PunchType == TPunchType.KICK)
            m_KickColl.SetActive(Active);

    }
    public void RestartGame()
    {
        m_CharacterController.enabled = false;
        transform.position = m_StartPosition;
        transform.rotation = m_StartRotation;
        m_CharacterController.enabled = true;
    }
}
