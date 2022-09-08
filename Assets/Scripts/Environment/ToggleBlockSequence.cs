using UnityEngine;

public class ToggleBlockSequence : MonoBehaviour
{
    public float interval = 1.616667f;
    public bool pingPong;
    public bool noRepeatPingPong;
    public int quantity = 1;

    private ToggleBlock[] _blocks;    
    private float _timer;
    private int _minIndex;
    private int _maxIndex;
    private int mod = 1;

    public void Start()
    {
        _blocks = GetComponentsInChildren<ToggleBlock>();
        _minIndex = 0;
        _maxIndex = quantity;

        for (int i = 0; i < _blocks.Length; i++)
        {
            if(i > _maxIndex)
            {
                _blocks[i].HideImmediate();
            }
        }
    }

    public void Update()
    {
        _timer += Time.deltaTime;
        if(_timer > interval)
        {
            _timer -= interval;
            _minIndex = (_minIndex + mod) % _blocks.Length;
            _maxIndex = (_maxIndex + mod) % _blocks.Length;

            if (pingPong)
            {
                if (mod == 1 && _maxIndex == 0)
                {
                    mod = -1;
                    _maxIndex = _blocks.Length - 1;
                    _minIndex = _blocks.Length - 1 - quantity;
                    if(noRepeatPingPong)
                    {
                        _timer += interval;
                    }
                    return;
                }
                else if( mod == -1 && _minIndex < 0)
                {
                    mod = 1;
                    _minIndex = 0;
                    _maxIndex = quantity;
                    if (noRepeatPingPong)
                    {
                        _timer += interval;
                    }
                    return;
                }
            }

            for (int i = 0; i < _blocks.Length; i++)
            {
                var block = _blocks[i];
                if(i >= _minIndex && (_maxIndex < _minIndex || i <= _maxIndex))
                {
                    block.Show();
                }
                else
                {
                    block.Hide();
                }
            }
        }
    }
}
