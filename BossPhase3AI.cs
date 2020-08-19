using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

/******************
 * Anton Ziegler
 * ****************/
public class BossPhase3AI : MonoBehaviour
{
      

    //The Boss Ai below is a Finite State Machine, with each of the possible attackes, numbered 1 through 5 being the states. 

    //These are the various variables that the boss needs to function. 
    [SerializeField] NavMeshAgent navAgent; //This is used so the boss can navigate the ground. 
    [SerializeField] GameManagerScript gameManager; //This is the Blackboard to keep track of the players and their states
    [SerializeField] Vector3 chosenPlayerPosition; //When a player is chosen to attack, this tracks the players position at that time
    [SerializeField] EntityScript HealthScript; //This handles the health of the boss, and whether it can be damaged.
    bool performingAttack = false; //If the boss is performaing an attack we dont need to do other actions. 
    bool choosingAttack = false; //If the boss is choosing an attack, we are vulnerable. 
    [SerializeField] Animator charAnimator; //The animator for the boss
    public Transform[] EnemySpawnPoints; //The boss has an attack that spawns aditional enemies, this is where we want them to spawn. 
    [SerializeField] GameObject EnemyAdds; //The additional enemies that the boss can spawn. 

    //THe boss attacks pawn various objects and colliders in the world, depending on which attack is chosen. 
    [SerializeField] GameObject swordAttackGround;
    [SerializeField] GameObject spinAttack;
    [SerializeField] Transform spinAttackLocation;
    [SerializeField] GameObject swordAttackSky;
    [SerializeField] GameObject handAttack;
    [SerializeField] GameObject isInvulnerableObject;
    [SerializeField] GameObject isAttackableObject;

    //Animation triggers. 
    //We used an integer value to handle the current animation the boss is in, and we reference them here. 
    [SerializeField] int idleAnimation;
    [SerializeField] int walkAnimation;
    [SerializeField] int introAnimation;

    [SerializeField] int attack1Animation;
    [SerializeField] int attack2Animation;
    [SerializeField] int attack3Animation;
    [SerializeField] int attack4Animation;
    [SerializeField] int attack5Animation;

    //This is here to keep track of the enemies the boss creates, when the boss is defeated, we dont want to have the enemies around so we use this to also delete them. 
    List<GameObject> Adds;

    //The finite state machine, uses enum switch cases and they are declared here. 
    enum States
    {
        Attack1Ground,
        Attack2Adds,
        Attack3SkySword,
        Attack4Rotate,
        Attack5Hand,
        numStates
    }

    //The state we are currently in. 
    [SerializeField] States currentState;
 
    void Start()
    {
        //When created we grab the game manager, as it is static, and remains in the world. 
        gameManager = this.GetComponent<EnemyVarScript>().gameManager.GetComponent<GameManagerScript>();

        //Initilize the Enemies List so we can add enemies to it. 
        Adds = new List<GameObject>();

        //Begin playing the animation for the boss entering the area. 
        SetAnimationInteger("SkeletonKingCondition", introAnimation);

        //We also setup the first attack that the boss chooses here, for when the boss is done playing the intro animation
        StartCoroutine(ChooseAttack(5.0f));

    }

    //This is a small function to make setting animations a tad cleaner. 
    private void SetAnimationInteger(string condition, int integer)
    {
        charAnimator.SetInteger(condition, integer);
    }


    // Update is called once per frame
    void Update()
    {
        //if we are attacking, or choosing an attack, we dont need to continue through the update loop. 
        if(performingAttack)
        {
            return;
        }

        if(choosingAttack)
        {
            return;
        }
       
        
       //The Boss has 2 attacks that require movement, and until we reach the desired location we dont want to perform the attacks. 
       //So we cann a function HAsReachedLocation, and if we are close enough to the location we then perform the chosen attack, if we arent we continue moving on.  
        switch (currentState)
        {
            case States.Attack1Ground:
                if (HasReachedLocation(chosenPlayerPosition))
                {
                    StartCoroutine(Attack1(2.0f));
                }
                break;
            case States.Attack4Rotate:
                if (HasReachedLocation(chosenPlayerPosition))
                {
                    StartCoroutine(Attack4(1.5f));
                }
                break;
                
        }
    }

    //the boss has 5 attacks
    //Attack one is the sword slam (at locaation) where some rocks come out
    //Attack 2 is to summon some minions
    //Skyward sword spawns a lighting attack
    //360 sword swing is move to location, play anim.
    //reach into the ground and spawn hand. 
    //the attacks can be broken into 2 catagories
    // move to location, perform action
    // stand still, spawn prefab (while in an animation)

