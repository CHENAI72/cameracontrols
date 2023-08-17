using System.Collections;
using UnityEngine;
using Cinemachine;
using DG.Tweening;
using UnityEngine.Rendering;
using System.Collections.Generic;

public class DollyMoveCamera : MonoBehaviour
{
    public CinemachineVirtualCamera dollyCamera;
    public CinemachineDollyCart cart;
    public CinemachinePath startPath;
    public CinemachinePath[] dollys;
    public Volume UIColorAdts;
    // public CinemachinePath[] startAnimPath;//备用

    public int NumbersTime;
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
   

    IEnumerator Toggle()
    {
     

        DOTween.To(() => AColor, x => AColor = x, 0, 0.5f).OnUpdate(() =>
        {
            profile[0].parameters[2].SetValue(new ColorParameter(new Color(AColor, AColor, AColor)));
        });
        yield return new WaitForSeconds(1f);
        while (true)
        {
            yield return null;
            time += Time.deltaTime;
            if (time > 1&&time<2f)
            {
            
                DOTween.To(() => AColor, x => AColor = x, 1, 0.5f).OnUpdate(() =>
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
           
                DOTween.To(() => AColor, x => AColor = x, 0, 0.5f).OnUpdate(() =>
                {
                    profile[0].parameters[2].SetValue(new ColorParameter(new Color(AColor, AColor, AColor)));
                });
            }


        }
    }
}
