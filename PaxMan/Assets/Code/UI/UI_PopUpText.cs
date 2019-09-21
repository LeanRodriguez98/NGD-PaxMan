using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_PopUpText : MonoBehaviour
{
    private TextMesh textMesh;
    public void DisplayText(string _text ,Vector2 _position, float _timeOfLife)
    {
        textMesh = GetComponent<TextMesh>();
        textMesh.GetComponent<Renderer>().sortingOrder = int.MaxValue;
        gameObject.transform.position = _position;
        textMesh.text = _text;
        Destroy(gameObject, _timeOfLife);
    }
}
