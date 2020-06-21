using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseTester : MonoBehaviour
{
    private GameManager _gameManager;

    // Start is called before the first frame update
    void Start()
    {
        _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            float _tileGap = .1f;
            int x = 0;
            int y = 0;

            GameObject board = GameObject.Find("Board");

            float x_scale = board.GetComponent<MeshRenderer>().bounds.size.x / 10 - _tileGap;
            float y_scale = board.GetComponent<MeshRenderer>().bounds.size.y / 10 - _tileGap;

            float x_min = -(board.GetComponent<MeshRenderer>().bounds.size.x / 2);// - x_scale / 2);
            float y_min = -(board.GetComponent<MeshRenderer>().bounds.size.y / 2);// - y_scale / 2);

            Vector3 mP = Input.mousePosition;

            mP.z = 14;

            Vector3 mousePos = Camera.main.ScreenToWorldPoint(mP);

            x = Mathf.RoundToInt((mousePos.x - x_min - _tileGap / 2) / (x_scale + _tileGap));
            y = Mathf.RoundToInt((mousePos.y - y_min - _tileGap / 2) / (y_scale + _tileGap));


            Debug.Log(x + "-" + y);
        }
        
    }

}
