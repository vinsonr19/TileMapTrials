using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Attack
{
    private int _range;
    private int _damage;
    private string _attackMethod;
    private string _defenseMethod;
    private string _type;
    private string _name;
    private Spirit _owner;
    private GameManager _gameManager;

    public Attack()
    {
        _range = 0;
        _damage = 0;
        _attackMethod = "";
        _type = "";
    }

    public Attack(int range, int damage, string attackMethod, string type, string name)
    {
        _range = range;
        _damage = damage;
        _attackMethod = attackMethod;
        _type = type;
        _name = name;

        if(_attackMethod == "Attack")
        {
            _defenseMethod = "Defense";
        }
        else
        {
            _defenseMethod = "Special Defense";
        }
    }


    public void SetOwner(Spirit spirit)
    {
        _owner = spirit;
        _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    public float CheckSTAB()
    {
        if (_type == _owner.GetStatsString()["Type1"] || _type == _owner.GetStatsString()["Type2"])
        {
            return (1.5f);
        }
        else
        {
            return (1f);
        }
    }
    

    public void BasicAttack(Spirit target)
    {
        Debug.Log(_attackMethod + " - " + _defenseMethod);
        int damageDealt = Mathf.CeilToInt(CheckSTAB() * _damage * ((float)_owner.GetStatsInt()[_attackMethod] / target.GetStatsInt()[_defenseMethod]));

        Debug.Log("Damage: " + damageDealt + "\nAttack Method: " + _owner.GetStatsInt()[_attackMethod] + "\nDefense Method: " + target.GetStatsInt()[_defenseMethod]);

        target.TakeDamage(damageDealt);
        

        /*if(otherTeam == 1)
        {
            attackTarget = _owner.GetGameManager().GetTeam2()[_owner.GetAttackTarget()].GetComponent<MoveableObject>().GetSpirit();
        }
        else
        {
            attackTarget = _owner.GetGameManager().GetTeam1()[_owner.GetAttackTarget()].GetComponent<MoveableObject>().GetSpirit();
        }

        int damageDealt;

        if(_attackMethod == "Attack")
        {
            damageDealt = (int)(CheckSTAB() * _damage * (_owner.GetAttackStat() / attackTarget.GetDefense()));
        }
        else
        {
            damageDealt = (int)(CheckSTAB() * _damage * (_owner.GetSpecialAttack() / attackTarget.GetSpecialDefense()));
        }

        attackTarget.TakeDamage(damageDealt);


        delete when above is uncommented
        target.TakeDamage(_damage);*/
    }

    public int GetRange()
    {
        return (_range);
    }

    public int GetDamage()
    {
        return (_damage);
    }

    public string GetAttackType()
    {
        return (_type);
    }

    public string GetName()
    {
        return _name;
    }
    
}
