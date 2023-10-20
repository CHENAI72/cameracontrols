using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using DG.Tweening;
using UnityEngine.Rendering;
using UnityEngine.Events;
using System;
public class CameraControlV3 : MonoBehaviour
{
    [SerializeField] CinemachineBrain MainCamera;
    [SerializeField] CinemachineStateDrivenCamera DrivenCamera;
    [SerializeField] DollyMoveCamera DollyMoveCam;
    [SerializeField] Volume VolumeColor;

    private CinemachineBlenderSettings.CustomBlend[] customBlend;


    private float AColor = 1;
    private List<VolumeComponent> profile;
    private CinemachineCameraOffset CameraOffset;
    private CinemachineFreeLook FreeLook;
    public UnityEvent<string> StartMoveCamera;//切换开始时响应，string为上一个摄像机名称
    public UnityEvent<string> MoveCameraArrival;//切换结束时响应，string为当前摄像机名称
    public UnityEvent<Vector2> OnFreeLookCameraRota;//在旋转
    public UnityEvent<Vector2> OnFixedCamerasRota;//在旋转

    [Header("FreelookCameraInput")]
    [SerializeField] CameraInputTou anchor;
    [SerializeField] float freeLookZoomSpeed = 4f;
    [SerializeField] float freeLookZoomMin = 15f;
    [SerializeField] float freeLookZoomMax = 80f;
    [SerializeField] bool Black = false;
    [SerializeField] float blackCameraTime = 0.5f;

    [Header("fixedCameraInput")]
    [SerializeField] float FixedZoomSpeed = 2f;
    [SerializeField] float FixedZoomMin = 15f;
    [SerializeField] float FixedZoomMax = 40f;

    [Header("DollyCameraSpeed")]
    [SerializeField] float DollyCameraSpeed = 0.3f;


    private bool LookCamera = true;
    private bool IsFixedCamera;
  

    private string Dollyname;
    private string IsEnterCamera;

    private float CameraStartTime;

    private string EndCamera;

    private Vector2 CameraPos;
    private string names;
    private string Isname;
    private bool dollybool;
    private bool CamIsBlending;

    private bool Security;//保护
    private Vector2 TouTapVetor;
    private List<CinemachineVirtualCameraBase> CameraChilds=new List<CinemachineVirtualCameraBase>();
    private Dictionary<string, fixedCameraRota> FixedCamera = new Dictionary<string, fixedCameraRota>();
    // Start is called before the first frame update

