using System;

public class ReactiveProperty<T>
{
    private T _value;

    public event Action<T> OnValueChanged;

    public ReactiveProperty(T value)
    {
        _value = value;
    }

    public T Value 
    { 
        get 
        { 
            return _value; 
        }
        set 
        { 
            if (_value.ToString() == value.ToString()) return;
            _value = value;
            OnValueChanged?.Invoke(value);
        } 
    }
}
