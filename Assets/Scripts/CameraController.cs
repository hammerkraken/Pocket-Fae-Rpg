using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController: MonoBehaviour
{
    [SerializeField]
    public Transform _target;

    private void LateUpdate()
    {
        if (_target != null)
        {
            transform.position = new Vector3(_target.position.x, _target.position.y, -10f);
        }
    }

}