    #region StartData
    private void Awake()
    {
        if (DrivenCamera.m_CustomBlends != null)
        {
            customBlend = DrivenCamera.m_CustomBlends.m_CustomBlends;

        }
         CameraStartTime = DrivenCamera.m_DefaultBlend.m_Time;
        CinemachineVirtualCameraBase[] Camera = DrivenCamera.ChildCameras;
        for (int i = 0; i < Camera.Length; i++)
        {
            CameraChilds.Add(Camera[i]);
        }
       
        for (int i = 0; i < CameraChilds.Count; i++)
        {
            if (CameraChilds[i].GetComponent<fixedCameraRota>()!=null)
            {
                FixedCamera.Add(CameraChilds[i].name, CameraChilds[i].GetComponent<fixedCameraRota>());
            }
            if (CameraChilds[i].GetComponent<CinemachineFreeLook>()!=null)
            {
                CameraOffset = CameraChilds[i].GetComponent<CinemachineFreeLook>().GetComponent<CinemachineCameraOffset>();
                CameraPos = new Vector2(CameraChilds[i].GetComponent<CinemachineFreeLook>().m_XAxis.Value,
                    CameraChilds[i].GetComponent<CinemachineFreeLook>().m_YAxis.Value);
            }
           
        }
        for (int i = 0; i < CameraChilds.Count; i++)
        {
            if (CameraChilds[i].GetComponent<CinemachineFreeLook>() != null)
            {
                FreeLook = CameraChilds[i].GetComponent<CinemachineFreeLook>();
                break;
            }
            else if (i == CameraChilds.Count - 1)
            {
                int point = 0;
                LookCamera = false;
                IsFixedCamera = true;
                for (int j = 0; j < CameraChilds.Count ; j++)
                {
                    if (CameraChilds[j].m_Priority > point)
                    {
                        point = j;
                      
                    }
                }
                Isname = CameraChilds[point].name;
                FixedCamera[Isname].ISROTA = true;
      
            }
        }
            if (DollyMoveCam!=null)
        {
           Dollyname= DollyMoveCam.dollyCamera.name;
        }
        if (VolumeColor!=null)
        {
            profile = VolumeColor.profile.components;
        }

   

    }
    private void Start()
    {
        if (anchor != null)
        {

            anchor.RegistPrimaryTouchPosChange(PrimaryTouchPosChange);
            anchor.RegistPrimaryTouchDetaChange(PrimaryTouchDeltaCallBack);
            anchor.RegistZoomCallBack(ZoomCallBack);
            anchor.RegisOnZoomDistance(Zoom);
            // anchor.RegistThreeFingerDeltaCallBack(ThreeFingerDelta);

        }
     
        MainCamera.m_CameraActivatedEvent.AddListener(virCamerathis);

        //StartMoveCamera.AddListener(StartCameraName);

        Invoke("IsSecurity", 0.2f);
        StartCoroutine(EpicJudgment());
    }
    private void OnDisable()
    {
        if (anchor != null)
        {



            anchor.UnRegistPrimaryTouchPosChange(PrimaryTouchPosChange);
            anchor.UnRegistPrimaryTouchDetaChange(PrimaryTouchDeltaCallBack);
            anchor.UnRegistZoomCallBack(ZoomCallBack);
            anchor.UnRegisOnZoomDistance(Zoom);
            //anchor.UnRegistThreeFingerDeltaCallBack(ThreeFingerDelta);

        }
        MainCamera.m_CameraActivatedEvent.RemoveListener(virCamerathis);
   
        StopCoroutine(EpicJudgment());
    }
    #endregion

   

    #region FreeLookCameraStartPos
    public void FreeLookCameraPosSave()
    {
        if (FreeLook != null)
        {
            CameraPos = new Vector2(FreeLook.m_XAxis.Value, FreeLook.m_YAxis.Value);
        }

    }

    public void FreeLookBackStartPos(float time)
    {
        if (FreeLook != null)
        {
            DOTween.To(() => FreeLook.m_XAxis.Value, x => FreeLook.m_XAxis.Value = x, CameraPos.x, time);
            DOTween.To(() => FreeLook.m_YAxis.Value, x => FreeLook.m_YAxis.Value = x, CameraPos.y, time);
        }
    }
    #endregion

    #region CamerasEvent
    private void IsSecurity()
    {
        Security = true;
    }
 
    private void virCamerathis(ICinemachineCamera Endcamera, ICinemachineCamera Startcamera)
    {
       

        if (Security)
        {
           
            StartMoveCamera.Invoke(Startcamera.Name);
  
            EndCamera = Endcamera.Name;
            //if (Startcamera.Name == this.name)
            //{
            //    Debug.LogError("无" + Startcamera);
           
            //}
            if (Endcamera.Name == Dollyname)
            {
                
                if (DollyMoveCam != null)
                {

                    if (dollybool != true)
                    {

                        if (CameraOffset != null)
                        {
                            DOTween.To(() => CameraOffset.m_Offset, x => CameraOffset.m_Offset = x, Vector3.zero, 1f);
                        }
                        IsFixedCamera = false;
                        LookCamera = false;
                        dollybool = true;
                        Isname = "";
                        IsEnterCamera = "";
                        DollyMoveCam.StartToggle();
                        DollyMoveCam.cart.m_Speed = DollyCameraSpeed;
                        FreeLookCameraTransitions(true);
                    }


                }
                else
                {
                    Debug.LogError("请添加DollyMoveCamera组件");
                }
            }
            if (Startcamera.Name == Dollyname)
            {
                if (dollybool)
                {
                    DollyMoveCam.StopToggle();
                    FalseDollyAll();
                    FreeLookCameraTransitions(false);
                    DollyMoveCam.cart.m_Speed = 0f;
                    dollybool = false;
                }

            }
        }
    }
    #endregion

