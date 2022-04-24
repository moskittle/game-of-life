using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GameOfLifeManager : MonoBehaviour
{
    public GameObject cellPrefab;

    [Range(10, 50)]
    public int gridSize = 20;

    [Range(0.2f, 2.0f)]
    public float timeStep = 1.0f;
    private float timeAccumulator = 0.0f;

    private GameObject[,] cells;
    private long[,] cellValues; // a cell is alive if value is greater than 0; otherwise, it is dead
    private long[,] newCellValues;


    // Start is called before the first frame update
    void Start()
    {
        cells = new GameObject[gridSize, gridSize];
        cellValues = new long[gridSize, gridSize];
        newCellValues = new long[gridSize, gridSize];

        for (int i = 0; i < gridSize; ++i)
        {
            for (int j = 0; j < gridSize; ++j)
            {
                cells[i, j] = GameObject.Instantiate(cellPrefab, transform);

                // origin at (0, 0), range is [-gridSize/2, gridSize/2]
                Vector3 position = new Vector3(i - gridSize / 2.0f, 0, j - gridSize / 2.0f);
                cells[i, j].transform.position = position;

                bool visible = Random.Range(0f, 1f) > 0.5f;
                cellValues[i, j] = visible ? 1 : 0;
            }
        }
    }

    void Update()
    {
        timeAccumulator += Time.deltaTime;

        if (timeAccumulator > timeStep)
        {
            UpdateCell();

            while (timeAccumulator > 0.0f)
            {
                timeAccumulator -= timeStep;
            }
        }

        DisplayCells();
    }

    void UpdateCell()
    {
        newCellValues = cellValues;

        for (int i = 0; i < gridSize; ++i)
        {
            for (int j = 0; j < gridSize; ++j)
            {
                int neighborCount = GetAliveNeighborCount(cellValues, i, j);
                bool alive = IsAliveCell(cellValues, i, j);
                long newCellValue = 0;

                // rules here
                if (alive)
                {
                    // 1. If an "alive" cell had less than 2 or more than 3 alive neighbors(in any of the 8 surrounding cells), it becomes dead.
                    if (neighborCount < 2 || neighborCount > 3)
                    {
                        newCellValue = 0;   // dead
                        SetCellValue(newCellValues, i, j, newCellValue);
                    }
                }
                else
                {
                    // 2. If a "dead" cell had *exactly* 3 alive neighbors, it becomes alive.
                    if (neighborCount == 3)
                    {
                        newCellValue = 1;   // alive
                        SetCellValue(newCellValues, i, j, newCellValue);
                    }
                }
            }
        }

        cellValues = newCellValues;
    }

    void DisplayCells()
    {
        for (int i = 0; i < gridSize; ++i)
        {
            for (int j = 0; j < gridSize; ++j)
            {
                var alive = IsAliveCell(cellValues, i, j);
                cells[i, j].SetActive(alive);
            }
        }
    }

    private bool IsValidCell(int i, int j)
    {
        if (i < 0 || i >= gridSize || j < 0 || j >= gridSize)
        {
            return false;
        }

        return true;
    }

    private bool IsAliveCell(long[,] cells, int i, int j)
    {
        if (IsValidCell(i, j) == false) { return false; }

        return cells[i, j] > 0;
    }

    private void SetCellValue(long[,] cells, int i, int j, long val)
    {
        if (!IsValidCell(i, j))
        {
            Debug.Log("Cell (" + i + ", " + j + ") is not valid.");
        }

        cells[i, j] = val;
    }


    private int GetAliveNeighborCount(long[,] cells, int i, int j)
    {
        int aliveCount = 0;

        for (int x = i - 1; x <= i + 1; x += 2)
        {
            for (int y = j - 1; y <= j + 1; y += 2)
            {
                if (IsValidCell(x, y) && IsAliveCell(cells, x, y))
                {
                    aliveCount++;
                }
            }
        }

        return aliveCount;
    }
}
