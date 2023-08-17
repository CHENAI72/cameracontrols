using Cinemachine;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Collections;
using System;
using UnityEngine.Rendering;

public class CameraManager1 : MonoBehaviour
{
   


    //相机
    [SerializeField] DollyMoveCamera DollyMoveCam;
    [SerializeField] CinemachineFreeLook freeLook;
    [SerializeField] CinemachineCameraOffset CameraOffset;//第三人称偏移
    [SerializeField] List<CinemachineVirtualCamera> fixedCamera;//固定

    [HideInInspector]
    public Dictionary<string, CinemachineVirtualCamera> Camerapairs = new Dictionary<string, CinemachineVirtualCamera>();
    [HideInInspector]
    public string names;
    [HideInInspector]
    public string Isname;
    private bool dollybool;
    private bool IsFixed = true;
    private Dictionary<string, fixedCameraRota> fixedRota = new Dictionary<string, fixedCameraRota>();

    public event Action windowStart;//进入车内
    public event Action windowEnd;
    public event Action<bool> OnDollyCamera;//是否在轨道移动
    [HideInInspector]
    public bool LookCamera;
    [HideInInspector]
    public bool FixedCamera;

    //Touch
    [SerializeField] float ZoomSpeed = 50f;
    [SerializeField] CameraInputTou anchor;
    [SerializeField] float Min = 15f;
    [SerializeField] float Max = 60f;
    private bool IsDolly;

    [SerializeField] List<Transform> UI3DPos;


   

    private Vector2 TouTapVetor;


    private Vector3 eulerAngle;
    private Vector3 targetEulerAngle;


   
    private void OnDisable()
    {
        if (anchor != null)
        {

          
            anchor.UnRegistPrimaryTouchTapCallBack(TouchTap);
            anchor.UnRegistPrimaryTouchPosChange(PrimaryTouchDeltaCallBack);
            anchor.UnRegistZoomCallBack(ZoomCallBack);
            anchor.UnRegistThreeFingerDeltaCallBack(ThreeFingerDelta);
        }
             OnDollyCamera -= FreeLookCameraTransitions;
             windowStart -= CamerawindowStart;
             windowEnd -= CamerawindowEnd;
    }
    private void Start()
    {
        if (anchor != null)
        {
            anchor.RegistPrimaryTouchTapCallBack(TouchTap);
            anchor.RegistPrimaryTouchPosChange(PrimaryTouchDeltaCallBack);
            anchor.RegistZoomCallBack(ZoomCallBack);
            anchor.RegistThreeFingerDeltaCallBack(ThreeFingerDelta);
            OnDollyCamera += value => { IsDolly = value; };

        }
        OnDollyCamera += FreeLookCameraTransitions;
        windowStart += CamerawindowStart;
        windowEnd += CamerawindowEnd;
    }
   
