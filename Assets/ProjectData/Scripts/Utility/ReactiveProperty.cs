using System;

public class ReactiveProperty<T>
{
    private T _value;

    public event Action<T> OnValueChanged;

    public void SetValue(T value)
    {
        if (_value.ToString() == value.ToString()) return;
        _value = value;
        OnValueChanged?.Invoke(value);
    }

    public T GetValue() => _value;
}
