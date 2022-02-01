using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveRightScript : MonoBehaviour
{
    public AnimationCurve position;
    Vector3 startPosVector;
    public float speed;

    // Start is called before the first frame update
    void Start()
    {
        startPosVector = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = startPosVector + new Vector3(position.Evaluate((Time.time * speed) % 1), 0, 0);
    }
}
