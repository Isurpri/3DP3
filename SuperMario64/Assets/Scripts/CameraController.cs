using System;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public PlayerController m_Player;

    float m_Yaw = 0.0f;
    float m_Pitch = 0.0f;
    public float m_YawSpeed = 360.0f;
    public float m_PitchSpeed = 180.0f;
    public float m_MinPitch = -60.0f;
    public float m_MaxPitch = 80.0f;
    public float m_MinDistance = 3.0f;
    public float m_MaxDistance = 12.0f;
    public LayerMask m_LayerMask;
    public float m_OffsetDistance = 0.1f;

    [Header("Auto camara")]
    public Transform m_cameraIdlePos;        
    public float m_IdleTime = 5f;        
    public float m_MoveSpeedCamera = 2f;       
    public float m_RotateSpeedCamera = 2f;

    [Header("Idle")]
    public float m_SpecialIdleTime = 10f;    
    bool m_HasPlayedSpecialIdle = false;
    float m_IdleTimer = 0f;
    bool m_IsAutoMoving = false;

    Vector3 m_CurrentLookAt;

    private void Start()
    {
        m_Yaw = transform.eulerAngles.y;
    }

    private void LateUpdate()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");


        if (Mathf.Abs(mouseX) > 0.01f || Mathf.Abs(mouseY) > 0.01f)
        {
            m_IsAutoMoving = false;
            m_IdleTimer = 0f;
            m_HasPlayedSpecialIdle = false;   
        }
        else
        {
            m_IdleTimer += Time.deltaTime;
        }
        if (!m_HasPlayedSpecialIdle && m_IdleTimer >= m_SpecialIdleTime)
        {
            TriggerSpecialIdleAnimation();
            m_HasPlayedSpecialIdle = true;
        }

        if (!m_IsAutoMoving && m_IdleTimer >= m_IdleTime)
        {
            if (m_cameraIdlePos != null)
                m_IsAutoMoving = true;
        }

        if (m_IsAutoMoving)
        {
            AutoMoveToIdleTarget();
            return;  
        }

        NormalCamera(mouseX, mouseY);
        FinishSpecialIdle();
    }


   
    void NormalCamera(float mouseX, float mouseY)
    {
        Vector3 l_LookAt = m_Player.m_LookAt.transform.position;
        float l_Distance = Vector3.Distance(l_LookAt, transform.position);

        m_Yaw += mouseX * m_YawSpeed * Time.deltaTime;
        m_Pitch -= mouseY * m_PitchSpeed * Time.deltaTime;
        m_Pitch = Mathf.Clamp(m_Pitch, m_MinPitch, m_MaxPitch);

        float l_YawRadians = m_Yaw * Mathf.Deg2Rad;
        float l_PitchRadians = m_Pitch * Mathf.Deg2Rad;
        Vector3 l_Direction = new Vector3(Mathf.Cos(l_PitchRadians) * Mathf.Sin(l_YawRadians),Mathf.Sin(l_PitchRadians),
            Mathf.Cos(l_PitchRadians) * Mathf.Cos(l_YawRadians));

        l_Distance = Mathf.Clamp(l_Distance, m_MinDistance, m_MaxDistance);

        Ray ray = new Ray(l_LookAt, -l_Direction);
        Vector3 desiredPos = l_LookAt - l_Direction * l_Distance;

        if (Physics.Raycast(ray, out RaycastHit hit, l_Distance, m_LayerMask.value))
            desiredPos = hit.point + l_Direction * m_OffsetDistance;

        transform.position = desiredPos;
        transform.LookAt(l_LookAt);
    }


    void AutoMoveToIdleTarget()
    {
        if (m_cameraIdlePos == null)
            return;

        transform.position = Vector3.Lerp(transform.position,m_cameraIdlePos.position,Time.deltaTime * m_MoveSpeedCamera);
        transform.rotation = Quaternion.Lerp(transform.rotation,m_cameraIdlePos.rotation,Time.deltaTime * m_RotateSpeedCamera);
    }
    void FinishSpecialIdle()
    {
        if (m_Player == null || m_Player.m_Animator == null)
            return;

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        bool isMoving = Mathf.Abs(h) > 0.1f || Mathf.Abs(v) > 0.1f;

        if (isMoving)
        {
            m_Player.m_Animator.SetBool("InSpecialidle", false);

            m_HasPlayedSpecialIdle = false;
            m_IdleTimer = 0f;
        }
    }
    void TriggerSpecialIdleAnimation()
    {
        if (m_Player != null && m_Player.m_Animator != null)
        {
            m_Player.m_Animator.SetBool("InSpecialidle", true);
        }
    }
}
