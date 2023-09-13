using Cinemachine;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Collections;
using System;
using UnityEngine.Rendering;
using UnityEngine.Events;

public class CameraManager1 : MonoBehaviour
{

    //相机
    [SerializeField] DollyMoveCamera DollyMoveCam;
    [SerializeField] CinemachineFreeLook freeLook;
    [SerializeField] CinemachineCameraOffset CameraOffset;//第三人称偏移
    [SerializeField] List<CinemachineVirtualCamera> fixedCamera;//固定
    [SerializeField] CinemachineBrain MainCamera;

    private string IsEnterCamera;
    private string Is2DCameraname;
    private float CameraStartTime;
    private Vector2 CameraPos;
    private string names;
    private string Isname;
    private bool dollybool;
    private bool IsFixed = true;
    private Dictionary<string, CinemachineVirtualCamera> Camerapairs = new Dictionary<string, CinemachineVirtualCamera>();
    private Dictionary<string, fixedCameraRota> fixedRota = new Dictionary<string, fixedCameraRota>();
  
    public UnityEvent MoveFixedCameraStart;//进入车内
    public UnityEvent<string> MoveFixedCameraArrival;
    public UnityEvent MoveFixedCameraEnd;
    public UnityEvent<bool> OnDollyCamera;//是否在轨道移动
    public UnityEvent<bool> OnFreeLookCameraIsRota;//是否在旋转
    public UnityEvent<bool> OnFreeLookCameraBoolReversal;//是否在旋转,但是反转bool;
    public UnityEvent<bool> OnFixedCamerasIsRota;//是否在旋转
    public UnityEvent<bool> OnFixedCamerasBoolReversal;//是否在旋转,但是反转bool;

    private bool LookCameraRota = true;
    private bool FixedCameraRota = true;
    private bool LookCamera =true;
    private bool FixedCamera;

    [Header("FreelookCameraInput")]
    [SerializeField] CameraInputTou anchor;
    [SerializeField] float freeLookXSpeed = 0.2f;
    [SerializeField] float freeLookYSpeed = 0.8f;
    [SerializeField] float freeLookZoomSpeed = 4f;
    [SerializeField] float freeLookZoomMin = 15f;
    [SerializeField] float freeLookZoomMax = 80f;
    [SerializeField] bool Black = false;
    [SerializeField] float startCameraTime = 0.5f;
    [SerializeField] float blackCameraTime = 1f;


    [Header("fixedCameraInput")]
    [SerializeField] float FixedXSpeed = 0.08f;
    [SerializeField] float FixedYSpeed = 0.4f;
    [SerializeField] float FixedZoomSpeed = 2f;
    [SerializeField] float FixedZoomMin = 15f;
    [SerializeField] float FixedZoomMax = 40f;

    [Header("fixedCameraRestrict")]
    [SerializeField] bool fixedIsClamp;
    [SerializeField] float FixedRotaMinX;
    [SerializeField] float FixedRotaMaxX;
    [SerializeField] float FixedRotaMinY;
    [SerializeField] float FixedRotaMaxY;

    private bool IsDolly;

    [SerializeField] List<Transform> UI3DPos;

    private Vector2 TouTapVetor;

    private void Start()
    {
        if (anchor != null)
        {
            anchor.RegistPrimaryTouchTapCallBack(TouchTap);
            anchor.RegistPrimaryTouchPosChange(PrimaryTouchDeltaCallBack);
            anchor.RegistZoomCallBack(ZoomCallBack);
            anchor.RegistThreeFingerDeltaCallBack(ThreeFingerDelta);
            OnDollyCamera.AddListener((e) => { IsDolly = e; });
           

        }
        if (freeLook!=null)
        {
            CameraPos = new Vector2(FreeLook.m_XAxis.Value, FreeLook.m_YAxis.Value);
        }
        OnDollyCamera.AddListener(FreeLookCameraTransitions);
        MoveFixedCameraStart.AddListener( CameraMoveFixedStart);
        MoveFixedCameraEnd.AddListener(CameraMoveFixedEnd);
        CameraStartTime = MainCamera.m_DefaultBlend.m_Time;
    }

