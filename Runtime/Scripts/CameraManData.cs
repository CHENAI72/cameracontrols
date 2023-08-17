using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public  class CameraManData : MonoBehaviour
{
    public CameraManager1 CameraMan1;
    private void Awake()
    {
        Init();
      //  ClientDataReciever.Instance.RegisteNoInputAction(CameraMan1.TimeNoTouCh);
       // ClientDataReciever.Instance.ReceiveOperator += CameraMan1. IsTouch;
    }
    public void InvokeCallBack(CameraManagerCallBackType type, CameraManagerData data)
    {
        switch (type)
        {
            case CameraManagerCallBackType.FreeLookCamera:
                CameraMan1.FreeLookCamera();
                break;
            case CameraManagerCallBackType.fixedCameraName:
               CameraMan1.fixedCameraName(data.fixedCameraNameText);
                break;
            case CameraManagerCallBackType.FreeLookCameraRota:
               CameraMan1.FreeLookCameraRota(data.FreeLookCameraRota);
                break;
            case CameraManagerCallBackType.DollyCamera:
                CameraMan1.DollyCamera(data.DollyCameraStatus);
                break;
            default:
                break;
        }
    }

    private void Init()
    {

    //    ClientDataReciever.Instance.RegisterAction(CameraManagerConst.CameraManagerDefaultkey, GetSocketMessage);
    }

    private void GetSocketMessage(string CameraMessage)
    {
        Debug.Log(CameraMessage);
        CameraManagerSocketData data = JsonUtility.FromJson<CameraManagerSocketData>(CameraMessage);
        InvokeCallBack(data.type, data.data);
    }
}



[Serializable]
public enum CameraManagerCallBackType
{
    FreeLookCamera, fixedCameraName, FreeLookCameraRota, DollyCamera
}

[Serializable]
public struct CameraManagerData
{
    public string fixedCameraNameText;
    public Vector2 FreeLookCameraRota;
    public bool DollyCameraStatus;

    public CameraManagerData(string fixedCameraNameText,Vector2 FreeLookCameraRota,bool DollyCameraStatus)
    {
        this.fixedCameraNameText = fixedCameraNameText;
        this.FreeLookCameraRota = FreeLookCameraRota;
        this.DollyCameraStatus = DollyCameraStatus;
    }
}

[Serializable]
public struct CameraManagerSocketData
{
    public CameraManagerData data;
    public CameraManagerCallBackType type;
    public CameraManagerSocketData(CameraManagerData data, CameraManagerCallBackType type)
    {
        this.type = type;
        this.data = data;
    }
}


public class CameraManagerHelper
{
   
}



public class CameraManagerConst
{
    public const string CameraManagerDefaultkey = "CameraManagerDefaultkey ";
}
