using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class LockPickMinigame : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private GameObject innerLock;
    [SerializeField] private GameObject outerLock;
    [SerializeField] private GameObject pinLock;
    [SerializeField] private float innerLockRotationSpeed;
    [SerializeField] private float pinSpeed;
    [SerializeField] [Range(1, 25)] private float unlockRange; //range for unlocking (difficulty) in degrees

    private Quaternion innerLockStartRotation;
    private Quaternion pinLockStartRotation;
    private float maxAngle = 90f;
    private float unlockAngle;
    private bool canMovePin = true;
    private bool canPlay = true;
    private bool unlocked = false;
    private float pinAngle; //angle between vector3.up and direction of pin


    // Start is called before the first frame update
    void Start()
    {
        NewMinigame();
        pinLock.GetComponent<Animator>().enabled = false;
        innerLockStartRotation = innerLock.transform.rotation;
        pinLockStartRotation = pinLock.transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        CheckInput();
        RotatePin();
        RotateInnerLock();
    }

    private void CheckInput()
    {
        
        if (Input.GetKeyDown(KeyCode.D))
        {
            canMovePin = false;
        }

        if (Input.GetKeyUp(KeyCode.D))
        {
            canMovePin = true;
            if (!unlocked)
                innerLock.transform.rotation = innerLockStartRotation;
        }
    }

    private void RotateInnerLock()
    {
        if (!canMovePin && canPlay)
        {
            float pinZ = pinLock.transform.localEulerAngles.z;
            float innerLockZ = innerLock.transform.localEulerAngles.z;
            pinZ = pinZ > 180 ? pinZ - 360 : pinZ;
            innerLockZ = innerLockZ > 180 ? innerLockZ - 360 : innerLockZ;

            if ((Mathf.Abs(unlockAngle - pinZ) < unlockRange))
            {
                if(innerLockZ < maxAngle && innerLockZ > -maxAngle)
                {
                    Debug.Log("Unlocking...");
                    innerLock.transform.Rotate(0f, 0f, -innerLockRotationSpeed * Time.deltaTime);
                }
                else
                {
                    unlocked = true;
                    StartCoroutine(ResetMinigame());                   
                }
                
            }

        }
    }

    private void RotatePin()
    {
        if(canMovePin && canPlay)
        {
            Vector3 worldPosition;
            Vector3 mousePos = Input.mousePosition;
            worldPosition = cam.ScreenToWorldPoint(mousePos);
            worldPosition.z = pinLock.transform.position.z;
            Vector2 dir = (worldPosition - pinLock.transform.position).normalized;

            pinAngle = Vector3.Angle(dir, Vector3.up);
            pinAngle = Mathf.Clamp(pinAngle, -maxAngle, maxAngle);

            float angle = Mathf.Atan2(dir.x, dir.y) * Mathf.Rad2Deg;
            angle = Mathf.Clamp(angle, -maxAngle, maxAngle);
            Quaternion rotation = Quaternion.AngleAxis(-angle, Vector3.forward);

            pinLock.transform.rotation = Quaternion.Slerp(pinLock.transform.rotation, rotation, pinSpeed * Time.deltaTime);
        }

    }

    private void NewMinigame()
    {
        unlocked = false;
        unlockAngle = Random.Range(-maxAngle + unlockRange, maxAngle + unlockRange);
        Debug.Log(unlockAngle);
    }

    private void OnDrawGizmos()
    {
        Vector3 worldPosition;
        Vector3 mousePos = Input.mousePosition;
        worldPosition = cam.ScreenToWorldPoint(mousePos);
        worldPosition.z = pinLock.transform.position.z;
        Vector3 dir = (worldPosition - pinLock.transform.position).normalized;


        Gizmos.color = Color.red;
        Gizmos.DrawRay(pinLock.transform.position, dir * 10f);
        Gizmos.color = Color.green;
        Gizmos.DrawRay(pinLock.transform.position, pinLock.transform.up * 5f);
        Gizmos.DrawWireSphere(worldPosition, 1f);
        Handles.DrawWireArc(outerLock.transform.position, -outerLock.transform.forward, -outerLock.transform.right, 180f, 3f);
    }

    private IEnumerator ResetMinigame()
    {
        canPlay = false;
        NewMinigame();
        yield return new WaitForSeconds(1f);
        Debug.Log("Reset minigame");
        innerLock.transform.rotation = innerLockStartRotation;
        pinLock.transform.rotation = pinLockStartRotation;     
        yield return new WaitForSeconds(0.3f);
        canPlay = true;
    }

}
