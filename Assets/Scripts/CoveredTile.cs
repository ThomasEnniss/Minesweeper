using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoveredTile : MonoBehaviour {

    public delegate void Action(Vector2 position);
    public event Action OnClick;

    private void OnMouseDown()
    {
        if (OnClick != null) { OnClick(new Vector2(transform.position.x, transform.position.y)); }
        Debug.Log("Clicked on a tile @ "+transform.position.ToString());
    }
}
