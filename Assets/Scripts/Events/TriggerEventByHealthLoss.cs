using UnityEngine;
using System.Collections;
using UnityEngine.Events;

[RequireComponent(typeof(Damageable))]
public class TriggerEventByHealthLoss : MonoBehaviour
{
    public float eventWarmUp;
    public UnityEvent onEventWarmUp;

    public UnityEvent eventToTrigger;
    public bool onlyOnce;
    private Damageable _damageable;
    public float _healthLastTriggered;
    public FXType fxType;
    public float healthTheshold;

    public void Start()
    {
        _damageable = GetComponent<Damageable>();
        _damageable.onHurt.AddListener(OnHurt);
        _healthLastTriggered = _damageable.health;
    }

    private void OnHurt()
    {
        if(!enabled)
        {
            return;
        }

        var healthDelta = _healthLastTriggered - Mathf.Clamp(_damageable.health, 0, _damageable.maxHealth);
        if (healthDelta >= healthTheshold)
        {
            var roundedDelta = Mathf.Floor((_damageable.maxHealth - _damageable.health) / healthTheshold) * healthTheshold;
            _healthLastTriggered = _damageable.maxHealth - roundedDelta;

            if (onlyOnce)
            {
                StartCoroutine(TriggerEvent());
            }
            else
            {
                var timesToTrigger = Mathf.FloorToInt(healthDelta / healthTheshold);
                for (int i = 0; i < timesToTrigger; i++)
                {
                    StartCoroutine(TriggerEvent());
                }
            }
        }
    }

    public IEnumerator TriggerEvent()
    {
        if (onEventWarmUp != null)
        {
            onEventWarmUp.Invoke();
        }

        if (eventWarmUp > 0)
        {
            yield return new WaitForSeconds(eventWarmUp);
        }

        if (eventToTrigger != null)
        {
            eventToTrigger.Invoke();
        }

        if (fxType != FXType.None)
        {
            FXManager.instance.SpawnFX(fxType, transform.position);
        }

        if (onlyOnce)
        {
            Destroy(this);
        }
    }


    public float GetHealthPercentage()
    {
        if (!_damageable)
        {
            _damageable = GetComponent<Damageable>();
        }

        return healthTheshold / _damageable.maxHealth;
    }

    public void SetHealthPercentage(float amount)
    {
        if (!_damageable)
        {
            _damageable = GetComponent<Damageable>();
        }

        healthTheshold = _damageable.maxHealth * amount;
    }
}