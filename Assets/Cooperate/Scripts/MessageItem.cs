using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class MessageItem : MonoBehaviour
{
    public Text messageText;
    public CanvasGroup canvasGroup;

    public void ShowMessage(string value,float deleteTime = 5) {
        messageText.DOText(value, 1);
        Invoke("DestoryMessage", deleteTime);
    }

    private void DestoryMessage() {
        canvasGroup.DOFade(0, 2);
        Destroy(gameObject, 2);
    }
}