    public CinemachineFreeLook FreeLook
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
    public CinemachineCameraOffset CameraOff
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
# region fixedCamera
    public void fixedCameraName(string name)//固定
    {

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

                if (Camerapairs[name].gameObject.GetComponent<fixedCameraRota>().ISZHONG && Camerapairs[name].gameObject.GetComponent<fixedCameraRota>().ThisZhongCamera.Count != 0 && FixedCamera)
                {
                    StartCoroutine("IsvirCameraZhong", name);
                   
                }
                else if(name.Substring(name.Length - 2, 2)=="2D"  && FixedCamera != true)
                {
                    Camerapairs[name].Priority = 11;
                    FalseDollyAll();
                 
                }
                else
                {
                    Camerapairs[name].Priority = 11;
                  
                }
                fixedRota[name].ISROTA = true;
                LookCamera = false;
                FixedCamera = true;
                DOTween.To(() => CameraOff.m_Offset, x => CameraOff.m_Offset = x, Vector3.zero, 1f);
                Isname = name;
                if (IsFixed)
                {
                    windowStart?.Invoke();
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

                            if (Camerapairs[name].gameObject.GetComponent<fixedCameraRota>().ISZHONG && Camerapairs[name].gameObject.GetComponent<fixedCameraRota>().ThisZhongCamera.Count != 0 && FixedCamera)
                            {

                                StartCoroutine("IsvirCameraZhong", name);

                            }
                            else if (name.Substring(name.Length - 2, 2) == "2D" && FixedCamera!=true)
                            {
                              
                              
                                Camerapairs[name].Priority = 11;
                                FalseDollyAll();
                            }
                            else
                            {
                                Camerapairs[name].Priority = 11;
                            }

                            Isname = name;
                          
                            if (IsFixed)
                            {
                                FixedCamera = true;
                                LookCamera = false;
                                DOTween.To(() => CameraOff.m_Offset, x => CameraOff.m_Offset = x, Vector3.zero, 1f);
                                windowStart?.Invoke();
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
                    Debug.LogError("列表没有这个名称的相机");
                }

            }
            else
            {
                Debug.LogError("没有相机被添加到列表");
            }

        }

    }

   

    IEnumerator IsvirCameraZhong(string name)
    {

        int i = 0;
        float time = 1f;
        while (true)
        {
            Camerapairs[name].gameObject.GetComponent<fixedCameraRota>().virCamera[i].m_Priority = 11;
            yield return new WaitForSeconds(time);
            Camerapairs[name].gameObject.GetComponent<fixedCameraRota>().virCamera[i].m_Priority = 10;
            i++;
            if (i > Camerapairs[name].gameObject.GetComponent<fixedCameraRota>().virCamera.Count - 1)
            {
                Camerapairs[name].Priority = 11;
                StopCoroutine("IsvirCameraZhong");
                break;
            }
        }


    }

    #endregion


    public void FreeLookCameraRota(Vector2 xy)
    {
        float time = 0;
        if (freeLook == null)
        {
            Debug.LogError("请添加第三人称虚拟相机");
        }
        else
        {

            DOTween.To(() => time, x => time = x, 1, 2).OnComplete(() => {

                DOTween.To(() => freeLook.m_XAxis.Value, x => freeLook.m_XAxis.Value = x, xy.x, 0.5f);
            });
            DOTween.To(() => time, x => time = x, 1, 2).OnComplete(() => {

                DOTween.To(() => freeLook.m_YAxis.Value, x => freeLook.m_YAxis.Value = x, xy.y, 0.5f);
            });

        }

    }



    public void FreeLookCamera()
    {
        if (freeLook == null)
        {
            Debug.LogError("请添加第三人称虚拟相机");
        }
        else
        {
            threeCamera();
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
                DOTween.To(() => CameraOff.m_Offset, x => CameraOff.m_Offset = x, Vector3.zero, 1f);
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

    private void FalseDollyAll()
    {
         DOTween.To(() => DollyMoveCam.AColor, x => DollyMoveCam.AColor = x, 0, 0.5f).OnUpdate(() =>
                {

        DollyMoveCam.profile[0].parameters[2].SetValue(new ColorParameter(new Color(DollyMoveCam.AColor, DollyMoveCam.AColor, DollyMoveCam.AColor)));
    });
                Invoke("FalseDolly", 1);


}
    private void FalseDolly()
    {

        DOTween.To(() => DollyMoveCam.AColor, x => DollyMoveCam.AColor = x, 1, 1f).OnUpdate(() =>
        {

            DollyMoveCam.profile[0].parameters[2].SetValue(new ColorParameter(new Color(DollyMoveCam.AColor, DollyMoveCam.AColor, DollyMoveCam.AColor)));
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
    private void threeCamera()
    {
      


        if (freeLook.m_Priority != 11)
        {
            freeLook.m_Priority = 11;
            Isname = null;
        }
        if (DollyMoveCam != null)
        {
            DollyCamera(false);
        }
        FixedCamera = false;
        LookCamera = true;
        CameraPrioritys(null, Camerapairs, null, DollyMoveCam, fixedRota, null);
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
                    windowEnd?.Invoke();
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
     

    }


    private void PrimaryTouchDeltaCallBack(Vector2 vector)
    {

        if (FixedCamera)
        {
            if (fixedRota[Isname].ISROTA)
            {
               
                if (TouTapVetor != Vector2.zero)
                {
                   
                    if (vector.x > 1f || vector.x < -1f || vector.y > 1f || vector.y < -1f)
                    {
                       targetEulerAngle = -(TouTapVetor - vector) * 0.08f;
                       eulerAngle = Vector3.Lerp(eulerAngle, targetEulerAngle, Time.deltaTime * fixedRota[Isname].RotaSpeed);
                        fixedRota[Isname].gameObject.transform.rotation = Quaternion.Lerp(fixedRota[Isname].gameObject.transform.rotation, fixedRota[Isname].gameObject.transform.rotation * Quaternion.Euler(eulerAngle.y, eulerAngle.x, eulerAngle.z), 1f);
                        fixedRota[Isname].gameObject.transform.localEulerAngles = new Vector3(fixedRota[Isname].gameObject.transform.localEulerAngles.x, fixedRota[Isname].gameObject.transform.localEulerAngles.y, 0);
                       
                    }

                }
                TouTapVetor = vector;
            }
        }

        if (LookCamera)
        {

            float time = 0;
            //  float time1 = 0;

            if (TouTapVetor != Vector2.zero)
            {

                float MoveX = (TouTapVetor.x - vector.x) * 0.2f;
                float MoveY = (TouTapVetor.y - vector.y) * 0.8f;
                if (MoveX > 1f || MoveX < -1f || MoveY > 1f || MoveY < -1f)
                {
                    DOTween.To(() => time, x => time = x, 1, 0.6f).OnUpdate(() =>
                    {

                        FreeLook.m_XAxis.m_InputAxisValue = Time.deltaTime * MoveX;
                       FreeLook.m_YAxis.m_InputAxisValue = Time.deltaTime * MoveY;

                    }).OnComplete(() => {

                        FreeLook.m_XAxis.m_InputAxisValue = 0;
                        FreeLook.m_YAxis.m_InputAxisValue = 0;
                    });
                }

            }
          

            TouTapVetor = vector;


            // if (CameraManager1.Instance.LookCamera)
            // {
            //    if (vector.x != 0)
            //    {


            //        CameraManager1.Instance.FreeLook.m_XAxis.m_InputAxisValue = vector.x / 150;
            //        DOTween.To(() => time, x => time = x, 1, 0.5f).OnComplete(() =>
            //        {
            //            CameraManager1.Instance.FreeLook.m_XAxis.m_InputAxisValue = 0;
            //        });

            //    }
            //    if (vector.y > 15f || vector.y < -15f)
            //    {


            //        CameraManager1.Instance.FreeLook.m_YAxis.m_InputAxisValue = vector.y / 50;
            //        DOTween.To(() => time1, x => time1 = x, 1, 0.5f).OnComplete(() =>
            //        {
            //            CameraManager1.Instance.FreeLook.m_YAxis.m_InputAxisValue = 0;


            //        });



            //    }


            // }
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
                       FreeLook.m_Lens.FieldOfView -= 1 * Time.deltaTime * ZoomSpeed;
                       FreeLook.m_Lens.FieldOfView = Mathf.Clamp(FreeLook.m_Lens.FieldOfView, Min, Max);
                    }

                    break;
                case ZoomType.ZoomOut:
                    if (anchor.ZoomDistance > 1f)
                    {
                        FreeLook.m_Lens.FieldOfView += 1 * Time.deltaTime * ZoomSpeed;
                       FreeLook.m_Lens.FieldOfView = Mathf.Clamp(FreeLook.m_Lens.FieldOfView, Min, Max);
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
                        Camerapairs[Isname].m_Lens.FieldOfView -= 1 * Time.deltaTime * fixedRota[Isname].ZoomSpeed;
                        Camerapairs[Isname].m_Lens.FieldOfView = Mathf.Clamp(Camerapairs[Isname].m_Lens.FieldOfView, fixedRota[Isname].Min, fixedRota[Isname].Max);
                    }

                    break;
                case ZoomType.ZoomOut:
                    if (anchor.ZoomDistance > 1f)
                    {


                        Camerapairs[Isname].m_Lens.FieldOfView += 1 * Time.deltaTime * fixedRota[Isname].ZoomSpeed;
                        Camerapairs[Isname].m_Lens.FieldOfView = Mathf.Clamp(Camerapairs[Isname].m_Lens.FieldOfView, fixedRota[Isname].Min, fixedRota[Isname].Max);
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
    public void IsTouch()
    {
        if (IsDolly)
        {
            FreeLookCamera();

        }


    }
    private void ThreeFingerDelta(Vector2 vector)
    {
        if (LookCamera)
        {

            if (vector.x > 8 || vector.x < -8)
            {

                CameraOff.m_Offset.x += vector.x / 500 * Time.deltaTime;
                CameraOff.m_Offset.x = Mathf.Clamp(CameraOff.m_Offset.x, -2f, 2f);

            }
            if (vector.y > 10 || vector.y < -10)
            {
               CameraOff.m_Offset.y += vector.y / 500 * Time.deltaTime;
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
    private void CamerawindowStart()
    {
       FreeLookCameraRota(new Vector2(-146f, 0.5f));
    }
    private void CamerawindowEnd()
    {
        foreach (var item in Camerapairs)
        {
            item.Value.m_Lens.FieldOfView = 40f;

        }
       Isname = "";
    }


    #endregion
}


