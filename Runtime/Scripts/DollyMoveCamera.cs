using System.Collections;
using UnityEngine;
using Cinemachine;
using DG.Tweening;
using UnityEngine.Rendering;
using System.Collections.Generic;
using System;

public class DollyMoveCamera : MonoBehaviour
{
    [Serializable]
    public class ChinePath
    {
        public CinemachinePath dollys;
        public float NumbersTime;
        public bool Isblack;
    }
   
   
    public CinemachineVirtualCamera dollyCamera;
    public CinemachineDollyCart cart;
    public CinemachinePath startPath;
    [SerializeField]
    public List<ChinePath> chinePaths;
    public Volume UIColorAdts;


    public float cartMoveSpeed = 0.3f;
    public float StartblackTime = 0.5f;
    public float UpdateblackTime = 0.5f;
    private int i;
    private float time;
    [HideInInspector]
    public List< VolumeComponent> profile;
    [HideInInspector]
    public  float AColor=1;
    // Start is called before the first frame update
    private void Awake()
    {
       
        if (cart.m_Path==null)
        {
            cart.m_Path = startPath;
          
        }
        profile = UIColorAdts.profile.components;
  
    }
   public void StartToggle()
    {
        StartCoroutine("Toggle");
    }
    public void StopToggle()
    {
        StopCoroutine("Toggle");
    }
    IEnumerator Toggle()
    {
            cart.m_Speed = cartMoveSpeed;
            if (chinePaths[i].Isblack)
            {
                DOTween.To(() => AColor, x => AColor = x, 0, StartblackTime).OnUpdate(() =>
                {
                    profile[0].parameters[2].SetValue(new ColorParameter(new Color(AColor, AColor, AColor)));
                });
            }
            yield return new WaitForSeconds(StartblackTime);
            if (AColor == 0)
            {
                DOTween.To(() => AColor, x => AColor = x, 1, UpdateblackTime).OnUpdate(() =>
                {
                    profile[0].parameters[2].SetValue(new ColorParameter(new Color(AColor, AColor, AColor)));
                });
            }
            while (true)
            {
                yield return null;
                time += Time.deltaTime;
                if (time >= chinePaths[i].NumbersTime - 0.6f)
                {
                    if (chinePaths[i].Isblack)
                    {
                        DOTween.To(() => AColor, x => AColor = x, 0, UpdateblackTime).OnUpdate(() =>
                        {
                            profile[0].parameters[2].SetValue(new ColorParameter(new Color(AColor, AColor, AColor)));
                        });

                    }

                }
                if (time >= chinePaths[i].NumbersTime)
                {
                    i++;
                    yield return null;
                    if (i > chinePaths.Count - 1)
                    {
                        i = 0;
                    }
                    cart.m_Position = 0;
                    cart.m_Path = chinePaths[i].dollys;
                    time = 0;

                }
              
                if (time > 1 && time < 2f&& AColor<=0.2f)
                {
                    DOTween.To(() => AColor, x => AColor = x, 1, UpdateblackTime).OnUpdate(() =>
                    {
                        profile[0].parameters[2].SetValue(new ColorParameter(new Color(AColor, AColor, AColor)));
                    });
                }

            }
        }
     
       
    
}
