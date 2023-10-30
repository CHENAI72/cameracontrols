using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using DG.Tweening;
using UnityEngine.Rendering;
using UnityEngine.Events;
using System.Reflection;
using System;

public class CameraControlV3 : MonoBehaviour
{
    private enum Components
    {
        No,DollyCamera
    }

    [SerializeField] CinemachineBrain MainCamera;
    [SerializeField] CinemachineStateDrivenCamera DrivenCamera;
    [SerializeField] DollyMoveCamera DollyMoveCam;
    [SerializeField] Volume VolumeColor;

    private float AColor = 1;
    private List<VolumeComponent> profile;
    public UnityEvent<string> StartMoveCamera;//切换开始时响应，string为开始摄像机名称
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

    [Header("FixedCameraInput")]
    [SerializeField] float FixedZoomSpeed = 2f;
    [SerializeField] float FixedZoomMin = 15f;
    [SerializeField] float FixedZoomMax = 40f;

    //[Header("Component")]
    //[SerializeField] Components m_AddComponent=Components.No;

    private string Dollyname;
    private float CameraStartTime;
    private string EndCamera;


    private string Isname;
    private bool dollybool;
    private bool CamIsBlending;

    private bool Security;//保护
    private Vector2 TouTapVetor;
    private List<CinemachineVirtualCameraBase> CameraChilds = new List<CinemachineVirtualCameraBase>();
    private Dictionary<string, fixedCameraRota> FixedCamera = new Dictionary<string, fixedCameraRota>();
    private Dictionary<string, CinemachineFreeLook>  FreeLook= new Dictionary<string, CinemachineFreeLook>();
    private Dictionary<string, Vector2> CameraPos = new Dictionary<string, Vector2>();
    // Start is called before the first frame update

