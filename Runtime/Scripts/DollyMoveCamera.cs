using System.Collections;
using UnityEngine;
using Cinemachine;
using DG.Tweening;
using UnityEngine.Rendering;
using System.Collections.Generic;
using System;

public class DollyMoveCamera : MonoBehaviour
{
    public CinemachineVirtualCamera dollyCamera;
    public CinemachineDollyCart cart;
    public CinemachinePath startPath;
    public CinemachinePath[] dollys;
    public Volume UIColorAdts;
    // public CinemachinePath[] startAnimPath;//±∏”√

    public float NumbersTime = 10f;
    public float StartblackTime = 0.5f;
    public float UpdateblackTime = 0.5f;
    private int i;
    private float time;

    private List<VolumeComponent> profile;
    [HideInInspector]
    public float AColor = 1;
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
        DOTween.To(() => AColor, x => AColor = x, 0, StartblackTime).OnUpdate(() =>
        {
            profile[0].parameters[2].SetValue(new ColorParameter(new Color(AColor, AColor, AColor)));
        });
        yield return new WaitForSeconds(1f);
        while (true)
        {
            yield return null;
            time += Time.deltaTime;
            if (time > 1 && time < 2f)
            {

                DOTween.To(() => AColor, x => AColor = x, 1, UpdateblackTime).OnUpdate(() =>
                {
                    profile[0].parameters[2].SetValue(new ColorParameter(new Color(AColor, AColor, AColor)));
                });
            }

            if (time >= NumbersTime)
            {
                i++;
                yield return null;
                if (i > dollys.Length - 1)
                {
                    i = 0;
                }
                cart.m_Position = 0;
                cart.m_Path = dollys[i];
                time = 0;

            }
            if (time >= NumbersTime - 0.6f)
            {

                DOTween.To(() => AColor, x => AColor = x, 0, UpdateblackTime).OnUpdate(() =>
                {
                    profile[0].parameters[2].SetValue(new ColorParameter(new Color(AColor, AColor, AColor)));
                });
            }


        }
    }
     
       
    
}
