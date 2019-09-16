using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    public static GameManager instance;
    public uint dotCount;
    public uint points;
    public int[] dotsToSpawnCherry;
    private Map map;
    public PaxMan player;
    private Ghost[] ghosts;
    private void Awake()
    {
        if (instance == null)
            instance = this;
    }

    void Start()
    {
        Application.targetFrameRate = 60;
        map = Map.instance;
        player = FindObjectOfType<PaxMan>();
        ghosts = FindObjectsOfType<Ghost>();
    }
    void Update()
    {
        CheckCollisions();
    }

    private void CheckCollisions()
    {
        for (int i = 0; i < map.smallDots.Count; i++)
        {
            if (map.smallDots[i].gameObject.activeSelf && map.smallDots[i].CheckIsPickUped(player.transform))
            {
                points += map.smallDots[i].pointsValue;
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
                map.bigDots[i].gameObject.SetActive(false);
                dotCount++;
                CheckSpawnCherry();
                break;
            }
        }

        if (map.cherrys.gameObject.activeSelf && map.cherrys.CheckIsPickUped(player.transform))
        {
            points += map.cherrys.pointsValue;
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

}
