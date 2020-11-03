using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Letter : MonoBehaviour
{
    public Text letterText;
    public Image letterBg;
    public Animation animation;

    public void Appear()
    {
        animation.Play();
    }
}
