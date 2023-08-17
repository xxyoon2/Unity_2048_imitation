using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeScene : MonoBehaviour
{
    // Main씬으로 가기
    public void ChangeToMainScene()
    {
        SceneManager.LoadScene(1);
    }

    // Title씬으로 가기
    public void ChangeToTitleScene()
    {
        SceneManager.LoadScene(0);
    }

}
