using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIDialogText : MonoBehaviour, IPointerDownHandler, IPointerMoveHandler, IPointerEnterHandler
{
    UIDialog _uiDialog;
    TextMeshProUGUI _dialogTextMesh;

    public Action<string, Vector2> DownCallback = null;

    int _selectionCount;
    int _selectIndex;
    string _mainText;
    List<string> _selectionTextList = new List<string>();


    public void Init(UIDialog dialog)
    {
        _dialogTextMesh = GetComponent<TextMeshProUGUI>();
        _uiDialog = dialog;
    }
    public void SetText(string text)
    {
        _selectionTextList.Clear();

        _mainText = ConvertText(text);
        _dialogTextMesh.text = _mainText;
        _selectionCount = 0;
        _selectIndex = -1;
    }

    string ConvertText(string text)
    {
        StringBuilder sb = new StringBuilder();

        string[] lines = text.Split('{');
        foreach(var line in lines)
        {
            string[] words = line.Split('}');

            if (words.Length == 1)
            {
                sb.Append(words[0]);
            }
            else
            {
                int arg = int.Parse(words[0]);
                // ������ ������Ƽ
                if (arg >= 0 && arg <= 9)
                {
                    sb.Append($"<color=green>{_uiDialog.NPC.ItemDataProperties[arg].ItemName}</color>");

                }
                sb.Append(words[1]);
            }
        }
        return sb.ToString();
    }

    public void AddSelectText(int id,string text)
    {
        _selectionTextList.Add(text);
        _dialogTextMesh.text += $"\n �� <link={id}>{text}</link>";
        _selectionCount++;
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
        else
        {
            if(_selectionCount <= 0)
                _uiDialog.NextDialog();
        }
    }
    public void OnPointerMove(PointerEventData eventData)
    {
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(_dialogTextMesh, Input.mousePosition, _uiDialog.Camera);

        if (_selectIndex != linkIndex)
        {
            Refresh(linkIndex);
            _selectIndex = linkIndex;
        }
    }
    void Refresh(int selection)
    {
        _dialogTextMesh.text = _mainText;
        for(int i =0; i < _selectionCount; i++)
        {
            if(selection == i)
                _dialogTextMesh.text += $"\n ��  <color=\"red\"><link={i + 1}>{_selectionTextList[i]}</link></color>";
            else
                _dialogTextMesh.text += $"\n �� <link={i + 1}>{_selectionTextList[i]}</link>";
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(_dialogTextMesh, Input.mousePosition, _uiDialog.Camera);

        if (_selectIndex != linkIndex)
        {
            Refresh(linkIndex);
            _selectIndex = linkIndex;
        }
    }
}
