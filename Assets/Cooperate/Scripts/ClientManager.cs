using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClientManager : MonoBehaviour
{
    public InputField input_name;

    private void Start() {
        RandomName(input_name.text);

        input_name.onEndEdit.AddListener(value => {
            RandomName(value);
        });
    }

    private void RandomName(string value) {
        if (value == "" || value == null) {
            string randomName = "User" + Random.Range(1000, 9999);
            GlobalData.instance.ownerPlayerName = randomName;
            input_name.text = randomName;
        }
        else {
            GlobalData.instance.ownerPlayerName = value;
        }
    }
}
