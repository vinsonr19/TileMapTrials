using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public GameObject tile;
    public GameObject spiritPrefab;
    public GameObject board;
    public Text stateText;
    public Material[] spiritColors;
    public Material[] spiritColorsTransparent;

    private int _boardSize = 10;
    private float _tileGap = .1f;
    private GameObject thisSpirit;
    //important to note that the way this is made right now
    //the first argument is y and the second is x
    //so _boardTiles[y][x] to get a tile
    private GameObject[][] _boardTiles;
    private int _numSpirits = 3;

    private GameObject[] team1;
    private GameObject[] team2;
    private Spirit[] _chosenSpirits;
    private Dictionary<int, GameObject[]> _teams;
    private List<Spirit> _selectedSpirits;

    private GameObject _selectedUnit;
    private GameObject _attacker;
    private GameObject _defender;
    private GameObject _mover;
    private bool _attacking;
    private string _firstChoice;
    private int _targetSpirit;
    private int _targetMoveX;
    private int _targetMoveY;
    private Tile _selectedTile;
    private Tile _mousedTile;
    private bool _moveSet = false;
    private bool _isDragging = false;
    private bool _isGameOver = false;
    private GameObject _confirmMove;
    private GameObject _attackChoice;
    private bool _attackerSelected = false;
    private Spirit _overriddenSpirit;
    private bool _isOverriding = false;

    private Dictionary<string, Spirit> _spiritDict;
    private Dictionary<string, Attack> _attackDict;

    //The main combat loop involves using the function
    //SetCombat, to assign combat actions to each spirit
    //Giving the plaayer a chance to override one of thoe
    //spirit's actions, and then executing those actions
    //with SpiritMove and SpiritAttack



    void Start()
    {
        //used to hold all randomly generated spirits
        _spiritDict = new Dictionary<string, Spirit>();
        //used to hold all randomly generated attacks
        _attackDict = new Dictionary<string, Attack>();

        //populates the attackDict
        CreateAttackDatabase();
        //populates the spiritDict
        CreateRandomSpiritDict();
        //chooses 6 spirits at random
        CreateRandomTeams();
        //creates the board
        BoardSetup();
        //spawn spirits objects on the board
        SpiritSpawn();
        ////assigns spirits to objects
        AssignSpirits();

        //finds the move confirm button (could make this a dependency)
        _confirmMove = GameObject.Find("ConfirmMove");
        //hides it
        _confirmMove.SetActive(false);
        //finds the attack choice button (could make this a dependency)
        _attackChoice = GameObject.Find("AttackChoice");
        //hides it
        _attackChoice.SetActive(false);


        //Sets the initial combat options
        SetCombat();

        //potentially unnecessary text
        stateText.text = "What spirit will you compel?";
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Q))
        {
            //finds the scale of the objects so they fit nicely on the tiles
            float x_scale = board.GetComponent<MeshRenderer>().bounds.size.x / _boardSize - _tileGap;
            float y_scale = board.GetComponent<MeshRenderer>().bounds.size.y / _boardSize - _tileGap;

            GameObject tempSpirit = Instantiate(spiritPrefab, Vector3.zero, Quaternion.identity);

            tempSpirit.transform.localScale = new Vector3(x_scale, y_scale, x_scale);
            tempSpirit.transform.SetParent(_teams[1][0].transform);
            tempSpirit.transform.position = _boardTiles[0][1].transform.position;

        }
    }

    private void CreateAttackDatabase()
    {
        //Creates a generic database of attacks

        //Attack types
        string[] types = new string[6] { "Fire", "Water", "Wind", "Earth", "Dark", "Light" };
        //Various nouns for attacks
        string[] nouns = new string[14] { "Wheel", "Pulse", "Bullet", "Lance", "Axe", "Sword", "Charge", "Stab", "Crush", "Claw", "Beam", "Gun", "Bomb", "Tornado" };


        //Create an attack combining each type and noun from above
        foreach (string t in types)
        {
            foreach (string n in nouns)
            {
                int range;
                int damage;
                string attackMethod;

                string name = t + n;

                float globalChance = Random.Range(0f, 1f);

                //10% chance of being a global attack
                if (globalChance < .1)
                {
                    //11 attack is a place holder for global
                    //11 is chosen because it works nicely for the damage calculation
                    //this will likely be 100 or something of that variety in production
                    range = 11;
                }
                else
                {
                    //90% chance to choose a range between 1 and 10
                    range = Random.Range(1, 11);
                }

                //Determines if the attack uses Attack or Special Attack
                int method = Random.Range(0, 2);

                if (method == 0)
                {
                    attackMethod = "Attack";
                }
                else
                {
                    attackMethod = "Special Attack";
                }

                //Damage calculation
                //Max damage is 60, minimum is 20
                damage = -4 * range + 64;

                _attackDict[name] = new Attack(range, damage, attackMethod, t, name);

            }
        }
    }

    private Spirit DistributeStats(float numStats, string previousEvolution)
    {
        //Randomly distributes stats fopr a given spirit

        Spirit newSpirit = new Spirit();
        int nS = Mathf.FloorToInt(numStats);
        List<string> types = new List<string>() { "Fire", "Water", "Wind", "Earth", "Dark", "Light" };

        //this is a dictionary of all stats
        Dictionary<string, int> statsInt = new Dictionary<string, int>() { { "HP", 0 }, { "Defense", 0 }, { "Special Defense", 0 }, { "Attack", 0 }, { "Special Attack", 0 }, { "Speed", 0 } };
        Dictionary<string, string> statsString = new Dictionary<string, string>();
        //I think this is the easiest way to create a list of all keys in the dictionary
        List<string> statNames = new List<string>(statsInt.Keys);

        //Distribute the number of stats equally to each stat
        foreach (string stat in statNames)
        {
            statsInt[stat] = nS / 6;
        }

        //Rearrange some stats, for more unique spirits
        foreach (string stat in statNames)
        {
            //take away up to 20% of the original stat total
            int dist = Random.Range(0, Mathf.FloorToInt(nS / 6 * .2f));
            statsInt[stat] -= dist;

            string newStat = statNames[Random.Range(0, statNames.Count)];

            while (newStat == stat)
            {
                newStat = statNames[Random.Range(0, statNames.Count)];
            }


            //Give that 20% to another stat
            statsInt[newStat] += dist;
        }


        //this definitely needs to get cleaned up
        //There should just be a SetStats function in spirit
        newSpirit.SetStatsInt(statsInt);


        //Check to see if it is the base spirit in an evolution chain
        if (previousEvolution == "Base Form")
        {
            //50% chance of having two types
            //I think I will change this on production, as I am leaning
            //toward having only single type spirits, that you can add
            //a second type to
            float numTypes = Random.Range(0f, 1f);

            if (numTypes < .5f)
            {
                statsString["Type1"] = types[Random.Range(0, types.Count)];
                statsString["Type2"] = "None";
            }
            else
            {
                string t1 = types[Random.Range(0, types.Count)];
                string t2 = types[Random.Range(0, types.Count)];

                while (t1 == t2)
                {
                    t2 = types[Random.Range(0, types.Count)];
                }

                statsString["Type1"] = t1;
                statsString["Type2"] = t2;
            }
        }
        //Otherwise, it inherits the previous typing
        //this should also change in the future if I decide
        //that I will still add types
        else
        {
            statsString["Type1"] = _spiritDict[previousEvolution].GetStatsString()["Type1"];
            statsString["Type2"] = _spiritDict[previousEvolution].GetStatsString()["Type2"];
        }

        statsString["EvolvesFrom"] = previousEvolution;

        newSpirit.SetStatsString(statsString);

        newSpirit.SetTotalStats(nS);



        return (newSpirit);
    }

    private void DistributeAttacks(Spirit spirit)
    {
        //List of attacks
        List<Attack> attackList = new List<Attack>();
        //Array of attack names
        string[] attackNames = new string[_attackDict.Keys.Count];

        Dictionary<string, string> statsString = spirit.GetStatsString();

        if (statsString["EvolvesFrom"] == "Base Form")
        {
            //Grab all attack names
            _attackDict.Keys.CopyTo(attackNames, 0);


            foreach (string attack in attackNames)
            {
                float acquire = Random.Range(0f, 1f);

                if (_attackDict[attack].GetAttackType() == statsString["Type1"] || _attackDict[attack].GetAttackType() == statsString["Type2"])
                {
                    //75% chance to get an on type attack
                    if (acquire <= .75f)
                    {
                        attackList.Add(_attackDict[attack]);
                    }
                }
                else
                {
                    //15% chance to get any off type attack
                    if (acquire < .15f)
                    {
                        attackList.Add(_attackDict[attack]);
                    }
                }
            }
        }
        //if it isn't the first evolution
        else
        {
            Spirit previousEvolution = _spiritDict[statsString["EvolvesFrom"]];

            foreach (Attack attack in previousEvolution.GetLearnableAttacks())
            {
                attackList.Add(attack);
            }

            _attackDict.Keys.CopyTo(attackNames, 0);

            foreach (string attack in attackNames)
            {
                float acquire = Random.Range(0f, 1f);

                //15% chance to get an on type attack
                if (_attackDict[attack].GetAttackType() == statsString["Type1"] || _attackDict[attack].GetAttackType() == statsString["Type2"])
                {
                    if (acquire <= .15f)
                    {
                        attackList.Add(_attackDict[attack]);
                    }
                }
                //5% chance to get an off type attack
                else
                {
                    if (acquire < .05f)
                    {
                        attackList.Add(_attackDict[attack]);
                    }
                }
            }
        }

        spirit.SetLearnableAttacks(attackList);
    }

    private void CreateRandomSpiritDict()
    {
        List<string> consonants = new List<string>() { "b", "c", "d", "f", "g", "h", "j", "k", "l", "m", "n", "p", "q", "r", "s", "t", "v", "w", "x", "y", "z" };
        List<string> vowels = new List<string>() { "a", "e", "i", "o", "u" };

        int totalSpirits = 200;

        int numAdded = 0;

        while (numAdded < totalSpirits)
        {
            //at most 3 spirits in an evo chain
            int maxChain = 3;

            //total stats jump by this amount each evolution
            //try to keep this divisible by 6
            int standardStatJump = 60;

            //total stats for final form of solo spirit (used as reference)
            int baseStats = 600;

            //this is used to calculate the stat jump based on how many evolutions there are
            //the idea is a single chain spirit has 600 stats
            //a 2 chain spirit has 588 stats at base, and 612 at evolution 1
            //a 3 chain spirit has 574 at base, 600 at second, and 624 at third
            List<float> baseStatAdjustment = new List<float>() { 1, 1.5f, 2 };

            //baseStats - baseStatAdjustment * standardStatJump + standardStatJump * evoChain
            //baseStats + standardStatJump(chainLink - baseStatAdjustment[evoChain - 1]

            //how many evos will be in this chain
            int evoChain = Random.Range(1, maxChain + 1);

            //make sure that isn't too many spirits
            if (evoChain > totalSpirits - numAdded)
            {
                evoChain = totalSpirits - numAdded;
            }

            //have a list of the names in this evo for reference
            List<string> evoNames = new List<string>();


            for (int chainLink = 1; chainLink <= evoChain; chainLink++)
            {
                //create a random name
                string name = consonants[Random.Range(0, consonants.Count)] + vowels[Random.Range(0, vowels.Count)] + consonants[Random.Range(0, consonants.Count)] + consonants[Random.Range(0, consonants.Count)];

                if (chainLink == 1)
                {
                    _spiritDict[name] = DistributeStats(baseStats + standardStatJump * (chainLink - baseStatAdjustment[evoChain - 1]), "Base Form");
                }
                else
                {
                    _spiritDict[name] = DistributeStats(baseStats + standardStatJump * (chainLink - baseStatAdjustment[evoChain - 1]), evoNames[chainLink - 2]);
                    _spiritDict[_spiritDict[name].GetStatsString()["EvolvesFrom"]].GetStatsString()["EvolvesTo"] = name;
                }

                //asign attacks
                DistributeAttacks(_spiritDict[name]);

                //check to see if it is the last link
                if (chainLink == evoChain)
                {
                    _spiritDict[name].GetStatsString()["EvolvesTo"] = "Final Form";
                }

                evoNames.Add(name);
                _spiritDict[name].GetStatsString()["Name"] = name;
            }

            numAdded += evoChain;
        }
    }

    private void CreateRandomTeams()
    {
        //get all spirit names
        List<string> spiritNames = new List<string>(_spiritDict.Keys);

        //make an array of 6 spirits
        _chosenSpirits = new Spirit[_numSpirits * 2];


        for (int i = 0; i < _numSpirits * 2; i++)
        {
            //choose them randomly
            _chosenSpirits[i] = _spiritDict[spiritNames[Random.Range(0, spiritNames.Count)]];
        }

    }

    private void BoardSetup()
    {
        //initialize the board
        _boardTiles = new GameObject[_boardSize][];

        //set the scale of the tiles so they fit like a grid, but with some gap
        float x_scale = board.GetComponent<MeshRenderer>().bounds.size.x / _boardSize - _tileGap;
        float y_scale = board.GetComponent<MeshRenderer>().bounds.size.y / _boardSize - _tileGap;

        //find the bottom left corner of the grid
        float x_min = -(board.GetComponent<MeshRenderer>().bounds.size.x / 2 - x_scale / 2);
        float y_min = -(board.GetComponent<MeshRenderer>().bounds.size.y / 2 - y_scale / 2);


        for (int i = 0; i < _boardSize; i++)
        {
            _boardTiles[i] = new GameObject[_boardSize];

            //instantiate the tiles
            for (int j = 0; j < _boardSize; j++)
            {
                GameObject tempTile = Instantiate(tile, new Vector3(x_min + _tileGap / 2 + (x_scale + _tileGap) * j, y_min + _tileGap / 2 + (y_scale + _tileGap) * i, -.5f), Quaternion.identity);
                tempTile.transform.localScale = new Vector3(x_scale, y_scale, .01f);
                tempTile.transform.SetParent(board.transform);
                tempTile.GetComponent<Tile>().SetCoords(j, i);
                _boardTiles[i][j] = tempTile;
            }
        }

    }

    private void SpiritSpawn()
    {
        //column where team1 spirits stop being able to spawn
        int team1_stop;
        //column where team2 spirits start being able to spawn
        int team2_start;

        //finds the scale of the objects so they fit nicely on the tiles
        float x_scale = board.GetComponent<MeshRenderer>().bounds.size.x / _boardSize - _tileGap;
        float y_scale = board.GetComponent<MeshRenderer>().bounds.size.y / _boardSize - _tileGap;

        //if there an even number of tiles
        //then the last column that team1 can spawn on
        //is numColumns / 2 - 1, because indexing starts 
        //at 0.  e.g. 10 tiles long, 10/2 - 1 = 4, 0,1,2,3,4 for team1 spawn
        if (_boardTiles.Length / 2f == Mathf.FloorToInt(_boardTiles.Length / 2))
        {
            team1_stop = _boardTiles.Length / 2 - 1;
            team2_start = team1_stop + 1;
        }
        //if there are an odd number of tiles, the center row
        //is not able to default spawns
        //e.g. 11 - 3 = 8 / 2 = 4, 0,1,2,3,4 for team1
        //6,7,8,9,10 for team2, no one spawns on 5
        else
        {
            team1_stop = (_boardTiles.Length - 3) / 2;
            team2_start = team1_stop + 2;
        }

        //for ease in the for loops
        int[] starts = new int[] { 0, team2_start };
        int[] stops = new int[] { team1_stop + 1, _boardTiles.Length };

        _teams = new Dictionary<int, GameObject[]>();

        for (int i = 0; i < 2; i++)
        {
            _teams[i + 1] = new GameObject[_numSpirits];


            for (int j = 0; j < _numSpirits; j++)
            {
                //find a random, valid, columns and range
                int column = Random.Range(starts[i], stops[i]);
                int row = Random.Range(0, _boardTiles.Length);

                //make sure no other spirit is already there
                while (_boardTiles[row][column].transform.childCount != 0)
                {
                    column = Random.Range(starts[i], stops[i]);
                    row = Random.Range(0, _boardTiles.Length);
                }

                //insantiate the spirit
                GameObject tempSpirit = Instantiate(spiritPrefab, _boardTiles[row][column].transform.position, Quaternion.identity);
                _teams[i + 1][j] = tempSpirit;
                tempSpirit.transform.localScale = new Vector3(x_scale, y_scale, x_scale);
                tempSpirit.transform.SetParent(_boardTiles[row][column].transform);
                tempSpirit.GetComponent<MeshRenderer>().material = spiritColors[i * _numSpirits + j];

                GameObject tempSpiritEcho = tempSpirit.transform.GetChild(0).gameObject;
                tempSpiritEcho.GetComponent<MeshRenderer>().material = spiritColorsTransparent[i * _numSpirits + j];
            }
        }

    }

    private void AssignSpirits()
    {
        for (int team = 1; team < 3; team++)
        {
            for (int i = 0; i < _numSpirits; i++)
            {
                _teams[team][i].GetComponent<MoveableObject>().SetSpirit(_chosenSpirits[(_numSpirits * (team - 1) + i)]);
                Spirit spirit = _teams[team][i].GetComponent<MoveableObject>().GetSpirit();
                spirit.SetTeam(team, i + 1);
                AssignSpiritAttacks(spirit);
            }
        }
    }

    private void AssignSpiritAttacks(Spirit spirit)
    {
        List<int> attackIDs = new List<int>();
        List<Attack> learnableAttacks = _spiritDict[spirit.GetStatsString()["Name"]].GetLearnableAttacks();

        while (attackIDs.Count < 4)
        {
            int attackID = Random.Range(0, learnableAttacks.Count);

            if (!attackIDs.Contains(attackID))
            {
                attackIDs.Add(attackID);
            }
        }

        List<Attack> learnedAttacks = new List<Attack>();

        foreach (int ID in attackIDs)
        {
            learnedAttacks.Add(learnableAttacks[ID]);
        }

        spirit.SetLearnedAttacks(learnedAttacks);
    }

    public List<Attack> ChooseAttacks(Spirit spirit)
    {
        List<Attack> attackList = new List<Attack>();

        List<Attack> learnableAttacks = spirit.GetLearnableAttacks();

        int numAdded = 0;

        while (numAdded < 4)
        {
            Attack newAttack = learnableAttacks[Random.Range(0, learnableAttacks.Count)];

            if (!attackList.Contains(newAttack))
            {
                newAttack.SetOwner(spirit);
                attackList.Add(newAttack);
                numAdded++;
            }
        }

        return (attackList);


    }

    private int CheckMaxDistance(Spirit spirit)
    {
        int maxDistance = -1;

        foreach (Attack attack in spirit.GetLearnedAttacks())
        {
            if (attack.GetRange() > maxDistance)
            {
                maxDistance = attack.GetRange();
            }
        }

        return (maxDistance);
    }

    private int ChooseMaxAttack(Spirit spirit, int distanceToTarget)
    {
        int currentAttack = -1;
        int currentPower = 0;

        List<Attack> attacks = spirit.GetLearnedAttacks();

        for (int i = 0; i < attacks.Count; i++)
        {
            if (attacks[i].GetRange() >= distanceToTarget || attacks[i].GetRange() == 11)
            {
                if (attacks[i].GetDamage() > currentPower)
                {
                    currentPower = attacks[i].GetDamage();
                    currentAttack = i;
                }
            }
        }

        return (currentAttack);
    }

    private List<int> Attacking(GameObject spirit, GameObject[] opposingTeam)
    {

        MoveableObject attackingSpiritObject = spirit.GetComponent<MoveableObject>();
        Spirit attackingSpirit = attackingSpiritObject.GetSpirit();

        int furthestTargettableDistance = CheckMaxDistance(attackingSpirit);

        List<Spirit> targettableSpirits = new List<Spirit>();

        foreach (GameObject sp in opposingTeam)
        {
            Spirit tempSp = sp.GetComponent<MoveableObject>().GetSpirit();

            if (tempSp.IsDead() == false)
            {
                int thisDistance = attackingSpiritObject.CheckDistance(sp);
                if (thisDistance <= furthestTargettableDistance || furthestTargettableDistance == 11)
                {
                    targettableSpirits.Add(tempSp);
                }
            }
        }

        if (targettableSpirits.Count == 0)
        {
            return (new List<int>() { -1, -1 });
        }
        else
        {
            int targetSpirit = Random.Range(0, targettableSpirits.Count);
            targetSpirit = targettableSpirits[targetSpirit].GetSpiritNumber() - 1;
            int thisDistance = attackingSpiritObject.CheckDistance(opposingTeam[targetSpirit]);
            int attackChoice = ChooseMaxAttack(attackingSpirit, thisDistance);

            return (new List<int>() { targetSpirit, attackChoice });
        }
    }

    //This function used to return List<int>
    private int[] Moving(GameObject spirit)
    {
        if (!spirit.GetComponent<MoveableObject>().GetSpirit().IsDead())
        {
            /* This block of code was working.  The
             * functionality has been updated below.
            int x = Random.Range(0, _boardSize);
            int y = Random.Range(0, _boardSize);

            return (new List<int> { x, y });
            */

            //This block is new and experimental
            //##################################
            List<int[]> possibleMoves = CalculatePossibleMoves(spirit, 1);

            int moveChoice = Random.Range(0, possibleMoves.Count);

            return (possibleMoves[moveChoice]);
            //##################################
            //end experimental block

            
        }
        else
        {
            return (new int[2] { 0, 0 });
        }
    }

    private void CombatAssign(int otherTeam, GameObject spirit)
    {
        Spirit attackingSpirit = spirit.GetComponent<MoveableObject>().GetSpirit();

        if (!attackingSpirit.IsDead())
        {

            List<int> attackOptions = Attacking(spirit, _teams[otherTeam]);
            int[] movementCoords = Moving(spirit);

            attackingSpirit.SetAttackTarget(attackOptions[0]);
            attackingSpirit.SetAttackChoice(attackOptions[1]);

            attackingSpirit.SetMoveX(movementCoords[0]);
            attackingSpirit.SetMoveY(movementCoords[1]);

            if (attackingSpirit.GetTeam() == 1)
            {
                spirit.transform.GetChild(0).transform.position = _boardTiles[movementCoords[1]][movementCoords[0]].transform.position;
            }

            attackingSpirit.UpdateCombat();
        }
    }

    private void SpiritMove(GameObject spirit)
    {
        Spirit movingSpirit = spirit.GetComponent<MoveableObject>().GetSpirit();
        Tile location = spirit.GetComponentInParent<Tile>();

        int tarX = movingSpirit.GetMoveX();
        int tarY = movingSpirit.GetMoveY();

        int curX = location.GetX();
        int curY = location.GetY();

        if (_boardTiles[tarY][tarX].transform.childCount == 0)
        {
            spirit.transform.SetParent(_boardTiles[tarY][tarX].transform);
            spirit.transform.localPosition = Vector3.zero;
        }
    }

    private void SpiritAttack(int otherTeam, GameObject spirit)
    {
        Spirit attackingSpirit = spirit.GetComponent<MoveableObject>().GetSpirit();
        Debug.Log("AttackTarget: " + attackingSpirit.GetAttackTarget());

        if (attackingSpirit.GetAttackChoice() == -1 || attackingSpirit.GetAttackTarget() == -1)
        {
        }
        else
        {
            Spirit defendingSpirit = _teams[otherTeam][attackingSpirit.GetAttackTarget()].GetComponent<MoveableObject>().GetSpirit();

            if (!defendingSpirit.IsDead())
            {
                attackingSpirit.GetLearnedAttacks()[attackingSpirit.GetAttackChoice()].BasicAttack(defendingSpirit);
            }
        }
    }

    public void SetCombat()
    {
        foreach (int team in new int[] { 1, 2 })
        {
            foreach (GameObject spirit in _teams[team])
            {
                int otherTeam = ((team - 1) * -1) + 2;
                CombatAssign(otherTeam, spirit);
            }
        }
    }

    private bool CheckGameOver()
    {

        for (int team = 1; team < 3; team++)
        {
            int deadCount = 0;
            foreach (GameObject spirit in _teams[team])
            {
                if (spirit.GetComponent<MoveableObject>().GetSpirit().IsDead())
                {
                    spirit.transform.SetParent(null);
                    spirit.SetActive(false);
                    deadCount++;
                }
            }

            if (deadCount == _numSpirits)
            {
                stateText.text = "Game Over";
                _isGameOver = true;
                return true;
            }
        }

        return (false);


    }

    public void MakeCombat()
    {
        if (!CheckGameOver())
        {
            foreach (int team in new int[] { 1, 2 })
            {
                foreach (GameObject spirit in _teams[team])
                {
                    Spirit attackingSpirit = spirit.GetComponent<MoveableObject>().GetSpirit();

                    //This block is new and experimental
                    //##################################
                    attackingSpirit.ConfirmTentative();
                    //##################################
                    //end experimental block

                    if (!attackingSpirit.IsDead())
                    {
                        int otherTeam = ((team - 1) * -1) + 2;
                        SpiritMove(spirit);
                        SpiritAttack(otherTeam, spirit);
                    }
                }
            }
        }

        //This block is new and experimental
        //##################################
        _isOverriding = false;
        _overriddenSpirit = null;
        //##################################
        //end experimental block

        CheckGameOver();
        SetCombat();
    }

    public Dictionary<int, GameObject[]> GetTeams()
    {
        return _teams;
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

    private int _tentMoveX;
    private int _tentMoveY;
    private bool _choiceShown = false;

    public void SetOverriddenSpirit(Spirit spirit)
    {
        _overriddenSpirit = spirit;
        _isOverriding = true;
    }

    public Spirit GetOverriddenSpirit()
    {
        return _overriddenSpirit;
    }

    public bool GetIsOverriding()
    {
        return _isOverriding;
    }

    public void CancelOverriding()
    {
        if (_overriddenSpirit != null)
        {
            _overriddenSpirit.CancelTentative(_boardTiles);
            _overriddenSpirit = null;
            _isOverriding = false;
            _choiceShown = false;
        }
    }

    public void DisplayAttackChoices(Spirit defender)
    {
        int distance = CheckDistance(_overriddenSpirit, defender);

        int numPossibleAttacks = 0;

        for (int i = 0; i < 4; i++)
        {
            if (_overriddenSpirit.GetAttackRange(i) < distance)
            {
                _attackChoice.transform.GetChild(i).GetComponent<Button>().interactable = false;
            }
            else
            {
                numPossibleAttacks += 1;
                _attackChoice.transform.GetChild(i).GetComponent<Button>().interactable = true;
            }
        }

        if (numPossibleAttacks == 0)
        {
            
        }
        else
        {
            _choiceShown = true;
            _attackChoice.transform.position = Camera.main.WorldToScreenPoint(_teams[defender.GetTeam()][defender.GetSpiritNumber() - 1].transform.position);

            Tile attackParent = _teams[defender.GetTeam()][defender.GetSpiritNumber() - 1].GetComponentInParent<Tile>();

            int x = attackParent.GetX();
            int y = attackParent.GetY();

            if (x == 0)
            {
                x = 1;
            }
            if(x == _boardSize - 1)
            {
                x = _boardSize - 2;
            }
            if(y == 0)
            {
                y = 1;
            }
            if(y == _boardSize - 1)
            {
                y = _boardSize - 2;
            }

            _attackChoice.transform.position = Camera.main.WorldToScreenPoint(_boardTiles[y][x].transform.position);




            _attackChoice.SetActive(true);
            _overriddenSpirit.SetTentativeAttackTarget(defender);
        }

        for (int i = 0; i < _attackChoice.transform.childCount; i++)
        {
            AttackButtonID button = _attackChoice.transform.GetChild(i).GetComponent<AttackButtonID>();

            if (button != null)
            {
                button.SetAttackName(_overriddenSpirit);
            }
        }
    }

    private int CheckDistance(Spirit attacker, Spirit defender)
    {
        Tile attackTile = _teams[attacker.GetTeam()][attacker.GetSpiritNumber() - 1].GetComponentInParent<Tile>();
        Tile defenderTile = _teams[defender.GetTeam()][defender.GetSpiritNumber() - 1].GetComponentInParent<Tile>();

        int distance = 0;

        distance += Mathf.Abs(attackTile.GetX() - defenderTile.GetX());
        distance += Mathf.Abs(attackTile.GetY() - defenderTile.GetY());

        return (distance);
    }

    public void ClearTentative()
    {
        _overriddenSpirit.ClearTentativeAttackTarget();
        HideAttackChoices();
    }

    public void HideAttackChoices()
    {
        _choiceShown = false;
        _attackChoice.SetActive(false);
    }

    public void DisplayMoveConfirm(int x, int y)
    {
        _choiceShown = true;
        _tentMoveX = x;
        _tentMoveY = y;

        int displayX = x;
        int displayY = y;

        if (x == 0)
        {
            displayX = 1;
        }
        if (x == _boardSize - 1)
        {
            displayX = _boardSize - 2;
        }
        if (y == 0)
        {
            displayY = 1;
        }
        if (y == _boardSize - 1)
        {
            displayY = _boardSize - 2;
        }

        _confirmMove.transform.position = Camera.main.WorldToScreenPoint(_boardTiles[displayY][displayX].transform.position);
        _confirmMove.SetActive(true);
    }

    public void SetMoves()
    {
        _overriddenSpirit.SetTentativeMove(_tentMoveX, _tentMoveY, _boardTiles);
        HideMoveConfirm();
    }

    public void HideMoveConfirm()
    {
        _choiceShown = false;
        _tentMoveX = -1;
        _tentMoveY = -1;

        _confirmMove.SetActive(false);
    }

    public bool GetChoiceShown()
    {
        return _choiceShown;
    }

    private List<int[]> CalculatePossibleMoves(GameObject spirit, int movement)
    {
        List<int[]> possibleMoves = new List<int[]>();

        Tile spiritTile = spirit.GetComponentInParent<Tile>();

        int x = spiritTile.GetX();
        int y = spiritTile.GetY();

        foreach(int i in new int[2] { 1, -1})
        {
            if (x + i >= 0 && x + i < _boardSize)
            {
                if(_boardTiles[y][x + i].transform.childCount == 0)
                {
                    possibleMoves.Add(new int[2] { x + i, y });
                }
            }
        }

        foreach (int i in new int[2] { 1, -1 })
        {
            if (y + i >= 0 && y + i < _boardSize)
            {
                if (_boardTiles[y + i][x].transform.childCount == 0)
                {
                    possibleMoves.Add(new int[2] { x, y + i});
                }
            }
        }

        return (possibleMoves);
    }

    /*public void ChangeSelected(GameObject spirit)
    {
        _selectedUnit = spirit;
    }

    public void Attacking()
    {
        _attacking = true;
        _attackChoice.SetActive(false);
    }

    public void ClearDefender()
    {
        _attacking = false;
        _defender = null;
        _attackChoice.SetActive(false);
    }

    public bool IsAttacking()
    {
        return _attacking;
    }

    public void SetAttacker(GameObject spirit)
    {
        //check if needed
        _attacker = spirit;
    }

    public void SetDefender(GameObject spirit)
    {
        //check if needed
        _defender = spirit;
    }

    public void SetCombatants(GameObject attacker, GameObject defender, int distance)
    {
        if (_mover != null && attacker != _mover)
        {
            _attacker = attacker;
            _mover = null;
        }
        else if (_attacker != null && _attacker != attacker)
        {
            _attacker = attacker;
        }
        else
        {
            _attacker = attacker;
        }

        _attacker = attacker;
        Spirit attackSpirit = _attacker.GetComponent<MoveableObject>().GetSpirit();
        _defender = defender;

        int numPossibleAttacks = 0;

        for (int i = 0; i < 4; i++)
        {
            if (attackSpirit.GetAttackRange(i) < distance)
            {
                _attackChoice.transform.GetChild(i).GetComponent<Button>().interactable = false;
            }
            else
            {
                numPossibleAttacks += 1;
                _attackChoice.transform.GetChild(i).GetComponent<Button>().interactable = true;
            }
        }

        if (numPossibleAttacks == 0)
        {
            stateText.text = "Invalid Attack Target";
            _attacker = null;
            _defender = null;
        }
        else
        {
            _attackChoice.transform.position = Camera.main.WorldToScreenPoint(_defender.transform.position);
            _attackChoice.SetActive(true);
        }

        for (int i = 0; i < _attackChoice.transform.childCount; i++)
        {
            AttackButtonID button = _attackChoice.transform.GetChild(i).GetComponent<AttackButtonID>();

            if (button != null)
            {
                button.SetAttackName(attackSpirit);
            }
        }

        UnsetPossibleMoves();


    }

    public void ConfirmAttack()
    {
        if (CheckGameOver())
        {
            _defender = null;
            _moveSet = false;
            return;
        }

        if (_attacker != null && _defender != null)
        {
            _attacker.GetComponent<MoveableObject>().GetSpirit().GetAttack().BasicAttack(_defender);
            _targetSpirit = _defender.GetComponent<MoveableObject>().GetSpirit().GetSpiritNumber();
            if (CheckGameOver())
            {
                return;
            }

            for (int team = 1; team < 3; team++)
            {
                int otherTeam = ((team - 1) * -1) + 2;

                foreach (GameObject spirit in _teams[team])
                {

                    if (spirit.GetComponent<MoveableObject>().GetSpirit().IsDead() == false)
                    {
                        if (spirit != _attacker)
                        {
                            Spirit defendingSpirit = _teams[otherTeam][Random.Range(0, team2.Length)].GetComponent<MoveableObject>().GetSpirit();

                            while (defendingSpirit.IsDead())
                            {
                                defendingSpirit = _teams[otherTeam][Random.Range(0, team2.Length)].GetComponent<MoveableObject>().GetSpirit();
                            }

                            defendingSpirit.TakeDamage(10);
                            if (CheckGameOver())
                            {
                                return;
                            }
                        }
                    }
                }
            }

            StateIntention(_attacker.GetComponent<MoveableObject>().GetSpirit());
            _defender = null;
            _moveSet = false;
            _attacking = false;
        }
    }

    public float GetTileGap()
    {
        return _tileGap;
    }

    public void StateIntention(Spirit spirit)
    {
        if (_firstChoice == "Attack")
        {
            stateText.text = spirit.GetStatsString()["Name"] + " attacked " + _teams[2][_targetSpirit - 1].GetComponent<MoveableObject>().GetSpirit().GetStatsString()["Name"] + " and then moved to " + _selectedTile.GetX() + "-" + _selectedTile.GetY();
        }
        else
        {
            stateText.text = spirit.GetStatsString()["Name"] + " moved to " + _selectedTile.GetX() + "-" + _selectedTile.GetY() + " and then attacked " + _teams[2][_targetSpirit - 1].GetComponent<MoveableObject>().GetSpirit().GetStatsString()["Name"];
        }

        StartCoroutine(DisplayBeforeCompel());
    }

    private IEnumerator DisplayBeforeCompel()
    {
        yield return new WaitForSeconds(3);
        if (CheckGameOver())
        {
            stateText.text = "Game Over";
        }
        else
        {
            stateText.text = "Who will you compel?";
        }
    }

    public void SetTile(GameObject t)
    {
        _mousedTile = t.GetComponent<Tile>();
    }

    public void SetSelectedTile(Tile t)
    {
        _selectedTile = t;

        _confirmMove.transform.position = Camera.main.WorldToScreenPoint(_selectedTile.transform.position);
        _confirmMove.SetActive(true);
    }

    public void SetMove()
    {
        _targetMoveX = _selectedTile.GetComponent<Tile>().GetX();
        _targetMoveY = _selectedTile.GetComponent<Tile>().GetY();
        _moveSet = true;
        _confirmMove.SetActive(false);
    }

    public void ClearMove()
    {
        _selectedTile = null;
        _confirmMove.SetActive(false);
    }

    private void MakeMoves()
    {
        for (int team = 1; team < 3; team++)
        {

            foreach (GameObject spirit in _teams[team])
            {
                if (spirit.GetComponent<MoveableObject>().GetSpirit().IsDead() == false)
                {

                    if (spirit != _attacker)
                    {
                        int x = Random.Range(0, _boardSize);
                        int y = Random.Range(0, _boardSize);

                        while (_boardTiles[x][y].transform.childCount > 0)
                        {
                            x = Random.Range(0, _boardSize);
                            y = Random.Range(0, _boardSize);
                        }

                        spirit.transform.position = _boardTiles[x][y].transform.position;
                        spirit.transform.SetParent(_boardTiles[x][y].transform);
                    }
                    else
                    {
                        if (_selectedTile.gameObject.transform.childCount == 0)
                        {
                            spirit.transform.position = _selectedTile.gameObject.transform.position;
                            spirit.transform.SetParent(_selectedTile.gameObject.transform);
                        }
                    }
                }
            }
        }

        _selectedTile = null;
        _attacker = null;
        _mover = null;
    }

    public Tile GetMousedTile()
    {
        return _mousedTile;
    }

    public void ChangeAttackerSelected()
    {
        if (_attackerSelected)
        {
            _attackerSelected = false;

            for (int team = 1; team < 3; team++)
            {
                foreach (GameObject spirit in _teams[team])
                {
                    spirit.GetComponent<SphereCollider>().enabled = true;
                }
            }
        }
        else
        {
            _attackerSelected = true;

            for (int team = 1; team < 3; team++)
            {
                foreach (GameObject spirit in _teams[team])
                {
                    spirit.GetComponent<SphereCollider>().enabled = false;
                }
            }
        }
    }

    public void ChangeDragging()
    {
        if (_isDragging)
        {
            _isDragging = false;

            for (int team = 1; team < 3; team++)
            {
                foreach (GameObject spirit in _teams[team])
                {
                    spirit.GetComponent<SphereCollider>().enabled = true;
                }
            }
        }
        else
        {
            _isDragging = true;
            for (int team = 1; team < 3; team++)
            {
                foreach (GameObject spirit in _teams[team])
                {
                    spirit.GetComponent<SphereCollider>().enabled = false;
                }
            }
        }
    }

    public GameObject GetAttacker()
    {
        if (_attacker != null)
        {
            return _attacker;
        }
        else
        {
            Debug.Log("Attacker not set in GameManager - GetAttacker");
            return null;
        }
    }

    public void SetPossibleMoves(int x, int y)
    {
        for (int i = -1; i < 3; i = i + 2)
        {
            if ((x + i) > 0 && (x + i) < _boardSize)
            {
                _boardTiles[y][x + i].GetComponent<Tile>().SetMoveable();
            }
        }

        for (int i = -1; i < 3; i = i + 2)
        {
            if ((y + i) > 0 && (y + i) < _boardSize)
            {
                _boardTiles[y + i][x].GetComponent<Tile>().SetMoveable();
            }
        }
    }

    public void UnsetPossibleMoves()
    {
        if (_overriddenSpirit != null)
        {
            int x = _overriddenSpirit.GetComponentInParent<Tile>().GetX();
            int y = _overriddenSpirit.GetComponentInParent<Tile>().GetY();

            for (int i = -1; i < 3; i = i + 2)
            {
                if ((x + i) > 0 && (x + i) < _boardSize)
                {
                    _boardTiles[x + i][y].GetComponent<Tile>().UnsetMoveable();
                }
            }

            for (int i = -1; i < 3; i = i + 2)
            {
                if ((y + i) > 0 && (y + i) < _boardSize)
                {
                    _boardTiles[x][y + i].GetComponent<Tile>().UnsetMoveable();
                }
            }
        }


    }

    public bool GetAttackerSelected()
    {
        return (_attackerSelected);
    }

    public void SetOverriddenSpirit(GameObject spirit)
    {
        _overriddenSpirit = spirit;
    }

    public GameObject GetOverriddenSpirit()
    {
        return (_overriddenSpirit);
    }*/
}
