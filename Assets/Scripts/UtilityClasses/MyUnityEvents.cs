using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class IntEvent : UnityEvent<int> { }

[System.Serializable]
public class FloatEvent : UnityEvent<float> { }

[System.Serializable]
public class EnemyEvent : UnityEvent<Enemy> { }

[System.Serializable]
public class PlayerEvent : UnityEvent<Player> { }