using System;
using UnityEngine;
public enum ZoomType
{
  NA, ZoomIn, ZoomOut
}

public class CameraInputTou : MonoBehaviour
{
 
    [HideInInspector]
    public float ZoomDistance=> cameraData.IsZoomDistance;
   [HideInInspector]
    public CameraData cameraData;

    private void Awake()
    {
        cameraData = new CameraData();
    
    }
    private void Update()
    {
            
    }
    public void RegistPrimaryTouchTapCallBack(Action<Vector2> vector)
    {
        cameraData.OnVariaTouchTap += vector;
    }
    public void RegistPrimaryTouchPosChange(Action<Vector2> vector)
    {
        cameraData.OnVariaTouchPos += vector;
        
    }

    public void RegistZoomCallBack(Action<ZoomType> Zoom)
    {
        cameraData.OnVariaZoom += Zoom;
    }

    public void RegistThreeFingerDeltaCallBack(Action<Vector2> vector)
    {
        cameraData.OnVariaThree += vector;
    }



    public void UnRegistPrimaryTouchTapCallBack(Action<Vector2> vector)
    {
        cameraData.OnVariaTouchTap -= vector;
    }
    public void UnRegistPrimaryTouchPosChange(Action<Vector2> vector)
    {
        cameraData.OnVariaTouchPos -= vector;
    }
    public void UnRegistZoomCallBack(Action<ZoomType> Zoom)
    {
        cameraData.OnVariaZoom -= Zoom;
     
    }
    public void UnRegistThreeFingerDeltaCallBack(Action<Vector2> vector)
    {
        cameraData.OnVariaThree -= vector;
    }

  
}
public class CameraData
{
    //public delegate void RegistPrimaryTouchTap(Vector2 vector);
    public event Action<Vector2> OnVariaTouchTap;

   // public delegate void RegistPrimaryTouchPos(Vector2 vector);
    public event Action<Vector2> OnVariaTouchPos;

   // public delegate void RegistZoom(ZoomType zoomType);
    public event Action<ZoomType> OnVariaZoom;
    public event Action<float> OnvariaZoomDistance;

   // public delegate void RegistThreeFinger(Vector2 vector);
    public event Action<Vector2> OnVariaThree;

    private Vector2 m_Touchpos = Vector2.zero;
    [SerializeField]
    private Vector2 m_TouchTap = Vector2.zero;
    private ZoomType m_Zoom = ZoomType.NA;
    private float m_ZoomDistance = 0f;
    private Vector2 m_Three = Vector2.zero;
    public Vector2 IsvectorTouchTap
    {
        set
        {
            m_TouchTap = value;
            OnVariaTouchTap?.Invoke(m_TouchTap);
        }
    }

    public Vector2 IsvectorTouchPos
    {
        set
        {
            m_Touchpos = value;
            OnVariaTouchPos?.Invoke(m_Touchpos);
       
        }
    }
    public ZoomType IszoomType
    {
        set
        {
            m_Zoom = value;
            OnVariaZoom?.Invoke(m_Zoom);
        }
    }
    public float IsZoomDistance
    {
        get
        {
            return m_ZoomDistance;
        }
        set
        {
            m_ZoomDistance = value;
            OnvariaZoomDistance?.Invoke(m_ZoomDistance);
        }
    }
    public Vector2 IsThreedata
    {
        set
        {
            m_Three = value;
            OnVariaThree?.Invoke(m_Three);
        }
    }
   
}