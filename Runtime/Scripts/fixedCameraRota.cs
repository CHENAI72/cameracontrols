using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using DG.Tweening;

public class fixedCameraRota : MonoBehaviour
{
    [SerializeField] bool TheISRota = false;
    [SerializeField] bool IsTransition;
    [SerializeField] bool IsTransitionOne;
    [SerializeField] List<CinemachineVirtualCamera> ThisTransitionOneCamera;
    [SerializeField] List<CinemachineVirtualCamera> ThisTransitionCamera;//要过渡的虚拟相机

    private bool IsRota = false;
    private float XAxis;
    private float YAxis;
    private float TheFov;
    private void Awake()
    {

        XAxis = this.GetComponent<CinemachineVirtualCamera>().GetCinemachineComponent<CinemachinePOV>().m_HorizontalAxis.Value;
        YAxis= this.GetComponent<CinemachineVirtualCamera>().GetCinemachineComponent<CinemachinePOV>().m_VerticalAxis.Value;
        TheFov = this.GetComponent<CinemachineVirtualCamera>().m_Lens.FieldOfView;
    }
  
    public void MoveStart()
    {
 
        DOTween.To(() => this.GetComponent<CinemachineVirtualCamera>().GetCinemachineComponent<CinemachinePOV>().m_HorizontalAxis.Value,
            x => this.GetComponent<CinemachineVirtualCamera>().GetCinemachineComponent<CinemachinePOV>().m_HorizontalAxis.Value = x, XAxis, 0.6f);
        DOTween.To(() => this.GetComponent<CinemachineVirtualCamera>().GetCinemachineComponent<CinemachinePOV>().m_VerticalAxis.Value,
            x => this.GetComponent<CinemachineVirtualCamera>().GetCinemachineComponent<CinemachinePOV>().m_VerticalAxis.Value = x, YAxis, 0.6f);
        DOTween.To(() => this.GetComponent<CinemachineVirtualCamera>().m_Lens.FieldOfView,
          x => this.GetComponent<CinemachineVirtualCamera>().m_Lens.FieldOfView = x, TheFov, 0.6f);
    }
   
    public bool therota
    {
        get
        {
            return TheISRota;
        }
    }
    public bool ISROTA
    {
        get
        {
            return IsRota;
        }
        set
        {
            IsRota = value;
        }
    }
    public bool IsOne
    {
        get
        {
            return IsTransitionOne;
        }
      
    }
    public List<CinemachineVirtualCamera> virCamera
    {
        get { return ThisTransitionCamera; }
       
    }
    public List<CinemachineVirtualCamera> virOneCamera
    {
        get { return ThisTransitionOneCamera; }
      
    }
    public bool ISZHONG
    {
        get
        {
         
            return IsTransition;
           
        }
     
    }


    public float ZoomLens(float Dis,float Min,float Max)
    {
        return Mathf.Clamp(Dis, Min, Max);
      
    }
    public bool IsVisableInCamera(Vector3 pos)
    {
        
            Camera mCamera = Camera.main;
            //转化为视角坐标
            Vector3 viewPos = mCamera.WorldToViewportPoint(pos);
            // z<0代表在相机背后
            if (viewPos.z < 0) return false;
            //太远了！看不到了！
            if (viewPos.z > mCamera.farClipPlane)
                return false;
            // x,y取值在 0~1之外时代表在视角范围外；
            if (viewPos.x < 0 || viewPos.y < 0 || viewPos.x > 1 || viewPos.y > 1) return false;
            return true;
        
    }
}
