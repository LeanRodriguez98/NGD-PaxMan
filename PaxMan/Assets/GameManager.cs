using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    public static GameManager instance;
    public uint dotCount;
    public uint points;

    private Map map;
    private PaxMan player;
    private void Awake()
    {
        if (instance == null)
            instance = this;
    }

    void Start()
    {
        map = Map.instance;
        player = FindObjectOfType<PaxMan>();
    }
    void Update()
    {
        ChechCollisions();
    }

    private void ChechCollisions()
    {
        for (int i = 0; i < map.smallDots.Count; i++)
        {
            if (map.smallDots[i].gameObject.activeSelf && map.smallDots[i].CheckIsPickUped(player.transform))
            {
                points += map.smallDots[i].pointsValue;
                map.smallDots[i].gameObject.SetActive(false);
                break;
            }
        }

        for (int i = 0; i < map.bigDots.Count; i++)
        {
            if (map.bigDots[i].gameObject.activeSelf && map.bigDots[i].CheckIsPickUped(player.transform))
            {
                points += map.bigDots[i].pointsValue;
                map.bigDots[i].gameObject.SetActive(false);
                break;
            }
        }

        for (int i = 0; i < map.cherrys.Count; i++)
        {
            if (map.cherrys[i].gameObject.activeSelf && map.cherrys[i].CheckIsPickUped(player.transform))
            {
                points += map.cherrys[i].pointsValue;
                map.cherrys[i].gameObject.SetActive(false);
                break;
            }
        }
    }
}
