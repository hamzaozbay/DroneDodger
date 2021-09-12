using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Diamond : MonoBehaviour {

    [SerializeField] private GameObject _diamondModel;
    [SerializeField] private ParticleSystem _explodeParticle;


    private void Awake() {
        _explodeParticle.Stop();
    }


    private void OnTriggerEnter(Collider other) {
        if (other.gameObject.CompareTag("Player")) {
            _diamondModel.SetActive(false);
            _explodeParticle.Play();
            StartCoroutine(WaitForDestroy());
            GameManager.instance.Score(10);
        }
    }

    private IEnumerator WaitForDestroy() {
        yield return new WaitForSeconds(_explodeParticle.main.duration);
        Destroy(this.gameObject);
    }
}
