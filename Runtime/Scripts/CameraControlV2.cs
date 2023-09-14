using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.Events;
using DG.Tweening;
using UnityEngine.Rendering;

public class CameraControlV2 : MonoBehaviour
{
    
    [SerializeField] DollyMoveCamera DollyMoveCam;
    [SerializeField] CinemachineFreeLook freeLook;
    [SerializeField] CinemachineCameraOffset CameraOffset;//第三人称偏移
    [SerializeField] List<CinemachineVirtualCamera> fixedCamera;//固定
    [SerializeField] List<string> OneMoveCameraEnd;
    [SerializeField] CinemachineBrain MainCamera;

    public UnityEvent<string> MoveFixedCameraArrival;//切换到fixedCamera
    public UnityEvent MoveFixedCamera;//进入车内
    public UnityEvent MoveFreeLookCamera;//返回第三相机
    public UnityEvent OnDollyCamera;//是在轨道移动
    public UnityEvent OnFreeLookCameraRota;//在旋转
    public UnityEvent OnFixedCamerasRota;//在旋转

    [Header("FreelookCameraInput")]
    [SerializeField] CameraInputTou anchor;
    [SerializeField] float freeLookXSpeed = 4f;
    [SerializeField] float freeLookYSpeed = 1f;
    [SerializeField] float freeLookZoomSpeed = 4f;
    [SerializeField] float freeLookZoomMin = 15f;
    [SerializeField] float freeLookZoomMax = 80f;
    [SerializeField] bool Black = false;
    [SerializeField] float blackCameraTime = 0.5f;//没有缓慢变黑

    [Header("fixedCameraInput")]
    [SerializeField] float FixedXSpeed = 4f;
    [SerializeField] float FixedYSpeed = 1f;
    [SerializeField] float FixedZoomSpeed = 2f;
    [SerializeField] float FixedZoomMin = 15f;
    [SerializeField] float FixedZoomMax = 40f;


    [Header("fixedCameraRestrict")]
    [SerializeField] bool fixedIsClamp;
    [SerializeField] float FixedRotaMinX;
    [SerializeField] float FixedRotaMaxX;
    [SerializeField] float FixedRotaMinY;
    [SerializeField] float FixedRotaMaxY;

    [Header("DollyCameraSpeed")]
    [SerializeField] float DollyCameraSpeed=0.3f;

    private string IsEnterCamera;
    private float CameraStartTime;
    private Vector2 CameraPos;
    private string names;
    private string Isname;
    private bool dollybool;

    private bool IsFixed = true;
    private bool LookCameraRota = true;
    private bool FixedCameraRota = true;
    private bool LookCamera = true;
    private bool FixedCamera;
    private bool IsvirArrival = true;

    private Vector2 TouTapVetor;
    private Dictionary<string, CinemachineVirtualCamera> Camerapairs = new Dictionary<string, CinemachineVirtualCamera>();
    private void Awake()
    {
        if (freeLook != null)
        {
            CameraPos = new Vector2(freeLook.m_XAxis.Value, freeLook.m_YAxis.Value);
        }
        else
        {
            Debug.LogError("请添加第三人称虚拟相机");
        }
        if (MainCamera != null)
        {
            CameraStartTime = MainCamera.m_DefaultBlend.m_Time;
        }
        else
        {
            Debug.LogError("请添加虚拟主相机");
        }
    }
    void Start()
    {
        if (anchor != null)
        {
            anchor.RegistPrimaryTouchTapCallBack(TouchTap);
            anchor.RegistPrimaryTouchPosChange(PrimaryTouchPosChange);
            anchor.RegistPrimaryTouchDetaChange(PrimaryTouchDeltaCallBack);
            anchor.RegistZoomCallBack(ZoomCallBack);
            // anchor.RegistThreeFingerDeltaCallBack(ThreeFingerDelta);
            MainCamera.m_CameraActivatedEvent.AddListener(virCamerathis);
        }
       
       
    }
    private void OnDisable()
    {
        if (anchor != null)
        {


            anchor.UnRegistPrimaryTouchTapCallBack(TouchTap);
            anchor.UnRegistPrimaryTouchPosChange(PrimaryTouchPosChange);
            anchor.UnRegistPrimaryTouchDetaChange(PrimaryTouchDeltaCallBack);
            anchor.UnRegistZoomCallBack(ZoomCallBack);
            //anchor.UnRegistThreeFingerDeltaCallBack(ThreeFingerDelta);
            MainCamera.m_CameraActivatedEvent.RemoveListener(virCamerathis);
        }
      

    }