    private void OnDisable()
    {
        if (anchor != null)
        {


            anchor.UnRegistPrimaryTouchTapCallBack(TouchTap);
            anchor.UnRegistPrimaryTouchPosChange(PrimaryTouchDeltaCallBack);
            anchor.UnRegistZoomCallBack(ZoomCallBack);
            anchor.UnRegistThreeFingerDeltaCallBack(ThreeFingerDelta);
        }
        OnDollyCamera .RemoveListener(FreeLookCameraTransitions);
        MoveFixedCameraStart.RemoveListener(CameraMoveFixedStart);
        MoveFixedCameraEnd.RemoveListener(CameraMoveFixedEnd);
    }
    private CinemachineFreeLook FreeLook
    {
        get
        {
            return freeLook;
        }
        set
        {
            freeLook = value;
        }
    }
    private CinemachineCameraOffset CameraOff
    {
        get
        {
            return CameraOffset;
        }
        set
        {
            CameraOffset = value;
        }
    }
  
    public void FreeLookCameraPosSave()
    {
        if (freeLook!=null)
        {
            CameraPos = new Vector2(FreeLook.m_XAxis.Value, FreeLook.m_YAxis.Value);
        }
       
    }
    public void FreeLookBackStartPos(float time)
    {
        if (freeLook != null)
        {
           DOTween.To(() => FreeLook.m_XAxis.Value, x => FreeLook.m_XAxis.Value = x, CameraPos.x, time);
           DOTween.To(() => FreeLook.m_YAxis.Value, x => FreeLook.m_YAxis.Value = x, CameraPos.y, time);
        }
    }

    #region fixedCamera
    public void fixedCameraName(string name)//固定
    {
        float time = 0;
        if (name != names)
        {
            if (DollyMoveCam != null)
            {
                DollyCamera(false);
            }
            CameraPrioritys(names, Camerapairs, freeLook, DollyMoveCam, fixedRota, names);
            names = name;

        }
        if (Camerapairs.ContainsKey(name))
        {
         

            if (name!=Isname)
            {

                if (Camerapairs[name].gameObject.GetComponent<fixedCameraRota>().ISZHONG && Camerapairs[name].gameObject.GetComponent<fixedCameraRota>().virCamera.Count != 0 && FixedCamera)
                {
                  
                    StartCoroutine(IsvirCameraZhong(name, MainCamera.m_DefaultBlend.m_Time));

                }
                else if(name.Substring(name.Length - 2, 2)=="2D"  && FixedCamera != true)
                {
                   
                    Is2DCameraname = name;
                    Camerapairs[name].Priority = 11;
                    FalseDollyAll();
                 
                }
                else if (Camerapairs[name].gameObject.GetComponent<fixedCameraRota>().IsOne&& Camerapairs[name].gameObject.GetComponent<fixedCameraRota>().virOneCamera.Count != 0
                     && FixedCamera != true)
                {
                   
                    StartCoroutine(IsvirOneCamera(name, MainCamera.m_DefaultBlend.m_Time));
                    IsEnterCamera = name;
                }
                else
                {
                    Camerapairs[name].Priority = 11;
                   
                }
                fixedRota[name].ISROTA = true;
                LookCamera = false;
                FixedCamera = true;
                if (CameraOff!=null)
                {
                    DOTween.To(() => CameraOff.m_Offset, x => CameraOff.m_Offset = x, Vector3.zero, 1f);
                }
              
                Isname = name;
                if (IsFixed)
                {
                    DOTween.To(() => time, x => time = x, 1, CameraStartTime).OnComplete(() => {
                        MoveFixedCameraArrival?.Invoke(name);
                    });
                    MoveFixedCameraStart?.Invoke();
                    IsFixed = false;

                } 
            }

            

        }
        else
        {
            
            if (fixedCamera.Count != 0)
            {
                if (fixedCamera.Exists(t => t.Name == name)|| fixedCamera.Exists(t => t.Name == name.Substring(0, name.Length - 2)))
                {

                    for (int i = 0; i < fixedCamera.Count; i++)
                    {
                        if (fixedCamera[i].gameObject.name == name|| fixedCamera[i].gameObject.name == name.Substring(0, name.Length - 2))
                        {

                            Camerapairs.Add(name, fixedCamera[i]);

                            if (Camerapairs[name].gameObject.GetComponent<fixedCameraRota>().ISZHONG && Camerapairs[name].gameObject.GetComponent<fixedCameraRota>().virCamera.Count != 0 && FixedCamera)
                            {
                           
                                StartCoroutine(IsvirCameraZhong(name, MainCamera.m_DefaultBlend.m_Time));

                            }
                            else if (name.Substring(name.Length - 2, 2) == "2D" && FixedCamera!=true)
                            {
                            
                                Is2DCameraname = name;
                                Camerapairs[name].Priority = 11;
                                FalseDollyAll();
                            }
                            else if (Camerapairs[name].gameObject.GetComponent<fixedCameraRota>().IsOne && Camerapairs[name].gameObject.GetComponent<fixedCameraRota>().virOneCamera.Count!=0
                                 && FixedCamera != true)
                            {
                               
                                StartCoroutine(IsvirOneCamera(name, MainCamera.m_DefaultBlend.m_Time));
                                IsEnterCamera = name;
                            }
                            else
                            {
                                Camerapairs[name].Priority = 11;
                                
                            }

                            Isname = name;
                          
                            if (IsFixed)
                            {
                                DOTween.To(() => time, x => time = x, 1, CameraStartTime).OnComplete(() => {
                                    MoveFixedCameraArrival?.Invoke(name);
                                });
                                FixedCamera = true;
                                LookCamera = false;
                                if (CameraOff!=null)
                                {
                                    DOTween.To(() => CameraOff.m_Offset, x => CameraOff.m_Offset = x, Vector3.zero, 1f);
                                }
                                MoveFixedCameraStart?.Invoke();
                                IsFixed = false;
                            }
                      
                            if (Camerapairs[name].GetComponent<fixedCameraRota>() != null)
                            {
                                fixedRota.Add(name, Camerapairs[name].GetComponent<fixedCameraRota>());
                                fixedRota[name].ISROTA = true;
                            }
                            break;

                        }
                    }


                }
                else
                {
                    Debug.LogError("列表没有这个名称的相机"+name);
                }

            }
            else
            {
                Debug.LogError("没有相机被添加到列表");
            }

        }

    }

