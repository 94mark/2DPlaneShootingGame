using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PanelManager : MonoBehaviour
{
    static Dictionary<Tyep, BasePanel> Panels = new Dictionary<Tyep, BasePanel>;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public static bool RegistPanel(Type PanelClassType, BasePanel basePanel)
    {
        if(Panels.ContainsKey(PanelClassType))
        {
            Debug.LogError("RegistPanel Error! Already exis Type! PanelClassType = " + PanelClassType.ToString());
            return false;
        }

        Debug.Log("RegistPanel is called! Type = " + PanelClassType.ToString() + ", basePanel = " + basePanel.name);

        Panels.Add(PanelClassType, basePanel);
        return true;
    }

    public static bool UnregistPanel(Type PanelClassType)
    {
        if(!Panels.ContainsKey(PanelClassType))
        {
            Debug.LogError("UnregistPanel Error! Can't Find Type! PanelClassType = " + PanelClassType.ToString());
            return false;
        }

        Panels.Remove(PanelClassType);
        return true;
    }

    public static BasePanel GetPanel(Type PanelClassType)
    {
        if(!Panels.ContainsKey(PanelClassType))
        {
            Debug.LogError("GetPanel Error! Can't Find Type! PanelClassType = " + PanelClassType.ToString());
            return null;
        }

        return Panels[PanelClassType];
    }
}
