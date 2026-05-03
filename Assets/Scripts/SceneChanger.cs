using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger 
{
    public void LoadTargetScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
