using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPosHosue : MonoBehaviour
{
    public Camera HosueCamera;
    private EventCamera Event;
    // Start is called before the first frame update
    void Start()
    {
        Event = new EventCamera();
        Event.OnVariableChange += CameraData;
    }

    private void OnDestroy()
    {
        Event.OnVariableChange -= CameraData;
    }
    // Update is called once per frame
    void Update()
    {
        ///赋值

        //Event.Isvector=
        //Event.Isquaternion=
       // Event.IsfiedView=
        Event. CheckUpdate();
    }
    private void CameraData(Vector3 newVal, Quaternion quaternion, float fiedView)
    {
        HosueCamera.transform.localPosition = newVal;
        HosueCamera.transform.localRotation = quaternion;
        HosueCamera.fieldOfView = fiedView;
    }

}
public class EventCamera
{
    public delegate void OnBoolChangeDelegate(Vector3 newVal, Quaternion quaternion, float fiedView);
    public event OnBoolChangeDelegate OnVariableChange;

    private Vector3 m_vector = Vector3.zero;
    private Quaternion m_quaternion = Quaternion.identity;
    private float m_fiedView = 0f;
    public Vector3 Isvector
    {
        get
        {
            return m_vector;

        }
        set
        {

            if (m_vector == value) return;

            m_vector = value;
            isUpdated = true;


        }
    }
    public Quaternion Isquaternion
    {
        get
        {
            return m_quaternion;

        }
        set
        {

            if (m_quaternion == value) return;

            m_quaternion = value;
            isUpdated = true;

        }
    }
    public float IsfiedView
    {
        get
        {
            return m_fiedView;

        }
        set
        {

            if (m_fiedView == value) return;

            m_fiedView = value;
            isUpdated = true;

        }
    }
    /// <summary>
    /// 数值更新位标
    /// </summary>
    bool isUpdated = false;

    /// <summary>
    /// 检查是否数值更新
    /// </summary>
    public void CheckUpdate()
    {
        if (isUpdated)
        {
            OnVariableChange?.Invoke(m_vector, m_quaternion, m_fiedView);

            isUpdated = false;
        }
    }
}