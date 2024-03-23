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
        // 터치가 됐을때 - URL 하이퍼링크 기능을 개조해서 사용했습니다.
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(_dialogTextMesh, Input.mousePosition, _uiDialog.Camera);

        if (linkIndex != -1)
        {
            TMP_LinkInfo linkInfo = _dialogTextMesh.textInfo.linkInfo[linkIndex];
            // 링크 index가 존재할때 터치 기능 - 보통 스킬 정보를 보내줘야하므로 임의로 작성한 ID값과 터치한 위치 전달
            this.DownCallback(linkInfo.GetLinkID(), Input.mousePosition);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        
    }
}
