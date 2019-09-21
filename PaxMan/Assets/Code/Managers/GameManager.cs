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
    }

    public static GameManager instance;
    public uint dotCount;
    public uint points;
    public int[] dotsToSpawnCherry;
    public GhostPoints ghostPoints;
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
        player = FindObjectOfType<PaxMan>();
        blinky = FindObjectOfType<Blinky>();
        ghosts = FindObjectsOfType<Ghost>();
    }
    void Update()
    {
        CheckCollisions();
        UpdateGlobalGameData();
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
        player.InitMovement();
        foreach (Ghost ghost in ghosts)
        {
            ghost.StartIA();
        }
    }

    public void OnGhostIsEaten(Vector2 _position)
    {
        points += ghostPoints.eatGhostPointsValue;
        GameObject popUpText = Instantiate(ghostPoints.popUpTextPrefab, _position, Quaternion.identity);
        popUpText.GetComponent<UI_PopUpText>().DisplayText(ghostPoints.eatGhostPointsValue.ToString(), _position, 1.5f);
        ghostPoints.eatGhostPointsValue *= ghostPoints.eatGhostPointsMultiplier;
    }
}
