using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class GameController : MonoBehaviour {

	public GameObject hazard;
	public Vector3 spawnValues;
	public int hazardCount;
    private static int highscore;
    private static int highestLevel = 1;
	public float spawnWait;
	public float startWait;
	public float waveWait;
    private static float playerSpeed;
    private int level = 1;

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
    public GameObject levelBackButton;
    public GameObject settingsButton;
    public GameObject playerCanvas;
    public GameObject menuCanvas;
    public GameObject player;
    public GameObject helpCanvas;
    public GameObject settingsCanvas;
    public GameObject levelCanvas;
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
        highestLevel = PlayerPrefs.GetInt ("highLevel");
        playerSpeed = PlayerPrefs.GetFloat("playerSpeed");
		highScoreText.text = "Highest Score : " + highscore;
		//restartButton.SetActive (false);
		gameOverText.text = "";
        fireZoneIndicatorText.text = "Tap \nthis \nto \nFire";
        movementZoneIndicatorText.text = "Track Pad \n\nTouch and drag\n to move player";
		score = 0;
        prevScore = 0;
		UpdateScore ();
        scrollBar.onValueChanged.AddListener(updateSpeed);
        //Set Levels
        setLevels();
        levelCanvas.SetActive(false);
	}
    
    void setLevels() {
        for (int i=1;i<=highestLevel;i++) {
            GameObject lvl = GameObject.Find("Level"+i);
            if (lvl != null) {
                //print("Setting level: "+i);
                lvl.GetComponent<Button>().interactable = true;
            }
        }
    }
    
    //Level related settings
    public void startWithLevel() {
        if (isPaused) {
            startGame();
        } else {
            menuCanvas.SetActive(false);
            levelCanvas.SetActive(true);
        }
    }
    
    public void startLevel1() {
        level = 1;
        player.GetComponent<PlayerComponent>().setFireRate(0.25f);
        hazard.GetComponent<DestroyByContact>().setScoreValue(10);
        hazard.GetComponent<Mover>().setSpeed(-5);
        hazard.GetComponent<RandomRotator>().setTumble(5);
        startGame();
    }
    
    public void startLevel2() {
        level = 2;
        player.GetComponent<PlayerComponent>().setFireRate(0.22f);
        hazard.GetComponent<DestroyByContact>().setScoreValue(15);
        hazard.GetComponent<Mover>().setSpeed(-8);
        hazard.GetComponent<RandomRotator>().setTumble(8);
        startGame();
    }
    
    public void startLevel3() {
        level = 3;
        player.GetComponent<PlayerComponent>().setFireRate(0.19f);
        hazard.GetComponent<DestroyByContact>().setScoreValue(20);
        hazard.GetComponent<Mover>().setSpeed(-11);
        hazard.GetComponent<RandomRotator>().setTumble(11);
        startGame();
    }
    
    public void startLevel4() {
        level = 4;
        player.GetComponent<PlayerComponent>().setFireRate(0.16f);
        hazard.GetComponent<DestroyByContact>().setScoreValue(25);
        hazard.GetComponent<Mover>().setSpeed(-14);
        hazard.GetComponent<RandomRotator>().setTumble(14);
        startGame();
    }
    
    public void startLevel5() {
        level = 5;
        player.GetComponent<PlayerComponent>().setFireRate(0.13f);
        hazard.GetComponent<DestroyByContact>().setScoreValue(30);
        hazard.GetComponent<Mover>().setSpeed(-17);
        hazard.GetComponent<RandomRotator>().setTumble(17);
        startGame();
    }
    
    public void startLevel6() {
        level = 6;
        player.GetComponent<PlayerComponent>().setFireRate(0.10f);
        hazard.GetComponent<DestroyByContact>().setScoreValue(35);
        hazard.GetComponent<Mover>().setSpeed(-20);
        hazard.GetComponent<RandomRotator>().setTumble(20);
        startGame();
    }
    
    public void startLevel7() {
        level = 7;
        player.GetComponent<PlayerComponent>().setFireRate(0.07f);
        hazard.GetComponent<DestroyByContact>().setScoreValue(40);
        hazard.GetComponent<Mover>().setSpeed(-25);
        hazard.GetComponent<RandomRotator>().setTumble(25);
        startGame();
    }
    
    public void startLevel8() {
        level = 8;
        player.GetComponent<PlayerComponent>().setFireRate(0.04f);
        hazard.GetComponent<DestroyByContact>().setScoreValue(45);
        hazard.GetComponent<Mover>().setSpeed(-30);
        hazard.GetComponent<RandomRotator>().setTumble(30);
        startGame();
    }
    
    public void startLevel9() {
        level = 9;
        player.GetComponent<PlayerComponent>().setFireRate(0.01f);
        hazard.GetComponent<DestroyByContact>().setScoreValue(50);
        hazard.GetComponent<Mover>().setSpeed(-35);
        hazard.GetComponent<RandomRotator>().setTumble(35);
        startGame();
    }
    
    public void startGame() {
        if (isPaused) {
            Time.timeScale = 1;
            menuCanvas.SetActive(false);
            player.SetActive(true);
        } else {
            levelCanvas.SetActive(false);
            restartButton.SetActive (false);
            displayHighText.SetActive(false);
            levelUpText.SetActive(false);
            playerCanvas.SetActive(true);
            player.SetActive(true);
            if (playerSpeed == 0) playerSpeed = 7;
            player.GetComponent<PlayerComponent>().setSpeed(playerSpeed);
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
        settingsCanvas.GetComponent<Scrollbar>().value = playerSpeed/20;
        player.SetActive(true);
    }
    
    public void backToMenuFromSettings() {
        settingsCanvas.SetActive(false);
        player.SetActive(false);
        menuCanvas.SetActive(true);
    }
    
    public void backToMenuFromLevels() {
        levelCanvas.SetActive(false);
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
            if (score - prevScore >= 400 && !gameOver) {
                prevScore = score;
                hazard.GetComponent<DestroyByContact>().increaseScoreValue(5);
                hazard.GetComponent<Mover>().increaseSpeed(-3);
                hazard.GetComponent<RandomRotator>().increaseTumble(3);
                player.GetComponent<PlayerComponent>().increaseFireRate(0.03f);
                levelUpText.SetActive(true);
                level++;
            }
			yield return new WaitForSeconds (waveWait);

			if (gameOver)
			{
				//restartText.text = "Press 'R' for Restart";
                if (score > highscore) {
                    displayHighText.SetActive(true);
                    PlayerPrefs.SetInt ("highscore", score);
                }
                if (level > highestLevel) {
                    PlayerPrefs.SetInt ("highLevel", level);
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
        if (value == 0) {
            value = 0.001f;
        }
        player.GetComponent<PlayerComponent>().setSpeed(value * 20);
        playerSpeed = value * 20;
        PlayerPrefs.SetFloat("playerSpeed",playerSpeed);
    }
}
