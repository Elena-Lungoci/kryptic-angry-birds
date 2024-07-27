using System.Collections;
using System.Collections.Generic;
// using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using DG.Tweening;


public class SlingShotHandler : MonoBehaviour
{
    [Header("Line Renderers")]
    public LineRenderer leftLineRenderer;
    public LineRenderer rightLineRenderer;
    [Header("TransformReferences")]
    public Transform leftStartPosition;
    public Transform rightStartPosition;
    public Transform centerPosition;
    public Transform idlePosition;
    [SerializeField] private Transform elasticTransform;
    
    [Header("Slingshot Stats")]
    public float maxDistance;
    public float shotForce=5f;
    [SerializeField] float shotForceMouse=5f;
    [SerializeField] private float timeBetweenBirdResponse = 2f;
    [SerializeField] private float elasticDivider = 1.2f;
    [SerializeField] private AnimationCurve elasticCurve;
    [SerializeField] private float maxAnimationTime = 1f;

    [Header("Scripts")]
    public SlingShotArea slingShotArea;
    [SerializeField] private CameraManager cameraManager;
    [SerializeField] private GetSensorsInput sensorsInput;

    [Header("Bird")]
    public GameObject angryBirdPrefab;
    public float angryBirdPositionOffset = 2f;
    [SerializeField] float BirdStartOffset = 2f;
    [SerializeField] float mouseOffset=2f;
    
    [Header("Sounds")]
    [SerializeField] private AudioClip elasticPulledClip;
    [SerializeField] private AudioClip[] elasticReleasedClips;

    [Header("Sensor Stuff")]
    [SerializeField] private float forceOffset;
    [SerializeField] private float mouseForceOffset;
    [SerializeField] private float orientationOffset;
    [SerializeField] private float releaseForce= 10f;
    public float rotationAngle;

    private AudioSource audioSource;
    bool clickedWithinArea;
    private Vector2 slingShotLinesPosition;
    private Vector2 direction;
    private Vector2 directionNormalized;
    private GameObject SpawnedAngryBird;

    private bool birdOnSlingshot;


    //sensor

    private float prevForce=0f;
    private bool isStretched=false;
    Vector2 directionVector;


    

