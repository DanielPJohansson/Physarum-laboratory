using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class DataManager
{

    public static Cell[] GenerateCellsWithStartPositions(uint numberOfCells, SpeciesSettings speciesSettings, List<Vector2> positions)
    {
        Cell[] cells = new Cell[numberOfCells];
        int numberOfPoints = positions.Count();

        for (int i = 0; i < numberOfCells; i++)
        {
            Vector2 cellPosition = positions[Random.Range(0, numberOfPoints)];

            Cell cell = new()
            {
                position = cellPosition,
                angle = Random.Range(0f, 2 * Mathf.PI),
                velocity = Random.Range(speciesSettings.velocity * (1 - speciesSettings.velocityVariation), speciesSettings.velocity * (1 + speciesSettings.velocityVariation)),
            };
            cells[i] = cell;
        }

        return cells;
    }

    public static List<Vector2> GenerateRandomPointsInCircle(int numberOfPoints, Vector2Int texResolution)
    {
        List<Vector2> positions = new();

        for (int i = 0; i < numberOfPoints; i++)
        {
            Vector2 newStartPosition = new Vector2(Random.Range(-texResolution.x / 2 + 1, texResolution.x / 2), Random.Range(-texResolution.y / 2 + 1, texResolution.y / 2));
            newStartPosition.Normalize();
            newStartPosition = newStartPosition * Random.Range(1, texResolution.x / 2) + texResolution / 2;

            positions.Add(newStartPosition);
        }

        return positions;
    }

    public static List<Vector2> GenerateRandomPointsInTexture(int numberOfPoints, Vector2Int texResolution)
    {
        List<Vector2> positions = new();

        for (int i = 0; i < numberOfPoints; i++)
        {
            Vector2 newStartPosition = new Vector2(Random.Range(1, texResolution.x), Random.Range(1, texResolution.y));
            positions.Add(newStartPosition);
        }

        return positions;
    }
}
