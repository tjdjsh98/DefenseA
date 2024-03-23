using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIDialogText : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    UIDialog _uiDialog;
    TextMeshProUGUI _dialogTextMesh;

    public Action<string, Vector2> DownCallback = null;

    private void Awake()
    {
        _dialogTextMesh = GetComponent<TextMeshProUGUI>();    
    }
    public void Init(UIDialog dialog )
    {
        _uiDialog= dialog;
    }
    public void SetText(string text)
    {
        _dialogTextMesh.text = text;
    }

    public void AddSelectText(int id,string text)
    {
        _dialogTextMesh.text += $"\n<link={id}>{text}</link>";
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        // ��ġ�� ������ - URL �����۸�ũ ����� �����ؼ� ����߽��ϴ�.
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(_dialogTextMesh, Input.mousePosition, _uiDialog.Camera);

        if (linkIndex != -1)
        {
            TMP_LinkInfo linkInfo = _dialogTextMesh.textInfo.linkInfo[linkIndex];
            // ��ũ index�� �����Ҷ� ��ġ ��� - ���� ��ų ������ ��������ϹǷ� ���Ƿ� �ۼ��� ID���� ��ġ�� ��ġ ����
            this.DownCallback(linkInfo.GetLinkID(), Input.mousePosition);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        
    }
}
