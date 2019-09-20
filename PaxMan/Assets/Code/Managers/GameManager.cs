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

    public static GameManager instance;
    public uint dotCount;
    public uint points;
    public int[] dotsToSpawnCherry;
    private GlobalGameData globalGameData;
    private Map map;
    private UI_CanvasManager canvasManager;
    private PaxMan player;
    private Blinky blinky;
    private Inky inky;
    private Pinky pinky;
    private Clyde clyde;

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
        player = FindObjectOfType<PaxMan>();
        blinky = FindObjectOfType<Blinky>();
        inky = FindObjectOfType<Inky>();
        pinky = FindObjectOfType<Pinky>();
        clyde = FindObjectOfType<Clyde>();
    }
    void Update()
    {
        CheckCollisions();

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
                map.smallDots[i].gameObject.SetActive(false);
                dotCount++;
                CheckSpawnCherry();
                break;
            }
        }

        for (int i = 0; i < map.bigDots.Count; i++)
        {

            if (map.bigDots[i].gameObject.activeSelf && map.bigDots[i].CheckIsPickUped(player.transform))
            {
                points += map.bigDots[i].pointsValue;
                canvasManager.UpdateScore(points);
                map.bigDots[i].gameObject.SetActive(false);
                dotCount++;
                CheckSpawnCherry();
                break;
            }
        }

        if (map.cherrys.gameObject.activeSelf && map.cherrys.CheckIsPickUped(player.transform))
        {
            points += map.cherrys.pointsValue;
            canvasManager.UpdateScore(points);
            canvasManager.ShowOneMoreCherry();
            map.DisableCherry();
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

    public void StopAllGameCorrutines()
    {
        player.StopAllCoroutines();
        blinky.StopAllCoroutines();
        pinky.StopAllCoroutines();
        inky.StopAllCoroutines();
        clyde.StopAllCoroutines();
    }

    public void OnDeadPaxMan()
    {
        player.InitMovement();
        blinky.StartIA();
        pinky.StartIA();
        inky.StartIA();
        clyde.StartIA();
    }

}