    IEnumerator IsvirOneCamera(string name, float time)
    {

        int i = 0;

        while (true)
        {
            Camerapairs[name].gameObject.GetComponent<fixedCameraRota>().virOneCamera[i].m_Priority = 11;
            yield return new WaitForSeconds(time);
            Camerapairs[name].gameObject.GetComponent<fixedCameraRota>().virOneCamera[i].m_Priority = 10;
            i++;
            if (i > Camerapairs[name].gameObject.GetComponent<fixedCameraRota>().virOneCamera.Count - 1)
            {
                Camerapairs[name].Priority = 11;
                StopCoroutine(IsvirOneCamera(name, MainCamera.m_DefaultBlend.m_Time));
                break;
            }
        }


    }

    IEnumerator IsvirCameraZhong(string name,float time)
    {

        int i = 0;
        
        while (true)
        {
            Camerapairs[name].gameObject.GetComponent<fixedCameraRota>().virCamera[i].m_Priority = 11;
            yield return new WaitForSeconds(time);
            Camerapairs[name].gameObject.GetComponent<fixedCameraRota>().virCamera[i].m_Priority = 10;
            i++;
            if (i > Camerapairs[name].gameObject.GetComponent<fixedCameraRota>().virCamera.Count - 1)
            {
                Camerapairs[name].Priority = 11;
                StopCoroutine(IsvirCameraZhong(name, MainCamera.m_DefaultBlend.m_Time));
                break;
            }
        }


    }

    #endregion


