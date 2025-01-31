using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class PlayerMovement : MonoBehaviour
{
    public HealthManager hp;
    public HealthManager st;
    public HealthManager hu;
    public Animator animator;
    public float acceleration = 3f;
    public float maxSpeed = 3f;
    public float maxSprintSpeed = 6f;
    public float jumpForce = 5f;
    public Camera playerCamera;
    public float mouseSensitivity = 100f;
    public float stamina = 100f;
    public float health = 100f;
    public float hunger = 100f;
    public float hungerDecreaseRate = 1f; // Decrease per second
    public float sprintHungerMultiplier = 3f;
    public float staminaDecreaseRate = 10f; // Per second while sprinting or flying
    public float fallDamageFactor = 5f; // Multiplier for fall damage
    private bool inAir=false;
    private Rigidbody rb;
    private float rotationX = 0f;
    private bool isGrounded;
    private bool gameOver = false;
    private Vector3 currentVelocity = Vector3.zero;
    private bool isFlying = false;
    private float flightStartY = 0f;
    private float lastHeight;
    private bool down=false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible=true;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SceneManager.LoadScene("Menu");
        }

        if (gameOver)
        {
            LookAround();

            if (Input.anyKeyDown || transform.position.y < -20)
            {
                RestartGame();
            }

            return;
        }
        if (Input.GetButtonDown("Fire1"))
			{
				animator.SetTrigger("Attack");
            }
        HungerHandler();
        Move();
        LookAround();

        if (Input.GetKey(KeyCode.Space) && stamina > 0 && (isGrounded||!isFlying&&inAir))
        {
            Fly();
            down=false;
        }

        if (Input.GetKey(KeyCode.LeftControl) && stamina > 0 && !isGrounded)
        {
            Fly();
            down=true;
        }

        if (isFlying)
        {
            int factorr=1;
            if(down==true){
                factorr=-1;
            }
            else{
                stamina -= staminaDecreaseRate * Time.deltaTime;
            }
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce*factorr, rb.linearVelocity.z);
            if (stamina <= 0 || Input.GetKeyUp(KeyCode.Space))
            {
                EndFlight();
                if(Input.GetKeyUp(KeyCode.Space) && stamina>0){
                    inAir=true;
                    rb.useGravity=false;
                    rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z); // Stops upward motion only

                }
            }
        }

        if (transform.position.y < -20)
        {
            EndGame();
        }
        hp.SetHealth(health);
        hu.SetHealth(((int)(hunger+20)/20)*20);
        st.SetHealth(stamina);
        if(health<=0f){
            die();
        }
    }

    private void die(){
        animator.SetTrigger("isDead");
        gameOver=true;
    }

    private void Move()
    {
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        Vector3 forward = playerCamera.transform.forward;
        Vector3 right = playerCamera.transform.right;

        forward.y = 0;
        right.y = 0;

        forward.Normalize();
        right.Normalize();

        Vector3 direction = (right * moveHorizontal + forward * moveVertical).normalized;

        bool isSprinting = Input.GetKey(KeyCode.LeftShift) && stamina > 0 && direction.magnitude > 0;

        float currentMaxSpeed = isSprinting ? maxSprintSpeed : maxSpeed;

        if(stamina<=0 && !rb.useGravity){
            EndFlight();
        }
        if(isSprinting || inAir){
        if (isSprinting)
        {
            stamina -= staminaDecreaseRate * Time.deltaTime;
            if (stamina <= 0)
            {
                stamina = 0;
            }
        }
        if (inAir)
        {
            stamina -= staminaDecreaseRate/5 * Time.deltaTime;
            if (stamina <= 0)
            {
                stamina = 0;
            }
        }}
        else if (!isFlying && stamina < 100f && isGrounded)
        {
            stamina += staminaDecreaseRate * 0.5f * Time.deltaTime; // Regenerate stamina when not sprinting
        }
        if (direction.magnitude > 0)
        {
            currentVelocity += direction * acceleration * Time.deltaTime;
            currentVelocity = Vector3.ClampMagnitude(currentVelocity, currentMaxSpeed);
        }
        else
        {
            currentVelocity = Vector3.Lerp(currentVelocity, Vector3.zero, Time.deltaTime * acceleration);
        }

        rb.linearVelocity = new Vector3(currentVelocity.x, rb.linearVelocity.y, currentVelocity.z);

        animator.SetFloat("Speed", direction.magnitude);
    }

    private void HungerHandler()
    {
        float hungerReduction = hungerDecreaseRate * Time.deltaTime;
        if (Input.GetKey(KeyCode.LeftShift))
        {
            hungerReduction *= sprintHungerMultiplier;
        }

        if (hunger < 0)
        {
            health -= hungerDecreaseRate * Time.deltaTime; // Health decreases when hunger is zero
        }
        else{
            hunger -= hungerReduction;
        }
    }

    private void Fly()
    {
        if (!isFlying)
        {
            isFlying = true;
            inAir=true;
            isGrounded = false;
            flightStartY = transform.position.y;
            rb.useGravity = false;
        }
    }

    private void EndFlight()
    {
        isFlying = false;
        rb.useGravity = true;
        Debug.Log("relly");
        lastHeight= rb.position.y;
    }

    private void LookAround()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, -20f, 50f);

        playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            if (lastHeight>5f && inAir && !isFlying) // Detect significant downward velocity
            {
                float damage = (lastHeight-rb.position.y)*3; // Calculate damage based on fall speed
                health -= damage; // Apply damage
                Debug.Log($"Fall Damage Taken: {damage}");
            }
            isGrounded = true;
            inAir=false;
            if (isFlying)
            {
                EndFlight();
            }
        }
    }

    private void EndGame()
    {
        gameOver = true;
    }

    void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