    // Start is called before the first frame update
    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        leftLineRenderer.enabled = false;
        rightLineRenderer.enabled = false;
        SpawnAngryBird();
    }

    // Update is called once per frame
    void Update()
    {
        // DrawSlingshotSensors();
        // PositionAndRotateAngryBird();

        Debug.Log(sensorsInput.y);

        //sensor version pressed
        if(prevForce < releaseForce && sensorsInput.force>= releaseForce && birdOnSlingshot){
            //was pressed this frame
            isStretched=true;
            if(birdOnSlingshot){
                SoundManager.instance.PLayClip(elasticPulledClip, audioSource);
                cameraManager.SwitchToFollowCam(SpawnedAngryBird.transform);
            }
            

        }
        //mouse version pressed
        if(Mouse.current.leftButton.wasPressedThisFrame && slingShotArea.IsWithinSlingshotArea() && birdOnSlingshot){
            clickedWithinArea = true;
            if(birdOnSlingshot){
                SoundManager.instance.PLayClip(elasticPulledClip, audioSource);
                cameraManager.SwitchToFollowCam(SpawnedAngryBird.transform);
            }
        }

        //sensor version draw
        if(sensorsInput.force >=releaseForce && isStretched&&birdOnSlingshot){
            DrawSlingshotSensors();
            PositionAndRotateAngryBird(angryBirdPositionOffset, -1);
        }
        //mouse version draw

        if(Mouse.current.leftButton.isPressed && clickedWithinArea && birdOnSlingshot && clickedWithinArea){
            DrawSlingshot();
            PositionAndRotateAngryBird(BirdStartOffset, 1);
        }
        //sensor version release
        // if(prevForce>=releaseForce && sensorsInput.force<releaseForce && isStretched){
        if (birdOnSlingshot && sensorsInput.dForce <= -10){
             if(GameManager.instance.HasEnoughShots()){
                isStretched = false;
                 birdOnSlingshot = false;

                SpawnedAngryBird.GetComponent<AngryBird>().LaunchBird(directionVector, shotForce);

                SoundManager.instance.PlayRandomClip(elasticReleasedClips, audioSource);

                GameManager.instance.UseShot(); 
                // SetLines(centerPosition.position);
                AnimateSlingshot();

                if(GameManager.instance.HasEnoughShots()){
                    StartCoroutine(SpawnAngryBirdAfterTime());
                }

            }
        }
        //mouse version release
        if(Mouse.current.leftButton.wasReleasedThisFrame && clickedWithinArea){
            if(GameManager.instance.HasEnoughShots()){
                clickedWithinArea = false;
                 birdOnSlingshot = false;

                SpawnedAngryBird.GetComponent<AngryBird>().LaunchBird(direction, shotForceMouse);

                SoundManager.instance.PlayRandomClip(elasticReleasedClips, audioSource);

                GameManager.instance.UseShot(); 
                // SetLines(centerPosition.position);
                AnimateSlingshot();

                if(GameManager.instance.HasEnoughShots()){
                    StartCoroutine(SpawnAngryBirdAfterTime());
                }

            }

        }
        prevForce=sensorsInput.force;
    }
   

    #region Slingshot Methods;
    private void DrawSlingshot(){
        Vector3 touchPosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        slingShotLinesPosition = centerPosition.position + Vector3.ClampMagnitude(touchPosition - centerPosition.position, maxDistance);
        SetLines(slingShotLinesPosition);

        direction = (Vector2)centerPosition.position - slingShotLinesPosition;
        directionNormalized = direction.normalized; //turns into unit vector
    }

    private void DrawSlingshotSensors(){
        Vector2 straightVector = new Vector2(-1f,0f);
        rotationAngle = sensorsInput.y * Mathf.PI;
        directionVector = RotateZ(straightVector, rotationAngle*-1);
        // Debug.Log(directionVector.x + " " + directionVector.y);
        directionVector *= sensorsInput.force * forceOffset;

        slingShotLinesPosition = centerPosition.position + Vector3.ClampMagnitude((Vector3)directionVector, maxDistance);
        SetLines((Vector2)slingShotLinesPosition);
        directionNormalized = directionVector.normalized;
    }
    private void SetLines(Vector2 position){
        if (!leftLineRenderer.enabled && !rightLineRenderer.enabled){
            leftLineRenderer.enabled = true;
            rightLineRenderer.enabled = true;
        }
        leftLineRenderer.SetPosition(0, position);
        leftLineRenderer.SetPosition(1, leftStartPosition.position);
        rightLineRenderer.SetPosition(0, position);
        rightLineRenderer.SetPosition(1, rightStartPosition.position);
    }

    public static Vector2 RotateZ(Vector2 v, float angle )
    {
        float sin = Mathf.Sin( angle );
        float cos = Mathf.Cos( angle );
        float newX = (cos * v.x) - (sin * v.y);
        float newY = (cos * v.y) + (sin * v.x);
        return new Vector2(newX, newY);
    }
    #endregion

    #region Angry Bird Methods
    private void SpawnAngryBird(){
        elasticTransform.DOComplete();
        SetLines(idlePosition.position);
        Vector2 dir = (centerPosition.position - idlePosition.position).normalized;
        Vector2 spawnPosition = (Vector2)idlePosition.position + dir * BirdStartOffset;
        SpawnedAngryBird = Instantiate(angryBirdPrefab, spawnPosition, Quaternion.identity);
        SpawnedAngryBird.transform.right = dir;
        birdOnSlingshot = true;
    }

    private void PositionAndRotateAngryBird(float offset, float orientation){
        SpawnedAngryBird.transform.position = slingShotLinesPosition + directionNormalized * offset;
        SpawnedAngryBird.transform.right = directionNormalized *orientation;
    }
    private IEnumerator SpawnAngryBirdAfterTime(){ //Coroutine; execute some code after a certain amount of time
        yield return new WaitForSeconds(timeBetweenBirdResponse);
        SpawnAngryBird();
        cameraManager.SwitchToFollowCam(SpawnedAngryBird.transform);
    }

    #endregion

    #region Animate SlingShot
    private void AnimateSlingshot() {
        elasticTransform.position = leftLineRenderer.GetPosition(0);
        float dist = Vector2.Distance(elasticTransform.position, centerPosition.position);

        float time = dist/elasticDivider;
        elasticTransform.DOMove(centerPosition.position, time).SetEase(elasticCurve);
        StartCoroutine(AnimateSlingshotLines(elasticTransform, time));
    }

    private IEnumerator AnimateSlingshotLines(Transform trans, float time){
        float elapsedTime = 0f;
        while(elapsedTime < time && elapsedTime<maxAnimationTime){
            elapsedTime += Time.deltaTime;
            SetLines(trans.position);
            yield return null;
        }
    }
    #endregion
}
