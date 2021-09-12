using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : MonoBehaviour {

    [SerializeField] private float _speed = 3f;



    private void Update() {
        if (transform.position.y <= -5f) transform.position = new Vector3(0f, 0f, 20f);

        transform.position += -transform.forward * _speed * Time.deltaTime;
    }


}