    private void virCamerathis(ICinemachineCamera camera1,ICinemachineCamera camera2)
    {
        if (IsvirArrival)
        {
            
            float time = 0;
            if (OneMoveCameraEnd.Count != 0)
            {
                for (int j = 0; j < OneMoveCameraEnd.Count; j++)
                {
                    if (OneMoveCameraEnd[j] == camera1.Name)
                    {
                       
                        DOTween.To(() => time, x => time = x, 1, CameraStartTime).OnComplete(() =>
                        {
                            MoveFixedCameraArrival?.Invoke(name);
                        }).SetId("movefixed");
                        IsvirArrival = false;
                       
                    }
                }
                if (MainCamera.m_DefaultBlend.m_Time == 0)
                {
                    MoveFixedCameraArrival?.Invoke(name);
                    IsvirArrival = false;
                }
            }
        }
    }

    #region FreeLookCameraStartPos
    public void FreeLookCameraPosSave()
    {
        if (freeLook != null)
        {
            CameraPos = new Vector2(freeLook.m_XAxis.Value, freeLook.m_YAxis.Value);
        }

    }

    public void FreeLookBackStartPos(float time)
    {
        if (freeLook != null)
        {
            DOTween.To(() => freeLook.m_XAxis.Value, x => freeLook.m_XAxis.Value = x, CameraPos.x, time);
            DOTween.To(() => freeLook.m_YAxis.Value, x => freeLook.m_YAxis.Value = x, CameraPos.y, time);
        }
    }
    #endregion

    #region fixedCamera
    public void fixedCameraName(string name,bool value=false)
    {
        if (Camerapairs.ContainsKey(name))
        {
            if (name != Isname)
            {
                if (Camerapairs[name].isActiveAndEnabled)
                {

                        fixedMovedate(name,value);        
                }
                else
                {
                    Debug.LogError("列表中的相机没有被激活:" + name);
                }
              
            }
        }
        else
        {
            if (fixedCamera.Count != 0)
            {
                if (fixedCamera.Exists(t => t.Name == name))
                {
                    if (fixedCamera.Exists(t => t.isActiveAndEnabled == true))
                    {
                        for (int i = 0; i < fixedCamera.Count; i++)
                        {
                            if (fixedCamera[i].gameObject.name == name)
                            {

                                Camerapairs.Add(name, fixedCamera[i]);
                                fixedMovedate(name, value);
                                break;
                            }

                        }
                    }
                    else
                    {
                        Debug.LogError("列表中的相机没有被激活:" + name);
                    }
                    
                }
                else
                {
                    Debug.LogError("列表没有这个名称的相机:" + name);
                }
            }
            else
            {
                Debug.LogError("没有相机被添加到列表");
            }
        }
    }
    private void CameraHandoverTime(float time)
    {
        
        MainCamera.m_DefaultBlend.m_Time = time;
       

    }
  
