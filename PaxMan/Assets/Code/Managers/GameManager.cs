﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    public static GameManager instance;
    public uint dotCount;
    public uint points;
    public int[] dotsToSpawnCherry;
    private Map map;
    private UI_CanvasManager canvasManager;
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
        canvasManager = UI_CanvasManager.instance;
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

    public void OnDeadPaxMan()
    {

        player.InitMovement();
        foreach (Ghost ghost in ghosts)
        {
            ghost.StopAllCoroutines();
            ghost.StartIA();
        }
    }

}
