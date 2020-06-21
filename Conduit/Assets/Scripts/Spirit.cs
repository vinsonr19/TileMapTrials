using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Spirit
{
    //vital stats
    //this should contain all interger stats
    //hp, def, spdef, atk, spatk, speed
    private Dictionary<string, int> _statsInt;
    //this should contain all string stats
    //name, type1, type2, evolvesfrom, evolvesto
    private Dictionary<string, string> _statsString;
    private List<Attack> _learnableAttacks;
    private int _totalStats;


    //battle stats
    private int _spiritNumber;
    private Text _uiTextHealth;
    private Text _uiTextCombat;
    private GameManager _gameManager;
    private int _attackChoice;
    private int _attackTarget;
    private int _moveX;
    private int _moveY;
    private int _currentHP;
    private int _teamNumber;
    private bool _isDead;
    private List<Attack> _learnedAttacks;
    private GameObject _gamePiece;


    public Spirit()
    {

    }

    public Spirit(Spirit spirit)
    {
        _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        _statsString = spirit.GetStatsString();
        _statsInt = spirit.GetStatsInt();
        _learnableAttacks = spirit.GetLearnableAttacks();
    }

    void Update()
    {
       
    }

    public void SetStatsInt(Dictionary<string, int> stats)
    {
        _statsInt = stats;
    }

    public void SetStatsString(Dictionary<string, string> stats)
    {
        _statsString = stats;
    }

    public void SetTotalStats(int st)
    {
        _totalStats = st;
    }

    public Dictionary<string, string> GetStatsString()
    {
        return _statsString;
    }

    public List<Attack> GetLearnableAttacks()
    {
        return (_learnableAttacks);
    }

    public void SetLearnableAttacks(List<Attack> attacks)
    {
        _learnableAttacks = attacks;
    }

    public Dictionary<string, int> GetStatsInt()
    {
        return _statsInt;
    }

    public void SetLearnedAttacks(List<Attack> attacks)
    {
        if (_learnedAttacks == null)
        {
            _learnedAttacks = new List<Attack>();
        }

        _learnedAttacks.Clear();
        _learnedAttacks = attacks;

        //This block is new and experimental
        //##################################
        foreach(Attack attack in _learnableAttacks)
        {
            attack.SetOwner(this);
        }
        //##################################
        //end experimental block
    }

    public void SetAttackTarget(int target)
    {
        _attackTarget = target;
    }

    public void SetAttackChoice(int choice)
    {
        _attackChoice = choice;
    }

    public void SetMoveX(int x)
    {
        _moveX = x;
    }

    public void SetMoveY(int y)
    {
        _moveY = y;
    }

    public int GetMoveX()
    {
        return _moveX;
    }

    public int GetMoveY()
    {
        return _moveY;
    }

    public void UpdateCombat()
    {
        if (_attackChoice != -1)
        {
            int oT = ((_teamNumber - 1) * -1) + 2;

            GameObject[] otherTeam = _gameManager.GetTeams()[oT];

            Spirit attackTarget = otherTeam[_attackTarget].GetComponent<MoveableObject>().GetSpirit();

            _attackString = _statsString["Name"] + " will attack " + attackTarget.GetStatsString()["Name"] + " with " + _learnedAttacks[_attackChoice].GetName();
            _moveString = "move to " + _moveY + " - " + _moveX;

            _uiTextCombat.text = _attackString + " and then " + _moveString;
        }
        else
        {
            _uiTextCombat.text = _statsString["Name"] + " has no targets";
        }
    }

    private void AttachText()
    {
        _uiTextHealth = GameObject.Find("Player" + _teamNumber.ToString() + "Spirit" + _spiritNumber.ToString() + "Health").GetComponent<Text>();
        _uiTextHealth.fontSize = 10;
        _uiTextCombat = GameObject.Find("Player" + _teamNumber.ToString() + "Spirit" + _spiritNumber.ToString() + "Combat").GetComponent<Text>();
        _uiTextCombat.fontSize = 10;

        _uiTextHealth.text = _statsString["Name"] + "    HP : " + _currentHP + "/" + _statsInt["HP"];
    }

    public void SetTeam(int t, int n)
    {
        _teamNumber = t;
        _spiritNumber = n;

        _currentHP = _statsInt["HP"];

        AttachText();
    }

    public void TakeDamage(int damage)
    {
        _currentHP -= damage;
        CheckDeath();
        UpdateHealth();
    }

    private void UpdateHealth()
    {
        _uiTextHealth.text = _statsString["Name"] + "    HP : " + _currentHP + "/" + _statsInt["HP"];
    }

    public int GetAttackChoice()
    {
        return _attackChoice;
    }

    public int GetAttackTarget()
    {
        return _attackTarget;
    }

    private void CheckDeath()
    {
        if(_currentHP <= 0)
        {
            _isDead = true;
            _uiTextHealth.transform.parent.gameObject.SetActive(false);
            _uiTextCombat.transform.parent.gameObject.SetActive(false);
        }
    }

    public bool IsDead()
    {
        return _isDead;
    }

    public List<Attack> GetLearnedAttacks()
    {
        return (_learnedAttacks);
    }

    public int GetSpiritNumber()
    {
        return _spiritNumber;
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

    private int _tentativeAttackChoice = -1;
    private int _tentativeAttackTarget = -1;
    private int _tentativeMoveX = -1;
    private int _tentativeMoveY = -1;
    private string _attackString;
    private string _moveString;

    public void SetTentativeAttackTarget(Spirit defender)
    {
        _tentativeAttackTarget = defender.GetSpiritNumber() - 1;
    }

    public void ClearTentativeAttackTarget()
    {
        _tentativeAttackTarget = -1;
    }

    public void SetTentativeAttackChoice(int choice)
    {
        _tentativeAttackChoice = choice;

        UpdateCombatAttackTentative();
    }

    private void UpdateCombatAttackTentative()
    {
        int oT = ((_teamNumber - 1) * -1) + 2;

        GameObject[] otherTeam = _gameManager.GetTeams()[oT];

        Spirit attackTarget = otherTeam[_tentativeAttackTarget].GetComponent<MoveableObject>().GetSpirit();

        _attackString = _statsString["Name"] + " will attack " + attackTarget.GetStatsString()["Name"] + " with " + _learnedAttacks[_tentativeAttackChoice].GetName();

        _uiTextCombat.text = _attackString + " and then " + _moveString;
    }

    public void SetTentativeMove(int x, int y, GameObject[][] board)
    {
        _tentativeMoveX = x;
        _tentativeMoveY = y;

        _gameManager.GetTeams()[_teamNumber][_spiritNumber - 1].transform.GetChild(0).transform.position = board[y][x].transform.position;

        UpdateCombatMoveTentative();
    }

    private void UpdateCombatMoveTentative()
    {
        _moveString = "move to " + _tentativeMoveX + " - " + _tentativeMoveY;

        _uiTextCombat.text = _attackString + " and then " + _moveString;
    }

    public void ConfirmTentative()
    {
        if(_tentativeAttackChoice != -1)
        {
            _attackChoice = _tentativeAttackChoice;
            _tentativeAttackChoice = -1;
        }

        if(_tentativeAttackTarget != -1)
        {
            _attackTarget = _tentativeAttackTarget;
            _tentativeAttackTarget = -1;
        }

        if(_tentativeMoveX != -1)
        {
            _moveX = _tentativeMoveX;
            _tentativeMoveX = -1;
        }

        if(_tentativeMoveY != -1)
        {
            _moveY = _tentativeMoveY;
            _tentativeMoveY = -1;
        }
    }

    public void CancelTentative(GameObject[][] board)
    {
        _tentativeAttackChoice = -1;
        _tentativeAttackTarget = -1;

        _tentativeMoveX = -1;
        _tentativeMoveY = -1;

        _gameManager.GetTeams()[_teamNumber][_spiritNumber - 1].transform.GetChild(0).transform.position = board[_moveY][_moveX].transform.position;

        UpdateCombat();
    }


    public int GetTeam()
    {
        return _teamNumber;
    }

    public int GetAttackRange(int ID)
    {
        return (_learnedAttacks[ID].GetRange());
    }

    /*public Attack GetAttack()
    {
        return (_learnedAttacks[_attackChoice]);
    }

    public int GetTotalStats()
    {
        return _totalStats;
    }

    public GameObject GetGamePiece()
    {
        return _gamePiece;
    }

    public GameManager GetGameManager()
    {
        return _gameManager;
    }
    
    public void SetSpiritInfo(Spirit spirit)
    {
        _statsInt = spirit.GetStatsInt();
        _statsString = spirit.GetStatsString();

        _currentHP = _statsInt["HP"];
        _totalStats = spirit.GetTotalStats();
    }*/

}