    #region SwitchCameras
    public void SwitchCamera(string CameraName,bool value = false)
    {
        for (int i = 0; i < CameraChilds.Count; i++)
        {
           
            if (CameraChilds[i].name == CameraName )
            {
                break;
            }
            else if (i == CameraChilds.Count - 1)
            {
                Debug.LogError("列表里没有" + CameraName + "的摄像机，请检查是否添加或名称是否正确");
                return;
            }

        }
   
      
            CameraDate(CameraName,value);
        
    }

    private void CameraDate(string name,bool value=false)
    {
       
        if (name != Isname)
        {
         
            Isname = name;
            StopAllCoroutines();
            //StopCoroutine(IsvirMoveCamera(names));
            //StopCoroutine(IsvirCamera(name));
            if (FixedCamera.ContainsKey(name))
            {
                
                if (value && IsFixedCamera != true)
                {
                    
                    CameraHandoverTime(0);
                    DrivenCamera.m_AnimatedTarget.Play(name);
                    FalseDollyAll();
                }
                else
                {
                
                    CameraHandoverTime(CameraStartTime);
                    if (FixedCamera[name].theIs2D == true && IsFixedCamera)
                    {
                        CameraHandoverTime(0);
                        DrivenCamera.m_AnimatedTarget.Play(name);
                        FalseDollyAll();

                    }
                    else if (FixedCamera[name].ISZHONG && FixedCamera[name].virCamera.Count != 0 && IsFixedCamera)
                    {
                        StartCoroutine(IsvirCamera(name));
                    }
                    else if (FixedCamera[name].IsOne && FixedCamera[name].virOneCamera.Count != 0 && IsFixedCamera != true)
                    {

                        StartCoroutine(IsvirCamera(name));
                        IsEnterCamera = name;
                     
                    }
                    else
                    {
                        DrivenCamera.m_AnimatedTarget.Play(name);
                        IsFixedCamera = true;
                    }
                }
               
                FixedCamera[name].ISROTA = true;
              
                LookCamera = false;
                if (CameraOffset != null)
                {
                    DOTween.To(() => CameraOffset.m_Offset, x => CameraOffset.m_Offset = x, Vector3.zero, 1f);
                }

            }
            else
            {
                
                foreach (var item in FixedCamera)
                {
                        item.Value.ISROTA = false;
                        item.Value.MoveStart();
                }
                if (value)
                {
                    MoveCamera(name, value);
                }
                else
                {
                    
                    if (FixedCamera.ContainsKey(DrivenCamera.LiveChild.Name))
                    {
                        
                        if (IsEnterCamera != null && IsEnterCamera != "")
                        {
                            if ( FixedCamera[DrivenCamera.LiveChild.Name].IsBreakMoveTransition)
                            {
                                StartCoroutine(IsvirMoveCamera(name));
                            }
                            else
                            {
                                MoveCamera(name);
                               
                            }
                            
                        }
                        else
                        {
                            //if (FixedCamera[DrivenCamera.LiveChild.Name].IsBreakMoveTransition)
                            //{
                            //    StartCoroutine(IsvirMoveTransition(name));
                            //}
                            //else
                            //{
                                MoveCamera(name);
                           // }
                             
                            
                          
                          
                        }
                    }
                    else
                    {
                        MoveCamera(name);
                      
                    }

                }
            }
        }
    }
    private void MoveCamera(string name,bool value=false)//非固定相机
    {

        if (value)
        {
            CameraHandoverTime(0);
            DrivenCamera.m_AnimatedTarget.Play(name);
            FalseDollyAll();
        }
        else
        {
            CameraHandoverTime(CameraStartTime);
            DrivenCamera.m_AnimatedTarget.Play(name);
        }
        IsEnterCamera = "";
        IsFixedCamera = false;
        LookCamera = true;
    }
    bool CamIsBlend;
    IEnumerator IsvirMoveCamera(string name)
    {
        int i = 0;
        if (FixedCamera[DrivenCamera.LiveChild.Name].virOneCamera.Count !=0)
        {
             i = FixedCamera[DrivenCamera.LiveChild.Name].virOneCamera.Count - 1;
            DrivenCamera.m_AnimatedTarget.Play(FixedCamera[DrivenCamera.LiveChild.Name].virOneCamera[i].name);
            yield return null;
        }
        else
        {
            DrivenCamera.m_AnimatedTarget.Play(FixedCamera[IsEnterCamera].virOneCamera[i].name);
            yield return null;
        }
      
     
       
        CameraHandoverTime(CameraStartTime);
        while (true)
        {

            if (CamIsBlend != DrivenCamera.IsBlending)
            {
                if (CamIsBlend)
                {
                    i--;
                    if (i < 0)
                    {

                        IsEnterCamera = "";
                        IsFixedCamera = false;
                        LookCamera = true;
                        DrivenCamera.m_AnimatedTarget.Play(name);
                        StopCoroutine(IsvirMoveCamera(name));
                        break;
                    }
                    DrivenCamera.m_AnimatedTarget.Play(FixedCamera[IsEnterCamera].virOneCamera[i].name);
                  
                }
                CamIsBlend = DrivenCamera.IsBlending;
            }
            yield return null;
          
        }
    }


