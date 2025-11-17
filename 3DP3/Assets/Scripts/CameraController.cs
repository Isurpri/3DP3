using UnityEngine;

public class CameraController : MonoBehaviour
{
    public PlayerController m_Player;
    float m_Yaw = 0.0f;
    float m_Pitch = 0.0f;
    public float m_YawSpeed = 360.0f;
    public float m_PitchSpeed= 180.0f;
    public float m_MinPitch = -60.0f;
    public float m_MaxPitch = 80.0f;
    public float m_MinDist = 3.0f;
    public float m_MaxDist = 12.0f;
    public LayerMask m_LayerMask;
    public float m_offsetDist = 0.1f;
    private void Start()
    {
        m_Yaw= transform.eulerAngles.y;
    }
    private void LateUpdate()
    {
        Vector3 l_lookAt=m_Player.m_LookAt.transform.position;
        float l_dist=Vector3.Distance(l_lookAt,transform.position);

        float l_HorizontalAxis = Input.GetAxis("Mouse X");
        float l_VerticalAxis = Input.GetAxis("Mouse Y");
        m_Yaw += l_HorizontalAxis * m_YawSpeed * Time.deltaTime;
        m_Pitch -= l_VerticalAxis * m_PitchSpeed * Time.deltaTime;
        m_Pitch=Mathf.Clamp(m_Pitch,m_MinPitch,m_MaxPitch);

        float l_YawRad= m_Yaw*Mathf.Deg2Rad;
        float l_PitchRad = m_Pitch*Mathf.Deg2Rad;
        Vector3 l_direction = new Vector3(Mathf.Cos(l_PitchRad)*Mathf.Sin(l_YawRad),Mathf.Sin(l_PitchRad),Mathf.Cos(l_PitchRad)*Mathf.Cos(l_YawRad));
        l_dist = Mathf.Clamp(l_dist,m_MinDist,m_MaxDist);

        Ray l_Ray = new Ray(l_lookAt,-l_direction);
        Vector3 l_DesiredPos = l_lookAt-l_direction*l_dist;
        if (Physics.Raycast(l_Ray, out RaycastHit l_RayCastHit, l_dist, m_LayerMask.value))
        {
            l_DesiredPos=l_RayCastHit.point + l_direction*m_offsetDist;
        }

        transform.position = l_DesiredPos;
        transform.LookAt(l_lookAt);
    }
}
