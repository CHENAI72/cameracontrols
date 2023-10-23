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


    private float AColor = 1;
    private List<VolumeComponent> profile;
    private CinemachineCameraOffset CameraOffset;
    private CinemachineFreeLook FreeLook;
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

    [Header("fixedCameraInput")]
    [SerializeField] float FixedZoomSpeed = 2f;
    [SerializeField] float FixedZoomMin = 15f;
    [SerializeField] float FixedZoomMax = 40f;

    [Header("DollyCameraSpeed")]
    [SerializeField] float DollyCameraSpeed = 0.3f;


    private string Dollyname;
    private float CameraStartTime;
    private string EndCamera;
    private Vector2 CameraPos;

    private string Isname;
    private bool dollybool;
    private bool CamIsBlending;

    private bool Security;//保护
    private Vector2 TouTapVetor;
    private List<CinemachineVirtualCameraBase> CameraChilds = new List<CinemachineVirtualCameraBase>();
    private Dictionary<string, fixedCameraRota> FixedCamera = new Dictionary<string, fixedCameraRota>();

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
            if (CameraChilds[i].GetComponent<CinemachineFreeLook>() != null)
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
                if (FreeLook.GetComponent<fixedCameraRota>()==null)
                {
                    Debug.LogError($"请将fixedCameraRota添加到{FreeLook.gameObject.name}上.");
                }
                Isname = FreeLook.name;
                FixedCamera[Isname].ISROTA = true;
                break;
            }
            else if (i == CameraChilds.Count - 1)
            {
                int point = 0;
                for (int j = 0; j < CameraChilds.Count; j++)
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



    #region FreeLookCameraStartPos
    public void FreeLookCameraPosSave()
    {
        if (FreeLook != null&& Isname== FreeLook.name)
        {
            CameraPos = new Vector2(FreeLook.m_XAxis.Value, FreeLook.m_YAxis.Value);
        }

    }

    public void FreeLookBackStartPos(float time)
    {
      
        if (FreeLook != null&& Isname == FreeLook.name)
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

                        dollybool = true;

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

                if (value|| FixedCamera[name].theIs2D)
                {

                    CameraHandoverTime(0);
                    DrivenCamera.m_AnimatedTarget.Play(name);
                    FalseDollyAll();
      
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
                if (CameraOffset != null)
                {
                    DOTween.To(() => CameraOffset.m_Offset, x => CameraOffset.m_Offset = x, Vector3.zero, 1f);
                }
              
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
                    FreeLook.m_XAxis.m_InputAxisValue = MoveX;
            }

            if (MoveY > 0.1f || MoveY < -0.1f)
            {

                    FreeLook.m_YAxis.m_InputAxisValue = MoveY;
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

            FreeLook.m_Lens.FieldOfView -= value;
            FreeLook.m_Lens.FieldOfView = Mathf.Clamp(FreeLook.m_Lens.FieldOfView, freeLookZoomMin, freeLookZoomMax);
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
        if (FreeLook != null)
            FreeLookCameraRota = new Vector2(FreeLook.m_XAxis.Value, FreeLook.m_YAxis.Value);

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
                        FreeLook.m_XAxis.m_InputAxisValue = 0;
                        FreeLook.m_YAxis.m_InputAxisValue = 0;
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
            if (FreeLook != null)
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
                    yield return null;
                }
                CamIsBlending = DrivenCamera.IsBlending;
            }
            yield return new WaitForSeconds(0.1f);
            touValue = TouTapVetor;
            if (Isname != "" && Isname != null && FixedCamera.ContainsKey(Isname) && FixedCamera[Isname].GetComponent<CinemachineVirtualCamera>())
                FixedCameraRota = new Vector2(FixedCamera[Isname].GetComponent<CinemachineVirtualCamera>().GetCinemachineComponent<CinemachinePOV>().m_HorizontalAxis.Value,
                FixedCamera[Isname].GetComponent<CinemachineVirtualCamera>().GetCinemachineComponent<CinemachinePOV>().m_VerticalAxis.Value);
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
    #endregion
}