    public void FreeLookCameraRotaLatency(Vector2 xy,float time,float time1)
    {
        float timeValue = 0;
        if (freeLook == null)
        {
            Debug.LogError("请添加第三人称虚拟相机");
        }
        else
        {

            DOTween.To(() => timeValue, x => timeValue = x, 1, time).OnComplete(() => {

                DOTween.To(() => freeLook.m_XAxis.Value, x => freeLook.m_XAxis.Value = x, xy.x, time1);
            });
            DOTween.To(() => timeValue, x => timeValue = x, 1, time).OnComplete(() => {

                DOTween.To(() => freeLook.m_YAxis.Value, x => freeLook.m_YAxis.Value = x, xy.y, time1);
            });

        }

    }
    public void FreeLookCameraRota(Vector2 xy, float time)
    {
    
        if (freeLook == null)
        {
            Debug.LogError("请添加第三人称虚拟相机");
        }
        else
        {

                DOTween.To(() => freeLook.m_XAxis.Value, x => freeLook.m_XAxis.Value = x, xy.x, time);
                DOTween.To(() => freeLook.m_YAxis.Value, x => freeLook.m_YAxis.Value = x, xy.y, time);

        }
    }


    public void FreeLookCamera(bool value)
    {
        if (freeLook == null)
        {
            Debug.LogError("请添加第三人称虚拟相机");
        }
        else
        {
            if (value)
            {
                threeCamera(value);
              
            }
            else
            {
                if (IsEnterCamera != null&&IsEnterCamera!="")
                {
                    StartCoroutine(IsvirBrackCamera(IsEnterCamera, MainCamera.m_DefaultBlend.m_Time));
                }
                else
                {
                   
                    threeCamera(value);
              
                }
            }
            
           
           
        }
    }
    public void CameraHandoverTime(float time,bool value=false)
    {
        float time1 = 0;
        MainCamera.m_DefaultBlend.m_Time = time;
        if (value)
        {
            DOTween.To(() => time1, X => time1 = X, 1, blackCameraTime).OnComplete(() => {

                MainCamera.m_DefaultBlend.m_Time = CameraStartTime;


            });
        }
       


    }
    public void DollyCamera(bool Bool)//轨道
    {
        if (dollybool != Bool)
        {
            if (Bool == true && DollyMoveCam != null)
            {
                FixedCamera = false;
                LookCamera = false;
                if (CameraOff!=null)
                {
                    DOTween.To(() => CameraOff.m_Offset, x => CameraOff.m_Offset = x, Vector3.zero, 1f);
                }
                OnDollyCamera?.Invoke(Bool);
                DollyMoveCam.dollyCamera.m_Priority = 11;
                CameraPrioritys(null, Camerapairs, freeLook, null, fixedRota, null);
                DollyMoveCam.StartCoroutine("Toggle");
                DollyMoveCam.cart.m_Speed = 0.3f;


            }
            else if (Bool == false && DollyMoveCam != null)
            {
                OnDollyCamera?.Invoke(Bool);
                DollyMoveCam.StopCoroutine("Toggle");
                FalseDollyAll();
                DollyMoveCam.cart.m_Speed = 0f;


            }
            else
            {
                Debug.LogError("请添加DollyMoveCamera组件");
            }

            dollybool = Bool;
        }

    }

    private void FalseDollyAll(bool value=false)
    {
      
         CameraHandoverTime(startCameraTime);
        DOTween.To(() => DollyMoveCam.AColor, x => DollyMoveCam.AColor = x, 0, startCameraTime).OnUpdate(() =>
                {
                    if (Black)
                    {
                        DollyMoveCam.profile[0].parameters[2].SetValue(new ColorParameter(new Color(DollyMoveCam.AColor, DollyMoveCam.AColor, DollyMoveCam.AColor)));
                    }
       
    }).OnComplete(()=> {

        DOTween.To(() => DollyMoveCam.AColor, x => DollyMoveCam.AColor = x, 1, blackCameraTime).OnUpdate(() =>
        {
            if (Black)
            {
                DollyMoveCam.profile[0].parameters[2].SetValue(new ColorParameter(new Color(DollyMoveCam.AColor, DollyMoveCam.AColor, DollyMoveCam.AColor)));
            }
           
        }).OnComplete(()=> {
          
            if (Is2DCameraname.Substring(Is2DCameraname.Length - 2) != "2D")
            {
                CameraHandoverTime(CameraStartTime);
            }
           }); 



    });        

    }
   
