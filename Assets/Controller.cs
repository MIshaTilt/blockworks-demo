using UnityEngine;

public class Controller : MonoBehaviour
{
    [SerializeField] private float speed = 1;

    private void Update()
    {
        var dir = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")).normalized;
        transform.position += dir * (speed * Time.deltaTime);
    }
}