    private void fixedMovedate(string name,bool value)
    {
        CameraPrioritys(Camerapairs, DolllyCamera: DollyMoveCam);
        if (value&& FixedCamera != true)
        {
            CameraHandoverTime(0);
            Camerapairs[name].Priority = 11;
            FalseDollyAll();
        }
        else
        {
            CameraHandoverTime(CameraStartTime);
            if (Camerapairs[name].gameObject.GetComponent<fixedCameraRota>().ISZHONG && Camerapairs[name].gameObject.GetComponent<fixedCameraRota>().virCamera.Count != 0 && FixedCamera)
            {
                StartCoroutine(IsvirCameraZhong(name, MainCamera.m_DefaultBlend.m_Time));
            }
            else if (Camerapairs[name].gameObject.GetComponent<fixedCameraRota>().IsOne && Camerapairs[name].gameObject.GetComponent<fixedCameraRota>().virOneCamera.Count != 0
    && FixedCamera != true)
            {

                StartCoroutine(IsvirOneCamera(name, MainCamera.m_DefaultBlend.m_Time));
                IsEnterCamera = name;
            }
            else
            {
                Camerapairs[name].Priority = 11;
            }
        }
        Isname = name;
        Camerapairs[name].GetComponent<fixedCameraRota>().ISROTA = true;
        FixedCamera = true;
        LookCamera = false;
        dollybool = false;
        if (CameraOffset != null)
        {
            DOTween.To(() => CameraOffset.m_Offset, x => CameraOffset.m_Offset = x, Vector3.zero, 1f);
        }
        if (IsFixed)
        {
           
            IsvirArrival = true;
            MoveFixedCamera?.Invoke();
            IsFixed = false;
        }
        if (Camerapairs[name].GetComponent<fixedCameraRota>() != null)
        {
            Camerapairs[name].GetComponent<fixedCameraRota>().ISROTA = true;
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
    IEnumerator IsvirCameraZhong(string name, float time)
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

    #region FalseDolly
    private void FalseDollyAll()
    {

        if (Black)
        {
            DOTween.To(() => DollyMoveCam.AColor, x => DollyMoveCam.AColor = x, 0, 0.1f).OnUpdate(() => {

                DollyMoveCam.profile[0].parameters[2].SetValue(new ColorParameter(new Color(DollyMoveCam.AColor, DollyMoveCam.AColor, DollyMoveCam.AColor)));

            }).OnComplete(() => {
                DOTween.To(() => DollyMoveCam.AColor, x => DollyMoveCam.AColor = x, 1, blackCameraTime).OnUpdate(() =>
                {

                    DollyMoveCam.profile[0].parameters[2].SetValue(new ColorParameter(new Color(DollyMoveCam.AColor, DollyMoveCam.AColor, DollyMoveCam.AColor)));


                });
            });


        }
    }
    #endregion

    #region FreeLookCamera
    public void FreeLookCameraRotaLatency(Vector2 xy, float time, float time1)
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
    public void FreeLookCamera(bool value=false)
    {
        if (freeLook == null)
        {
            Debug.LogError("请添加第三人称虚拟相机");
        }
        else
        {
            if (value)
            {
                BrackCamera();
            }
            else
            {
                if (IsEnterCamera != null && IsEnterCamera != "")
                {
                    StartCoroutine(IsvirBrackCamera(IsEnterCamera, MainCamera.m_DefaultBlend.m_Time));
                }
                else
                {
                    BrackCamera();
                }
            }



        }
    }
    public void FreeLook2DCamera()
    {
        if (freeLook == null)
        {
            Debug.LogError("请添加第三人称虚拟相机");
        }
        else
        {
            CameraHandoverTime(0);
            CameraPrioritys(Camerapairs, DolllyCamera: DollyMoveCam);
            StopCoroutine(IsvirOneCamera(Isname, MainCamera.m_DefaultBlend.m_Time));
            freeLook.m_Priority = 11;
            Isname = "";
            IsEnterCamera = "";
            IsFixed = true;
            DOTween.Kill("movefixed");
            if (LookCamera!=true)
            {
                FalseDollyAll();
                MoveFreeLookCamera?.Invoke();
            }
            dollybool = false;
            FixedCamera = false;
            LookCamera = true;
          
        }
    }
    private void BrackCamera()
    {
        CameraHandoverTime(CameraStartTime);
        CameraPrioritys(Camerapairs, DolllyCamera: DollyMoveCam);
        freeLook.m_Priority = 11;
        StopCoroutine(IsvirOneCamera(Isname, MainCamera.m_DefaultBlend.m_Time));
        Isname = "";
        IsEnterCamera = "";
        IsFixed = true;
        DOTween.Kill("movefixed");
        if (LookCamera != true)
        {

            MoveFreeLookCamera?.Invoke();
        }
        dollybool = false;
        FixedCamera = false;
        LookCamera = true;

    }
    IEnumerator IsvirBrackCamera(string name, float time)
    {
       
        int i = Camerapairs[name].gameObject.GetComponent<fixedCameraRota>().virOneCamera.Count - 1;
        CameraHandoverTime(CameraStartTime);
        CameraPrioritys(Camerapairs, DolllyCamera: DollyMoveCam);
        dollybool = false;
        FixedCamera = false;
        LookCamera = true;
        IsFixed = true;
        Isname = "";
        IsEnterCamera = "";
        DOTween.Kill("movefixed");
        while (true)
        {
            Camerapairs[name].gameObject.GetComponent<fixedCameraRota>().virOneCamera[i].m_Priority = 11;
            yield return new WaitForSeconds(time);
            Camerapairs[name].gameObject.GetComponent<fixedCameraRota>().virOneCamera[i].m_Priority = 10;
            i--;
            if (i < Camerapairs[name].gameObject.GetComponent<fixedCameraRota>().virOneCamera.Count - 1)
            {
                freeLook.m_Priority = 11;
                StopCoroutine(IsvirBrackCamera(names, MainCamera.m_DefaultBlend.m_Time));
                break;
            }
        }
    }


    #endregion

    #region DollCamera
    public void DollyCamera()//轨道
    {
        if (DollyMoveCam != null)
        {
           
            if (dollybool!=true)
            {
                CameraPrioritys(Camerapairs, freeLook);
                CameraHandoverTime(0);
                if (CameraOffset != null)
                {
                    DOTween.To(() => CameraOffset.m_Offset, x => CameraOffset.m_Offset = x, Vector3.zero, 1f);
                }
                OnDollyCamera?.Invoke();
                DollyMoveCam.dollyCamera.m_Priority = 11;
                FixedCamera = false;
                LookCamera = false;
                dollybool = true;
                IsFixed = true;
                Isname = "";
                IsEnterCamera = "";
                DOTween.Kill("movefixed");
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

    private void FreeLookCameraTransitions(bool value)
    {
        float time = 0;
        DOTween.To(() => time, x => time = x, 1, 0.5f).OnComplete(() => {
            freeLook.m_Transitions.m_InheritPosition = value;
        });

    }
    #endregion

    #region ThisTouch
    private void TouchTap(Vector2 vector)
    {
        TouTapVetor = Vector2.zero;
        if (FixedCamera)
        {
            if (Camerapairs.Count != 0)
            {

                if (Camerapairs[Isname].GetCinemachineComponent<CinemachinePOV>().m_VerticalAxis.m_InputAxisValue != 0 ||
               Camerapairs[Isname].GetCinemachineComponent<CinemachinePOV>().m_HorizontalAxis.m_InputAxisValue != 0)
                {
                    Camerapairs[Isname].GetCinemachineComponent<CinemachinePOV>().m_VerticalAxis.m_InputAxisValue = 0;
                    Camerapairs[Isname].GetCinemachineComponent<CinemachinePOV>().m_HorizontalAxis.m_InputAxisValue = 0;
                    FixedCameraRota = true;

                }


            }


        }
        if (LookCamera)
        {
            if (freeLook != null)
            {

                if (freeLook.m_XAxis.m_InputAxisValue != 0 || freeLook.m_YAxis.m_InputAxisValue != 0)
                {
                    freeLook.m_XAxis.m_InputAxisValue = 0;
                    freeLook.m_YAxis.m_InputAxisValue = 0;
                    LookCameraRota = true;
                   
                }

            }


        }
    }

    private void PrimaryTouchPosChange(Vector2 vector)
    {
        
        if (TouTapVetor.x - vector.x == 0 && TouTapVetor.y - vector.y == 0)
        {
            TouchTap(vector);
        }
        if (FixedCamera)
        {
            if (Camerapairs[Isname].GetComponent<fixedCameraRota>().ISROTA && Camerapairs[Isname].GetComponent<fixedCameraRota>().therota)
            {

                if (TouTapVetor != Vector2.zero)
                {
                    float MoveX = (TouTapVetor.x - vector.x) * FixedXSpeed*0.002f;
                    float MoveY = (TouTapVetor.y - vector.y) * FixedYSpeed * 0.002f;
                    if (vector.x > 0.1f || vector.x < -0.1f)
                    {

                        Camerapairs[Isname].GetCinemachineComponent<CinemachinePOV>().m_HorizontalAxis.m_InputAxisValue = MoveX;

                    }
                    if (vector.y > 0.1f || vector.y < -0.1f)
                    {

                        Camerapairs[Isname].GetCinemachineComponent<CinemachinePOV>().m_VerticalAxis.m_InputAxisValue =  MoveY;
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

                        OnFixedCamerasRota?.Invoke();
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

                float MoveX = (TouTapVetor.x - vector.x) * freeLookXSpeed *0.002f;
                float MoveY = (TouTapVetor.y - vector.y) * freeLookYSpeed * 0.002f;
                if (MoveX > 0.1f || MoveX < -0.1f)
                {
                    freeLook.m_XAxis.m_InputAxisValue = MoveX;

                }
                if (MoveY > 0.1f || MoveY < -0.1f)
                {
                    freeLook.m_YAxis.m_InputAxisValue = MoveY;

                }
                if (LookCameraRota)
                {

                    OnFreeLookCameraRota?.Invoke();
                    LookCameraRota = false;
                }

            }


            TouTapVetor = vector;

        }
    }

    private void PrimaryTouchDeltaCallBack(Vector2 vector)
    {
        float timex = 0;
        float timey = 0;
        if (FixedCamera)
        {
            if (Camerapairs[Isname].GetComponent<fixedCameraRota>().ISROTA && Camerapairs[Isname].GetComponent<fixedCameraRota>().therota)
            {


                float MoveX = vector.x * FixedXSpeed * 0.0005f;
                float MoveY = vector.y * FixedXSpeed * 0.0005f;
                if (vector.x > 0.1f || vector.x < -0.1f)
                    {
                    DOTween.To(() => timex, x => timex = x, 1, 0.1f).OnUpdate(() => {
                        Camerapairs[Isname].GetCinemachineComponent<CinemachinePOV>().m_HorizontalAxis.m_InputAxisValue  = MoveX;

                    }).OnComplete(() => {

                        DOTween.To(() => Camerapairs[Isname].GetCinemachineComponent<CinemachinePOV>().m_HorizontalAxis.m_InputAxisValue, 
                            x => Camerapairs[Isname].GetCinemachineComponent<CinemachinePOV>().m_HorizontalAxis.m_InputAxisValue = x, 0, 0.1f).OnComplete(() => {
                                FixedCameraRota = true;
                            }); 
                        FixedCameraRota = true;
                    });
                    }

                    if (vector.y > 0.1f || vector.y < -0.1f)
                    {
                    DOTween.To(() => timey, x => timey = x, 1, 0.1f).OnUpdate(() => {
                        Camerapairs[Isname].GetCinemachineComponent<CinemachinePOV>().m_VerticalAxis.m_InputAxisValue = MoveY;

                    }).OnComplete(() => {

                        DOTween.To(() => Camerapairs[Isname].GetCinemachineComponent<CinemachinePOV>().m_VerticalAxis.m_InputAxisValue,
                            x => Camerapairs[Isname].GetCinemachineComponent<CinemachinePOV>().m_VerticalAxis.m_InputAxisValue = x, 0, 0.1f).OnComplete(() => {
                                FixedCameraRota = true;
                            });
                   
                    });
                 
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

                        OnFixedCamerasRota?.Invoke();
                        FixedCameraRota = false;
                    }
                
              
            }
        }

        if (LookCamera)
        {

                float MoveX = vector.x * freeLookXSpeed * 0.005f;
                float MoveY = vector.y * freeLookYSpeed * 0.005f;
                if (MoveX > 0.1f || MoveX < -0.1f)
                {
                DOTween.To(() => timex, x => timex = x, 1, 0.1f).OnUpdate(()=> {
                    freeLook.m_XAxis.m_InputAxisValue = MoveX;

                }).OnComplete(() => {
                   
                    DOTween.To(() => freeLook.m_XAxis.m_InputAxisValue, x => freeLook.m_XAxis.m_InputAxisValue = x, 0, 0.1f).OnComplete(()=> {
                        LookCameraRota = true;
                    });
                });
                }

                if (MoveY > 0.1f || MoveY < -0.1f)
                {
               
                    DOTween.To(() => timey, x => timey = x, 1, 0.1f).OnUpdate(() => {
                        freeLook.m_YAxis.m_InputAxisValue = MoveY;

                    }).OnComplete(() => {
                       
                        DOTween.To(() => freeLook.m_YAxis.m_InputAxisValue, x => freeLook.m_YAxis.m_InputAxisValue = x, 0, 0.1f).OnComplete(() => {
                            LookCameraRota = true;
                        });
                    });
                }
                if (LookCameraRota)
                {

                    OnFreeLookCameraRota?.Invoke();
                    LookCameraRota = false;
                }

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
                        freeLook.m_Lens.FieldOfView -=  anchor.ZoomDistance * Time.deltaTime * freeLookZoomSpeed*0.005f;
                        freeLook.m_Lens.FieldOfView = Mathf.Clamp(freeLook.m_Lens.FieldOfView, freeLookZoomMin, freeLookZoomMax);
                    }

                    break;
                case ZoomType.ZoomOut:
                    if (anchor.ZoomDistance > 0.5f)
                    {
                        freeLook.m_Lens.FieldOfView +=anchor.ZoomDistance * Time.deltaTime * freeLookZoomSpeed*0.005f;
                        freeLook.m_Lens.FieldOfView = Mathf.Clamp(freeLook.m_Lens.FieldOfView, freeLookZoomMin, freeLookZoomMax);
                    }

                    break;
                default:
                    break;
            }

        }
        if (FixedCamera)
        {
            switch (zoom)
            {
                case ZoomType.ZoomIn:
                    if (anchor.ZoomDistance > 1f)
                    {
                        Camerapairs[Isname].m_Lens.FieldOfView -= anchor.ZoomDistance * Time.deltaTime * FixedZoomSpeed*0.005f;
                        Camerapairs[Isname].m_Lens.FieldOfView = Mathf.Clamp(Camerapairs[Isname].m_Lens.FieldOfView, FixedZoomMin, FixedZoomMax);
                    }

                    break;
                case ZoomType.ZoomOut:
                    if (anchor.ZoomDistance > 1f)
                    {


                        Camerapairs[Isname].m_Lens.FieldOfView += anchor.ZoomDistance * Time.deltaTime * FixedZoomSpeed*0.005f;
                        Camerapairs[Isname].m_Lens.FieldOfView = Mathf.Clamp(Camerapairs[Isname].m_Lens.FieldOfView, FixedZoomMin, FixedZoomMax);
                    }
                    break;

                default:
                    break;
            }
        }
    }

    #endregion
    private void CameraPrioritys(Dictionary<string, CinemachineVirtualCamera> CinCam=null, CinemachineFreeLook freeLook= null, DollyMoveCamera DolllyCamera =null)
    {
        if (CinCam.Count!=0)
        {
            foreach (var item in CinCam)
            {
                item.Value.Priority = 10;
                item.Value.GetComponent<fixedCameraRota>().ISROTA = false;
                item.Value.GetComponent<fixedCameraRota>().MoveStart();
                 
            }
          
           
        }
        if (freeLook!=null)
        {
            freeLook.m_Priority = 10;
        }
        if (DolllyCamera != null)
        {
            DolllyCamera.dollyCamera.m_Priority = 10;
            if (dollybool)
            {
                DolllyCamera.StopToggle();
                FalseDollyAll();
                FreeLookCameraTransitions(false);
            }
            DollyMoveCam.cart.m_Speed = 0f;
        }
    }


}
