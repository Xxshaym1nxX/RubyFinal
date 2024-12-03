using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class RubyController : MonoBehaviour
{
    public GameObject gameOver;
    public GameObject youWin;
    public bool gameOverBool;
    // ========= MOVEMENT =================
    public float speed = 4;
    public InputAction moveAction;
    
    // ======== HEALTH ==========
    public int maxHealth = 5;
    public float timeInvincible = 2.0f;
    public Transform respawnPosition;
    public ParticleSystem hitParticle;
    
    // ======== PROJECTILE ==========
    public GameObject projectilePrefab;
    public InputAction launchAction;
    //Added for this assignment, the first 2 are for cooldown, the other two help the power up. One to check if it is active, the other sets the size.
    private float nextShot = 0.25f;
    [SerializeField]
    private float fireDelay = 0.5f;
    public bool powerUpActive;
    [SerializeField]
    private int projectileSize;

    // ======== AUDIO ==========
    public AudioClip hitSound;
    public AudioClip shootingSound;
    //These two audioclips were added for this assignment. soundPlaying is to have the sound only play once
    public AudioClip victorySound;
    public AudioClip gameOverSound;
    bool soundPlaying;
    
    // ======== HEALTH ==========
    public int health
    {
        get { return currentHealth; }
    }
    
    // ======== DIALOGUE ==========
    public InputAction dialogAction;
    
    // =========== MOVEMENT ==============
    Rigidbody2D rigidbody2d;
    Vector2 currentInput;
    
    // ======== HEALTH ==========
    int currentHealth;
    float invincibleTimer;
    bool isInvincible;
    public int score;
   
    // ==== ANIMATION =====
    Animator animator;
    Vector2 lookDirection = new Vector2(1, 0);
    
    // ================= SOUNDS =======================
    AudioSource audioSource;
    
    void Start()
    {
        youWin.SetActive(false);
        gameOver.SetActive(false);
        // =========== MOVEMENT ==============
        rigidbody2d = GetComponent<Rigidbody2D>();
        moveAction.Enable();
        gameOverBool = false;
        
        // ======== HEALTH ==========
        invincibleTimer = -1.0f;
        currentHealth = maxHealth;
        
        // ==== ANIMATION =====
        animator = GetComponent<Animator>();
        
        // ==== AUDIO =====
        audioSource = GetComponent<AudioSource>();
        // Added for later code.
        soundPlaying = false;

        // ==== ACTIONS ====
        launchAction.Enable();
        dialogAction.Enable();

        launchAction.performed += LaunchProjectile;

        //Added for this assignment, starts the boolean at false
        powerUpActive = false;
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.R)) // check to see if the player is pressing R

        {

            if (gameOverBool == true) // check to see if the game over boolean has been set to true

            {

                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); // this loads the currently active scene, which results in a restart of whatever scene the player is currently in

            }

        }
        if (score > 3)
        {
            youWin.SetActive(true);
            // Checks if a victory/game over sound is already playing, and if not, plays a victory jingle, then sets the variable to false
            if (soundPlaying == false)
            {
                PlaySound(victorySound);
                soundPlaying = true;
            }
            gameOverBool = true;
            speed = 0;
        }
        if (currentHealth <= 0)
        {
            gameOver.SetActive(true);
            // Checks if a victory/game over sound is already playing, and if not, plays a game over jingle, then sets the variable to false
            if (soundPlaying == false)
            {
                PlaySound(gameOverSound);
                soundPlaying = true;
            }
            gameOverBool = true;
            speed = 0;
        }
        // ================= HEALTH ====================
        if (isInvincible)
        {
            invincibleTimer -= Time.deltaTime;
            if (invincibleTimer < 0)
                isInvincible = false;
        }

        // ============== MOVEMENT ======================
        Vector2 move = moveAction.ReadValue<Vector2>();
        
        if(!Mathf.Approximately(move.x, 0.0f) || !Mathf.Approximately(move.y, 0.0f))
        {
            lookDirection.Set(move.x, move.y);
            lookDirection.Normalize();
        }

        currentInput = move;


        // ============== ANIMATION =======================

        animator.SetFloat("Look X", lookDirection.x);
        animator.SetFloat("Look Y", lookDirection.y);
        animator.SetFloat("Speed", move.magnitude);

        // ======== DIALOGUE ==========
        if (dialogAction.WasPressedThisFrame())
        {
            RaycastHit2D hit = Physics2D.Raycast(rigidbody2d.position + Vector2.up * 0.2f, lookDirection, 1.5f, 1 << LayerMask.NameToLayer("NPC"));
            if (hit.collider != null)
            {
                NonPlayerCharacter character = hit.collider.GetComponent<NonPlayerCharacter>();
                if (character != null)
                {
                    UIHandler.instance.DisplayDialog();
                }  
            }
        }
 
    }

    void FixedUpdate()
    {
        Vector2 position = rigidbody2d.position;
        
        position = position + currentInput * speed * Time.deltaTime;
        
        rigidbody2d.MovePosition(position);
    }

    // ===================== HEALTH ==================
    public void ChangeHealth(int amount)
    {
        if (amount < 0)
        { 
            if (isInvincible)
                return;
            
            isInvincible = true;
            invincibleTimer = timeInvincible;
            
            animator.SetTrigger("Hit");
            audioSource.PlayOneShot(hitSound);

            Instantiate(hitParticle, transform.position + Vector3.up * 0.5f, Quaternion.identity);
        }
        
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        
        UIHandler.instance.SetHealthValue(currentHealth / (float)maxHealth);
        //UIHealthBar.Instance.SetValue(currentHealth / (float)maxHealth);
    }
    
    void Respawn()
    {
        ChangeHealth(maxHealth);
        transform.position = respawnPosition.position;
    }
    
    // =============== PROJECTICLE ========================
    void LaunchProjectile(InputAction.CallbackContext context)
    {
        //Added a cooldown to the projectile
        if (Time.time > nextShot)
        {
            GameObject projectileObject = Instantiate(projectilePrefab, rigidbody2d.position + Vector2.up * 0.5f, Quaternion.identity);

            Projectile projectile = projectileObject.GetComponent<Projectile>();
            projectile.Launch(lookDirection, 300);
            //If the power up is active, the projectile will scale up, from testing, I noticed this includes the hit box too.
            if (powerUpActive)
            {
                projectile.transform.localScale = new Vector3(projectileSize, projectileSize, projectileSize);
            }
            animator.SetTrigger("Launch");
            audioSource.PlayOneShot(shootingSound);
            nextShot = Time.time + fireDelay;
        }
    }
    
    // =============== SOUND ==========================

    //Allow to play a sound on the player sound source. used by Collectible
    public void PlaySound(AudioClip clip)
    {
        audioSource.PlayOneShot(clip);
    }

    public void ChangeScore(int scoreAmount)
    {
        score += scoreAmount;
    }

    //Added for assignment, makes the boolean "powerUpActive" true to make projectiles larger
    public void ActivatePowerUp ()
    {
        powerUpActive = true;
    }

}
