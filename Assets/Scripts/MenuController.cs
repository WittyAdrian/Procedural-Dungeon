using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour {

	// Use this for initialization
	void Start () {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // Configure GUI elements
        GameObject.Find("StartButton").GetComponent<Button>().onClick.AddListener(() => { SceneManager.LoadScene("game"); });
        //GameObject.Find("OptionsButton").GetComponent<Button>().onClick.AddListener(() => { SceneManager.LoadScene("options"); });
        GameObject.Find("ExitButton").GetComponent<Button>().onClick.AddListener(() => { Application.Quit(); });
    }
}