    //2 of the attacks have the boss move to a location, so to do that, we set the boss to move along the navmesh, look at the location and play the walk animation. 
    void Moveto(Vector3 location, bool LookAt)
    {
        this.navAgent.destination = location;
        if(LookAt)
        {
            this.transform.LookAt(location);
        }
        SetAnimationInteger("SkeletonKingCondition", walkAnimation);
    }

    //Attacks choose a random player, in the list of available players from the game manager. If a player is dead, or drops out of the game, they wont appear in this list. 
    void ChooseAPlayer()
    {
        int playerNum = Random.Range(0, gameManager.numPlayers); //inclusive of min, exclusive of max
        Debug.Log("PlayerNum" + playerNum);
       // Debug.Log(" " + gameManager.numPlayers);
        chosenPlayerPosition = gameManager.currentPlayers[playerNum].gameObject.transform.position;
    }

    //A Check to see if we are close enough to the location of the player we want to attack that location. 
    bool HasReachedLocation(Vector3 location) 
    {

        if(Vector3.Distance(this.gameObject.transform.position, location) < 1.0f)
        {
            return true;
        }
        return false;
    }


    //Sword Attack
    //This attack is animation based, the boss holds a large sword, with a hitbox on it. 
    //The animation that plays has the sword swing at the location, attemting to hit players. 
    //When complete, it goes back into choosing another attack. 
    IEnumerator Attack1(float waitTimer)
    {
        SetAnimationInteger("SkeletonKingCondition", attack1Animation);
        
        //Spawn rocks 
        performingAttack = true;
        Debug.Log("ATK 1");


        yield return new WaitForSeconds(waitTimer);
        performingAttack = false;
        Debug.Log("Done ATK 1");
        StartCoroutine(ChooseAttack(3.0f));
    }


    //Spawn Adds
    //This attacks spawns enemies for the players to fight, at the chosen locations for the players. 
    
    IEnumerator Attack2(float waitTimer)
    {
        SetAnimationInteger("SkeletonKingCondition", attack2Animation);
    
        foreach ( Transform spawnPoint in EnemySpawnPoints)
        {
            GameObject Add = GameObject.Instantiate(EnemyAdds, spawnPoint.position, spawnPoint.rotation); //This creates the enemy at the spawn location 
            Adds.Add(Add); //This is a little ambigous, but its add the enemies that the boss spawns to a list so they can be destroyed when the boss is. 
            Add.GetComponent<EnemyVarScript>().SpawningfromBoss(GameObject.FindWithTag("GameManager"), Random.Range(0, gameManager.numPlayers)); //The enemy needs to have information about which player to chase and fight, and that is done here. 
        }
        //Spawn rocks 
        performingAttack = true;
        Debug.Log("ATK 2");

        yield return new WaitForSeconds(waitTimer);

        performingAttack = false;
        Debug.Log("Done ATK 2");
        StartCoroutine(ChooseAttack(3.0f));
    }

    //Sky sword
    //This attack is performaed at a standing location, before selecting a player to spawn a lighting strike object on top of before going back into other attacks. 
    IEnumerator Attack3(float waitTimer)
    {

        SetAnimationInteger("SkeletonKingCondition", attack3Animation);

        StartCoroutine(Attack3Delay(0.8f));
       
        performingAttack = true;
        Debug.Log("ATK 3");

        yield return new WaitForSeconds(waitTimer);

       // Destroy(LightningStrike, 3.0f);
        performingAttack = false;
        Debug.Log("Done ATK 3");
        StartCoroutine(ChooseAttack(3.0f));
    }

    //Spin Attack
    //The boss spins around in a large circle. This is part animation based, using the collider on the sword, and part not.
    //we spawn an object under the boss, between its body and the sword to keep players from getting in under the sword and damaging the boss from up close instantly. 
    IEnumerator Attack4(float waitTimer)
    {
        GameObject SpinAttack = GameObject.Instantiate(spinAttack, spinAttackLocation.position, spinAttackLocation.rotation);
        Destroy(SpinAttack, 3.5f);
        SetAnimationInteger("SkeletonKingCondition", attack4Animation);

        performingAttack = true;
        Debug.Log("ATK 4");
        yield return new WaitForSeconds(waitTimer);
        
        performingAttack = false;
        Debug.Log("Done ATK 4");
        StartCoroutine(ChooseAttack(3.0f));
    }

