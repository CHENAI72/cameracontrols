using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using DG.Tweening;

public class fixedCameraRota : MonoBehaviour
{

    [SerializeField] bool TheRota ;
    [SerializeField] List<GameObject> TransitionCamera;//经过那些虚拟相机需要过渡
    [SerializeField] List<CinemachineVirtualCamera> TransitionList;//过渡的虚拟相机
   
    private bool IsRota = false;
    private float XAxis;
    private float YAxis;
    private float TheFov;
    private CinemachineCameraOffset Offset;
    private Vector3 CamaeraOffset;
    private void Awake()
    {
        if (this.GetComponent<CinemachineVirtualCamera>() != null)
        {
            XAxis = this.GetComponent<CinemachineVirtualCamera>().GetCinemachineComponent<CinemachinePOV>().m_HorizontalAxis.Value;
            YAxis = this.GetComponent<CinemachineVirtualCamera>().GetCinemachineComponent<CinemachinePOV>().m_VerticalAxis.Value;
            TheFov = this.GetComponent<CinemachineVirtualCamera>().m_Lens.FieldOfView;
        }

        if (TransitionCamera.Count!=0)
        {
            for (int i = 0; i < TransitionCamera.Count; i++)
            {
                if (TransitionCamera[i].GetComponent<CinemachineVirtualCamera>() == null && TransitionCamera[i].GetComponent<CinemachineFreeLook>() == null)
                {
                    Debug.LogError($"请不要在{gameObject.name}的TransitionCamera列表中添加没有挂载虚拟相机的组件.");
                }
            } 
        }
        if (this.GetComponent<CinemachineCameraOffset>()!=null)
        {
            Offset = this.GetComponent<CinemachineCameraOffset>();
            CamaeraOffset = Offset.m_Offset;
        }
    }
  
    public void MoveStart()
    {
        if (this.GetComponent<CinemachineVirtualCamera>()!=null)
        {
            DOTween.To(() => this.GetComponent<CinemachineVirtualCamera>().GetCinemachineComponent<CinemachinePOV>().m_HorizontalAxis.Value,
        x => this.GetComponent<CinemachineVirtualCamera>().GetCinemachineComponent<CinemachinePOV>().m_HorizontalAxis.Value = x, XAxis, 0.6f);
            DOTween.To(() => this.GetComponent<CinemachineVirtualCamera>().GetCinemachineComponent<CinemachinePOV>().m_VerticalAxis.Value,
                x => this.GetComponent<CinemachineVirtualCamera>().GetCinemachineComponent<CinemachinePOV>().m_VerticalAxis.Value = x, YAxis, 0.6f);
            DOTween.To(() => this.GetComponent<CinemachineVirtualCamera>().m_Lens.FieldOfView,
              x => this.GetComponent<CinemachineVirtualCamera>().m_Lens.FieldOfView = x, TheFov, 0.6f);
        }
    
    }
   public void MoveStartOffset(float time)
    {
      
        if (Offset != null)
        {
            DOTween.To(() => Offset.m_Offset, x => Offset.m_Offset = x, CamaeraOffset, time);
        }
        else
        {
            Debug.LogError($"{gameObject.name}没有添加CinemachineCameraOffset组件");
        }
    }
    public void SetOffset(Vector3 vector,float time)
    {
        
        if (Offset != null)
        {
            DOTween.To(() => Offset.m_Offset, x => Offset.m_Offset = x, vector, time);
        }
        else
        {
            Debug.LogError($"{gameObject.name}没有添加CinemachineCameraOffset组件");
        }
    }
    public bool therota
    {
        get
        {
            return TheRota;
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
    public List<GameObject> m_TransitionCamera
    {
        get { return TransitionCamera; }
    }
  
    
    public List<CinemachineVirtualCamera> m_TransitionList
    {
        get { return TransitionList; }
      
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
