using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AttackButtonID : MonoBehaviour
{
    public int ID;
    private GameManager _gameManager;
    private Button _owner;

    private void Start()
    {
        _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        _owner = this.gameObject.GetComponent<Button>();

        _owner.onClick.AddListener(SetAttackChoice);
    }

    public void SetAttackChoice()
    {
        _gameManager.GetOverriddenSpirit().SetTentativeAttackChoice(ID);
        _gameManager.HideAttackChoices();
    }

    public void SetAttackName(Spirit attacker)
    {
        gameObject.GetComponentInChildren<Text>().text = attacker.GetLearnedAttacks()[ID].GetName();
    }
}