    //IEnumerator IsvirMoveTransition(string name)
    //{

    //}

    bool Blending;
    IEnumerator IsvirCamera(string name)
    {
        
        int i = 0;
        if (IsFixedCamera)
        {
            DrivenCamera.m_AnimatedTarget.Play(FixedCamera[name].virCamera[i].name);
            yield return null;
        }
        else
        {
            DrivenCamera.m_AnimatedTarget.Play(FixedCamera[name].virOneCamera[i].name);
            yield return null;
        }
       
        while (true)
        {

            
            if (IsFixedCamera)
            {

               
                if (Blending != DrivenCamera.IsBlending)
                {
                    if (Blending)
                    {
                        //有问题
                       
                  
                        i++;
                        if (i >FixedCamera[name].virCamera.Count-1 )
                        {
                            
                            DrivenCamera.m_AnimatedTarget.Play(name);
                            StopCoroutine(IsvirCamera(name));
                            break;
                        }
                        DrivenCamera.m_AnimatedTarget.Play(FixedCamera[name].virCamera[i].name);
                    }
                    Blending = DrivenCamera.IsBlending;
                }
              
            }
            else
            {
              
                if (Blending != DrivenCamera.IsBlending)
                {
                    if (Blending)
                    {
                        i++;
                        if (i > FixedCamera[name].virOneCamera.Count - 1)
                        {

                            DrivenCamera.m_AnimatedTarget.Play(name);
                            IsFixedCamera = true;
                            StopCoroutine(IsvirCamera(name));
                            break;
                        }
                        DrivenCamera.m_AnimatedTarget.Play(FixedCamera[name].virOneCamera[i].name);
                    }
                    Blending = DrivenCamera.IsBlending;
                }
            }
          
            yield return null;
           
          
        }
    }
    private void FreeLookCameraTransitions(bool value)
    {
        float time = 0;
       
        DOTween.To(() => time, x => time = x, 1, 0.5f).OnComplete(() => {
            FreeLook.m_Transitions.m_InheritPosition = value;
        });

    }
    private void CameraHandoverTime(float time)
    {
        DrivenCamera.m_DefaultBlend.m_Time = time;
    }
    #endregion

