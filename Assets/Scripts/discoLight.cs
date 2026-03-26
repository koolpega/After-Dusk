using UnityEngine;

public class discoLight : MonoBehaviour
{
    private Light _discoLight;

    [SerializeField] private float changeInterval = 0.2f;
    [SerializeField] private float transitionSpeed = 2f;

    private Color _currentColor;
    private Color _targetColor;

    private void Start()
    {
        _discoLight = GetComponent<Light>();

        _currentColor = _discoLight.color;
        _targetColor = GetRandomColor();

        InvokeRepeating(nameof(SetNewTargetColor), 0f, changeInterval);
    }

    private void Update()
    {
        _currentColor = Color.Lerp(
            _currentColor,
            _targetColor,
            transitionSpeed * Time.deltaTime
        );

        _discoLight.color = _currentColor;
    }

    private void SetNewTargetColor()
    {
        _targetColor = GetRandomColor();
    }

    private Color GetRandomColor()
    {
        return new Color(Random.value, Random.value, Random.value);
    }
}