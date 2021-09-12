using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Level {

    public int levelName;
    public int floorLength;
    public string[] obstacleNames;
    public float[,] diamondPositions;

    public void ConvertPosition(Transform[] diamonds) {
        diamondPositions = new float[diamonds.Length, 3];

        for (int i = 0; i < diamonds.Length; i++) {
            diamondPositions[i, 0] = (float)Math.Round(diamonds[i].position.x, 2);
            diamondPositions[i, 1] = (float)Math.Round(diamonds[i].position.y, 2);
            diamondPositions[i, 2] = (float)Math.Round(diamonds[i].position.z, 2);
        }
    }

}
