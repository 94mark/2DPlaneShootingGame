using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleSceneMain : BaseSceneMain
{
    public void OnStartButton()
    {
        SceneController.Instance.LoadScene(SceneNameConstants.LoadingScene);
    }
}
