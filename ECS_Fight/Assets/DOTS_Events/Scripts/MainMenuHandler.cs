/* 
    ------------------- Code Monkey -------------------

    Thank you for downloading this package
    I hope you find it useful in your projects
    If you have any questions let me know
    Cheers!

               unitycodemonkey.com
    --------------------------------------------------
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using CodeMonkey.Utils;

public class MainMenuHandler : MonoBehaviour {
    private void Awake() {
        transform.Find("playBtn").GetComponent<Button_UI>().ClickFunc = () => { SceneManager.LoadScene("GameScene_FlappyBird"); };
    }
}