    //Hand
    //This attacks spawns an objet under the players feet that they need to jump to avoid. 
    //We tried to make the players use different mechanics for each attack, requiring them to remain in constant motion, to void getting hit. 
    
    IEnumerator Attack5(float waitTimer)
    {
        SetAnimationInteger("SkeletonKingCondition", attack5Animation);

        //Spawn rocks 
        performingAttack = true;
        StartCoroutine(Attack5wait(1.0f));
        Debug.Log("ATK 5");
        yield return new WaitForSeconds(waitTimer);
        performingAttack = false;
        Debug.Log(" Done ATK 5");
        StartCoroutine(ChooseAttack(3.0f));
    }

    //This goes along with the hand attack, to create and then destroy the object seperatly from the attack animation, as it doesnt have the same timer as the boss choosing its next attack. 
    IEnumerator Attack5wait(float waitTimer)
    {

        yield return new WaitForSeconds(waitTimer);
        for (int x = 0; x < gameManager.numPlayers; ++x)
        {
            //The obect is spawned on a plane that is equal to the bosses feet, as this was the easiest reference location.
            //Using the players feet resulted in them either always jumping over it, or always getting hit. 
            GameObject hand = GameObject.Instantiate(handAttack, new Vector3(gameManager.currentPlayers[x].transform.position.x, this.transform.position.y, gameManager.currentPlayers[x].transform.position.z), gameManager.currentPlayers[x].transform.rotation);
            Destroy(hand, 1.0f);
        }
    }

    //This attacks spawns an object above the players head, in the form of a lightingin strike to drop down on the player. 
    IEnumerator Attack3Delay(float waitTimer)
    {
        yield return new WaitForSeconds(waitTimer);
        ChooseAPlayer(); //We needed to select which player to hit with attack. 
        GameObject LightningStrike = GameObject.Instantiate(swordAttackSky, new Vector3(chosenPlayerPosition.x, chosenPlayerPosition.y + 10, chosenPlayerPosition.z), this.transform.rotation);
       //We dont destory the object here, as the object handled that itself when its lifecycle was over. 
    }


    //This selects one of the five available attacks the boss can perform. 
    IEnumerator ChooseAttack(float waitTimer)
    {
        SetAnimationInteger("SkeletonKingCondition", idleAnimation);
        Debug.Log("Attack waiting");
        choosingAttack = true;

        //While we wait for the boss attack to be chosen, the boss can be damaged, and this is done here. 
        HealthScript.SetInvulnerable(false);
        isInvulnerableObject.SetActive(false);
        isAttackableObject.SetActive(true);

        yield return new WaitForSeconds(waitTimer);

        //Once we have chosen an attack, and are either performing it, or moving to it, we no longer can be damaged. 
        HealthScript.SetInvulnerable(true);
        isInvulnerableObject.SetActive(true);
        isAttackableObject.SetActive(false);


        choosingAttack = false;
        //Choose an attack, update the state machine, call the correct functions
        int randNum = Random.Range(1, 6);
        Debug.Log(randNum);
        Debug.Log("CHose an attack");

        //Where we choose the actions the attack, and what functions to perform. 
        switch (randNum)
        {
            case 1:
                currentState = States.Attack1Ground;
                ChooseAPlayer();
                Moveto(chosenPlayerPosition, true);
                break;
            case 2:
                currentState = States.Attack2Adds;
                //This state bypasses having to call functions in update
                StartCoroutine(Attack2(3.0f));
                break;
            case 3:
                currentState = States.Attack3SkySword;
                //This state also bypasses having to call functions in update
                StartCoroutine(Attack3(3.0f));
                break;
            case 4:
                currentState = States.Attack4Rotate;
                ChooseAPlayer();
                Moveto(chosenPlayerPosition, true);
                break;
            case 5:
                currentState = States.Attack5Hand;
                StartCoroutine(Attack5(3.0f));
                //This state as well as the other two bypasses having to call update functions
                break;
        }
    }

    //When the boss is killed, if there are enemies in the world, we need to destroy them as well. 
    private void OnDestroy()
    {
        foreach(GameObject Add in Adds)
        {
            Add.GetComponent<EnemyScript>().DestoryedByBoss();//When the enemies are killed, they had functionality to let the game manager know they were destoryed, and this bypasses that. 
        }
    }
}
