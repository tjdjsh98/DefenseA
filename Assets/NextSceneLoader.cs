using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NextSceneLoader : MonoBehaviour
{
    [SerializeField] bool _debug;
    [SerializeField]Define.Range _trigger;
    [SerializeField] string _nextScene;

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
            Util.RangeCastAll2D(gameObject, _trigger, Define.CharacterMask, (go) =>
            {
                if (go == Managers.GetManager<GameManager>().Wall.gameObject)
                {
                    SceneManager.LoadScene(_nextScene);
                }
                return false;
            });

            yield return new WaitForSeconds(1);
        }
    }
}
