using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Overrideratk : MonoBehaviour
{
    [SerializeField] private float coreOffsetx;
    [SerializeField] private float coreOffsety;
    public GameObject bullet1;

    private Vector2 coreOffset;
    private Vector2 fireDir;
    private BoxCollider2D box;
    private void Start()
    {
        box = GetComponent<BoxCollider2D>();
    }
    private void Update()
    {
        if( Input.GetKeyDown(KeyCode.J) )
        {
            Fire();
        }
    }

    private void Fire()
    {
        fireDir = this.transform.forward;
        coreOffset = (Vector2)box.bounds.center + new Vector2( fireDir.x * box.bounds.extents.x, fireDir.y * box.bounds.extents.y);
        GameObject bullet = Instantiate(bullet1,coreOffset,Quaternion.identity);
    }
}
