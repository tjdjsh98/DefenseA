using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Orbital : MonoBehaviour
{
    public int _count;

    [SerializeField] LineRenderer _lineRenderer;
    public Transform centerObject; // �߽� õü (��: �¾�)
    public float semiMajorAxis = 5f; // ������
    public float semiMinorAxis = 3f; // �ݴ���
    public float orbitSpeed = 1f; // �˵� �ӵ�

    [ContextMenu("DrawLine")]
    void DrawLine()
    {
        _lineRenderer.positionCount = _count+1;

        int index = 0;
        // Ÿ�� �˵��� ���� ȸ��
        for (float angle = 0; angle <= 360; angle += 360/_count)
        {
            float x = centerObject.position.x + semiMajorAxis * Mathf.Cos(angle*Mathf.Deg2Rad);
            float y = centerObject.position.y + semiMinorAxis * Mathf.Sin(angle*Mathf.Deg2Rad);

            _lineRenderer.SetPosition(index++,new Vector3(x, y, 0));
        }
    }
}