    #region ThisTouch
    private void PrimaryTouchPosChange(Vector2 vector)
    {
      
        if (IsFixedCamera&& FixedCamera.ContainsKey(Isname))
        {
           
            if (FixedCamera[Isname].ISROTA && FixedCamera[Isname].therota)
            {
               
                if (TouTapVetor != Vector2.zero)
                {
                    
                    float MoveX = (TouTapVetor.x - vector.x) * 0.1f;
                    float MoveY = (TouTapVetor.y - vector.y) * 0.1f;

                    if (vector.x > 0.1f || vector.x < -0.1f)
                    {

                        FixedCamera[Isname].GetComponent<CinemachineVirtualCamera>()
                            .GetCinemachineComponent<CinemachinePOV>().m_HorizontalAxis.m_InputAxisValue = MoveX;
                    
                    }
                    if (vector.y > 0.1f || vector.y < -0.1f)
                    {

                        FixedCamera[Isname].GetComponent<CinemachineVirtualCamera>()
                           .GetCinemachineComponent<CinemachinePOV>().m_VerticalAxis.m_InputAxisValue = MoveY;

                    }
                 
                }

                TouTapVetor = vector;
            }
        }

        if (LookCamera)
        {

           
            if (TouTapVetor != Vector2.zero)
            {

                float MoveX = (TouTapVetor.x - vector.x) * 0.1f;
                float MoveY = (TouTapVetor.y - vector.y) * 0.1f;


                if (MoveX > 0.1f || MoveX < -0.1f)
                {
                    FreeLook.m_XAxis.m_InputAxisValue = MoveX;
                
                }
                if (MoveY > 0.1f || MoveY < -0.1f)
                {
                    FreeLook.m_YAxis.m_InputAxisValue = MoveY;

                }

             

            }
            TouTapVetor = vector;

        }
    }