    #region StartData
    private void Awake()
    {

        CameraStartTime = DrivenCamera.m_DefaultBlend.m_Time;
        CinemachineVirtualCameraBase[] Camera = DrivenCamera.ChildCameras;
        for (int i = 0; i < Camera.Length; i++)
        {
            CameraChilds.Add(Camera[i]);
        }

        for (int i = 0; i < CameraChilds.Count; i++)
        {
            if (CameraChilds[i].GetComponent<fixedCameraRota>() != null)
            {
                FixedCamera.Add(CameraChilds[i].name, CameraChilds[i].GetComponent<fixedCameraRota>());
            }
            else
            {
                Debug.LogError($"请将fixedCameraRota添加到{CameraChilds[i].name}上.");
                return;
            }
         

        }
        int point = 0;
        for (int i = 0; i < CameraChilds.Count; i++)
        {
            if (CameraChilds[i].GetComponent<CinemachineFreeLook>() != null)
            {
                FreeLook.Add(CameraChilds[i].name, CameraChilds[i].GetComponent<CinemachineFreeLook>());
                CameraPos.Add(CameraChilds[i].name, new Vector2(FreeLook[CameraChilds[i].name].m_XAxis.Value, FreeLook[CameraChilds[i].name].m_YAxis.Value));

            }
                    if (CameraChilds[i].m_Priority > CameraChilds[point].m_Priority )
                    {
                        point = i;

                    }

        }
        Isname = CameraChilds[point].name;
        FixedCamera[Isname].ISROTA = true;


        if (DollyMoveCam != null)//要改掉以插件的形式
        {
            Dollyname = DollyMoveCam.dollyCamera.name;
        }
        if (VolumeColor != null)//要改掉以插件的形式
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

    #region CameraOffset
    public void CameraSetOffset(string CameraName,Vector3 value,float time) 
    {
      
        if (FixedCamera.ContainsKey(CameraName))
        {
            FixedCamera[CameraName].SetOffset(value, time);
        }
        else
        {
            Debug.LogError($"列表里没有{CameraName}这个相机");
        }
    }
    public void CameraBackOffset(string CameraName,float time)
    {
        if (FixedCamera.ContainsKey(CameraName))
        {
            FixedCamera[CameraName].MoveStartOffset(time);
        }
        else
        {
            Debug.LogError($"列表里没有{CameraName}这个相机");
        }
    }
    #endregion

    #region FreeLookCameraStartPos
    public void FreeLookCameraPosSave()
    {
        if (FreeLook.ContainsKey(Isname) &&CameraPos.ContainsKey(Isname))
        {
            CameraPos[Isname] = new Vector2(FreeLook[Isname].m_XAxis.Value, FreeLook[Isname].m_YAxis.Value);
        }

    }
    
    public void FreeLookBackStartPos(float time)
    {
      
        if (FreeLook.ContainsKey(Isname) && CameraPos.ContainsKey(Isname))
        {
            DOTween.To(() => FreeLook[Isname].m_XAxis.Value, x => FreeLook[Isname].m_XAxis.Value = x, CameraPos[Isname].x, time);
            DOTween.To(() => FreeLook[Isname].m_YAxis.Value, x => FreeLook[Isname].m_YAxis.Value = x, CameraPos[Isname].y, time);
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

                        dollybool = true;

                        DollyMoveCam.StartToggle();
                       // DollyMoveCam.cart.m_Speed = DollyCameraSpeed;
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
    public void SwitchCamera(string CameraName, bool value = false)
    {
        for (int i = 0; i < CameraChilds.Count; i++)
        {

            if (CameraChilds[i].name == CameraName)
            {
                break;
            }
            else if (i == CameraChilds.Count - 1)
            {
                Debug.LogError("列表里没有" + CameraName + "的摄像机，请检查是否添加或名称是否正确");
                return;
            }

        }


        CameraDate(CameraName, value);

    }

    private void CameraDate(string name, bool value = false)
    {

        if (name != Isname)
        {

            Isname = name;
            StopAllCoroutines();
            StartCoroutine(EpicJudgment());
            if (FixedCamera.ContainsKey(name))
            {

                if (value)
                {

                    CameraHandoverTime(0);
                    FalseDollyAll();
                    DrivenCamera.m_AnimatedTarget.Play(name);
                    CamIsBlending = !CamIsBlending;


                }
                else
                {

                    CameraHandoverTime(CameraStartTime);
                     if (FixedCamera[name].m_TransitionList.Count != 0 && FixedCamera[name].m_TransitionCamera.Count != 0)
                    {
                       
                        if (FixedCamera[name].m_TransitionCamera.Exists(t => t.name == DrivenCamera.LiveChild.Name))
                        {
                            StartCoroutine(IsvirCamera(name));
                        }
                        else
                        {
                            DrivenCamera.m_AnimatedTarget.Play(name);
                        }

                    }
                    else
                    {
                        DrivenCamera.m_AnimatedTarget.Play(name);
                    }

                }
                foreach (var item in FixedCamera)
                {

                    item.Value.ISROTA = false;
                    item.Value.MoveStart();
                }
                FixedCamera[name].ISROTA = true;
             
              
            }

        }

    }


    bool Blending;
    IEnumerator IsvirCamera(string name)
    {
        int i = 0;
        DrivenCamera.m_AnimatedTarget.Play(FixedCamera[name].m_TransitionList[i].name);
        yield return null;


        while (true)
        {

            if (Blending != DrivenCamera.IsBlending)
            {
                if (Blending)
                {
                    i++;
                    if (i > FixedCamera[name].m_TransitionList.Count - 1)
                    {

                        DrivenCamera.m_AnimatedTarget.Play(name);
                        StopCoroutine(IsvirCamera(name));
                        break;
                    }
                    DrivenCamera.m_AnimatedTarget.Play(FixedCamera[name].m_TransitionList[i].name);
                    yield return null;
                }
                Blending = DrivenCamera.IsBlending;
            }


            yield return null;


        }
    }
    private void FreeLookCameraTransitions(bool value)
    {
        float time = 0;

        DOTween.To(() => time, x => time = x, 1, 0.5f).OnComplete(() =>
        {
            FreeLook[Isname].m_Transitions.m_InheritPosition = value;
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

        if (FixedCamera[Isname].GetComponent<CinemachineVirtualCamera>() != null &&
            FixedCamera[Isname].therota && FixedCamera[Isname].ISROTA)
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

        if (FixedCamera[Isname].ISROTA && FixedCamera[Isname].GetComponent<CinemachineVirtualCamera>() == null &&
            FixedCamera[Isname].therota && FreeLook != null)
        {


            if (TouTapVetor != Vector2.zero)
            {

                float MoveX = (TouTapVetor.x - vector.x) * 0.1f;
                float MoveY = (TouTapVetor.y - vector.y) * 0.1f;


                if (MoveX > 0.1f || MoveX < -0.1f)
                {
                    FreeLook[Isname].m_XAxis.m_InputAxisValue = MoveX;

                }
                if (MoveY > 0.1f || MoveY < -0.1f)
                {
                    FreeLook[Isname].m_YAxis.m_InputAxisValue = MoveY;

                }



            }
            TouTapVetor = vector;

        }
    }

    private void PrimaryTouchDeltaCallBack(Vector2 vector)
    {
       
            if (FixedCamera[Isname].GetComponent<CinemachineVirtualCamera>() != null &&
            FixedCamera[Isname].therota && FixedCamera[Isname].ISROTA)
            {

                float MoveX = vector.x * 0.1f;
                float MoveY = vector.y * 0.1f;
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

                TouTapVetor = vector;
            }
        

        if (FixedCamera[Isname].ISROTA && FixedCamera[Isname].GetComponent<CinemachineVirtualCamera>() == null &&
            FixedCamera[Isname].therota && FreeLook != null)
        {

            float MoveX = vector.x * 0.1f;
            float MoveY = vector.y * 0.1f;
            if (MoveX > 0.1f || MoveX < -0.1f)
            {
                    FreeLook[Isname].m_XAxis.m_InputAxisValue = MoveX;
            }

            if (MoveY > 0.1f || MoveY < -0.1f)
            {

                    FreeLook[Isname].m_YAxis.m_InputAxisValue = MoveY;
            }

            TouTapVetor = vector;
        }

    }
    private void ZoomCallBack(ZoomType zoom)
    {
        if (FixedCamera[Isname].ISROTA && FixedCamera[Isname].GetComponent<CinemachineVirtualCamera>() == null &&
            FixedCamera[Isname].therota && FreeLook != null)
        {
            switch (zoom)
            {

                case ZoomType.ZoomIn:
                    if (anchor.ZoomDistance > 0.1f)
                    {
                        FreeLook[Isname].m_Lens.FieldOfView -= anchor.ZoomDistance * Time.deltaTime * freeLookZoomSpeed * 0.005f;
                        FreeLook[Isname].m_Lens.FieldOfView = Mathf.Clamp(FreeLook[Isname].m_Lens.FieldOfView, freeLookZoomMin, freeLookZoomMax);
                    }

                    break;
                case ZoomType.ZoomOut:
                    if (anchor.ZoomDistance > 0.1f)
                    {
                        FreeLook[Isname].m_Lens.FieldOfView += anchor.ZoomDistance * Time.deltaTime * freeLookZoomSpeed * 0.005f;
                        FreeLook[Isname].m_Lens.FieldOfView = Mathf.Clamp(FreeLook[Isname].m_Lens.FieldOfView, freeLookZoomMin, freeLookZoomMax);
                    }

                    break;
                default:
                    break;
            }

        }
        if (FixedCamera[Isname].GetComponent<CinemachineVirtualCamera>() != null &&
            FixedCamera[Isname].therota && FixedCamera[Isname].ISROTA)
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

        if (FixedCamera[Isname].GetComponent<CinemachineVirtualCamera>() != null &&
            FixedCamera[Isname].therota && FixedCamera[Isname].ISROTA)
        {
            FixedCamera[Isname].GetComponent<CinemachineVirtualCamera>().m_Lens.FieldOfView -= value;
            FixedCamera[Isname].GetComponent<CinemachineVirtualCamera>().m_Lens.FieldOfView = Mathf.Clamp(FixedCamera[Isname]
                .GetComponent<CinemachineVirtualCamera>().m_Lens.FieldOfView, FixedZoomMin, FixedZoomMax);
        }
        if (FixedCamera[Isname].ISROTA && FixedCamera[Isname].GetComponent<CinemachineVirtualCamera>() == null &&
            FixedCamera[Isname].therota && FreeLook != null)
        {

            FreeLook[Isname].m_Lens.FieldOfView -= value;
            FreeLook[Isname].m_Lens.FieldOfView = Mathf.Clamp(FreeLook[Isname].m_Lens.FieldOfView, freeLookZoomMin, freeLookZoomMax);
        }
    }
    IEnumerator EpicJudgment()
    {
        Vector2 FixedCameraRota = Vector2.zero;
        Vector2 FreeLookCameraRota = Vector2.zero;
        Vector2 touValue = Vector2.zero;
        if (Isname != "" && Isname != null && FixedCamera.ContainsKey(Isname) && FixedCamera[Isname].GetComponent<CinemachineVirtualCamera>() != null)
            FixedCameraRota = new Vector2(FixedCamera[Isname].GetComponent<CinemachineVirtualCamera>().GetCinemachineComponent<CinemachinePOV>().m_HorizontalAxis.Value,
            FixedCamera[Isname].GetComponent<CinemachineVirtualCamera>().GetCinemachineComponent<CinemachinePOV>().m_VerticalAxis.Value);
        if (FreeLook.ContainsKey(Isname))
            FreeLookCameraRota = new Vector2(FreeLook[Isname].m_XAxis.Value, FreeLook[Isname].m_YAxis.Value);

        while (true)
        {

            if (TouTapVetor == touValue)
            {

                TouTapVetor = Vector2.zero;
                if (Isname != "" && Isname != null && FixedCamera.ContainsKey(Isname) && FixedCamera[Isname].GetComponent<CinemachineVirtualCamera>())
                {
                    if (FixedCamera.Count != 0)
                    {
                        FixedCamera[Isname].GetComponent<CinemachineVirtualCamera>().GetCinemachineComponent<CinemachinePOV>().m_VerticalAxis.m_InputAxisValue = 0;
                        FixedCamera[Isname].GetComponent<CinemachineVirtualCamera>().GetCinemachineComponent<CinemachinePOV>().m_HorizontalAxis.m_InputAxisValue = 0;

                    }

                }
                if (Isname != "" && Isname != null && FixedCamera.ContainsKey(Isname) && FixedCamera[Isname].GetComponent<CinemachineVirtualCamera>()==null)
                {
                    if (FreeLook != null)
                    {
                        FreeLook[Isname].m_XAxis.m_InputAxisValue = 0;
                        FreeLook[Isname].m_YAxis.m_InputAxisValue = 0;
                    }

                }

            }


            yield return null;
            if (Isname != "" && Isname != null && FixedCamera.ContainsKey(Isname) && FixedCamera[Isname].GetComponent<CinemachineVirtualCamera>())
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
            if (FreeLook.ContainsKey(Isname))
            {
                if (FreeLookCameraRota.x != FreeLook[Isname].m_XAxis.Value || FreeLookCameraRota.y != FreeLook[Isname].m_YAxis.Value)
                {
                    OnFreeLookCameraRota?.Invoke(new Vector2(FreeLook[Isname].m_XAxis.Value, FreeLook[Isname].m_YAxis.Value));
                }
            }


            if (CamIsBlending != DrivenCamera.IsBlending)
            {
                if (CamIsBlending)
                {
                    MoveCameraArrival?.Invoke(EndCamera);
                    EndCamera = "";
                    yield return null;
                }
                CamIsBlending = DrivenCamera.IsBlending;
            }
            yield return new WaitForSeconds(0.1f);
            touValue = TouTapVetor;
            if (Isname != "" && Isname != null && FixedCamera.ContainsKey(Isname) && FixedCamera[Isname].GetComponent<CinemachineVirtualCamera>())
                FixedCameraRota = new Vector2(FixedCamera[Isname].GetComponent<CinemachineVirtualCamera>().GetCinemachineComponent<CinemachinePOV>().m_HorizontalAxis.Value,
                FixedCamera[Isname].GetComponent<CinemachineVirtualCamera>().GetCinemachineComponent<CinemachinePOV>().m_VerticalAxis.Value);
            if (FreeLook.ContainsKey(Isname))
                FreeLookCameraRota = new Vector2(FreeLook[Isname].m_XAxis.Value, FreeLook[Isname].m_YAxis.Value);
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
                DOTween.To(() => AColor, x => AColor = x, 0, 0.1f).OnUpdate(() =>
                {

                    profile[0].parameters[2].SetValue(new ColorParameter(new Color(AColor, AColor, AColor)));

                }).OnComplete(() =>
                {
                    DOTween.To(() => AColor, x => AColor = x, 1, blackCameraTime).OnUpdate(() =>
                    {

                        profile[0].parameters[2].SetValue(new ColorParameter(new Color(AColor, AColor, AColor)));
                      

                    });
                });
            }



        }
    }
//#if UNITY_EDITOR
//    private static List<GameObject> GameComponents=new List<GameObject>();
//    private void OnValidate()
//    {

//        if (Application.isPlaying!=true)
//        {
//            switch (m_AddComponent)
//            {
//                case Components.No:
//                    if (GameComponents.Count==0)
//                    {
//                        return;
//                    }
//                    for (int i = 0; i < gameObject.GetComponents<Component>().Length; i++)
//                    {
//                        Debug.Log(gameObject.GetComponents<Component>()[i]+"dsf"+ GameComponents[0]);
//                        if (GameComponents.Exists(t=>t==gameObject.GetComponents<Component>()[i]))
//                        {
                         
//                            Destroy(gameObject.GetComponents<Component>()[i]);
//                            GameComponents.Remove(GameComponents[i]);
//                        }
//                    }
//                    break;
//                case Components.DollyCamera:
//                    if (gameObject.GetComponent<DollyMoveCamera>())
//                    {
//                        return;
//                    }

//                       AddComponent(this.gameObject, "com.hmi.cameracontrol", "DollyMoveCamera");

                    
//                    break;
//                default:
//                    break;
//            } 
//        }
//    }
//    public static Component AddComponent(GameObject go, string assembly, string classname)
//    {
//        var asmb = System.Reflection.Assembly.Load(assembly);
//        var t = asmb.GetType( classname); 
       
//        if (null != t)
//        {
//            GameComponents.Add(go);
//            return go.AddComponent(t);
//        }
//        else
//            return null;
       
//    }
//#endif

    #endregion
}

