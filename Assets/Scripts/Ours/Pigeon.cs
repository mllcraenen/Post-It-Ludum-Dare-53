using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEditor;
using UnityEngine.U2D.Animation;

public class Pigeon : MonoBehaviour {
    private Vector3 mPos;
    private Rigidbody2D rb;
    private Vector3 wind;
    private bool isGrounded = false;
    private bool isFlapping = false;

    [Header("Flight settings")]
    public AnimationCurve forceCurve;
    public float flapForce = 200;
    public float flapDuration = .3f;
    public float rotationSpeed = 5f;
    public float flapVectorAngle = -45f;
    private Vector3 flapVector;

    public float maxSpeed = 5f;
    public float liftCoefficient = 1f;


    [Header("Tiling settings")]
    public int collisionLayer = 0;
    private int upperBound = 0;
    public GameObject grid;

    [Header("Sprite settings")]
    public Sprite[] sprites;
    private SpriteRenderer[] bodyRenderers;
    private SpriteResolver bodySpriteResolver;
    private SpriteResolver letterSpriteResolver;
    private Transform headTransform;



    // Use this for initialization
    void Start() {
        rb = GetComponent<Rigidbody2D>();
        upperBound = grid.GetComponent<Collision>().layers.Length;

        bodyRenderers = transform.Find("PIGEON_BODIES").gameObject.GetComponentsInChildren<SpriteRenderer>();
        bodySpriteResolver = transform.Find("PIGEON_BODIES").Find("BODY").gameObject.GetComponent<SpriteResolver>();
        headTransform = transform.Find("PIGEON_BODIES").Find("HEADBONE");
        spriteTakeOff();
        //letterSpriteResolver = transform.Find("PIGEON_BODIES").Find("LETTER").gameObject.GetComponent<SpriteResolver>();
    }

    void Update() {
        keyInputs();
        layerColour();
        rotatePigeon();

        // Clamp max speed
        rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxSpeed);

        // Add lift TODO:: Make this not suck
        rb.AddForce(transform.up * liftCoefficient);

        // Add wind if present
        rb.AddForce(wind);
    }

    void rotatePigeon() {
        if(!isGrounded) {
            //rotate body to mouse
            mPos = Input.mousePosition;
            transform.rotation = Quaternion.Euler(0, 0, Mathf.Lerp(-90, 90, Mathf.InverseLerp(0, Screen.height, mPos.y)));
            ////rotate head to right
            //Vector3 targetVector = transform.position + new Vector3(1f, 0f, 0f);
            //headTransform.LookAt(targetVector);
        } 
        //else {
        //    //rotate head to mouse
        //    mPos = Input.mousePosition;
        //    headTransform.rotation = Quaternion.Euler(0, 0, Mathf.Lerp(-90, 90, Mathf.InverseLerp(0, Screen.height, mPos.y)));
        //}


        if (!isFlapping) {
            if(Vector3.Dot(transform.right, Vector3.down) > 0.3f) {
                bodySpriteResolver.SetCategoryAndLabel("pigeonBody", "dive");

            } else bodySpriteResolver.SetCategoryAndLabel("pigeonBody", "glide");
        } 
    }

    void keyInputs() {
        // Flap
        if (Input.GetMouseButtonDown(0))
            Flap();
        // Move to a layer up
        if (Input.GetKeyDown(KeyCode.W) && collisionLayer + 1 < upperBound)
            collisionLayer++;

        // Move to a layer down 
        if (Input.GetKeyDown(KeyCode.S) && collisionLayer > 0)
            collisionLayer--;
    }

    void layerColour() {
        Color color = grid.GetComponent<Collision>().layers[collisionLayer].GetComponent<Tilemap>().color;
        //spriteRenderer.color = color;
    }

    public void Flap() {
        flapVector = Quaternion.AngleAxis(flapVectorAngle, Vector3.forward) * transform.up;
        StartCoroutine(FlapCoroutine(forceCurve));
    }

    private IEnumerator FlapCoroutine(AnimationCurve forceCurve) {
        isFlapping = true;
        bodySpriteResolver.SetCategoryAndLabel("pigeonBody", "flap");
        float timer = 0f;
        float maxForce = flapForce;
        float currentForce = 0f;
        while (timer < flapDuration) {
            currentForce = forceCurve.Evaluate(timer / flapDuration) * maxForce;
            rb.AddForce(flapVector * currentForce);
            timer += Time.deltaTime;
            yield return null;
        }
        bodySpriteResolver.SetCategoryAndLabel("pigeonBody", "glide");
        isFlapping = false;
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if(collision.gameObject.tag == "Wind") {
            Wind collidedWind = collision.gameObject.GetComponent<Wind>();
            wind += collidedWind.transform.right * collidedWind.strength;
        }
    }
    private void OnTriggerExit2D(Collider2D collision) {
        if (collision.gameObject.tag == "Wind") {
            Wind collidedWind = collision.gameObject.GetComponent<Wind>();
            wind -= collidedWind.transform.right * collidedWind.strength;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.tag == "Floor") {
            isGrounded = true;
            spriteGoSit();
		}
    }
    private void OnCollisionExit2D(Collision2D collision) {
        if (collision.gameObject.tag == "Floor") {
            isGrounded = false;
            spriteTakeOff();
        }
    }

    private void spriteGoSit() {
        foreach (SpriteRenderer i in bodyRenderers) {
            if (i.gameObject.name == "PIGEON_SIT")
                i.enabled = true;
            else i.enabled = false;
        }
    }
    private void spriteTakeOff() {
        foreach (SpriteRenderer i in bodyRenderers) {
            if (i.gameObject.name != "PIGEON_SIT")
                i.enabled = true;
            else i.enabled = false;
		}
	}

	private void OnDrawGizmos() {
        Debug.DrawRay(transform.position, transform.right, Color.blue);
    }
}