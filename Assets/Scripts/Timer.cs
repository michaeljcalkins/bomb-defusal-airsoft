using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
	public AudioSource alarmSound;
	public AudioSource defusedSound;
	public Text percentText;
	public Text timerText;
	public Renderer progressBar;
	public bool isArmed = false;
	public bool isArming = false;
	public bool isDisarming = false;
	public float timeRemaining;
	public double startTimestamp;
	public double endTimestamp;
	public double explodeTimestamp;
	public bool isSpaceDown = false;
	public float progress = 0;
	public double progressBarBlinkTimer;
	public bool onoff;

	private Camera cam;
	private Vector2 progressBarStartingPosition = new Vector2 (0, 25000);
	private bool isPlaying = false;

	// Use this for initialization
	void Start ()
	{
		cam = GameObject.Find ("Main Camera").GetComponent<Camera> ();
		percentText = GameObject.Find ("PercentText").GetComponent<Text> ();
		timerText = GameObject.Find ("TimerText").GetComponent<Text> ();
		alarmSound = GameObject.Find ("AlarmAudioClip").GetComponent<AudioSource> ();
		defusedSound = GameObject.Find ("DefusedAudioClip").GetComponent<AudioSource> ();
		progressBar = GameObject.Find ("ProgressBar").GetComponent<Renderer> ();
	}

	// Update is called once per frame
	void Update ()
	{
		if (Input.GetKeyDown (KeyCode.Space)) {
			isSpaceDown = true;
		} else if (Input.GetKeyUp (KeyCode.Space)) {
			isSpaceDown = false;
		}

		if (
			isArmed &&
			GetTime () > (explodeTimestamp + 10000)) {
			isArming = false;
			isDisarming = false;
			isArmed = false;
			progress = 0;
			explodeTimestamp = 0;
		}

		// Render bomb progress
		if (
			(isArming || isDisarming) &&
			isSpaceDown &&
			(progress > 0 || progress < 5000)) {
			progress = isArming ? progress + Time.deltaTime * 1000 : progress - Time.deltaTime * 1000;
		} else {
			progress = isArmed ? 5000 : 0;
		}

		UpdateProgressBarSize ();

		// Updates the display text
		percentText.text = GetPercentString ();
		timerText.text = GetRemainingTimeText ();

		percentText.enabled = !isArmed;
		timerText.enabled = isArmed;

		ProgressBarBlink ();

		if (isArmed) {
			ProgressBarBlink ();
		}

		// Nothing should be in progress when not holding down space
		if (!isSpaceDown) {
			isArming = false;
			isDisarming = false;
		}

		// Stop playing the audio if nothing is happening
		if (!isSpaceDown && !isArmed && !isArming) {
			alarmSound.Stop ();
			isPlaying = false;
		}

		// Player pushed space to arm the bomb
		if (!isArmed && isSpaceDown && progress < 5000) {
			if (!isPlaying) {
				alarmSound.Play ();
				isPlaying = true;
			}

			isArming = true;
			explodeTimestamp = GetTime () + 46500;

			return;
		}

		// Player attempting to disarm the bomb
		if (isArmed && !isArming && isSpaceDown && progress > 0) {
			isDisarming = true;

			return;
		}

		// Bomb armed
		if (progress >= 5000 && isArming) {
			ArmBomb ();

			return;
		}

		// Bomb disarmed
		if (progress <= 0 && isDisarming) {
			DisarmBomb ();

			return;
		}
	}

	void ProgressBarBlink ()
	{
		if (isArming || isDisarming) {
			progressBar.material.color = Color.red;
			return;
		}

		if (Time.time > progressBarBlinkTimer) {
			progressBarBlinkTimer = Time.time + .3;
			onoff = !onoff;

			if (onoff) {
				progressBar.material.color = Color.red;
			} else {
				progressBar.material.color = Color.yellow;
			}
		}
	}

	private string GetPercentString ()
	{
		return (progress / 5000 * 100).ToString ("0") + "%";
	}

	void UpdateProgressBarSize ()
	{
		float width = Camera.main.orthographicSize * 2 * Screen.width / Screen.height;
		float height = Screen.height * 2;

		// Set the size of the progress bar
		progressBar.transform.localScale = new Vector3 (progress / 5000 * width * 2, height, 0);

		// Posistion the progress bar in the bottom left of the screen
		progressBar.transform.position = Camera.main.ScreenToWorldPoint (new Vector3 (0, 0, 190));
	}

	private double GetTime ()
	{
		System.DateTime epochStart = new System.DateTime (1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);
		int currentTimestamp = (int)(System.DateTime.UtcNow - epochStart).TotalSeconds;

		return currentTimestamp * 1000;
	}

	private string GetRemainingTimeText ()
	{
		int secondsRemaining = Mathf.Min ((int)((explodeTimestamp - GetTime ()) / 1000), 49);
			
		if (secondsRemaining <= 0) {
			return "0:00";
		}

		int minutes = secondsRemaining / 60;
		int seconds = secondsRemaining - minutes * 60;

		return "0:" + seconds.ToString ();
	}

	void ArmBomb ()
	{
		print ("Bomb armed");

		progress = 5000;
		isArmed = true;
		isArming = false;
	}

	void DisarmBomb ()
	{
		print ("Bomb disarmed");

		alarmSound.Stop ();
		defusedSound.Play ();
		isPlaying = false;

		progress = 0;
		isArmed = false;
		isArming = false;
		isDisarming = false;
		explodeTimestamp = 0;
	}
}
