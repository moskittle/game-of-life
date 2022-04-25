using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GameOfLifeManager : MonoBehaviour
{
    public GameObject cellPrefab;

    [Range(4, 100)]
    public int gridSize = 20;

    [Range(0.05f, 2.0f)]
    public float timeStep = 1.0f;
    private float timeAccumulator = 0.0f;

    public Gradient colorPallete;

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
                cells[i, j].name = "Cell_" + i + "_" + j;

                //bool visible = Random.Range(0f, 1f) > 0.5f;
                //cellValues[i, j] = visible ? 1 : 0;
                cellValues[i, j] = 0;
            }
        }

        //cellValues[0, 1] = 1;
        //cellValues[1, 2] = 1;
        //cellValues[2, 0] = 1;
        //cellValues[2, 1] = 1;
        //cellValues[2, 2] = 1;

        //cellValues[0, 1] = 1;
        //cellValues[1, 1] = 1;
        //cellValues[2, 1] = 1;
        //cellValues[2, 2] = 1;
        //cellValues[1, 3] = 1;

        //cellValues[gridSize - 1, 0] = 1;
        //cellValues[gridSize - 2, 1] = 1;
        //cellValues[gridSize - 3, 1] = 1;
        //cellValues[gridSize - 1, 2] = 1;
        //cellValues[gridSize - 2, 2] = 1;

        SetCapPattern(cellValues);

        UpdateCellVisibility();

    }

    void Update()
    {
        timeAccumulator += Time.deltaTime;

        if (timeAccumulator > timeStep)
        {
            UpdateCell();
            UpdateCellVisibility();


            while (timeAccumulator > 0.0f)
            {
                timeAccumulator -= timeStep;
            }
        }

    }

    void UpdateCell()
    {
        CopyCellsValue(cellValues, newCellValues);

        for (int i = 0; i < gridSize; ++i)
        {
            for (int j = 0; j < gridSize; ++j)
            {
                int neighborCount = GetAliveNeighborCount(cellValues, i, j);
                bool alive = IsAliveCell(cellValues, i, j);
                long newValue = 0;

                // TODO: remove log
                //Debug.Log("(" + i + ", " + j + ") : " + "alive(" + alive + ")" + ", neighbors(" + neighborCount + ")");

                // rules here
                if (alive)
                {
                    // 1. If an "alive" cell had less than 2 or more than 3 alive neighbors(in any of the 8 surrounding cells), it becomes dead.
                    if (neighborCount < 2 || neighborCount > 3)
                    {
                        newValue = 0;   // dead
                        SetCellValue(newCellValues, i, j, newValue);
                    }
                    // 2. If survived, grow older by increase value by 1
                    else
                    {
                        newValue = cellValues[i, j] + 1;
                        newValue = newValue > 4 ? 4 : newValue;

                        SetCellValue(newCellValues, i, j, newValue);
                    }
                }
                else
                {
                    // 3. If a "dead" cell had *exactly* 3 alive neighbors, it becomes alive.
                    if (neighborCount == 3)
                    {
                        newValue = 1;   // alive
                        SetCellValue(newCellValues, i, j, newValue);
                    }
                }
            }
        }

        CopyCellsValue(newCellValues, cellValues);
    }

    void UpdateCellVisibility()
    {
        for (int i = 0; i < gridSize; ++i)
        {
            for (int j = 0; j < gridSize; ++j)
            {
                var alive = IsAliveCell(cellValues, i, j);
                cells[i, j].SetActive(alive);

                if (alive)
                {
                    var newColorRange = cellValues[i, j] / 4.0f;

                    Material mat = cells[i, j].GetComponent<MeshRenderer>().material;
                    var displayColor = colorPallete.Evaluate(newColorRange);
                    mat.SetColor("_Color", displayColor);
                }
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

        for (int x = i - 1; x <= i + 1; ++x)
        {
            for (int y = j - 1; y <= j + 1; ++y)
            {
                if (x == i && y == j) { continue; } // skip this cell itself

                if (IsValidCell(x, y) && IsAliveCell(cells, x, y))
                {
                    aliveCount++;
                }
            }
        }

        return aliveCount;
    }

    private void ResetPattern(long[,] grid)
    {
        for (int i = 0; i < gridSize; ++i)
        {
            for (int j = 0; j < gridSize; ++j)
            {
                grid[i, j] = 0;
            }
        }
    }

    private void SetCapPattern(long[,] grid)
    {
        ResetPattern(grid);

        // Cap
        int center = gridSize / 2;
        cellValues[center, center] = 1;
        cellValues[center + 1, center] = 1;
        cellValues[center + 2, center] = 1;
        cellValues[center + 3, center] = 1;
        cellValues[center, center + 1] = 1;
        cellValues[center + 3, center + 1] = 1;
        cellValues[center + 1, center + 2] = 1;
        cellValues[center + 2, center + 2] = 1;
    }

    private void CopyCellsValue(long[,] src, long[,] dest)
    {
        for (int i = 0; i < gridSize; ++i)
        {
            for (int j = 0; j < gridSize; ++j)
            {
                dest[i, j] = src[i, j];
            }
        }
    }
}
