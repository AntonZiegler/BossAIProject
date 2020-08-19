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
        //The boss is a finite state machine. On a rewrite this would only posses 3 states, moveTo, ChooseAction, and PerformAction.
        //In this iteration, the states are each attack, with aditional functions for moveTo and SelectAttack. 
    [SerializeField] NavMeshAgent navAgent;
    [SerializeField] GameManagerScript gameManager;
    [SerializeField] Vector3 chosenPlayerPosition;
    [SerializeField] EntityScript HealthScript;
    bool performingAttack = false;
    bool choosingAttack = false;
    [SerializeField] Animator charAnimator;
    public Transform[] EnemySpawnPoints;
    [SerializeField] GameObject EnemyAdds;

    //The list of what each attack spawns
    [SerializeField] GameObject swordAttackGround;
    [SerializeField] GameObject spinAttack;
    [SerializeField] Transform spinAttackLocation;
    [SerializeField] GameObject swordAttackSky;
    [SerializeField] GameObject handAttack;
    [SerializeField] GameObject isInvulnerableObject;
    [SerializeField] GameObject isAttackableObject;

    //Animation triggers. 
    [SerializeField] int idleAnimation;
    [SerializeField] int walkAnimation;
    [SerializeField] int introAnimation;

    [SerializeField] int attack1Animation;
    [SerializeField] int attack2Animation;
    [SerializeField] int attack3Animation;
    [SerializeField] int attack4Animation;
    [SerializeField] int attack5Animation;

    List<GameObject> Adds;

    enum States
    {
        Attack1Ground,
        Attack2Adds,
        Attack3SkySword,
        Attack4Rotate,
        Attack5Hand,
        numStates
    }

    [SerializeField] States currentState;
 
    void Start()
    {

        gameManager = this.GetComponent<EnemyVarScript>().gameManager.GetComponent<GameManagerScript>();
        Adds = new List<GameObject>();
        SetAnimationInteger("SkeletonKingCondition", introAnimation);
        StartCoroutine(ChooseAttack(5.0f));

        // Set up the healthbar
        // If we find the tagged gameObject
        if (GameObject.FindGameObjectWithTag("BossHealthBar"))
        {
            // Finds the health bar
            GameObject healthBar = GameObject.FindGameObjectWithTag("BossHealthBar");
            // Sets it to active so that it appears
            healthBar.GetComponent<ToggleBossHealthBar>().EnableHealthBar();
            // Then set the entity script to use this health bar
            GetComponent<EntityScript>().ChangeHealthBar(healthBar);
        }
        else
        {
            Debug.Log("Can't find Boss Health Bar!");
        }
      
      
    }


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
       
        
        //once we reach a location we can perform these actions
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

    void Moveto(Vector3 location, bool LookAt)
    {
        this.navAgent.destination = location;
        if(LookAt)
        {
            this.transform.LookAt(location);
        }
        SetAnimationInteger("SkeletonKingCondition", walkAnimation);
    }

    void ChooseAPlayer()
    {
        int playerNum = Random.Range(0, gameManager.numPlayers); //inclusive of min, exclusive of max
        Debug.Log("PlayerNum" + playerNum);
       // Debug.Log(" " + gameManager.numPlayers);
        chosenPlayerPosition = gameManager.currentPlayers[playerNum].gameObject.transform.position;
    }

    bool HasReachedLocation(Vector3 location) 
    {

        if(Vector3.Distance(this.gameObject.transform.position, location) < 1.0f)
        {
            return true;
        }
        return false;
    }


    //Sword Attack
    IEnumerator Attack1(float waitTimer)
    {
        SetAnimationInteger("SkeletonKingCondition", attack1Animation);
        // Audio
        if (audioSource != null && swordAttackAudio != null)
        {
            audioSource.clip = swordAttackAudio;
            audioSource.Play();
        }

        //Spawn rocks 
        performingAttack = true;
        Debug.Log("ATK 1");


        yield return new WaitForSeconds(waitTimer);
        performingAttack = false;
        Debug.Log(" Done ATK 1");
        StartCoroutine(ChooseAttack(3.0f));
    }
    //Spawn Adds
    IEnumerator Attack2(float waitTimer)
    {
        //WIll need to make another dark knight or something, as a custom ai, as they dont spawn normally
        SetAnimationInteger("SkeletonKingCondition", attack2Animation);
    
        foreach ( Transform spawnPoint in EnemySpawnPoints)
        {
            GameObject Add = GameObject.Instantiate(EnemyAdds, spawnPoint.position, spawnPoint.rotation);
            Adds.Add(Add); //This is a little ambigous, but its add the enemies that the boss spawns to a list so they can be destroyed when the boss is. 
            Add.GetComponent<EnemyVarScript>().SpawningfromBoss(GameObject.FindWithTag("GameManager"), Random.Range(0, gameManager.numPlayers));
        }
        //Spawn rocks 
        performingAttack = true;
        Debug.Log("ATK 2");
        yield return new WaitForSeconds(waitTimer);
        performingAttack = false;
        Debug.Log(" Done ATK 2");
        StartCoroutine(ChooseAttack(3.0f));
    }
    //Sky sword
    IEnumerator Attack3(float waitTimer)
    {

        SetAnimationInteger("SkeletonKingCondition", attack3Animation);

        StartCoroutine(Attack3Delay(0.8f));
       
        performingAttack = true;
        Debug.Log("ATK 3");
        yield return new WaitForSeconds(waitTimer);

       // Destroy(LightningStrike, 3.0f);
        performingAttack = false;
        Debug.Log(" Done ATK 3");
        StartCoroutine(ChooseAttack(3.0f));
    }

    //Spin Attack
    IEnumerator Attack4(float waitTimer)
    {
        GameObject SpinAttack = GameObject.Instantiate(spinAttack, spinAttackLocation.position, spinAttackLocation.rotation);
        Destroy(SpinAttack, 3.5f);
        SetAnimationInteger("SkeletonKingCondition", attack4Animation);

        performingAttack = true;
        Debug.Log("ATK 4");
        yield return new WaitForSeconds(waitTimer);
        
        performingAttack = false;
        Debug.Log(" Done ATK 4");
        StartCoroutine(ChooseAttack(3.0f));
    }

    //Hand
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

    IEnumerator Attack5wait(float waitTimer)
    {

        yield return new WaitForSeconds(waitTimer);
        for (int x = 0; x < gameManager.numPlayers; ++x)
        {
            GameObject hand = GameObject.Instantiate(handAttack, new Vector3(gameManager.currentPlayers[x].transform.position.x, this.transform.position.y, gameManager.currentPlayers[x].transform.position.z), gameManager.currentPlayers[x].transform.rotation);
            Destroy(hand, 1.0f);
        }
    }

    IEnumerator Attack3Delay(float waitTimer)
    {
        yield return new WaitForSeconds(waitTimer);
        ChooseAPlayer();
        GameObject LightningStrike = GameObject.Instantiate(swordAttackSky, new Vector3(chosenPlayerPosition.x, chosenPlayerPosition.y + 10, chosenPlayerPosition.z), this.transform.rotation);
       
    }


    IEnumerator ChooseAttack(float waitTimer)
    {
        SetAnimationInteger("SkeletonKingCondition", idleAnimation);
        Debug.Log("Attack waiting");
        choosingAttack = true;

        HealthScript.SetInvulnerable(false);
        isInvulnerableObject.SetActive(false);
        isAttackableObject.SetActive(true);

        yield return new WaitForSeconds(waitTimer);

        HealthScript.SetInvulnerable(true);
        isInvulnerableObject.SetActive(true);
        isAttackableObject.SetActive(false);


        choosingAttack = false;
        //Choose an attack, update the state machine, call the correct functions
        int randNum = Random.Range(1, 6);
        Debug.Log(randNum);
        Debug.Log("CHose an attack");

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

    private void OnDestroy()
    {
        foreach(GameObject Add in Adds)
        {
            Add.GetComponent<EnemyScript>().DestoryedByBoss();
        }
    }
}
