using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NextSceneLoader : MonoBehaviour
{
    [SerializeField] bool _debug;
    [SerializeField]Define.Range _trigger;
    [SerializeField] MapData _nextMapData;
    [SerializeField] bool _end;

    private void OnDrawGizmosSelected()
    {
        if (!_debug) return;

        Util.DrawRangeOnGizmos(gameObject, _trigger, Color.green);
    }

    private void Awake()
    {
        StartCoroutine(CorCheckTrigger());
    }
    IEnumerator CorCheckTrigger()
    {
        while (true)
        {
            yield return new WaitForSeconds(1);

            if (Managers.GetManager<GameManager>().IsLoadEnd)
            {
                Util.RangeCastAll2D(gameObject, _trigger, Define.CharacterMask, (go) =>
                {
                    if (!_end)
                    {
                        if (go == Managers.GetManager<GameManager>().Wall.gameObject)
                        {
                            Managers.GetManager<GameManager>().LoadScene(_nextMapData);
                            _end = true;
                        }
                    }
                    return false;
                });

                if (_end) break;
            }
        }
    }
}
