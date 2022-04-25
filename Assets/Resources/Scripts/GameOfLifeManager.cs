using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameOfLifeManager : MonoBehaviour
{
    public GameObject cellPrefab;

    [Range(4, 100)]
    public int gridSize = 20;

    [Range(0.03f, 1.0f)]
    public float timeStep = 1.0f;
    private float timeAccumulator = 0.0f;
    public Slider timeStepSlider;

    public Gradient defaultColorPallete;
    private Gradient currColorPallete;

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

        // init pattern
        SetCapPattern();

        // init color
        currColorPallete = new Gradient();
        SetDefaultColorPallet();

        UpdateCellVisibility();
    }

    void Update()
    {
        timeAccumulator += Time.deltaTime;

        if (timeAccumulator > timeStep)
        {
            UpdateCellVisibility();
            UpdateCell();


            while (timeAccumulator > 0.0f)
            {
                timeAccumulator -= timeStep;
            }
        }

    }

    public void TimeStepValueChangeCheck()
    {
        timeStep = Mathf.SmoothStep(0.01f, 1.0f, timeStepSlider.value);

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
                    var displayColor = currColorPallete.Evaluate(newColorRange);
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

    #region Pattern

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

    // randomly choose 25% cells as alive
    public void SetRandomPattern25()
    {
        ResetPattern(cellValues);

        for (int i = 0; i < gridSize; ++i)
        {
            for (int j = 0; j < gridSize; ++j)
            {
                float rand = Random.Range(0f, 1f);

                if (rand > 0.75f)
                {
                    cellValues[i, j] = 1;
                }
            }
        }
    }

    public void SetCapPattern()
    {
        ResetPattern(cellValues);

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

    public void SetTearDropPattern()
    {
        ResetPattern(cellValues);

        int center = gridSize / 2;
        cellValues[center - 1, center] = 1;
        cellValues[center - 1, center + 1] = 1;
        cellValues[center, center - 1] = 1;
        cellValues[center + 1, center - 1] = 1;
        cellValues[center, center + 2] = 1;
        cellValues[center + 1, center + 2] = 1;
        cellValues[center + 2, center + 2] = 1;
        cellValues[center + 2, center + 1] = 1;
        cellValues[center + 2, center] = 1;
    }

    public void SetTestTubeBabyPattern()
    {
        ResetPattern(cellValues);

        int center = gridSize / 2;
        cellValues[center - 1, center] = 1;
        cellValues[center - 1, center + 1] = 1;
        cellValues[center - 1, center + 2] = 1;

        cellValues[center - 2, center + 3] = 1;
        cellValues[center - 3, center + 3] = 1;
        cellValues[center - 3, center + 2] = 1;

        cellValues[center, center - 1] = 1;
        cellValues[center + 1, center - 1] = 1;

        cellValues[center + 2, center] = 1;
        cellValues[center + 2, center + 1] = 1;
        cellValues[center + 2, center + 2] = 1;

        cellValues[center + 3, center + 3] = 1;
        cellValues[center + 4, center + 3] = 1;
        cellValues[center + 4, center + 2] = 1;
    }

    #endregion // Pattern

    #region Color Pallete

    public void SetDefaultColorPallet()
    {
        currColorPallete.SetKeys(defaultColorPallete.colorKeys, defaultColorPallete.alphaKeys);
        currColorPallete.mode = defaultColorPallete.mode;
    }

    public void SetBlueGradientPallete()
    {
        SetGradientPallete(Color.blue);
    }

    public void SetGreenGradientPallete()
    {
        SetGradientPallete(Color.green);
    }

    public void SetRedGradientPallete()
    {
        SetGradientPallete(Color.red);
    }


    public void SetGradientPallete(Color color)
    {
        GradientColorKey[] colorKey;
        GradientAlphaKey[] alphaKey;

        colorKey = new GradientColorKey[2];
        colorKey[0].color = color;
        colorKey[0].time = 0.0f;
        colorKey[1].color = color;
        colorKey[1].time = 1.0f;

        // Populate the alpha  keys at relative time 0 and 1  (0 and 100%)
        alphaKey = new GradientAlphaKey[2];
        alphaKey[0].alpha = 0.3f;
        alphaKey[0].time = 0.0f;
        alphaKey[1].alpha = 1.0f;
        alphaKey[1].time = 1.0f;

        currColorPallete.SetKeys(colorKey, alphaKey);
        currColorPallete.mode = GradientMode.Blend;
    }

    #endregion // Color Pallete

}
