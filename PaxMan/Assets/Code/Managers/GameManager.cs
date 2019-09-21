using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    public uint dotCount;
    public uint points;
    public int[] dotsToSpawnCherry;
    public GhostPoints ghostPoints;
    public SO_ScoreData scoreData;
    private GlobalGameData globalGameData;
    private Map map;
    private UI_CanvasManager canvasManager;
    private PaxMan player;
    private Blinky blinky;
    private Ghost[] ghosts;
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

    }
    void Update()
    {
        CheckCollisions();
        UpdateGlobalGameData();
        CheckVictory();
    }

    private void CheckVictory()
    {
        if (dotCount >= (map.smallDots.Count + map.bigDots.Count))
        {
            Time.timeScale = 0;
            scoreData.currentScore = (int)points;
            SerializeSystem.SaveGame(scoreData);
            Debug.Log("Win");
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
        for (int i = 0; i < dotsToSpawnCherry.Length; i++)
        {
            if (dotsToSpawnCherry[i] == dotCount)
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
            Time.timeScale = 0;
            Debug.Log("GameOver");
        }
    }

    public void OnGhostIsEaten(Vector2 _position)
    {
        points += ghostPoints.eatGhostPointsValue;
        GameObject popUpText = Instantiate(ghostPoints.popUpTextPrefab, _position, Quaternion.identity);
        popUpText.GetComponent<UI_PopUpText>().DisplayText(ghostPoints.eatGhostPointsValue.ToString(), _position, ghostPoints.popUpTextDuration);
        ghostPoints.eatGhostPointsValue *= ghostPoints.eatGhostPointsMultiplier;
        canvasManager.UpdateScore(points);
        CheckUpdateHighScore();
    }
}
