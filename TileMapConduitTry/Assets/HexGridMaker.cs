using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexGridMaker : MonoBehaviour
{
    public GameObject whitehex;
    public GameObject redhex;
    public GameObject boardImage;

    float root3over2 = 0.86602540378f;
    bool oddRow = true;
    int desiredRows = 4; //should be odd
    int desiredColumns = 10; //should be even
    float tileGap = .05f;

    float maxLen;
    float maxHeight;

    // Start is called before the first frame update
    void Start()
    {
        /*float xScale = (maxLen - tileGap * (desiredColumns)) / (desiredColumns * centToSide * 2);
        float yScale = (maxHeight - tileGap * (desiredRows + 1)) / desiredRows;


        for(float x = -8.5f + tileGap; x <= 8.5 + centToSide * 2; x = x + centToSide * 2)
        {
            for(float y = 4.5f; y >= -4.5f; y = y - 1.5f)
            {
                GameObject tempHex = Instantiate(whitehex, new Vector3(x, y, 0f), Quaternion.identity);
                tempHex.transform.localScale = new Vector2(xScale, yScale);
            }
        }

        for(float x = -8.5f - centToSide + tileGap; x <= 8.5 + centToSide * 2; x = x + centToSide * 2)
        {
            for (float y = 4.5f + .75f; y >= -4.5f - .75f; y = y - 1.5f)
            {
                GameObject tempHex = Instantiate(redhex, new Vector3(x, y, 0f), Quaternion.identity);
                tempHex.transform.localScale = new Vector2(xScale, yScale);
            }
        }*/


        int mostHexes = 0;
        int highestColumns = 0;
        int highestRows = 0;
        float threshhold = .75f;
        for(int numCols = 0; numCols < 12; numCols++)
        {
            for(int numRows = 0; numRows < 12; numRows++)
            {
                maxLen = boardImage.GetComponent<SpriteRenderer>().bounds.size.x;
                maxHeight = boardImage.GetComponent<SpriteRenderer>().bounds.size.y;

                maxLen = ((maxHeight - 2 * numRows * tileGap) / ((3 * numRows - 1) / 2f)) * root3over2 * (numCols - tileGap) + tileGap;

                while (maxLen > boardImage.GetComponent<SpriteRenderer>().bounds.size.x)
                {
                    maxHeight -= .5f;
                    maxLen = ((maxHeight - 2 * numRows * tileGap) / ((3 * numRows - 1) / 2f)) * root3over2 * (numCols - tileGap) + tileGap;
                }

                if((maxLen * maxHeight) / (boardImage.GetComponent<SpriteRenderer>().bounds.size.x * boardImage.GetComponent<SpriteRenderer>().bounds.size.y) >= threshhold)
                {
                    if((numCols*numRows) + (numRows-1) * (numCols-1) > mostHexes)
                    {
                        highestColumns = numCols;
                        highestRows = numRows;
                    }
                }
            }
        }

        desiredColumns = highestColumns;
        desiredRows = highestRows;

        maxLen = boardImage.GetComponent<SpriteRenderer>().bounds.size.x;
        maxHeight = boardImage.GetComponent<SpriteRenderer>().bounds.size.y;

        maxLen = ((maxHeight - 2 * desiredRows * tileGap) / ((3 * desiredRows - 1) / 2f)) * root3over2 * (desiredColumns - tileGap) + tileGap;

        while(maxLen > boardImage.GetComponent<SpriteRenderer>().bounds.size.x)
        {
            maxHeight -= .5f;
            maxLen = ((maxHeight - 2 * desiredRows * tileGap) / ((3 * desiredRows - 1) / 2f)) * root3over2 * (desiredColumns - tileGap) + tileGap;
        }

        Debug.Log((maxLen * maxHeight) / (boardImage.GetComponent<SpriteRenderer>().bounds.size.x * boardImage.GetComponent<SpriteRenderer>().bounds.size.y));

        float xScale = ((maxLen - tileGap) / desiredColumns - tileGap) / root3over2;
        float yScale = ((maxHeight - 2 * desiredRows * tileGap) / ((3 * desiredRows - 1) / 2f));

        Debug.Log(maxHeight - 2 * desiredRows * tileGap + "::::" + (3 * desiredRows - 1) / 2f);


        float xmin = -maxLen / 2f;
        float ymin = -maxHeight / 2f;

        float xStartOdd = xmin + tileGap + root3over2 / 2f * xScale;
        float yStartOdd = ymin + tileGap + yScale / 2f;

        float xStartEven = xmin + tileGap + root3over2 * xScale + tileGap / 2f;
        float yStartEven = ymin + 2 * tileGap + yScale + yScale / 4f;

        float xIncrement = tileGap + root3over2 * xScale;
        float yIncrement = 2 * tileGap +  1.5f * yScale;

        for (int i = 0; i < desiredColumns; i++)
        {
            for(int j = 0; j < desiredRows; j++)
            {
                GameObject tempHex = Instantiate(whitehex, new Vector3(xStartOdd + xIncrement * i, yStartOdd + yIncrement * j, 0f), Quaternion.identity);
                tempHex.transform.localScale = new Vector2(xScale, yScale);
            }
        }

        for(int i = 0; i < desiredColumns - 1; i++)
        {
            for(int j = 0; j < desiredRows - 1; j++)
            {
                GameObject tempHex = Instantiate(whitehex, new Vector3(xStartEven + xIncrement * i, yStartEven + yIncrement * j, 0f), Quaternion.identity);
                tempHex.transform.localScale = new Vector2(xScale, yScale);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
    }
}
