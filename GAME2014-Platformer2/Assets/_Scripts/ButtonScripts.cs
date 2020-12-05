using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonScripts : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void _OpenMain()
    {
        SceneManager.LoadScene("MainScene");
    }
    public void _OpenGame()
    {
        SceneManager.LoadScene("Platformer");
    }

    public void _OpenInst()
    {
        SceneManager.LoadScene("InstructionScene");
    }

    public void _Quit()
    {
        Application.Quit();
    }
}
