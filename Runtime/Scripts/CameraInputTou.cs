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
   
    public void RegistPrimaryTouchPosChange(Action<Vector2> vector)
    {
        cameraData.OnVariaTouchPos += vector;
        
    }
    public void RegistPrimaryTouchDetaChange(Action<Vector2> vector)
    {
        cameraData.OnVariaTouchDeta += vector;
    }
    public void RegistZoomCallBack(Action<ZoomType> Zoom)
    {
        cameraData.OnVariaZoom += Zoom;
    }
    public void RegisOnZoomDistance(Action<float> zoomDitstance)
    {
        cameraData.OnZoomDistance += zoomDitstance;
    }
    public void RegistThreeFingerDeltaCallBack(Action<Vector2> vector)
    {
        cameraData.OnVariaThree += vector;
    }


    public void UnRegisOnZoomDistance(Action<float> zoomDitstance)
    {
        cameraData.OnZoomDistance -= zoomDitstance;
    }

    public void UnRegistPrimaryTouchPosChange(Action<Vector2> vector)
    {
        cameraData.OnVariaTouchPos -= vector;
    }
    public void UnRegistPrimaryTouchDetaChange(Action<Vector2> vector)
    {
        cameraData.OnVariaTouchDeta -= vector;
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

    public event Action<Vector2> OnVariaTouchPos;
    public event Action<Vector2> OnVariaTouchDeta;
    public event Action<float> OnZoomDistance;
    public event Action<ZoomType> OnVariaZoom;
    public event Action<float> OnvariaZoomDistance;
    public event Action<Vector2> OnVariaThree;

    private Vector2 m_Touchpos = Vector2.zero;
    private Vector2 m_TouchDeta = Vector2.zero;
    private ZoomType m_Zoom = ZoomType.NA;
    private float m_ZoomDistance = 0f;
    private float m_zoomOnDistance = 0f;
    private Vector2 m_Three = Vector2.zero;
   

    public Vector2 IsvectorTouchPos
    {
        set
        {
            m_Touchpos = value;
            OnVariaTouchPos?.Invoke(m_Touchpos);
       
        }
    }
    public Vector2 IsOnVariaTouchDeta
    {
        set
        {
            m_TouchDeta = value;
            OnVariaTouchDeta?.Invoke(m_TouchDeta);

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
    public float IsOnZoomDistance
    {
        set
        {
            m_zoomOnDistance = value;
            OnZoomDistance?.Invoke(m_zoomOnDistance);

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