using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AmbientController : MonoBehaviour {

	AudioSource ambientAudio;
	List<AudioClip> ambientClips;

	int playerDistance;
	float ambientTimer;

	// Use this for initialization
	void Start () {
		ambientAudio = this.gameObject.GetComponent<AudioSource>();
		ambientClips = new List<AudioClip>();
		ambientClips.Add(Resources.Load("Audio/metalhit_far1") as AudioClip);
		ambientClips.Add(Resources.Load("Audio/metalhit_far2") as AudioClip);
		ambientClips.Add(Resources.Load("Audio/metalhit_far4") as AudioClip);

		playerDistance = 13;
		ambientTimer = Random.Range(10f, 30f);
	}

	// Update is called once per frame
	void Update() {
		if (!ambientAudio.isPlaying) {
			Rotate();
			ambientTimer -= Time.deltaTime;
		}

		if (ambientTimer <= 0) {
			int targetClip = Random.Range(1, ambientClips.Count);
			ambientAudio.clip = ambientClips[targetClip];
			ambientAudio.Play();

			ambientClips[targetClip] = ambientClips[0];
			ambientClips[0] = ambientAudio.clip;

			ambientTimer = Random.Range(10f, 30f);
		}
	}

	void Rotate() {
		this.transform.LookAt(GameController.player.transform);
		while (Vector3.Distance(this.transform.position, GameController.player.transform.position) < playerDistance) {
			this.transform.position -= this.transform.forward;
		}

		while (Vector3.Distance(this.transform.position, GameController.player.transform.position) > playerDistance) {
			this.transform.position += this.transform.forward;
		}

		this.transform.position += this.transform.right;
	}
}
