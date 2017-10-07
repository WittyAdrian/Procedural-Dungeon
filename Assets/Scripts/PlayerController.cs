using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour {

    bool lightOn;
    float maxIntensity, speedIntensity, flashlightIntensity, flickerTimer;
	GameObject flashLight;
	AudioSource flashlightAudio;

	// Use this for initialization
	void Start () {
		RenderSettings.ambientLight = new Color(0, 0, 0);
		lightOn = true;
        maxIntensity = .07f;
		speedIntensity = .0002f;
		flickerTimer = Random.Range(10f, 20f);

		flashLight = GameObject.Find("Flashlight");
		flashlightAudio = flashLight.GetComponent<AudioSource>();
		flashlightIntensity = flashlightAudio.GetComponent<Light>().intensity;
	}
	
	// Update is called once per frame
	void Update () {
        // Movement
        if(Input.GetKeyDown(KeyCode.LeftControl)) {
            this.transform.position -= new Vector3(0, 4, 0);
        } else if(Input.GetKeyDown(KeyCode.LeftAlt)) {
            this.transform.position += new Vector3(0, 52, 0);
        }

        // Flashlight
        if(Input.GetMouseButtonDown(0)) {
            lightOn = !lightOn;
            flashLight.GetComponent<Light>().enabled = lightOn;
			if (lightOn) {
				RenderSettings.ambientLight = new Color(0, 0, 0);
				flashlightAudio.clip = Resources.Load("Audio/flashlight_on") as AudioClip;
			} else {
				flashlightAudio.clip = Resources.Load("Audio/flashlight_off") as AudioClip;
			}
			
			flashlightAudio.Play();
        }

		// Flashlight flicker
		if (flickerTimer <= 0) {
			FlashlightFlicker();

			if (flickerTimer <= -5) {
				flickerTimer = Random.Range(20f, 35f);
				flashLight.GetComponent<Light>().intensity = flashlightIntensity;
			}
		}

		flickerTimer -= Time.deltaTime;

		// Ambient light adjustment
        if(!lightOn && RenderSettings.ambientLight.r < maxIntensity) {
			RenderSettings.ambientLight = new Color(RenderSettings.ambientLight.r + speedIntensity, RenderSettings.ambientLight.g + speedIntensity, RenderSettings.ambientLight.b + speedIntensity);
		}
	}

	void FlashlightFlicker() {
		float offset = .2f, range = .2f;

		flashLight.GetComponent<Light>().intensity = Random.Range(flashlightIntensity * range + offset, flashlightIntensity + offset);
	}
}
