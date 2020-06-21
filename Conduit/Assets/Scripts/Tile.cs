using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    private int _x;
    private int _y;
    private GameManager _gameManager;
    public Material red;
    public Material transparent;
    public Material transparentBlue;

    private Material _currentDefault;

    private void Start()
    {
        _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        _currentDefault = transparent;
    }

    public void SetCoords(int x, int y)
    {
        _x = x;
        _y = y;
    }

    public int GetX()
    {
        return _x;
    }

    public int GetY()
    {
        return _y;
    }

    /*    ##########################################
     *    ##########################################
     *    ##########################################
     *    
     *    Everything above this is necessary for functional, automated
     *    combat.  Everything below is to add on to the functionality.
     *    
     *    ##########################################
     *    ##########################################
     *    ########################################## */

    private Spirit _spirit;

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

        Tile targetParent = target.gameObject.GetComponentInParent<Tile>();

        totalDistance += Mathf.Abs(this.GetX() - targetParent.GetX());
        totalDistance += Mathf.Abs(this.GetY() - targetParent.GetY());

        return (totalDistance);
    }

    private void OnMouseDown()
    {
        if (!_gameManager.GetChoiceShown())
        {
            if (transform.childCount != 0 && _gameManager.GetIsOverriding() == false)
            {
                _gameManager.SetOverriddenSpirit(GetComponentInChildren<MoveableObject>().GetSpirit());
            }
            else if (transform.childCount == 0 && _gameManager.GetIsOverriding() == true)
            {
                _gameManager.DisplayMoveConfirm(_x, _y);
                //_gameManager.GetOverriddenSpirit().SetTentativeMove(_x, _y);
            }
        }
    }

    private void OnMouseUp()
    {
        if (!_gameManager.GetChoiceShown())
        {
            if (transform.childCount != 0 && _gameManager.GetIsOverriding() == true)
            {
                Spirit defender = GetComponentInChildren<MoveableObject>().GetSpirit();
                Spirit attacker = _gameManager.GetOverriddenSpirit();

                if (attacker.GetTeam() != defender.GetTeam())
                {
                    _gameManager.DisplayAttackChoices(defender);
                }
            }
        }
    }

    private void OnMouseEnter()
    {
        gameObject.GetComponent<MeshRenderer>().material = red;
        
    }

    private void OnMouseExit()
    {
        gameObject.GetComponent<MeshRenderer>().material = _currentDefault;
    }

    /*private void OnMouseDown()
    {
        //if an attack or move spirit has been selected
        if (_gameManager.GetAttackerSelected() == true)
        {
            //if it has no children, then there isn't a spirit on it
            if (gameObject.transform.childCount == 0)
            {
                //so we move there
                //set the selected tile to this tile
                _gameManager.SetSelectedTile(this);
            }
            //if it does have children, then there is a spirit on that tile
            else
            {
                
                GameObject child = gameObject.transform.GetChild(0).gameObject;
                Spirit childSpirit = child.GetComponent<MoveableObject>().GetSpirit();
                GameObject attacker = _gameManager.GetOverriddenSpirit();
                Spirit attackerSpirit = attacker.GetComponent<MoveableObject>().GetSpirit();
                //check to make sure that this child is on the other team than the attacker
                if (childSpirit.GetTeam() != attackerSpirit.GetTeam())
                {
                    _gameManager.SetCombatants(attacker, child, CheckDistance(attacker));
                }
            }

            _gameManager.ChangeAttackerSelected();
        }
        
    }

    public void SetMoveable()
    {
        this.gameObject.GetComponent<MeshRenderer>().material = transparentBlue;
        _currentDefault = transparentBlue;
    }

    public void UnsetMoveable()
    {
        this.gameObject.GetComponent<MeshRenderer>().material = transparent;
        _currentDefault = transparent;
    }

    public int CheckDistance(GameObject target)
    {
        int totalDistance = 0;

        Tile attacker = target.GetComponentInParent<Tile>();

        totalDistance += Mathf.Abs(_y - attacker.GetY());

        return (totalDistance);
    }*/
}
