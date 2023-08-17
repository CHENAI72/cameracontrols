using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using DG.Tweening;
using System;

public class fixedCameraRota : MonoBehaviour
{

  

    private bool IsRota=false;
    public bool IsZhong;
    public List<CinemachineVirtualCamera> ThisZhongCamera;//要过度的虚拟相机
    public float RotaSpeed=0.08f;
    public float ZoomSpeed=2f;
    private  Quaternion startQuta;
    [HideInInspector]
    public float Min=15f;
    [HideInInspector]
    public float Max=40f;
  
    private void Awake()
    {
       
        startQuta = transform.rotation;
   

    }
  
    public void MoveStart()
    {

        transform.DOLocalRotateQuaternion(startQuta, 1.5f);
       
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
    public List<CinemachineVirtualCamera> virCamera
    {
        get { return ThisZhongCamera; }
        set
        {
            ThisZhongCamera = value;
        }
    }
    public bool ISZHONG
    {
        get
        {
            return IsZhong;
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