    public void CameraDictionaryClear()
    {
        Camerapairs.Clear();
    }
    public void fixedDictionaryClear()
    {
        fixedRota.Clear();
    }

    private void threeCamera(bool value)
    {
        if (value)
        {
            CameraHandoverTime(CameraStartTime);
        }
        
        if (Is2DCameraname != "" && Is2DCameraname != null)
        {
           
            if (Is2DCameraname.Substring(Is2DCameraname.Length - 2) == "2D"&& value!=true)
            {
            
                FalseDollyAll(value);
            }
        }
        if (freeLook.m_Priority != 11)
        {
            freeLook.m_Priority = 11;
            Isname = null;
            IsEnterCamera = null;
            Is2DCameraname = null;
        }
        if (DollyMoveCam != null)
            {
                DollyCamera(false);
            }
            FixedCamera = false;
            LookCamera = true;
            CameraPrioritys(null, Camerapairs, null, DollyMoveCam, fixedRota, null);
      
            
        

    }
    IEnumerator IsvirBrackCamera(string name, float time)
    {
        
        int i = Camerapairs[name].gameObject.GetComponent<fixedCameraRota>().virOneCamera.Count-1;
        if (DollyMoveCam != null)
        {
            DollyCamera(false);
        }
        FixedCamera = false;
        LookCamera = true;
        CameraPrioritys(null, Camerapairs, null, DollyMoveCam, fixedRota, null);
        while (true)
        {
            Camerapairs[name].gameObject.GetComponent<fixedCameraRota>().virOneCamera[i].m_Priority = 11;
            yield return new WaitForSeconds(time);
            Camerapairs[name].gameObject.GetComponent<fixedCameraRota>().virOneCamera[i].m_Priority = 10;
            i--;
            if (i < Camerapairs[name].gameObject.GetComponent<fixedCameraRota>().virOneCamera.Count - 1)
            {
                
                if (freeLook.m_Priority != 11)
                {
                    freeLook.m_Priority = 11;
                    Isname = null;
                    IsEnterCamera = null;
                    Is2DCameraname = null;
                }
               
                StopCoroutine(IsvirBrackCamera(names,MainCamera.m_DefaultBlend.m_Time));
                break;
            }
        }
        

    }
    private void CameraPrioritys(string name, Dictionary<string, CinemachineVirtualCamera> valuePairs, CinemachineFreeLook freeLook, DollyMoveCamera DolllyCamera, Dictionary<string, fixedCameraRota> fixedCamera, string fixedname)
    {
        CameraPriority(name != null ? name : null, valuePairs != null ? Camerapairs : null, freeLook != null ? freeLook : null, DolllyCamera != null ? DollyMoveCam.dollyCamera : null, fixedCamera != null ? fixedCamera : null, fixedname != null ? fixedname : null);
    }
    private void CameraPriority(string name, Dictionary<string, CinemachineVirtualCamera> valuePairs, CinemachineFreeLook freeLook, CinemachineVirtualCamera DolllyCamera, Dictionary<string, fixedCameraRota> fixedCamera, string fixedname)
    {
        if (valuePairs.Count > 0 && name != "" && name != null)
        {
            if (valuePairs.ContainsKey(name))//指定
            {
                valuePairs[name].Priority = 10;

            }

        }
        else
        {
            if (valuePairs.Count > 0)
            {
                foreach (var item in valuePairs)//所有
                {
                    item.Value.Priority = 10;
                }
                if (IsFixed == false)
                {
                    MoveFixedCameraEnd?.Invoke();
                    IsFixed = true;
                }

            }



        }
        if (freeLook != null)
        {
            freeLook.m_Priority = 10;
           

        }
        if (DolllyCamera != null)
        {
            DolllyCamera.m_Priority = 10;

        }

        if (fixedCamera.Count > 0 && fixedname != "" && fixedname != null)
        {
            if (fixedCamera.ContainsKey(fixedname))//指定
            {
                fixedCamera[fixedname].ISROTA = false;
               fixedRota[fixedname].MoveStart();
           
            }

        }
        else
        {
            if (fixedCamera.Count > 0)
            {
                foreach (var item in fixedCamera)
                {
                    item.Value.ISROTA = false;
                   item.Value.MoveStart();
                 


                }
            }
        }

    }

   

