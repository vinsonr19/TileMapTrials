using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MoveableObject : MonoBehaviour
{
    private GameManager _gameManager;
    private float _staticZ;
    private bool _isDragged = false;
    private Spirit _spirit;
    
    // Start is called before the first frame update
    void Start()
    {
        _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

        _staticZ = Camera.main.WorldToScreenPoint(new Vector3(0f, 0f, -.6f)).z;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public Spirit GetSpirit()
    {
        return _spirit;
    }

    public void SetSpirit(Spirit spirit)
    {
        _spirit = new Spirit(spirit);
    }

    public int CheckDistance(GameObject target)
    {
        int totalDistance = 0;

        Tile parent = gameObject.GetComponentInParent<Tile>();
        Tile targetParent = target.gameObject.GetComponentInParent<Tile>();

        totalDistance += Mathf.Abs(parent.GetX() - targetParent.GetX());
        totalDistance += Mathf.Abs(parent.GetY() - targetParent.GetY());

        return (totalDistance);
    }

    /*public void DragPiece()
    {
        if (_isDragged == false)
        {
            _isDragged = true;
            _gameManager.ChangeDragging();
        }
        Vector3 mousePoint = Input.mousePosition;

        mousePoint.z = _staticZ;


        this.transform.position = Camera.main.ScreenToWorldPoint(mousePoint);
    }

    private void OnMouseDrag()
    {
        //DragPiece();
    }

    private void OnMouseDown()
    {
        bool attackerSelected = _gameManager.GetAttackerSelected();
        GameObject oS = _gameManager.GetOverriddenSpirit();


        if (attackerSelected == false &&  oS == null)
        {
            Tile parent = GetComponentInParent<Tile>();

            int x = parent.GetX();
            int y = parent.GetY();

            //sets attacker selected to true and disables all spirit collidere
            //this allows us to click on the tiles below them
            _gameManager.ChangeAttackerSelected();
            //sets the spirit that you plan to override
            _gameManager.SetOverriddenSpirit(gameObject);
            //highlights possible moves
            _gameManager.SetPossibleMoves(x, y);
        }
        else
        {
            Tile mousedTile = _gameManager.GetMousedTile();

            if (mousedTile.transform.childCount == 0)
            {
                _gameManager.SetSelectedTile(mousedTile, this.gameObject);
                transform.localPosition = Vector3.zero;
            }
            else
            {
                if (mousedTile.transform.GetChild(0).GetComponent<MoveableObject>().GetSpirit().GetTeam() != this.gameObject.GetComponent<MoveableObject>().GetSpirit().GetTeam())
                {
                    _gameManager.SetCombatants(this.gameObject, mousedTile.transform.GetChild(0).gameObject, CheckDistance(mousedTile));

                    transform.localPosition = Vector3.zero;
                }
                else
                {
                    transform.localPosition = Vector3.zero;
                }
            }
            _gameManager.ChangeAttackerSelected();
        }
    }

    private void OnMouseUp()
    {
        Tile mousedTile = _gameManager.GetMousedTile();

        if (mousedTile.transform.childCount == 0)
        {
            _gameManager.SetSelectedTile(mousedTile, this.gameObject);
            transform.localPosition = Vector3.zero;
        }
        else
        {
            if (mousedTile.transform.GetChild(0).GetComponent<MoveableObject>().GetSpirit().GetTeam() != this.gameObject.GetComponent<MoveableObject>().GetSpirit().GetTeam())
            {
                _gameManager.SetCombatants(this.gameObject, mousedTile.transform.GetChild(0).gameObject, CheckDistance(mousedTile));

                transform.localPosition = Vector3.zero;
            }
            else
            {
                transform.localPosition = Vector3.zero;
            }
        }
        _gameManager.ChangeDragging();
        _isDragged = false;
    }*/
}
