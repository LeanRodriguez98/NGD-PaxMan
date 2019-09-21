using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{

    public struct GlobalGameData
    {
        public Vector2 paxManPosition;
        public Vector2 paxManDirection;
        public Vector2 blinkyPosition;
    }

    [System.Serializable]
    public struct GhostPoints
    {
        public GameObject popUpTextPrefab;
        public uint eatGhostPointsValue;
        public uint eatGhostPointsMultiplier;
        public float popUpTextDuration;
    }

    public static GameManager instance;
    [Space(10)]
    public int[] dotsNesesatyToSpawnCherry;
    [Space(10)]
    public GhostPoints ghostPoints;
    [Space(10)]
    public SO_ScoreData scoreData;
    [Space(10)]
    public float gameOverSignDuration;

    [HideInInspector] public bool gameOver;

    private uint points;
    private uint dotCount;
    private GlobalGameData globalGameData;
    private Map map;
    private UI_CanvasManager canvasManager;
    private PaxMan player;
    private Blinky blinky;
    private Ghost[] ghosts;

    private const string mainMenuSceneName = "MainMenu";
    public GlobalGameData GameData
    {
        get { return globalGameData; }
    }


    private void Awake()
    {
        if (instance == null)
            instance = this;
    }

    void Start()
    {
        Application.targetFrameRate = 60;
        map = Map.instance;
        canvasManager = UI_CanvasManager.instance;
        SerializeSystem.LoadGame(scoreData);
        points = (uint)scoreData.currentScore;
        canvasManager.UpdateScore(points);
        canvasManager.UpdateHighScore((uint)scoreData.highScore);
        player = FindObjectOfType<PaxMan>();
        blinky = FindObjectOfType<Blinky>();
        ghosts = FindObjectsOfType<Ghost>();
        gameOver = false;

    }
    void Update()
    {
        CheckCollisions();
        UpdateGlobalGameData();
        CheckVictory();
        if (Input.GetKeyDown(KeyCode.X))
            BackToMainMenu();
    }

    private void CheckVictory()
    {
        if (dotCount >= (map.smallDots.Count + map.bigDots.Count))
        {
            scoreData.currentScore = (int)points;
            SerializeSystem.SaveGame(scoreData);
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    private void UpdateGlobalGameData()
    {
        globalGameData.paxManPosition = player.transform.position;
        globalGameData.paxManDirection = player.Direction;
        globalGameData.blinkyPosition = blinky.transform.position;
    }
    private void CheckCollisions()
    {
        for (int i = 0; i < map.smallDots.Count; i++)
        {
            if (map.smallDots[i].gameObject.activeSelf && map.smallDots[i].CheckIsPickUped(player.transform))
            {
                points += map.smallDots[i].pointsValue;
                canvasManager.UpdateScore(points);
                CheckUpdateHighScore();
                map.smallDots[i].gameObject.SetActive(false);
                dotCount++;
                CheckSpawnCherry();
                CheckGhostLeaving();
                break;
            }
        }

        for (int i = 0; i < map.bigDots.Count; i++)
        {

            if (map.bigDots[i].gameObject.activeSelf && map.bigDots[i].CheckIsPickUped(player.transform))
            {
                points += map.bigDots[i].pointsValue;
                canvasManager.UpdateScore(points);
                CheckUpdateHighScore();
                map.bigDots[i].gameObject.SetActive(false);
                dotCount++;
                CheckSpawnCherry();
                SetPoweredPaxMan();
                CheckGhostLeaving();
                break;
            }
        }

        if (map.cherrys.gameObject.activeSelf && map.cherrys.CheckIsPickUped(player.transform))
        {
            points += map.cherrys.pointsValue;
            canvasManager.UpdateScore(points);
            CheckUpdateHighScore();
            canvasManager.ShowOneMoreCherry();
            map.DisableCherry();
        }
    }

    private void CheckUpdateHighScore()
    {
        if (points > scoreData.highScore)
        {
            canvasManager.UpdateHighScore(points);
            scoreData.highScore = (int)points;
        }
    }

    private void CheckSpawnCherry()
    {
        for (int i = 0; i < dotsNesesatyToSpawnCherry.Length; i++)
        {
            if (dotsNesesatyToSpawnCherry[i] == dotCount)
            {
                map.EnableCherry();
            }
        }
    }

    private void CheckGhostLeaving()
    {
        foreach (Ghost ghost in ghosts)
        {
            if (ghost.homePatron.dotsNecesaryToLeave <= dotCount)
            {
                ghost.LeaveHose();
            }
        }
    }

    private void SetPoweredPaxMan()
    {
        player.EnablePower();
        foreach (Ghost ghost in ghosts)
        {
            ghost.SetPanic();
        }
    }

    public void StopAllGameCorrutines()
    {
        player.StopAllCoroutines();
        foreach (Ghost ghost in ghosts)
        {
            ghost.StopAllCoroutines();
        }
    }

    public void OnDeadPaxMan()
    {
        if (player.lifes > 0)
        {
            player.InitMovement();
            foreach (Ghost ghost in ghosts)
            {
                ghost.StartIA();
            }
            CheckGhostLeaving();
            canvasManager.RemoveOneLifeIcon();
        }
        else
        {
            scoreData.currentScore = 0;
            SerializeSystem.SaveGame(scoreData);
            gameOver = true;
            canvasManager.ShowGameOverSign();
            player.TurnOffSprite();
            Invoke("BackToMainMenu", gameOverSignDuration);
        }
    }

    public void BackToMainMenu()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void OnGhostIsEaten(Ghost _ghost)
    {
        points += ghostPoints.eatGhostPointsValue;
        GameObject popUpText = Instantiate(ghostPoints.popUpTextPrefab, _ghost.transform.position, Quaternion.identity);
        popUpText.GetComponent<UI_PopUpText>().DisplayText(ghostPoints.eatGhostPointsValue.ToString(), _ghost.transform.position, ghostPoints.popUpTextDuration);
        ghostPoints.eatGhostPointsValue *= ghostPoints.eatGhostPointsMultiplier;
        canvasManager.UpdateScore(points);
        CheckUpdateHighScore();
        player.PauseMovement(ghostPoints.popUpTextDuration);
        foreach (Ghost ghost in ghosts)
        {
            ghost.PauseMovement(ghostPoints.popUpTextDuration);
        }
        player.TurnOffSprite(ghostPoints.popUpTextDuration);
        _ghost.TurnOffSprite(ghostPoints.popUpTextDuration);
    }
}