    private void PrimaryTouchDeltaCallBack(Vector2 vector)
    {
        float timex = 0;
        float timey = 0;
        if (IsFixedCamera && FixedCamera.ContainsKey(Isname))
        {
            if (FixedCamera[Isname].ISROTA && FixedCamera[Isname].therota)
            {

                float MoveX = vector.x * 0.1f;
                float MoveY = vector.y * 0.1f;
                if (vector.x > 0.1f || vector.x < -0.1f)
                {
                    DOTween.To(() => timex, x => timex = x, 1, 0.1f).OnUpdate(() => {
                        FixedCamera[Isname].GetComponent<CinemachineVirtualCamera>()
                            .GetCinemachineComponent<CinemachinePOV>().m_HorizontalAxis.m_InputAxisValue = MoveX;

                    });
                }

                if (vector.y > 0.1f || vector.y < -0.1f)
                {
                    DOTween.To(() => timey, x => timey = x, 1, 0.1f).OnUpdate(() => {
                        FixedCamera[Isname].GetComponent<CinemachineVirtualCamera>()
                           .GetCinemachineComponent<CinemachinePOV>().m_VerticalAxis.m_InputAxisValue = MoveY;

                    });
                }
             
                TouTapVetor = vector;
            }
        }

        if (LookCamera)
        {

            float MoveX = vector.x * 0.1f;
            float MoveY = vector.y * 0.1f;
            if (MoveX > 0.1f || MoveX < -0.1f)
            {
                DOTween.To(() => timex, x => timex = x, 1, 0.1f).OnUpdate(() => {
                    FreeLook.m_XAxis.m_InputAxisValue = MoveX;

                });
            }

            if (MoveY > 0.1f || MoveY < -0.1f)
            {

                DOTween.To(() => timey, x => timey = x, 1, 0.1f).OnUpdate(() => {
                    FreeLook.m_YAxis.m_InputAxisValue = MoveY;

                });
            }
           
            TouTapVetor = vector;
        }

    }
    private void ZoomCallBack(ZoomType zoom)
    {
        if (LookCamera)
        {
            switch (zoom)
            {

                case ZoomType.ZoomIn:
                    if (anchor.ZoomDistance > 0.1f)
                    {
                        FreeLook.m_Lens.FieldOfView -= anchor.ZoomDistance * Time.deltaTime * freeLookZoomSpeed * 0.005f;
                        FreeLook.m_Lens.FieldOfView = Mathf.Clamp(FreeLook.m_Lens.FieldOfView, freeLookZoomMin, freeLookZoomMax);
                    }

                    break;
                case ZoomType.ZoomOut:
                    if (anchor.ZoomDistance > 0.1f)
                    {
                        FreeLook.m_Lens.FieldOfView += anchor.ZoomDistance * Time.deltaTime * freeLookZoomSpeed * 0.005f;
                        FreeLook.m_Lens.FieldOfView = Mathf.Clamp(FreeLook.m_Lens.FieldOfView, freeLookZoomMin, freeLookZoomMax);
                    }

                    break;
                default:
                    break;
            }

        }
        if (IsFixedCamera)
        {
            switch (zoom)
            {
                case ZoomType.ZoomIn:
                    if (anchor.ZoomDistance > 1f)
                    {
                        FixedCamera[Isname].GetComponent<CinemachineVirtualCamera>().m_Lens.FieldOfView -= anchor.ZoomDistance * Time.deltaTime * FixedZoomSpeed * 0.005f;
                        FixedCamera[Isname].GetComponent<CinemachineVirtualCamera>().m_Lens.FieldOfView = Mathf.Clamp(FixedCamera[Isname]
                            .GetComponent<CinemachineVirtualCamera>().m_Lens.FieldOfView, FixedZoomMin, FixedZoomMax);
                    }

                    break;
                case ZoomType.ZoomOut:
                    if (anchor.ZoomDistance > 1f)
                    {


                        FixedCamera[Isname].GetComponent<CinemachineVirtualCamera>().m_Lens.FieldOfView += anchor.ZoomDistance * Time.deltaTime * FixedZoomSpeed * 0.005f;
                        FixedCamera[Isname].GetComponent<CinemachineVirtualCamera>().m_Lens.FieldOfView = Mathf.Clamp(FixedCamera[Isname]
                            .GetComponent<CinemachineVirtualCamera>().m_Lens.FieldOfView, FixedZoomMin, FixedZoomMax);
                    }
                    break;

                default:
                    break;
            }
        }
    }

