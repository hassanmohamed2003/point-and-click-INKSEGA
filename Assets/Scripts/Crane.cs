using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class Crane : MonoBehaviour
{    
    [Header("Movement")]
    public float LeftRightSpeed;
    public float PieceLoweringSpeed;
    public float PieceTimeInterval;
    public float MaxAcceleration;

    [Header("Behaviour")]
    public float pieceStartHeight;
    public Vector2 ropeStartOffset;
    public float InitialSwingForce;

    [Header("Rope Settings")]
    public float ropeBreakTime;
    public float blinkStartInterval;
    public float blinkActiveTime;
    public float timeTillFirstBlink;

    [Header("Events")]
    public UnityEvent RopeBreakEvent;
    public UnityEvent RopeBlinkEvent;
    public UnityEvent FirstPressEvent;
    
    private GameObject currentBuildingPiece;
    private Rigidbody2D rb;
    private float leftEdgeScreenX, rightEdgeScreenX;
    private LineRenderer lineRenderer;
    private bool enableRopeBreak = true;
    private IEnumerator RopeRoutine;
    private bool firstPressComplete = false;
    bool movingRight = true;


    [SerializeField] private float limit = 130f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        lineRenderer = GetComponent<LineRenderer>();
    }

    void Start()
    {
        Input.gyro.enabled = true;

        // Get the edge of the screen
        leftEdgeScreenX = Camera.main.ViewportToWorldPoint(new Vector3(0,0,0)).x;
        rightEdgeScreenX = Camera.main.ViewportToWorldPoint(new Vector3(1, 0, 0)).x;
    }

    // Update is called once per frame
    void Update()
    {
        if (!Game.instance.IsGameOver)
        {
            HandleUserInput();
        }
        UpdateRope();


    }

    private void HandleUserInput()
    {


        // Get the tilt of the phone
        float accelerationX = Math.Clamp(Input.acceleration.x, -MaxAcceleration, MaxAcceleration);

        accelerationX *= Time.deltaTime * LeftRightSpeed;

        // Get the top of the camera screen
        float translationY = Camera.main.ViewportToWorldPoint(new Vector3(0, 1, 0)).y;
        translationY -= rb.position.y;

        // Build translation vector for this frame
        Vector2 craneTranslation = new(accelerationX, translationY);

        Vector2 oldCranePosition = rb.position + craneTranslation;

        float leftBorder = leftEdgeScreenX + 0.28f;
        float rightBorder = rightEdgeScreenX - 0.28f;
/*

        if (movingRight)
        {
            craneTranslation.x = 0.03f * LeftRightSpeed;

            if (rb.position.x >= rightBorder)
            {
                movingRight = false;
            }
        }
        else
        {
            craneTranslation.x = 0.03f * -LeftRightSpeed;

            if (rb.position.x <= leftBorder)
            {
                movingRight = true;
            }
        }
*/
        // Override tilt controls for testing with keyboard
        if (Input.GetKey(KeyCode.A))
        {
            craneTranslation.x = 0.03f * -LeftRightSpeed;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            craneTranslation.x = 0.03f * LeftRightSpeed;
        }

        // Move crane
        Vector2 newCranePosition = rb.position + craneTranslation;

        newCranePosition.x = Math.Clamp(newCranePosition.x, leftEdgeScreenX + 0.28f, rightEdgeScreenX - 0.28f);


        // Mouse movement override using **right mouse button**, only in level 2
        if (GameState.CurrentLevelID == 2 && Input.GetMouseButton(1))
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            float targetX = Math.Clamp(mousePosition.x, leftEdgeScreenX + 0.28f, rightEdgeScreenX - 0.28f);
            newCranePosition = new(targetX, rb.position.y);
            float smoothSpeed = 1f; // Adjust to taste
            float smoothedX = Mathf.Lerp(rb.position.x, targetX, smoothSpeed * Time.deltaTime);
            rb.MovePosition(new Vector2(smoothedX, rb.position.y));
        }


        rb.MovePosition(newCranePosition);

        // Release building piece when touch input is detected
        if (Input.touchCount > 0 || Input.GetMouseButtonDown(0))
        {   
            ReleaseConnectedPiece();
            if(!firstPressComplete)
            {
                FirstPressEvent.Invoke();
                firstPressComplete = true;
            }            
        } 
        if (Input.GetKeyDown(KeyCode.R))
        {
            ReleaseConnectedPiece();
            if(!firstPressComplete)
            {
                FirstPressEvent.Invoke();
                firstPressComplete = true;
            }
        }
    }

    private void UpdateRope()
    {
        if (currentBuildingPiece != null && lineRenderer != null)
        {
            // Increase the length of the joint
            SpringJoint2D joint = currentBuildingPiece.GetComponent<SpringJoint2D>();
            joint.distance += PieceLoweringSpeed * Time.deltaTime;

            // Update the line from the crane to the connected block
            Vector2 newRopeStartPos = (Vector2)transform.position - joint.anchor;
            Vector2 newRopeEndPos = currentBuildingPiece.transform.position;
            if (currentBuildingPiece.TryGetComponent(out BuildingBlock block))
            {
                newRopeEndPos.y += block.ropeStartOffset;
            }
            lineRenderer.SetPosition(0, newRopeStartPos);
            lineRenderer.SetPosition(1, newRopeEndPos);
        }
    }

    public void OnUpdateRopeSwing(float value)
    {
        InitialSwingForce += value;
        if(InitialSwingForce > limit) InitialSwingForce = limit;
    }

    public void SetRopeBreak(bool enabled)
    {
        enableRopeBreak = enabled;
    }

    public void OnGameOver()
    {
        PieceLoweringSpeed = 0.0f;
        if(RopeRoutine != null) StopCoroutine(RopeRoutine);
        Center();
    }

    public void Center()
    {
        rb.MovePosition(Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 1f, 0f)));
    }

    public void SetConnectedPiece(GameObject newBlock)
    {        
        currentBuildingPiece = newBlock;
        
        // Setup SpringJoint
        SpringJoint2D joint = currentBuildingPiece.GetComponent<SpringJoint2D>();
        joint.connectedBody = rb;
        joint.anchor = ropeStartOffset;
        joint.autoConfigureDistance = false;
        joint.distance = pieceStartHeight;

        // Setup LineRenderer
        if(lineRenderer)
        {
            lineRenderer.startColor = Color.black;
            lineRenderer.endColor = Color.black;
            lineRenderer.forceRenderingOff = false;
        }        

        // Update new block's RigidBody
        Rigidbody2D rigidbodyPiece = currentBuildingPiece.GetComponent<Rigidbody2D>();
        Vector2 force = new(UnityEngine.Random.Range(0.5f, 0.8f), 0.0f);
        if(UnityEngine.Random.Range(0.0f, 1.0f) > 0.5f)
        {
            force *= -1.0f;
        }

        // Initiate block swing
        if(!GameState.DisableRope)
        {
            rigidbodyPiece.AddForce(InitialSwingForce * rigidbodyPiece.mass * force, ForceMode2D.Force);
        }        

        // Start the rope breaking if enabled
        if(enableRopeBreak && !GameState.DisableRope)
        {
            RopeRoutine = RopeBreakingRoutine();
            StartCoroutine(RopeRoutine);        
        }
    }

    public void ReleaseConnectedPiece(){
        if(currentBuildingPiece != null){
            // Break the SpringJoint
            SpringJoint2D joint = currentBuildingPiece.GetComponent<SpringJoint2D>();
            joint.breakForce = 0.0f;
            joint.breakTorque = 0.0f;

            RopeBreakEvent.Invoke();
            currentBuildingPiece = null;

            // Stop the rope breaking, it's already broken
            if(RopeRoutine != null) StopCoroutine(RopeRoutine);

            // Hide line
            if(lineRenderer) lineRenderer.forceRenderingOff = true;
        }
    }

    IEnumerator RopeBreakingRoutine()
    {
        float startTime = Time.realtimeSinceStartup;
        float blinkInterval = blinkStartInterval;
        yield return new WaitForSeconds(timeTillFirstBlink);
        for(float time = startTime; time < startTime + ropeBreakTime; time = Time.realtimeSinceStartup)
        {
            yield return new WaitForSeconds(blinkInterval);
            RopeBlinkEvent.Invoke();
            lineRenderer.startColor = Color.red;
            lineRenderer.endColor = Color.red;
            yield return new WaitForSeconds(blinkActiveTime);
            lineRenderer.startColor = Color.black;
            lineRenderer.endColor = Color.black;
            blinkInterval -= 0.05f;
        }
        if(currentBuildingPiece != null) ReleaseConnectedPiece();
    }
}
