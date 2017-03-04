using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class GameController : MonoBehaviour {

	//GameObjects : player,hazard
	public GameObject hazard;
	public GameObject hazardRight;
	public GameObject player;
	
	//Counts and coordinates
	public Vector3 spawnValues;
	public int hazardCount;
	public int rightHazardCount;
	private static int highscore;
	private static int highestLevel = 1;
	public float spawnWait;
	public float startWait;
	public float waveWait;
	public float horizontalWaveWait;
	private static float playerSpeed;
	private int level = 1;
	
	//State variables
	private bool gameOver;
	private bool restart;
	private int score;
	private int prevScore;
	private bool isPaused;
	private bool isRightHand;
	private bool isSwitched;
	private static int playerSide;  //0 means right handed, 1 means left handed
	private bool everLeft;
	private bool levelJump;
	
	//Menu Canvas
	public GameObject startButton;
	public GameObject helpButton;
	public GameObject settingsButton;
	public GameObject exitButton;
	public GameObject menuCanvas;
	public Text highScoreText;
	
	//Settings Canvas
	public GameObject settingBackButton;
	public Scrollbar scrollBar;
	public GameObject settingsCanvas;
	public GameObject switchButton;
	
	//Player Canvas
	public GameObject restartButton;
	public Text scoreText;
	public Text gameOverText;
	public GameObject levelUpText;
	public GameObject displayHighText;
	public GameObject playerCanvas;
	public GameObject movementZone;
	public GameObject fireZone;
	public GameObject playInfo;
	
	//Help Canvas
	public GameObject backButton;
	public GameObject pauseButton;
	public GameObject helpCanvas;
	public Text fireZoneIndicatorText;
	public Text movementZoneIndicatorText;
	
	//Level Canvas
	public GameObject levelBackButton;
	public GameObject levelCanvas;
	public GameObject level_1_9;
	public GameObject level_10_18;
	public GameObject level_back;
	public GameObject level_forward;
	
	//First Time Canvas
	public GameObject firstTimeGame;
	public GameObject continueButton;
	
	//Level Thresholds
	private static int level_1_2 = 250;
	private static int level_2_3 = 350;
	private static int level_3_4 = 500;
	private static int level_4_5 = 700;
	private static int level_5_6 = 900;
	private static int level_6_7 = 1200;
	private static int level_7_8 = 1500;
	private static int level_8_9 = 2000;
	private static int level_9_10 = 2700;
	private static int level_10_11 = 400;
	private static int level_11_12 = 560;
	private static int level_12_13 = 740;
	private static int level_13_14 = 940;
	private static int level_14_15 = 1160;
	private static int level_15_16 = 1400;
	private static int level_16_17 = 1650;
	private static int level_17_18 = 1950;
	private static int level_18_19 = 2300;

	void Start ()
	{
		playerCanvas.SetActive(false);
		helpCanvas.SetActive(false);
		settingsCanvas.SetActive(false);
		player.SetActive(false);
		gameOver = false;
		restart = false;
		isPaused = false;
		isRightHand = true;
		isSwitched = false;
		levelJump = false;
		playerSide = PlayerPrefs.GetInt ("playerSide");
		if (playerSide == 1) {
			movementZone.GetComponent<RectTransform>().localPosition += new Vector3(79.01f, 0, 0);
			fireZone.GetComponent<RectTransform>().localPosition -= new Vector3(180,0,0);
			isSwitched = true;
			isRightHand = false;
		}
		highscore = PlayerPrefs.GetInt ("highscore");
		highestLevel = PlayerPrefs.GetInt ("highLevel");
		playerSpeed = PlayerPrefs.GetFloat("playerSpeed");
		highScoreText.text = "Highest Score : " + highscore;
		//restartButton.SetActive (false);
		if (highscore == 0) {
			menuCanvas.SetActive(false);
			firstTimeGame.SetActive(true);
		} else {
			firstTimeGame.SetActive(false);
		}
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
	
	public void musicOnOff() {
		AudioListener.volume = 1 - AudioListener.volume;
	}
	
	//Level related settings
	public void startWithLevel() {
		if (isPaused) {
			startGame();
		} else {
			menuCanvas.SetActive(false);
			levelCanvas.SetActive(true);
			level_1_9.SetActive(true);
			level_10_18.SetActive(false);
			level_back.GetComponent<Button>().interactable = false;
			level_forward.GetComponent<Button>().interactable = true;
		}
	}
	
	public void onLevelForward() {
		level_1_9.SetActive(false);
		level_10_18.SetActive(true);
		level_back.GetComponent<Button>().interactable = true;
		level_forward.GetComponent<Button>().interactable = false;
	}
	
	public void onLevelBack() {
		level_1_9.SetActive(true);
		level_10_18.SetActive(false);
		level_back.GetComponent<Button>().interactable = false;
		level_forward.GetComponent<Button>().interactable = true;
	}
	
	void gameOverAction() {
		if (score > highscore) {
			displayHighText.SetActive(true);
			PlayerPrefs.SetInt ("highscore", score);
		}
		if (level > highestLevel) {
			PlayerPrefs.SetInt ("highLevel", level);
		}
		restartButton.SetActive (true);
		restart = true;
	}
	
	//************* Common gameplay functions **************//
	
	public void startGame() {
		if (isPaused) {
			Time.timeScale = 1;
			menuCanvas.SetActive(false);
			player.SetActive(true);
			playerCanvas.SetActive(true);
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
	
	IEnumerator SpawnWaves ()
	{
		gameOverText.text = "Level " + level.ToString();
		yield return new WaitForSeconds (startWait);
		while (true)
		{
			gameOverText.text = "";
			levelUpText.SetActive(false);
			for (int i = 0; i < hazardCount; i++)
			{
				if (gameOver) break;
				Vector3 spawnPosition = new Vector3 (Random.Range (-spawnValues.x, spawnValues.x), spawnValues.y, spawnValues.z);
				Quaternion spawnRotation = Quaternion.identity;
				Instantiate (hazard, spawnPosition, spawnRotation);
				yield return new WaitForSeconds (spawnWait);
			}
			// Level up logic
			if (score - prevScore >= 20000 && !gameOver) {
				prevScore = score;
				hazard.GetComponent<DestroyByContact>().increaseScoreValue(5);
				hazard.GetComponent<Mover>().increaseSpeed(-3);
				hazard.GetComponent<RandomRotator>().increaseTumble(3);
				player.GetComponent<PlayerComponent>().increaseFireRate(0.95f);
				levelUpText.SetActive(true);
				level++;
			}
			//yield return new WaitForSeconds (waveWait);

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
	
	//************* Code for Level 1 Gameplay *************//

	public void startLevel1() {
		level = 1;
		player.GetComponent<PlayerComponent>().setFireRate(0.25f);
		if (!levelJump) {
			hazard.GetComponent<DestroyByContact>().setScoreValue(10);
			hazard.GetComponent<Mover>().setSpeed(-5);
			hazard.GetComponent<RandomRotator>().setTumble(5);
		}
		startGame1();
	}
	
	public void startGame1() {
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
			StartCoroutine (SpawnWaves1 ());
		}
	}
	
	IEnumerator SpawnWaves1 ()
	{
		gameOverText.text = "Level 1";
		yield return new WaitForSeconds (startWait);
		while (true)
		{
			gameOverText.text = "";
			levelUpText.SetActive(false);
			for (int i = 0; i < hazardCount; i++)
			{
				if (gameOver) break;
				Vector3 spawnPosition = new Vector3 (Random.Range (-spawnValues.x, spawnValues.x), spawnValues.y, spawnValues.z);
				Quaternion spawnRotation = Quaternion.identity;
				Instantiate (hazard, spawnPosition, spawnRotation);
				if (i==0) {
					hazard.GetComponent<DestroyByContact>().setScoreValue(10);
					hazard.GetComponent<Mover>().setSpeed(-5);
					hazard.GetComponent<RandomRotator>().setTumble(5);
				}
				yield return new WaitForSeconds (spawnWait);
			}
			// Level up logic
			if (score - prevScore >= level_1_2 && !gameOver) {
				prevScore = score;
				levelUpText.SetActive(true);
				levelUpText.GetComponent<Text>().text = "Nice work !!";
				level++;
				levelJump = true;
				break;
			}
			//yield return new WaitForSeconds (waveWait);

			if (gameOver)
			{
				gameOverAction();
				break;
			}
		}
		if (!gameOver) {
			startLevel2();
		}
	}
	
	//************* Code for Level 2 Gameplay *************//
	
	public void startLevel2() {
		level = 2;
		player.GetComponent<PlayerComponent>().setFireRate(0.23f);
		if (!levelJump) {
			hazard.GetComponent<DestroyByContact>().setScoreValue(15);
			hazard.GetComponent<Mover>().setSpeed(-8);
			hazard.GetComponent<RandomRotator>().setTumble(8);
		}
		startGame2();
	}
	
	public void startGame2() {
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
			StartCoroutine (SpawnWaves2 ());
		}
	}
	
	IEnumerator SpawnWaves2 ()
	{
		if (levelJump) {
			levelUpText.SetActive(true);
		}
		gameOverText.text = "Level 2";
		yield return new WaitForSeconds (startWait);
		while (true)
		{
			gameOverText.text = "";
			levelUpText.SetActive(false);
			for (int i = 0; i < hazardCount; i++)
			{
				if (gameOver) break;
				Vector3 spawnPosition = new Vector3 (Random.Range (-spawnValues.x, spawnValues.x), spawnValues.y, spawnValues.z);
				Quaternion spawnRotation = Quaternion.identity;
				Instantiate (hazard, spawnPosition, spawnRotation);
				if (i==0) {
					hazard.GetComponent<DestroyByContact>().setScoreValue(15);
					hazard.GetComponent<Mover>().setSpeed(-8);
					hazard.GetComponent<RandomRotator>().setTumble(8);
				}
				yield return new WaitForSeconds (spawnWait);
			}
			// Level up logic
			if (score - prevScore >= level_2_3 && !gameOver) {
				prevScore = score;
				levelUpText.SetActive(true);
				levelUpText.GetComponent<Text>().text = "Watch out for the waves ;)";
				//level++;
				//levelJump = true;
				break;
			}
			//yield return new WaitForSeconds (waveWait);

			if (gameOver)
			{
				gameOverAction();
				break;
			}
		}
		if (!gameOver) {
			StartCoroutine (SpawnHorizontalWaves1 ());
		}
	}
	
	IEnumerator SpawnHorizontalWaves1 ()
	{
		yield return new WaitForSeconds (horizontalWaveWait);
		levelUpText.SetActive(false);
		for (int i = 0; i < rightHazardCount; i++)
		{
			if (gameOver) break;
			Vector3 spawnPosition = new Vector3 (8, 0, Random.Range (0, 12));
			Quaternion spawnRotation = Quaternion.identity;
			Instantiate (hazardRight, spawnPosition, spawnRotation);
			if (i == 0) {
				hazardRight.GetComponent<MoveHorizontal>().setSpeed(-5,-2);
			}
			yield return new WaitForSeconds (spawnWait);
		}

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
			restartButton.SetActive (true);
			restart = true;
		} else {
			levelUpText.SetActive(true);
			levelUpText.GetComponent<Text>().text = "You have got skills.";
			level++;
			levelJump = true;
			startLevel3();
		}
	}
	
	//************* Code for Level 3 Gameplay *************//
	
	public void startLevel3() {
		level = 3;
		player.GetComponent<PlayerComponent>().setFireRate(0.21f);
		if (!levelJump) {
			hazard.GetComponent<DestroyByContact>().setScoreValue(20);
			hazard.GetComponent<Mover>().setSpeed(-11);
			hazard.GetComponent<RandomRotator>().setTumble(11);
		}
		startGame3();
	}
	
	public void startGame3() {
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
			StartCoroutine (SpawnWaves3 ());
		}
	}
	
	IEnumerator SpawnWaves3 ()
	{
		if (levelJump) {
			levelUpText.SetActive(true);
		}
		gameOverText.text = "Level 3";
		yield return new WaitForSeconds (startWait);
		while (true)
		{
			gameOverText.text = "";
			levelUpText.SetActive(false);
			for (int i = 0; i < hazardCount; i++)
			{
				if (gameOver) break;
				Vector3 spawnPosition = new Vector3 (Random.Range (-spawnValues.x, spawnValues.x), spawnValues.y, spawnValues.z);
				Quaternion spawnRotation = Quaternion.identity;
				Instantiate (hazard, spawnPosition, spawnRotation);
				if (i==0) {
					hazard.GetComponent<DestroyByContact>().setScoreValue(20);
					hazard.GetComponent<Mover>().setSpeed(-11);
					hazard.GetComponent<RandomRotator>().setTumble(11);
				}
				yield return new WaitForSeconds (spawnWait);
			}
			// Level up logic
			if (score - prevScore >= level_3_4 && !gameOver) {
				prevScore = score;
				levelUpText.SetActive(true);
				levelUpText.GetComponent<Text>().text = "Going Strong !!";
				level++;
				levelJump = true;
				break;
			}
			//yield return new WaitForSeconds (waveWait);

			if (gameOver)
			{
				gameOverAction();
				break;
			}
		}
		if (!gameOver) {
			startLevel4();
		}
	}
	
	//************* Code for Level 4 Gameplay *************//
	
	public void startLevel4() {
		level = 4;
		player.GetComponent<PlayerComponent>().setFireRate(0.19f);
		if (!levelJump) {
			hazard.GetComponent<DestroyByContact>().setScoreValue(25);
			hazard.GetComponent<Mover>().setSpeed(-14);
			hazard.GetComponent<RandomRotator>().setTumble(14);
		}
		startGame4();
	}
	
	public void startGame4() {
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
			StartCoroutine (SpawnWaves4 ());
		}
	}
	
	IEnumerator SpawnWaves4 ()
	{
		if (levelJump) {
			levelUpText.SetActive(true);
		}
		gameOverText.text = "Level 4";
		yield return new WaitForSeconds (startWait);
		while (true)
		{
			gameOverText.text = "";
			levelUpText.SetActive(false);
			for (int i = 0; i < hazardCount; i++)
			{
				if (gameOver) break;
				Vector3 spawnPosition = new Vector3 (Random.Range (-spawnValues.x, spawnValues.x), spawnValues.y, spawnValues.z);
				Quaternion spawnRotation = Quaternion.identity;
				Instantiate (hazard, spawnPosition, spawnRotation);
				if (i==0) {
					hazard.GetComponent<DestroyByContact>().setScoreValue(25);
					hazard.GetComponent<Mover>().setSpeed(-14);
					hazard.GetComponent<RandomRotator>().setTumble(14);
				}
				yield return new WaitForSeconds (spawnWait);
			}
			// Level up logic
			if (score - prevScore >= level_4_5 && !gameOver) {
				prevScore = score;
				levelUpText.SetActive(true);
				levelUpText.GetComponent<Text>().text = "Half way through !!";
				level++;
				levelJump = true;
				break;
			}
			//yield return new WaitForSeconds (waveWait);

			if (gameOver)
			{
				gameOverAction();
				break;
			}
		}
		if (!gameOver) {
			startLevel5();
		}
	}
	
	//************* Code for Level 5 Gameplay *************//
	
	public void startLevel5() {
		level = 5;
		player.GetComponent<PlayerComponent>().setFireRate(0.17f);
		if (!levelJump) {
			hazard.GetComponent<DestroyByContact>().setScoreValue(30);
			hazard.GetComponent<Mover>().setSpeed(-17);
			hazard.GetComponent<RandomRotator>().setTumble(17);
		}
		startGame5();
	}
	
	public void startGame5() {
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
			StartCoroutine (SpawnWaves5 ());
		}
	}
	
	IEnumerator SpawnWaves5 ()
	{
		if (levelJump) {
			levelUpText.SetActive(true);
		}
		gameOverText.text = "Level 5";
		yield return new WaitForSeconds (startWait);
		while (true)
		{
			gameOverText.text = "";
			levelUpText.SetActive(false);
			for (int i = 0; i < hazardCount; i++)
			{
				if (gameOver) break;
				Vector3 spawnPosition = new Vector3 (Random.Range (-spawnValues.x, spawnValues.x), spawnValues.y, spawnValues.z);
				Quaternion spawnRotation = Quaternion.identity;
				Instantiate (hazard, spawnPosition, spawnRotation);
				if (i==0) {
					hazard.GetComponent<DestroyByContact>().setScoreValue(30);
					hazard.GetComponent<Mover>().setSpeed(-17);
					hazard.GetComponent<RandomRotator>().setTumble(17);
				}
				yield return new WaitForSeconds (spawnWait);
			}
			// Level up logic
			if (score - prevScore >= level_5_6 && !gameOver) {
				prevScore = score;
				levelUpText.SetActive(true);
				levelUpText.GetComponent<Text>().text = "Time to show \nyour manoeuvring skills";
				//level++;
				//levelJump = true;
				break;
			}
			//yield return new WaitForSeconds (waveWait);

			if (gameOver)
			{
				gameOverAction();
				break;
			}
		}
		if (!gameOver) {
			StartCoroutine (SpawnHorizontalWaves2 ());
		}
	}
	
	IEnumerator SpawnHorizontalWaves2 ()
	{
		yield return new WaitForSeconds (horizontalWaveWait);
		levelUpText.SetActive(false);
		for (int i = 0; i < 3*rightHazardCount; i++)
		{
			if (gameOver) break;
			Vector3 spawnPosition = new Vector3 (8, 0, Random.Range (0, 12));
			Quaternion spawnRotation = Quaternion.identity;
			Instantiate (hazardRight, spawnPosition, spawnRotation);
			if (i%2==0) {
				hazardRight.GetComponent<MoveHorizontal>().setSpeed(-10,-4);
			} else {
				hazardRight.GetComponent<MoveHorizontal>().setSpeed(-5,-2);
			}
			yield return new WaitForSeconds (spawnWait);
		}
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
			restartButton.SetActive (true);
			restart = true;
		} else {
			levelUpText.SetActive(true);
			levelUpText.GetComponent<Text>().text = "Awesome job !";
			level++;
			levelJump = true;
			startLevel6();
		}
	}
	
	//************* Code for Level 6 Gameplay *************//
	
	public void startLevel6() {
		level = 6;
		player.GetComponent<PlayerComponent>().setFireRate(0.15f);
		if (!levelJump) {
			hazard.GetComponent<DestroyByContact>().setScoreValue(35);
			hazard.GetComponent<Mover>().setSpeed(-20);
			hazard.GetComponent<RandomRotator>().setTumble(20);
		}
		startGame6();
	}
	
	public void startGame6() {
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
			StartCoroutine (SpawnWaves6 ());
		}
	}
	
	IEnumerator SpawnWaves6 ()
	{
		if (levelJump) {
			levelUpText.SetActive(true);
		}
		gameOverText.text = "Level 6";
		yield return new WaitForSeconds (startWait);
		while (true)
		{
			gameOverText.text = "";
			levelUpText.SetActive(false);
			for (int i = 0; i < hazardCount; i++)
			{
				if (gameOver) break;
				Vector3 spawnPosition = new Vector3 (Random.Range (-spawnValues.x, spawnValues.x), spawnValues.y, spawnValues.z);
				Quaternion spawnRotation = Quaternion.identity;
				Instantiate (hazard, spawnPosition, spawnRotation);
				if (i==0) {
					hazard.GetComponent<DestroyByContact>().setScoreValue(35);
					hazard.GetComponent<Mover>().setSpeed(-20);
					hazard.GetComponent<RandomRotator>().setTumble(20);
				}
				yield return new WaitForSeconds (spawnWait);
			}
			// Level up logic
			if (score - prevScore >= level_6_7 && !gameOver) {
				prevScore = score;
				levelUpText.SetActive(true);
				levelUpText.GetComponent<Text>().text = "Getting close :D";
				level++;
				levelJump = true;
				break;
			}
			//yield return new WaitForSeconds (waveWait);

			if (gameOver)
			{
				gameOverAction();
				break;
			}
		}
		if (!gameOver) {
			startLevel7();
		}
	}
	
	//************* Code for Level 7 Gameplay *************//
	
	public void startLevel7() {
		level = 7;
		player.GetComponent<PlayerComponent>().setFireRate(0.13f);
		if (!levelJump) {
			hazard.GetComponent<DestroyByContact>().setScoreValue(40);
			hazard.GetComponent<Mover>().setSpeed(-25);
			hazard.GetComponent<RandomRotator>().setTumble(25);
		}
		startGame7();
	}
	
	public void startGame7() {
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
			StartCoroutine (SpawnWaves7 ());
		}
	}
	
	IEnumerator SpawnWaves7 ()
	{
		if (levelJump) {
			levelUpText.SetActive(true);
		}
		gameOverText.text = "Level 7";
		yield return new WaitForSeconds (startWait);
		while (true)
		{
			gameOverText.text = "";
			levelUpText.SetActive(false);
			for (int i = 0; i < hazardCount; i++)
			{
				if (gameOver) break;
				Vector3 spawnPosition = new Vector3 (Random.Range (-spawnValues.x, spawnValues.x), spawnValues.y, spawnValues.z);
				Quaternion spawnRotation = Quaternion.identity;
				Instantiate (hazard, spawnPosition, spawnRotation);
				if (i==0) {
					hazard.GetComponent<DestroyByContact>().setScoreValue(40);
					hazard.GetComponent<Mover>().setSpeed(-25);
					hazard.GetComponent<RandomRotator>().setTumble(25);
				}
				yield return new WaitForSeconds (spawnWait);
			}
			// Level up logic
			if (score - prevScore >= level_7_8 && !gameOver) {
				prevScore = score;
				levelUpText.SetActive(true);
				levelUpText.GetComponent<Text>().text = "Things are \nabout to get hard !";
				level++;
				levelJump = true;
				break;
			}
			//yield return new WaitForSeconds (waveWait);

			if (gameOver)
			{
				gameOverAction();
				break;
			}
		}
		if (!gameOver) {
			startLevel8();
		}
	}
	
	//************* Code for Level 8 Gameplay *************//
	
	public void startLevel8() {
		level = 8;
		player.GetComponent<PlayerComponent>().setFireRate(0.11f);
		if (!levelJump) {
			hazard.GetComponent<DestroyByContact>().setScoreValue(45);
			hazard.GetComponent<Mover>().setSpeed(-30);
			hazard.GetComponent<RandomRotator>().setTumble(30);
		}
		startGame8();
	}
	
	public void startGame8() {
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
			StartCoroutine (SpawnWaves8 ());
		}
	}
	
	IEnumerator SpawnWaves8 ()
	{
		if (levelJump) {
			levelUpText.SetActive(true);
		}
		gameOverText.text = "Level 8";
		yield return new WaitForSeconds (startWait);
		while (true)
		{
			gameOverText.text = "";
			levelUpText.SetActive(false);
			for (int i = 0; i < hazardCount; i++)
			{
				if (gameOver) break;
				Vector3 spawnPosition = new Vector3 (Random.Range (-spawnValues.x, spawnValues.x), spawnValues.y, spawnValues.z);
				Quaternion spawnRotation = Quaternion.identity;
				Instantiate (hazard, spawnPosition, spawnRotation);
				if (i==0) {
					hazard.GetComponent<DestroyByContact>().setScoreValue(45);
					hazard.GetComponent<Mover>().setSpeed(-30);
					hazard.GetComponent<RandomRotator>().setTumble(30);
				}
				yield return new WaitForSeconds (spawnWait);
			}
			// Level up logic
			if (score - prevScore >= level_8_9 && !gameOver) {
				prevScore = score;
				levelUpText.SetActive(true);
				levelUpText.GetComponent<Text>().text = "The real deal o_O";
				//level++;
				//levelJump = true;
				break;
			}
			//yield return new WaitForSeconds (waveWait);

			if (gameOver)
			{
				gameOverAction();
				break;
			}
		}
		if (!gameOver) {
			StartCoroutine (SpawnHorizontalWaves3 ());
		}
	}
	
	IEnumerator SpawnHorizontalWaves3 ()
	{
		yield return new WaitForSeconds (horizontalWaveWait);
		levelUpText.SetActive(false);
		for (int i = 0; i < 4*rightHazardCount; i++)
		{
			if (gameOver) break;
			Vector3 spawnPosition = new Vector3 (8, 0, Random.Range (0, 12));
			Quaternion spawnRotation = Quaternion.identity;
			Instantiate (hazardRight, spawnPosition, spawnRotation);
			if (i%2==0) {
				hazardRight.GetComponent<MoveHorizontal>().setSpeed(-10,-4);
			} else {
				hazardRight.GetComponent<MoveHorizontal>().setSpeed(-5,-2);
			}
			spawnPosition = new Vector3 (Random.Range (-spawnValues.x, spawnValues.x), spawnValues.y, spawnValues.z);
			spawnRotation = Quaternion.identity;
			Instantiate (hazard, spawnPosition, spawnRotation);
			if (i==0) {
				hazard.GetComponent<DestroyByContact>().setScoreValue(0);
				hazard.GetComponent<Mover>().setSpeed(-20);
				hazard.GetComponent<RandomRotator>().setTumble(30);
			}
			yield return new WaitForSeconds (spawnWait);
		}
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
			restartButton.SetActive (true);
			restart = true;
		} else {
			levelUpText.SetActive(true);
			levelUpText.GetComponent<Text>().text = "Respect _/\\_";
			level++;
			levelJump = true;
			startLevel9();
		}
	}
	
	//************* Code for Level 9 Gameplay *************//
	
	public void startLevel9() {
		level = 9;
		player.GetComponent<PlayerComponent>().setFireRate(0.09f);
		if (!levelJump) {
			hazard.GetComponent<DestroyByContact>().setScoreValue(50);
			hazard.GetComponent<Mover>().setSpeed(-35);
			hazard.GetComponent<RandomRotator>().setTumble(35);
		}
		startGame9();
	}
	
	public void startGame9() {
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
			StartCoroutine (SpawnWaves9 ());
		}
	}
	
	IEnumerator SpawnWaves9 ()
	{
		if (levelJump) {
			levelUpText.SetActive(true);
		}
		gameOverText.text = "Level 9";
		yield return new WaitForSeconds (startWait);
		while (true)
		{
			gameOverText.text = "";
			levelUpText.SetActive(false);
			for (int i = 0; i < hazardCount; i++)
			{
				if (gameOver) break;
				Vector3 spawnPosition = new Vector3 (Random.Range (-spawnValues.x, spawnValues.x), spawnValues.y, spawnValues.z);
				Quaternion spawnRotation = Quaternion.identity;
				Instantiate (hazard, spawnPosition, spawnRotation);
				if (i==0) {
					hazard.GetComponent<DestroyByContact>().setScoreValue(50);
					hazard.GetComponent<Mover>().setSpeed(-35);
					hazard.GetComponent<RandomRotator>().setTumble(35);
				}
				yield return new WaitForSeconds (spawnWait);
			}
			// Level up logic
			if (score - prevScore >= level_9_10 && !gameOver) {
				prevScore = score;
				levelUpText.SetActive(true);
				levelUpText.GetComponent<Text>().text = "Going to the next page.";
				level++;
				levelJump = true;
				break;
			}
			//yield return new WaitForSeconds (waveWait);

			if (gameOver)
			{
				gameOverAction();
				break;
			}
		}
		if (!gameOver) {
			startLevel10();
		}
	}
	
	//************* Code for Level 10 Gameplay *************//

	public void startLevel10() {
		level = 10;
		player.GetComponent<PlayerComponent>().setFireRate(0.25f);
		if (!levelJump) {
			hazard.GetComponent<DestroyByContact>().setScoreValue(60);
			hazard.GetComponent<Mover>().setSpeed(-5);
			hazard.GetComponent<RandomRotator>().setTumble(5);
		}
		startGame10();
	}
	
	public void startGame10() {
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
			StartCoroutine (SpawnWaves10 ());
		}
	}
	
	IEnumerator SpawnWaves10 ()
	{
		gameOverText.text = "Level 10";
		yield return new WaitForSeconds (startWait);
		while (true)
		{
			gameOverText.text = "";
			levelUpText.SetActive(false);
			for (int i = 0; i < hazardCount; i++)
			{
				if (gameOver) break;
				Vector3 spawnPosition = new Vector3 (8, 0, Random.Range (0, 12));
				Quaternion spawnRotation = Quaternion.identity;
				Instantiate (hazardRight, spawnPosition, spawnRotation);
				if (i==0) {
					hazardRight.GetComponent<DestroyByContact>().setScoreValue(30);
				}
				if (i%2==0) {
					hazardRight.GetComponent<MoveHorizontal>().setSpeed(-10,-4);
				} else {
					hazardRight.GetComponent<MoveHorizontal>().setSpeed(-5,-2);
				}
				spawnPosition = new Vector3 (Random.Range (-spawnValues.x, spawnValues.x), spawnValues.y, spawnValues.z);
				spawnRotation = Quaternion.identity;
				Instantiate (hazard, spawnPosition, spawnRotation);
				if (i==0) {
					hazard.GetComponent<DestroyByContact>().setScoreValue(60);
					hazard.GetComponent<Mover>().setSpeed(-5);
					hazard.GetComponent<RandomRotator>().setTumble(5);
				}
				if (i%2==0) {
					hazard.GetComponent<Mover>().setSpeed(-5);
				} else {
					hazard.GetComponent<Mover>().setSpeed(-8);
				}
				yield return new WaitForSeconds (spawnWait);
			}
			// Level up logic
			if (score - prevScore >= level_10_11 && !gameOver) {
				prevScore = score;
				levelUpText.SetActive(true);
				levelUpText.GetComponent<Text>().text = "Getting a hang of things now";
				level++;
				levelJump = true;
				break;
			}
			//yield return new WaitForSeconds (waveWait);

			if (gameOver)
			{
				gameOverAction();
				break;
			}
		}
		if (!gameOver) {
			startLevel11();
		}
	}
	
	 //************* Code for Level 11 Gameplay *************//

	public void startLevel11() {
		level = 11;
		player.GetComponent<PlayerComponent>().setFireRate(0.25f);
		if (!levelJump) {
			hazard.GetComponent<DestroyByContact>().setScoreValue(65);
			hazard.GetComponent<Mover>().setSpeed(-7);
			hazard.GetComponent<RandomRotator>().setTumble(7);
		}
		startGame11();
	}
	
	public void startGame11() {
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
			StartCoroutine (SpawnWaves11 ());
		}
	}
	
	IEnumerator SpawnWaves11 ()
	{
		gameOverText.text = "Level 11";
		yield return new WaitForSeconds (startWait);
		while (true)
		{
			gameOverText.text = "";
			levelUpText.SetActive(false);
			for (int i = 0; i < hazardCount; i++)
			{
				if (gameOver) break;
				Vector3 spawnPosition = new Vector3 (8, 0, Random.Range (0, 12));
				Quaternion spawnRotation = Quaternion.identity;
				Instantiate (hazardRight, spawnPosition, spawnRotation);
				if (i==0) {
					hazardRight.GetComponent<DestroyByContact>().setScoreValue(33);
				}
				if (i%2==0) {
					hazardRight.GetComponent<MoveHorizontal>().setSpeed(-10,-4);
				} else {
					hazardRight.GetComponent<MoveHorizontal>().setSpeed(-6,-3);
				}
				spawnPosition = new Vector3 (Random.Range (-spawnValues.x, spawnValues.x), spawnValues.y, spawnValues.z);
				spawnRotation = Quaternion.identity;
				Instantiate (hazard, spawnPosition, spawnRotation);
				if (i==0) {
					hazard.GetComponent<DestroyByContact>().setScoreValue(65);
					hazard.GetComponent<Mover>().setSpeed(-5);
					hazard.GetComponent<RandomRotator>().setTumble(5);
				}
				if (i%2==0) {
					hazard.GetComponent<Mover>().setSpeed(-5);
				} else {
					hazard.GetComponent<Mover>().setSpeed(-8);
				}
				yield return new WaitForSeconds (spawnWait);
			}
			// Level up logic
			if (score - prevScore >= level_11_12 && !gameOver) {
				prevScore = score;
				levelUpText.SetActive(true);
				levelUpText.GetComponent<Text>().text = "Twist and turns to follow";
				level++;
				levelJump = true;
				break;
			}
			//yield return new WaitForSeconds (waveWait);

			if (gameOver)
			{
				gameOverAction();
				break;
			}
		}
		if (!gameOver) {
			startLevel12();
		}
	}
	
	//************* Code for Level 12 Gameplay *************//
	
	public void startLevel12() {
		level = 12;
		player.GetComponent<PlayerComponent>().setFireRate(0.23f);
		if (!levelJump) {
			hazard.GetComponent<DestroyByContact>().setScoreValue(70);
			hazard.GetComponent<Mover>().setSpeed(-8);
			hazard.GetComponent<RandomRotator>().setTumble(8);
		}
		startGame12();
	}
	
	public void startGame12() {
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
			StartCoroutine (SpawnWaves12 ());
		}
	}
	
	IEnumerator SpawnWaves12 ()
	{
		if (levelJump) {
			levelUpText.SetActive(true);
		}
		gameOverText.text = "Level 12";
		yield return new WaitForSeconds (startWait);
		while (true)
		{
			gameOverText.text = "";
			levelUpText.SetActive(false);
			for (int i = 0; i < hazardCount; i++)
			{
				if (gameOver) break;
				Vector3 spawnPosition = new Vector3 (8, 0, Random.Range (0, 12));
				Quaternion spawnRotation = Quaternion.identity;
				Instantiate (hazardRight, spawnPosition, spawnRotation);
				if (i==0) {
					hazardRight.GetComponent<DestroyByContact>().setScoreValue(35);
				}
				if (i%2==0) {
					hazardRight.GetComponent<MoveHorizontal>().setSpeed(-11,-5);
				} else {
					hazardRight.GetComponent<MoveHorizontal>().setSpeed(-6,-3);
				}
				spawnPosition = new Vector3 (Random.Range (-spawnValues.x, spawnValues.x), spawnValues.y, spawnValues.z);
				spawnRotation = Quaternion.identity;
				Instantiate (hazard, spawnPosition, spawnRotation);
				if (i==0) {
					hazard.GetComponent<DestroyByContact>().setScoreValue(70);
					hazard.GetComponent<Mover>().setSpeed(-8);
					hazard.GetComponent<RandomRotator>().setTumble(8);
				}
				if (i%2==0) {
					hazard.GetComponent<Mover>().setSpeed(-8);
				} else {
					hazard.GetComponent<Mover>().setSpeed(-11);
				}
				yield return new WaitForSeconds (spawnWait);
			}
			// Level up logic
			if (score - prevScore >= level_12_13 && !gameOver) {
				prevScore = score;
				levelUpText.SetActive(true);
				levelUpText.GetComponent<Text>().text = "Watch out for the waves";
				//level++;
				//levelJump = true;
				break;
			}
			//yield return new WaitForSeconds (waveWait);

			if (gameOver)
			{
				gameOverAction();
				break;
			}
		}
		if (!gameOver) {
			StartCoroutine (SpawnHorizontalWaves4 ());
		}
	}
	
	IEnumerator SpawnHorizontalWaves4 ()
	{
		yield return new WaitForSeconds (horizontalWaveWait);
		levelUpText.SetActive(false);
		for (int i = 0; i < rightHazardCount; i++)
		{
			if (gameOver) break;
			Vector3 spawnPosition = new Vector3 (8, 0, Random.Range (0, 12));
			Quaternion spawnRotation = Quaternion.identity;
			GameObject hr = Instantiate (hazardRight, spawnPosition, spawnRotation);
			hr.GetComponent<MoveHorizontal>().setSpeed(-5,-2);
			spawnPosition = new Vector3 (-8, 0, Random.Range (0, 12));
			spawnRotation = Quaternion.identity;
			GameObject hr1 =Instantiate (hazardRight, spawnPosition, spawnRotation);
			hr1.GetComponent<MoveHorizontal>().setSpeed(8,-4);
			yield return new WaitForSeconds (spawnWait);
		}

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
			restartButton.SetActive (true);
			restart = true;
		} else {
			levelUpText.SetActive(true);
			levelUpText.GetComponent<Text>().text = "Niceee ;)";
			level++;
			levelJump = true;
			startLevel13();
		}
	}
	
	//************* Code for Level 13 Gameplay *************//
	
	public void startLevel13() {
		level = 13;
		player.GetComponent<PlayerComponent>().setFireRate(0.21f);
		if (!levelJump) {
			hazard.GetComponent<DestroyByContact>().setScoreValue(80);
			hazard.GetComponent<Mover>().setSpeed(-11);
			hazard.GetComponent<RandomRotator>().setTumble(11);
		}
		startGame13();
	}
	
	public void startGame13() {
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
			StartCoroutine (SpawnWaves13 ());
		}
	}
	
	IEnumerator SpawnWaves13 ()
	{
		if (levelJump) {
			levelUpText.SetActive(true);
		}
		gameOverText.text = "Level 13";
		yield return new WaitForSeconds (startWait);
		while (true)
		{
			gameOverText.text = "";
			levelUpText.SetActive(false);
			for (int i = 0; i < hazardCount; i++)
			{
				if (gameOver) break;
				Vector3 spawnPosition = new Vector3 (-8, 0, Random.Range (0, 12));
				Quaternion spawnRotation = Quaternion.identity;
				Instantiate (hazardRight, spawnPosition, spawnRotation);
				if (i==0) {
					hazardRight.GetComponent<DestroyByContact>().setScoreValue(40);
				}
				if (i%2==0) {
					hazardRight.GetComponent<MoveHorizontal>().setSpeed(12,-6);
				} else {
					hazardRight.GetComponent<MoveHorizontal>().setSpeed(7,-4);
				}
				spawnPosition = new Vector3 (Random.Range (-spawnValues.x, spawnValues.x), spawnValues.y, spawnValues.z);
				spawnRotation = Quaternion.identity;
				Instantiate (hazard, spawnPosition, spawnRotation);
				if (i==0) {
					hazard.GetComponent<DestroyByContact>().setScoreValue(80);
					hazard.GetComponent<Mover>().setSpeed(-11);
					hazard.GetComponent<RandomRotator>().setTumble(11);
				}
				if (i%2==0) {
					hazard.GetComponent<Mover>().setSpeed(-11);
				} else {
					hazard.GetComponent<Mover>().setSpeed(-14);
				}
				yield return new WaitForSeconds (spawnWait);
			}
			// Level up logic
			if (score - prevScore >= level_13_14 && !gameOver) {
				prevScore = score;
				levelUpText.SetActive(true);
				levelUpText.GetComponent<Text>().text = "Going Great !!";
				level++;
				levelJump = true;
				break;
			}
			//yield return new WaitForSeconds (waveWait);

			if (gameOver)
			{
				gameOverAction();
				break;
			}
		}
		if (!gameOver) {
			startLevel14();
		}
	}
	
	//************* Code for Level 14 Gameplay *************//
	
	public void startLevel14() {
		level = 14;
		player.GetComponent<PlayerComponent>().setFireRate(0.19f);
		if (!levelJump) {
			hazard.GetComponent<DestroyByContact>().setScoreValue(90);
			hazard.GetComponent<Mover>().setSpeed(-14);
			hazard.GetComponent<RandomRotator>().setTumble(14);
		}
		startGame14();
	}
	
	public void startGame14() {
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
			StartCoroutine (SpawnWaves14 ());
		}
	}
	
	IEnumerator SpawnWaves14 ()
	{
		if (levelJump) {
			levelUpText.SetActive(true);
		}
		gameOverText.text = "Level 14";
		yield return new WaitForSeconds (startWait);
		while (true)
		{
			gameOverText.text = "";
			levelUpText.SetActive(false);
			for (int i = 0; i < hazardCount; i++)
			{
				if (gameOver) break;
				Vector3 spawnPosition = new Vector3 (-8, 0, Random.Range (0, 12));
				Quaternion spawnRotation = Quaternion.identity;
				Instantiate (hazardRight, spawnPosition, spawnRotation);
				if (i==0) {
					hazardRight.GetComponent<DestroyByContact>().setScoreValue(45);
				}
				if (i%2==0) {
					hazardRight.GetComponent<MoveHorizontal>().setSpeed(13,-7);
				} else {
					hazardRight.GetComponent<MoveHorizontal>().setSpeed(8,-5);
				}
				spawnPosition = new Vector3 (Random.Range (-spawnValues.x, spawnValues.x), spawnValues.y, spawnValues.z);
				spawnRotation = Quaternion.identity;
				Instantiate (hazard, spawnPosition, spawnRotation);
				if (i==0) {
					hazard.GetComponent<DestroyByContact>().setScoreValue(90);
					hazard.GetComponent<Mover>().setSpeed(-14);
					hazard.GetComponent<RandomRotator>().setTumble(14);
				}
				if (i%2==0) {
					hazard.GetComponent<Mover>().setSpeed(-14);
				} else {
					hazard.GetComponent<Mover>().setSpeed(-17);
				}
				yield return new WaitForSeconds (spawnWait);
			}
			// Level up logic
			if (score - prevScore >= level_14_15 && !gameOver) {
				prevScore = score;
				levelUpText.SetActive(true);
				levelUpText.GetComponent<Text>().text = "Half way through this !!";
				level++;
				levelJump = true;
				break;
			}
			//yield return new WaitForSeconds (waveWait);

			if (gameOver)
			{
				gameOverAction();
				break;
			}
		}
		if (!gameOver) {
			startLevel15();
		}
	}
	
	//************* Code for Level 15 Gameplay *************//
	
	public void startLevel15() {
		level = 15;
		player.GetComponent<PlayerComponent>().setFireRate(0.17f);
		if (!levelJump) {
			hazard.GetComponent<DestroyByContact>().setScoreValue(100);
			hazard.GetComponent<Mover>().setSpeed(-17);
			hazard.GetComponent<RandomRotator>().setTumble(17);
		}
		startGame15();
	}
	
	public void startGame15() {
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
			StartCoroutine (SpawnWaves15 ());
		}
	}
	
	IEnumerator SpawnWaves15 ()
	{
		if (levelJump) {
			levelUpText.SetActive(true);
		}
		gameOverText.text = "Level 15";
		yield return new WaitForSeconds (startWait);
		while (true)
		{
			gameOverText.text = "";
			levelUpText.SetActive(false);
			for (int i = 0; i < hazardCount; i++)
			{
				if (gameOver) break;
				Vector3 spawnPosition = new Vector3 (-8, 0, Random.Range (0, 12));
				Quaternion spawnRotation = Quaternion.identity;
				Instantiate (hazardRight, spawnPosition, spawnRotation);
				if (i==0) {
					hazardRight.GetComponent<DestroyByContact>().setScoreValue(50);
				}
				if (i%2==0) {
					hazardRight.GetComponent<MoveHorizontal>().setSpeed(14,-8);
				} else {
					hazardRight.GetComponent<MoveHorizontal>().setSpeed(9,-6);
				}
				spawnPosition = new Vector3 (Random.Range (-spawnValues.x, spawnValues.x), spawnValues.y, spawnValues.z);
				spawnRotation = Quaternion.identity;
				Instantiate (hazard, spawnPosition, spawnRotation);
				if (i==0) {
					hazard.GetComponent<DestroyByContact>().setScoreValue(100);
					hazard.GetComponent<Mover>().setSpeed(-17);
					hazard.GetComponent<RandomRotator>().setTumble(17);
				}
				if (i%2==0) {
					hazard.GetComponent<Mover>().setSpeed(-17);
				} else {
					hazard.GetComponent<Mover>().setSpeed(-20);
				}
				yield return new WaitForSeconds (spawnWait);
			}
			// Level up logic
			if (score - prevScore >= level_15_16 && !gameOver) {
				prevScore = score;
				levelUpText.SetActive(true);
				levelUpText.GetComponent<Text>().text = "Show time";
				//level++;
				//levelJump = true;
				break;
			}
			//yield return new WaitForSeconds (waveWait);

			if (gameOver)
			{
				gameOverAction();
				break;
			}
		}
		if (!gameOver) {
			StartCoroutine (SpawnHorizontalWaves5 ());
		}
	}
	
	IEnumerator SpawnHorizontalWaves5 ()
	{
		yield return new WaitForSeconds (horizontalWaveWait);
		levelUpText.SetActive(false);
		for (int i = 0; i < 3*rightHazardCount; i++)
		{
			if (gameOver) break;
			Vector3 spawnPosition = new Vector3 (8, 0, Random.Range (0, 12));
			Quaternion spawnRotation = Quaternion.identity;
			GameObject hr = Instantiate (hazardRight, spawnPosition, spawnRotation);
			if (i%2==0) {
				hr.GetComponent<MoveHorizontal>().setSpeed(-10,-4);
			} else {
				hr.GetComponent<MoveHorizontal>().setSpeed(-5,-2);
			}
			spawnPosition = new Vector3 (-8, 0, Random.Range (0, 12));
			spawnRotation = Quaternion.identity;
			hr = Instantiate (hazardRight, spawnPosition, spawnRotation);
			if (i%2==0) {
				hr.GetComponent<MoveHorizontal>().setSpeed(10,-4);
			} else {
				hr.GetComponent<MoveHorizontal>().setSpeed(5,-2);
			}
			yield return new WaitForSeconds (spawnWait);
		}
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
			restartButton.SetActive (true);
			restart = true;
		} else {
			levelUpText.SetActive(true);
			levelUpText.GetComponent<Text>().text = "Awesomenesss !";
			level++;
			levelJump = true;
			startLevel16();
		}
	}
	
	//************* Code for Level 16 Gameplay *************//
	
	public void startLevel16() {
		level = 16;
		player.GetComponent<PlayerComponent>().setFireRate(0.15f);
		if (!levelJump) {
			hazard.GetComponent<DestroyByContact>().setScoreValue(110);
			hazard.GetComponent<Mover>().setSpeed(-20);
			hazard.GetComponent<RandomRotator>().setTumble(20);
		}
		startGame16();
	}
	
	public void startGame16() {
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
			StartCoroutine (SpawnWaves16 ());
		}
	}
	
	IEnumerator SpawnWaves16 ()
	{
		if (levelJump) {
			levelUpText.SetActive(true);
		}
		gameOverText.text = "Level 16";
		yield return new WaitForSeconds (startWait);
		while (true)
		{
			gameOverText.text = "";
			levelUpText.SetActive(false);
			for (int i = 0; i < hazardCount; i++)
			{
				if (gameOver) break;
				Vector3 spawnPosition = new Vector3 (-8, 0, Random.Range (0, 12));
				Quaternion spawnRotation = Quaternion.identity;
				Instantiate (hazardRight, spawnPosition, spawnRotation);
				if (i==0) {
					hazardRight.GetComponent<DestroyByContact>().setScoreValue(55);
				}
				if (i%2==0) {
					hazardRight.GetComponent<MoveHorizontal>().setSpeed(15,-9);
				} else {
					hazardRight.GetComponent<MoveHorizontal>().setSpeed(10,-7);
				}
				spawnPosition = new Vector3 (Random.Range (-spawnValues.x, spawnValues.x), spawnValues.y, spawnValues.z);
				spawnRotation = Quaternion.identity;
				Instantiate (hazard, spawnPosition, spawnRotation);
				if (i==0) {
					hazard.GetComponent<DestroyByContact>().setScoreValue(110);
					hazard.GetComponent<Mover>().setSpeed(-20);
					hazard.GetComponent<RandomRotator>().setTumble(20);
				}
				if (i%2==0) {
					hazard.GetComponent<Mover>().setSpeed(-20);
				} else {
					hazard.GetComponent<Mover>().setSpeed(-23);
				}
				yield return new WaitForSeconds (spawnWait);
			}
			// Level up logic
			if (score - prevScore >= level_16_17 && !gameOver) {
				prevScore = score;
				levelUpText.SetActive(true);
				levelUpText.GetComponent<Text>().text = "Getting closer :D";
				level++;
				levelJump = true;
				break;
			}
			//yield return new WaitForSeconds (waveWait);

			if (gameOver)
			{
				gameOverAction();
				break;
			}
		}
		if (!gameOver) {
			startLevel17();
		}
	}
	
	//************* Code for Level 17 Gameplay *************//
	
	public void startLevel17() {
		level = 17;
		player.GetComponent<PlayerComponent>().setFireRate(0.13f);
		if (!levelJump) {
			hazard.GetComponent<DestroyByContact>().setScoreValue(120);
			hazard.GetComponent<Mover>().setSpeed(-25);
			hazard.GetComponent<RandomRotator>().setTumble(25);
		}
		startGame17();
	}
	
	public void startGame17() {
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
			StartCoroutine (SpawnWaves17 ());
		}
	}
	
	IEnumerator SpawnWaves17 ()
	{
		if (levelJump) {
			levelUpText.SetActive(true);
		}
		gameOverText.text = "Level 17";
		yield return new WaitForSeconds (startWait);
		while (true)
		{
			gameOverText.text = "";
			levelUpText.SetActive(false);
			for (int i = 0; i < hazardCount; i++)
			{
				if (gameOver) break;
				Vector3 spawnPosition = new Vector3 (-8, 0, Random.Range (0, 12));
				Quaternion spawnRotation = Quaternion.identity;
				Instantiate (hazardRight, spawnPosition, spawnRotation);
				if (i==0) {
					hazardRight.GetComponent<DestroyByContact>().setScoreValue(60);
				}
				if (i%2==0) {
					hazardRight.GetComponent<MoveHorizontal>().setSpeed(16,-10);
				} else {
					hazardRight.GetComponent<MoveHorizontal>().setSpeed(11,-8);
				}
				spawnPosition = new Vector3 (Random.Range (-spawnValues.x, spawnValues.x), spawnValues.y, spawnValues.z);
				spawnRotation = Quaternion.identity;
				Instantiate (hazard, spawnPosition, spawnRotation);
				if (i==0) {
					hazard.GetComponent<DestroyByContact>().setScoreValue(120);
					hazard.GetComponent<Mover>().setSpeed(-25);
					hazard.GetComponent<RandomRotator>().setTumble(25);
				}
				if (i%2==0) {
					hazard.GetComponent<Mover>().setSpeed(-25);
				} else {
					hazard.GetComponent<Mover>().setSpeed(-28);
				}
				yield return new WaitForSeconds (spawnWait);
			}
			// Level up logic
			if (score - prevScore >= level_17_18 && !gameOver) {
				prevScore = score;
				levelUpText.SetActive(true);
				levelUpText.GetComponent<Text>().text = "Penultimate level !";
				level++;
				levelJump = true;
				break;
			}
			//yield return new WaitForSeconds (waveWait);

			if (gameOver)
			{
				gameOverAction();
				break;
			}
		}
		if (!gameOver) {
			startLevel8();
		}
	}
	
	//************* Code for Level 18 Gameplay *************//
	
	public void startLevel18() {
		level = 18;
		player.GetComponent<PlayerComponent>().setFireRate(0.11f);
		if (!levelJump) {
			hazard.GetComponent<DestroyByContact>().setScoreValue(130);
			hazard.GetComponent<Mover>().setSpeed(-30);
			hazard.GetComponent<RandomRotator>().setTumble(30);
		}
		startGame18();
	}
	
	public void startGame18() {
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
			StartCoroutine (SpawnWaves18 ());
		}
	}
	
	IEnumerator SpawnWaves18 ()
	{
		if (levelJump) {
			levelUpText.SetActive(true);
		}
		gameOverText.text = "Level 18";
		yield return new WaitForSeconds (startWait);
		while (true)
		{
			gameOverText.text = "";
			levelUpText.SetActive(false);
			for (int i = 0; i < hazardCount; i++)
			{
				if (gameOver) break;
				Vector3 spawnPosition = new Vector3 (-8, 0, Random.Range (0, 12));
				Quaternion spawnRotation = Quaternion.identity;
				Instantiate (hazardRight, spawnPosition, spawnRotation);
				if (i==0) {
					hazardRight.GetComponent<DestroyByContact>().setScoreValue(65);
				}
				if (i%2==0) {
					hazardRight.GetComponent<MoveHorizontal>().setSpeed(17,-11);
				} else {
					hazardRight.GetComponent<MoveHorizontal>().setSpeed(12,-9);
				}
				spawnPosition = new Vector3 (Random.Range (-spawnValues.x, spawnValues.x), spawnValues.y, spawnValues.z);
				spawnRotation = Quaternion.identity;
				Instantiate (hazard, spawnPosition, spawnRotation);
				if (i==0) {
					hazard.GetComponent<DestroyByContact>().setScoreValue(130);
					hazard.GetComponent<Mover>().setSpeed(-30);
					hazard.GetComponent<RandomRotator>().setTumble(30);
				}
				if (i%2==0) {
					hazard.GetComponent<Mover>().setSpeed(-30);
				} else {
					hazard.GetComponent<Mover>().setSpeed(-33);
				}
				yield return new WaitForSeconds (spawnWait);
			}
			// Level up logic
			if (score - prevScore >= level_18_19 && !gameOver) {
				prevScore = score;
				levelUpText.SetActive(true);
				levelUpText.GetComponent<Text>().text = "The ultimate fight";
				//level++;
				//levelJump = true;
				break;
			}
			//yield return new WaitForSeconds (waveWait);

			if (gameOver)
			{
				gameOverAction();
				break;
			}
		}
		if (!gameOver) {
			StartCoroutine (SpawnHorizontalWaves6 ());
		}
	}
	
	IEnumerator SpawnHorizontalWaves6 ()
	{
		yield return new WaitForSeconds (horizontalWaveWait);
		levelUpText.SetActive(false);
		for (int i = 0; i < 4*rightHazardCount; i++)
		{
			if (gameOver) break;
			Vector3 spawnPosition = new Vector3 (8, 0, Random.Range (0, 12));
			Quaternion spawnRotation = Quaternion.identity;
			GameObject hr = Instantiate (hazardRight, spawnPosition, spawnRotation);
			if (i%2==0) {
				hr.GetComponent<MoveHorizontal>().setSpeed(-10,-4);
			} else {
				hr.GetComponent<MoveHorizontal>().setSpeed(-5,-2);
			}
			spawnPosition = new Vector3 (-8, 0, Random.Range (0, 12));
			spawnRotation = Quaternion.identity;
			hr = Instantiate (hazardRight, spawnPosition, spawnRotation);
			if (i%2==0) {
				hr.GetComponent<MoveHorizontal>().setSpeed(10,-4);
			} else {
				hr.GetComponent<MoveHorizontal>().setSpeed(5,-2);
			}
			spawnPosition = new Vector3 (Random.Range (-spawnValues.x, spawnValues.x), spawnValues.y, spawnValues.z);
			spawnRotation = Quaternion.identity;
			Instantiate (hazard, spawnPosition, spawnRotation);
			if (i==0) {
				hazard.GetComponent<DestroyByContact>().setScoreValue(0);
				hazard.GetComponent<Mover>().setSpeed(-20);
				hazard.GetComponent<RandomRotator>().setTumble(30);
			}
			yield return new WaitForSeconds (spawnWait);
		}
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
			restartButton.SetActive (true);
			restart = true;
		} else {
			levelUpText.SetActive(true);
			levelUpText.GetComponent<Text>().text = "You are the champion !!\nGoing to level 1 now";
			level++;
			levelJump = true;
			startLevel1();
		}
	}
	
	//************* Button onclick functions **************//
	
	//Help screen functions
	
	public void helpScreen() {
		menuCanvas.SetActive(false);
		helpCanvas.SetActive(true);
		playerCanvas.SetActive(true);
		if (!isPaused) {
			player.SetActive(true);
		}
		movementZone.SetActive(true);
		fireZone.SetActive(true);
		playInfo.SetActive(false);
		pauseButton.SetActive(false);
		var color = Color.grey;
		color.a = 0.1f;
		movementZone.GetComponent<Image>().color = color;
		color = Color.red;
		color.a = 0.1f;
		fireZone.GetComponent<Image>().color = color;
		if (isSwitched) {
			if (isRightHand) {
				if (everLeft) {
					GameObject.Find("Fire Zone Text").GetComponent<RectTransform>().localPosition += new Vector3(110,0,0);
					GameObject.Find("Movement Zone Text").GetComponent<RectTransform>().localPosition -= new Vector3(55,0,0);
				}
			} else {
				GameObject.Find("Fire Zone Text").GetComponent<RectTransform>().localPosition -= new Vector3(110,0,0);
				GameObject.Find("Movement Zone Text").GetComponent<RectTransform>().localPosition += new Vector3(55,0,0);
				everLeft = true;
			}
		}
	}
	
	public void backToMenu() {
		helpCanvas.SetActive(false);
		player.SetActive(false);
		menuCanvas.SetActive(true);
		var color = Color.grey;
		color.a = 0;
		movementZone.GetComponent<Image>().color = color;
		color = Color.red;
		color.a = 0;
		fireZone.GetComponent<Image>().color = color;
		playInfo.SetActive(true);
		pauseButton.SetActive(true);
		playerCanvas.SetActive(false);
	}
	
	//Setting screen functions
	
	public void settingsScreen() {
		menuCanvas.SetActive(false);
		settingsCanvas.SetActive(true);
		playerSpeed = PlayerPrefs.GetFloat("playerSpeed");
		if (playerSpeed == 0) playerSpeed = 7;
		scrollBar.value = playerSpeed/20;
		playerCanvas.SetActive(true);
		if (!isPaused) {
			player.SetActive(true);
		}
		movementZone.SetActive(true);
		fireZone.SetActive(true);
		pauseButton.SetActive(false);
		playInfo.SetActive(false);
		var color = Color.grey;
		color.a = 0.1f;
		movementZone.GetComponent<Image>().color = color;
		color = Color.red;
		color.a = 0.1f;
		fireZone.GetComponent<Image>().color = color;
	}
	
	public void backToMenuFromSettings() {
		settingsCanvas.SetActive(false);
		player.SetActive(false);
		menuCanvas.SetActive(true);
		var color = Color.grey;
		color.a = 0;
		movementZone.GetComponent<Image>().color = color;
		color = Color.red;
		color.a = 0;
		fireZone.GetComponent<Image>().color = color;
		pauseButton.SetActive(true);
		playInfo.SetActive(true);
		playerCanvas.SetActive(false);
	}
	
	public void switchSide() {
		isSwitched = true;
		if (isRightHand) {
			movementZone.GetComponent<RectTransform>().localPosition += new Vector3(79.01f, 0, 0);
			fireZone.GetComponent<RectTransform>().localPosition -= new Vector3(180,0,0);
			isRightHand = false;
			PlayerPrefs.SetInt ("playerSide",1);
		} else {
			movementZone.GetComponent<RectTransform>().localPosition -= new Vector3(79.01f, 0, 0);
			fireZone.GetComponent<RectTransform>().localPosition += new Vector3(180,0,0);
			isRightHand = true;
			PlayerPrefs.SetInt ("playerSide",0);
		}
	}
	
	public void backToMenuFromLevels() {
		levelCanvas.SetActive(false);
		menuCanvas.SetActive(true);
	}
	
	public void continueToMainMenu() {
		firstTimeGame.SetActive(false);
		menuCanvas.SetActive(true);
	}
	
	public void pauseGame() {
		Time.timeScale = 0;
		isPaused = true;
		player.SetActive(false);
		menuCanvas.SetActive(true);
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
