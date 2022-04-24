using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GameOfLifeManager : MonoBehaviour
{
    public GameObject cellPrefab;

    public int gridSize = 20;


    private GameObject[,] cells;
    private long[,] cellValues;


    // Start is called before the first frame update
    void Start()
    {
        cells = new GameObject[gridSize, gridSize];
        cellValues = new long[gridSize, gridSize];


        for (int i = 0; i < gridSize; ++i)
        {
            for (int j = 0; j < gridSize; ++j)
            {
                cells[i, j] = GameObject.Instantiate(cellPrefab, transform);
                Vector3 position = new Vector3(i - gridSize / 2.0f, 0, j - gridSize / 2.0f);
                cells[i, j].transform.position = position;

                bool visible = Random.Range(0f, 1f) > 0.5f;
                cells[i, j].SetActive(visible);
                cellValues[i, j] = 0;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        updateCell();
        displayCells();
    }

    void updateCell()
    {
    }

    void displayCells()
    {

        for (int i = 0; i < gridSize; ++i)
        {
            for (int j = 0; j < gridSize; ++j)
            {
                if (cellValues[i, j] > 0)
                {
                    cells[i, j].SetActive(true);
                }
            }
        }

    }
}
