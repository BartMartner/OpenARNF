using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleBlockGroupSequence : MonoBehaviour
{
    public float interval = 1.616667f;

    private ToggleBlockGroup[] _blockGroups;
    private int _currentIndex;
    private float _timer;

    public void Awake()
    {
        _blockGroups = GetComponentsInChildren<ToggleBlockGroup>();
        for (int i = 0; i < _blockGroups.Length; i++)
        {
            if(i != 0)
            {
                _blockGroups[i].startHidden = true;
            }
        }
    }

    public void Update()
    {
        _timer += Time.deltaTime;
        if(_timer > interval)
        {
            _timer -= interval;
            _currentIndex = (_currentIndex + 1) % _blockGroups.Length;
            for (int i = 0; i < _blockGroups.Length; i++)
            {
                var group = _blockGroups[i];
                if(i == _currentIndex)
                {
                    group.Show();
                }
                else
                {
                    group.Hide();
                }
            }
        }
    }
}
