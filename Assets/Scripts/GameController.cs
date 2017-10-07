using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameController : MonoBehaviour {

    public static GameObject player;
    public static bool playerEnabled;

    static GameObject splashScreen;

    // Use this for initialization
    void Start () {
        player = GameObject.Find("FPSController");
        GameController.player.SetActive(false);
        playerEnabled = false;

        splashScreen = GameObject.Find("SplashScreen");
	}

    // Update is called once per frame
    void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            SceneManager.LoadScene("menu");
        }
	}

    public static void RemoveSplashScreen() {
        player.SetActive(true);
        playerEnabled = true;

        GameObject.Destroy(splashScreen);
    }
}
