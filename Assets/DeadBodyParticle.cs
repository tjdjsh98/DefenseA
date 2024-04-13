using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeadBodyParticle : MonoBehaviour
{
    SpriteRenderer _spriteRenderer;
    Rigidbody2D _rigidBody;

    [SerializeField] float _time = 5;
    float _elasepdTime;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _rigidBody = GetComponent<Rigidbody2D>();
    }
    private void OnEnable()
    {
        if (_spriteRenderer)
            _spriteRenderer.color = new Color(1, 1, 1, 1);
    }
    void Update()
    {
        _elasepdTime += Time.deltaTime;

        if (_elasepdTime > _time)
        {
            if (_spriteRenderer)
            {
                float alpha = _time - _elasepdTime;
                if (alpha < 2)
                {
                    _spriteRenderer.color = new Color(1, 1, 1, alpha / 2);
                }
            }
            _elasepdTime = 0;
            Managers.GetManager<ResourceManager>().Destroy(gameObject);
        }
        if (_rigidBody)
        {
            float breakPower = 60;
            Vector3 speed = _rigidBody.velocity;
            if (speed.y <= 0.1f && Mathf.Abs(speed.x) > 0.1f)
            {
                if (speed.x > 0)
                {
                    if (speed.x - Time.deltaTime * breakPower < 0)
                    {
                        speed.x = 0;
                    }
                    else
                    {
                        speed.x -= Time.deltaTime * breakPower;
                    }
                }
                else
                {
                    if (speed.x + Time.deltaTime * breakPower > 0)
                    {
                        speed.x = 0;
                    }
                    else
                    {
                        speed.x += Time.deltaTime * breakPower;
                    }
                }
            }

            _rigidBody.velocity = speed;
        }
    }
}