    private void Zoom(float value)
    {

        if (IsFixedCamera)
        {
            FixedCamera[Isname].GetComponent<CinemachineVirtualCamera>().m_Lens.FieldOfView -= value;
            FixedCamera[Isname].GetComponent<CinemachineVirtualCamera>().m_Lens.FieldOfView = Mathf.Clamp(FixedCamera[Isname]
                .GetComponent<CinemachineVirtualCamera>().m_Lens.FieldOfView, FixedZoomMin, FixedZoomMax);
        }
        if (LookCamera)
        {

            FreeLook.m_Lens.FieldOfView -= value;
            FreeLook.m_Lens.FieldOfView = Mathf.Clamp(FreeLook.m_Lens.FieldOfView, freeLookZoomMin, freeLookZoomMax);
        }
    }
    IEnumerator EpicJudgment()
    {
        Vector2 FixedCameraRota = Vector2.zero;
        Vector2 FreeLookCameraRota = Vector2.zero;
        Vector2 touValue = Vector2.zero;
        if (Isname != "" && Isname != null && FixedCamera.ContainsKey(Isname))
            FixedCameraRota = new Vector2(FixedCamera[Isname].GetComponent<CinemachineVirtualCamera>().GetCinemachineComponent<CinemachinePOV>().m_HorizontalAxis.Value,
            FixedCamera[Isname].GetComponent<CinemachineVirtualCamera>().GetCinemachineComponent<CinemachinePOV>().m_VerticalAxis.Value);
        if (FreeLook != null)
            FreeLookCameraRota = new Vector2(FreeLook.m_XAxis.Value, FreeLook.m_YAxis.Value);

        while (true)
        {
            
            if (TouTapVetor == touValue)
            {
               
                TouTapVetor = Vector2.zero;
                if (IsFixedCamera && FixedCamera.ContainsKey(Isname))
                {
                    if (FixedCamera.Count != 0)
                    {
                        FixedCamera[Isname].GetComponent<CinemachineVirtualCamera>().GetCinemachineComponent<CinemachinePOV>().m_VerticalAxis.m_InputAxisValue = 0;
                        FixedCamera[Isname].GetComponent<CinemachineVirtualCamera>().GetCinemachineComponent<CinemachinePOV>().m_HorizontalAxis.m_InputAxisValue = 0;
                       
                    }

                }
                if (LookCamera)
                {
                    if (FreeLook != null)
                    {
                        FreeLook.m_XAxis.m_InputAxisValue = 0;
                        FreeLook.m_YAxis.m_InputAxisValue = 0;
                    }

                }

            }
           
        
             yield return null;
            if (Isname != "" && Isname != null&& FixedCamera.ContainsKey(Isname))
            {
                if (FixedCameraRota.x != FixedCamera[Isname].GetComponent<CinemachineVirtualCamera>().GetCinemachineComponent<CinemachinePOV>().m_HorizontalAxis.Value
                       || FixedCameraRota.y != FixedCamera[Isname].GetComponent<CinemachineVirtualCamera>().GetCinemachineComponent<CinemachinePOV>().m_VerticalAxis.Value)
                {
                    OnFixedCamerasRota?.Invoke(new Vector2(FixedCamera[Isname].GetComponent<CinemachineVirtualCamera>()
                        .GetCinemachineComponent<CinemachinePOV>().m_HorizontalAxis.Value,
                            FixedCamera[Isname].GetComponent<CinemachineVirtualCamera>()
                            .GetCinemachineComponent<CinemachinePOV>().m_VerticalAxis.Value
                        ));
                } 
            }
            if (FreeLook!=null)
            {
                if (FreeLookCameraRota.x != FreeLook.m_XAxis.Value || FreeLookCameraRota.y != FreeLook.m_YAxis.Value)
                {
                    OnFreeLookCameraRota?.Invoke(new Vector2(FreeLook.m_XAxis.Value, FreeLook.m_YAxis.Value));
                } 
            }


            if (CamIsBlending != DrivenCamera.IsBlending)
            {
                if (CamIsBlending)
                {
                    MoveCameraArrival?.Invoke(EndCamera);
                    EndCamera = "";
                }
                CamIsBlending = DrivenCamera.IsBlending;
            }
            yield return new WaitForSeconds(0.1f);
            touValue = TouTapVetor;
            if (Isname != "" && Isname != null && FixedCamera.ContainsKey(Isname))
                FixedCameraRota = new Vector2(FixedCamera[Isname].GetComponent<CinemachineVirtualCamera>().GetCinemachineComponent<CinemachinePOV>().m_HorizontalAxis.Value, 
                FixedCamera[Isname].GetComponent<CinemachineVirtualCamera>().GetCinemachineComponent<CinemachinePOV>().m_VerticalAxis.Value );
            if (FreeLook != null)
                FreeLookCameraRota = new Vector2(FreeLook.m_XAxis.Value, FreeLook.m_YAxis.Value);
        }
    }
    #endregion

    #region FalseDolly
    private void FalseDollyAll()
    {
      
        if (Black)
        {
            if (VolumeColor != null)
            {
                DOTween.To(() => AColor, x => AColor = x, 0, 0.1f).OnUpdate(() => {

                    profile[0].parameters[2].SetValue(new ColorParameter(new Color(AColor, AColor, AColor)));

                }).OnComplete(() => {
                    DOTween.To(() => AColor, x => AColor = x, 1, blackCameraTime).OnUpdate(() =>
                    {

                        profile[0].parameters[2].SetValue(new ColorParameter(new Color(AColor, AColor, AColor)));


                    });
                });
            }
           


        }
    }
    #endregion
}
