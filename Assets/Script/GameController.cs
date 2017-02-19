using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class GameController : MonoBehaviour {

	public GameObject hazard;
	public Vector3 spawnValues;
	public int hazardCount;
    private static int highscore;
	public float spawnWait;
	public float startWait;
	public float waveWait;

	public Text scoreText;
	public Text highScoreText;
	public Text gameOverText;
    public Text fireZoneIndicatorText;
    public Text movementZoneIndicatorText;

    public GameObject displayHighText;
    public GameObject levelUpText;
	private bool gameOver;
	private bool restart;
	private int score;
    private int prevScore;
    private bool isPaused;
	public GameObject restartButton;
    public GameObject startButton;
    public GameObject helpButton;
    public GameObject backButton;
    public GameObject pauseButton;
    public GameObject exitButton;
    public GameObject settingBackButton;
    public GameObject settingsButton;
    public GameObject playerCanvas;
    public GameObject menuCanvas;
    public GameObject player;
    public GameObject helpCanvas;
    public GameObject settingsCanvas;
    public Scrollbar scrollBar;

	void Start ()
	{
        playerCanvas.SetActive(false);
        helpCanvas.SetActive(false);
        settingsCanvas.SetActive(false);
        player.SetActive(false);
		gameOver = false;
		restart = false;
        isPaused = false;
        highscore = PlayerPrefs.GetInt ("highscore");
		highScoreText.text = "Highest Score : " + highscore;
		//restartButton.SetActive (false);
		gameOverText.text = "";
        fireZoneIndicatorText.text = "Tap \nthis \nto \nFire";
        movementZoneIndicatorText.text = "Track Pad \n\nTouch and drag\n to move player";
		score = 0;
        prevScore = 0;
		UpdateScore ();
        scrollBar.onValueChanged.AddListener(updateSpeed);
	}
    
    public void startGame() {
        if (isPaused) {
            Time.timeScale = 1;
            menuCanvas.SetActive(false);
            player.SetActive(true);
        } else {
            menuCanvas.SetActive(false);
            restartButton.SetActive (false);
            displayHighText.SetActive(false);
            levelUpText.SetActive(false);
            playerCanvas.SetActive(true);
            player.SetActive(true);
            player.GetComponent<PlayerComponent>().setFireRate(0.25f);
            StartCoroutine (SpawnWaves ());
        }
    }
    
    public void helpScreen() {
        menuCanvas.SetActive(false);
        helpCanvas.SetActive(true);
        player.SetActive(true);
    }
    
    public void settingsScreen() {
        menuCanvas.SetActive(false);
        settingsCanvas.SetActive(true);
        player.SetActive(true);
    }
    
    public void backToMenuFromSettings() {
        settingsCanvas.SetActive(false);
        player.SetActive(false);
        menuCanvas.SetActive(true);
    }
    
    public void backToMenu() {
        helpCanvas.SetActive(false);
        player.SetActive(false);
        menuCanvas.SetActive(true);
    }
    
    public void pauseGame() {
        Time.timeScale = 0;
        isPaused = true;
        player.SetActive(false);
        menuCanvas.SetActive(true);
    }

	//void Update ()
	//{
//		if (restart)
//		{
		//	if (Input.GetKeyDown (KeyCode.R))
	//		{
//				Application.LoadLevel (Application.loadedLevel);
//			}
//		}
//	}

	IEnumerator SpawnWaves ()
	{
		yield return new WaitForSeconds (startWait);
		while (true)
		{
            levelUpText.SetActive(false);
			for (int i = 0; i < hazardCount; i++)
			{
				Vector3 spawnPosition = new Vector3 (Random.Range (-spawnValues.x, spawnValues.x), spawnValues.y, spawnValues.z);
				Quaternion spawnRotation = Quaternion.identity;
				Instantiate (hazard, spawnPosition, spawnRotation);
				yield return new WaitForSeconds (spawnWait);
			}
            // Level up logic
            if (score - prevScore >= 500 && !gameOver) {
                prevScore = score;
                hazard.GetComponent<DestroyByContact>().increaseScoreValue(5);
                hazard.GetComponent<Mover>().increaseSpeed(-5);
                hazard.GetComponent<RandomRotator>().increaseTumble(5);
                player.GetComponent<PlayerComponent>().increaseFireRate(0.75f);
                levelUpText.SetActive(true);
            }
			yield return new WaitForSeconds (waveWait);

			if (gameOver)
			{
				//restartText.text = "Press 'R' for Restart";
                if (score > highscore) {
                    displayHighText.SetActive(true);
                    PlayerPrefs.SetInt ("highscore", score);
                }
                hazard.GetComponent<DestroyByContact>().setScoreValue(10);
                hazard.GetComponent<Mover>().setSpeed(-5);
                hazard.GetComponent<RandomRotator>().setTumble(5);
				restartButton.SetActive (true);
				restart = true;
				break;
			}
		}
	}

	public void AddScore (int newScoreValue)
	{
		score += newScoreValue;
		UpdateScore ();
	}

	void UpdateScore ()
	{
		scoreText.text = "Score: " + score;
	}

	public void GameOver ()
	{
		gameOverText.text = "Game Over!";
		gameOver = true;
        pauseButton.SetActive(false);
	}

	public void RestartGame () {
		Application.LoadLevel (Application.loadedLevel);
	}
    
    public void QuitGame () {
		Application.Quit ();
	}
    
    public void updateSpeed(float value) {
        player.GetComponent<PlayerComponent>().setSpeed(value * 20);
    }
}