    #region ThisTouch
   
    private void TouchTap(Vector2 vector)
    {
      
        TouTapVetor = Vector2.zero;
        if (FixedCamera )
        {
            if (Camerapairs.Count!=0)
            {

                    if (Camerapairs[Isname].GetCinemachineComponent<CinemachinePOV>().m_VerticalAxis.m_InputAxisValue != 0 ||
                   Camerapairs[Isname].GetCinemachineComponent<CinemachinePOV>().m_HorizontalAxis.m_InputAxisValue != 0)
                    {
                        Camerapairs[Isname].GetCinemachineComponent<CinemachinePOV>().m_VerticalAxis.m_InputAxisValue = 0;
                        Camerapairs[Isname].GetCinemachineComponent<CinemachinePOV>().m_HorizontalAxis.m_InputAxisValue = 0;
                    OnFixedCamerasIsRota?.Invoke(false);
                        OnFixedCamerasBoolReversal?.Invoke(true);
                        FixedCameraRota = true;

                    }
                
             
            }

         
        }
        if (LookCamera)
        {
            if (FreeLook!=null)
            {

                    if (FreeLook.m_XAxis.m_InputAxisValue != 0 || FreeLook.m_YAxis.m_InputAxisValue != 0)
                    {
                    FreeLook.m_XAxis.m_InputAxisValue = 0;
                    FreeLook.m_YAxis.m_InputAxisValue = 0;
                    OnFreeLookCameraIsRota?.Invoke(false);
                    OnFreeLookCameraBoolReversal?.Invoke(true);
                    LookCameraRota = true;
                  
                    }

            }
          
          
        }
      

    }


