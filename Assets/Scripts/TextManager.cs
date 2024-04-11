using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TextManager : ManagerBase
{
    List<TextMeshPro> _textList= new List<TextMeshPro>();
    GameObject _textFolder;

    public override void Destroy()
    {
    }

    public override void Init()
    {
        _textFolder = new GameObject("TextContainer");
        _textFolder.transform.SetParent(transform);
    }

    public override void ManagerUpdate()
    {
    }

    public void ShowText(Vector3 position, string text, float fontSize, Color fontColor)
    {
        TextMeshPro textMesh = null;

        foreach(var t in _textList)
        {
            if (!t.gameObject.activeSelf)
            {
                textMesh = t;
                break;
            }
        }

        if (textMesh == null)
        {
            textMesh = Managers.GetManager<ResourceManager>().Instantiate("Text").GetComponent<TextMeshPro>();
            textMesh.transform.SetParent(_textFolder.transform);
            _textList.Add(textMesh);
        }

        textMesh.text = text;
        textMesh.fontSize = fontSize;
        textMesh.color = fontColor;
        textMesh.transform.position = position;
        textMesh.gameObject.SetActive(true);

        StartCoroutine(CorTextMeshUp(textMesh));
    }
  
    IEnumerator CorTextMeshUp(TextMeshPro textMesh)
    {
        Vector3 initPos = Vector3.zero;
        if (textMesh)
            initPos = textMesh.transform.position;

        for(int i =1 ; i <= 60; i++)
        {
            if(textMesh)
                textMesh.transform.position += Vector3.up * Time.fixedDeltaTime;

            yield return new WaitForFixedUpdate();
        }
        if (textMesh)
            textMesh.gameObject.SetActive(false);
    }
}