    private void PrimaryTouchDeltaCallBack(Vector2 vector)
    {
       
        if (TouTapVetor.x - vector.x==0&& TouTapVetor.y - vector.y==0)
        {
            TouchTap(vector);
        }
            if (FixedCamera)
          {
            if (fixedRota[Isname].ISROTA&& fixedRota[Isname].therota)
            {
               
                if (TouTapVetor != Vector2.zero)
                {
                    float MoveX = (TouTapVetor.x - vector.x) * FixedXSpeed;
                    float MoveY = (TouTapVetor.y - vector.y) * FixedYSpeed;
                    if (vector.x > 1f || vector.x < -1f )
                    {
                       
                            Camerapairs[Isname].GetCinemachineComponent<CinemachinePOV>().m_HorizontalAxis.m_InputAxisValue = Time.deltaTime * MoveX;
                     
                    }
                    if (vector.y > 1f || vector.y < -1f)
                    {

                            Camerapairs[Isname].GetCinemachineComponent<CinemachinePOV>().m_VerticalAxis.m_InputAxisValue = Time.deltaTime * MoveY;
                    }
                    if (fixedIsClamp)
                    {
                        Camerapairs[Isname].GetCinemachineComponent<CinemachinePOV>().m_HorizontalAxis.Value = Mathf.Clamp(
                             Camerapairs[Isname].GetCinemachineComponent<CinemachinePOV>().m_HorizontalAxis.Value, FixedRotaMinX, FixedRotaMaxX);

                        Camerapairs[Isname].GetCinemachineComponent<CinemachinePOV>().m_VerticalAxis.Value = Mathf.Clamp(
                             Camerapairs[Isname].GetCinemachineComponent<CinemachinePOV>().m_VerticalAxis.Value, FixedRotaMinY, FixedRotaMaxY);
                    }
                    if (FixedCameraRota)
                    {
                     
                        OnFixedCamerasIsRota?.Invoke(true);
                        OnFixedCamerasBoolReversal?.Invoke(false);
                          FixedCameraRota = false;
                    }
                }
                TouTapVetor = vector;
            }
        }

        if (LookCamera)
        {


            if (TouTapVetor != Vector2.zero)
            {
               
                float MoveX = (TouTapVetor.x - vector.x) * freeLookXSpeed;
                float MoveY = (TouTapVetor.y - vector.y) * freeLookYSpeed; 
                if (MoveX > 1f || MoveX < -1f )
                {
                    FreeLook.m_XAxis.m_InputAxisValue = Time.deltaTime * MoveX;
                    
                }
                if (MoveY > 1f || MoveY < -1f)
                {
                    FreeLook.m_YAxis.m_InputAxisValue = Time.deltaTime * MoveY;
                 
                }
                if (LookCameraRota)
                {
         
                    OnFreeLookCameraIsRota?.Invoke(true);
                    OnFreeLookCameraBoolReversal?.Invoke(false);
                    LookCameraRota = false;
                }

            }
          

            TouTapVetor = vector;

        }
    }
    private void ZoomCallBack(ZoomType touch)//缩放
    {
        if (LookCamera)
        {
            switch (touch)
            {

                case ZoomType.ZoomIn:
                    if (anchor.ZoomDistance > 1f)
                    {
                       FreeLook.m_Lens.FieldOfView -= 1 * Time.deltaTime * freeLookZoomSpeed;
                       FreeLook.m_Lens.FieldOfView = Mathf.Clamp(FreeLook.m_Lens.FieldOfView, freeLookZoomMin, freeLookZoomMax);
                    }

                    break;
                case ZoomType.ZoomOut:
                    if (anchor.ZoomDistance > 1f)
                    {
                        FreeLook.m_Lens.FieldOfView += 1 * Time.deltaTime * freeLookZoomSpeed;
                       FreeLook.m_Lens.FieldOfView = Mathf.Clamp(FreeLook.m_Lens.FieldOfView, freeLookZoomMin, freeLookZoomMax);
                    }

                    break;
                default:
                    break;
            }

        }
        if (FixedCamera)
        {
            switch (touch)
            {
                case ZoomType.ZoomIn:
                    if (anchor.ZoomDistance > 1f)
                    {
                        Camerapairs[Isname].m_Lens.FieldOfView -= 1 * Time.deltaTime * FixedZoomSpeed;
                        Camerapairs[Isname].m_Lens.FieldOfView = Mathf.Clamp(Camerapairs[Isname].m_Lens.FieldOfView, FixedZoomMin, FixedZoomMax);
                    }

                    break;
                case ZoomType.ZoomOut:
                    if (anchor.ZoomDistance > 1f)
                    {


                        Camerapairs[Isname].m_Lens.FieldOfView += 1 * Time.deltaTime * FixedZoomSpeed;
                        Camerapairs[Isname].m_Lens.FieldOfView = Mathf.Clamp(Camerapairs[Isname].m_Lens.FieldOfView, FixedZoomMin, FixedZoomMax);
                    }
                    break;

                default:
                    break;
            }
        }
    }
    public void TimeNoTouCh()
    {
        if (LookCamera)
        {
            DollyCamera(true);
        }

    }
    private void IsTouch()
    {
        if (IsDolly)
        {
           // FreeLookCamera();

        }


    }
    private void ThreeFingerDelta(Vector2 vector)
    {
        if (LookCamera)
        {

            if (vector.x > 1 || vector.x < -1)
            {

                CameraOff.m_Offset.x += vector.x / 700 * Time.deltaTime;
                CameraOff.m_Offset.x = Mathf.Clamp(CameraOff.m_Offset.x, -2f, 2f);

            }
            if (vector.y > 1 || vector.y < -1)
            {
               CameraOff.m_Offset.y += vector.y / 700 * Time.deltaTime;
               CameraOff.m_Offset.y = Mathf.Clamp(CameraOff.m_Offset.y, -0.5f, 2f);
            }

        }
    }
  
   
    private void FreeLookCameraTransitions(bool value)
    {
        float time = 0;
        DOTween.To(() => time, x => time = x, 1, 0.5f).OnComplete(() => {
          FreeLook.m_Transitions.m_InheritPosition = value;
        });

    }
    private void CameraMoveFixedStart()
    {
      // FreeLookCameraRota(new Vector2(-146f, 0.5f));
    }
    private void CameraMoveFixedEnd()
    {
        //foreach (var item in fixedRota)
        //{
        //    //item.Value.MoveStart();

        //}
       Isname = "";
    }


    #endregion
}


