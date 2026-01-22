namespace DataFileReader.Class.JSON;

public class JsonValue : IJsonPrimitive, IConvertible
{
    public TypeCode GetTypeCode()
    {
        return TypeCode;
    }

    public bool ToBoolean(IFormatProvider? provider)
    {
        return Convert.ToBoolean(Value, provider);
    }

    public byte ToByte(IFormatProvider? provider)
    {
        return Convert.ToByte(Value, provider);
    }

    public char ToChar(IFormatProvider? provider)
    {
        return Convert.ToChar(Value, provider);
    }

    public DateTime ToDateTime(IFormatProvider? provider)
    {
        return Convert.ToDateTime(Value, provider);
    }

    public decimal ToDecimal(IFormatProvider? provider)
    {
        return Convert.ToDecimal(Value, provider);
    }

    public double ToDouble(IFormatProvider? provider)
    {
        return Convert.ToDouble(Value, provider);
    }

    public short ToInt16(IFormatProvider? provider)
    {
        return Convert.ToInt16(Value, provider);
    }

    public int ToInt32(IFormatProvider? provider)
    {
        return Convert.ToInt32(Value, provider);
    }

    public long ToInt64(IFormatProvider? provider)
    {
        return Convert.ToInt64(Value, provider);
    }

    public sbyte ToSByte(IFormatProvider? provider)
    {
        return Convert.ToSByte(Value, provider);
    }

    public float ToSingle(IFormatProvider? provider)
    {
        return Convert.ToSingle(Value, provider);
    }

    public string ToString(IFormatProvider? provider)
    {
        return Convert.ToString(Value, provider)!;
    }

    public object ToType(Type conversionType, IFormatProvider? provider)
    {
        return Value == null ? null! : Convert.ChangeType(Value, conversionType, provider)!;
    }

    public ushort ToUInt16(IFormatProvider? provider)
    {
        return Convert.ToUInt16(Value, provider);
    }

    public uint ToUInt32(IFormatProvider? provider)
    {
        return Convert.ToUInt32(Value, provider);
    }

    public ulong ToUInt64(IFormatProvider? provider)
    {
        return Convert.ToUInt64(Value, provider);
    }

    public TypeCode TypeCode { get; internal set; }
    public Type Type { get; internal set; } = typeof(void);
    public string? Name { get; set; }
    public IJson? Parent { get; set; }
    public object? Value { get; set; }
    public bool IsArray => false;
    public bool IsObject => false;
    public bool IsValue => true;
    public bool IsEmpty => Value is string text ? string.IsNullOrEmpty(text) : Value == null;

    public IJson As(string rename)
    {
        return new JsonValue
        {
                TypeCode = TypeCode,
                Type = Type,
                Name = rename,
                Value = Value
        };
    }

    internal static JsonValue? Create(string? name, object? value)
    {
        if (value is JsonValue json)
            return new JsonValue
            {
                    TypeCode = json.TypeCode,
                    Type = json.Type,
                    Name = name,
                    Value = json.Value
            };
        return Create(name, value?.GetType() ?? typeof(void), value);
    }

    internal static JsonValue? Create(string? name, Type? type, object? value)
    {
        var typeCode = TypeCode.Empty;
        if (type == null && string.IsNullOrWhiteSpace(name) && value == null)
            return null;

        if (Types.BOOL.Equals(type))
            typeCode = TypeCode.Boolean;
        else if (Types.BYTE.Equals(type))
            typeCode = TypeCode.Byte;
        else if (Types.CHAR.Equals(type))
            typeCode = TypeCode.Char;
        else if (Types.DATETIME.Equals(type))
            typeCode = TypeCode.DateTime;
        else if (Types.DECIMAL.Equals(type))
            typeCode = TypeCode.Decimal;
        else if (Types.DOUBLE.Equals(type))
            typeCode = TypeCode.Double;
        else if (Types.SHORT.Equals(type))
            typeCode = TypeCode.Int16;
        else if (Types.INT.Equals(type))
            typeCode = TypeCode.Int32;
        else if (Types.LONG.Equals(type))
            typeCode = TypeCode.Int64;
        else if (Types.SBYTE.Equals(type))
            typeCode = TypeCode.SByte;
        else if (Types.FLOAT.Equals(type))
            typeCode = TypeCode.Single;
        else if (Types.STRING.Equals(type))
            typeCode = TypeCode.String;
        else if (Types.USHORT.Equals(type))
            typeCode = TypeCode.UInt16;
        else if (Types.UINT.Equals(type))
            typeCode = TypeCode.UInt32;
        else if (Types.ULONG.Equals(type))
            typeCode = TypeCode.UInt64;
        else if (Types.BYTE_ARR.Equals(type))
            typeCode = (TypeCode)20;

        return new JsonValue
        {
                TypeCode = typeCode,
                Type = type ?? typeof(void),
                Name = name,
                Value = value
        };
    }

    public static explicit operator bool(JsonValue value)
    {
        if (value == null || value.Value == null || value.Value.Equals(0) || value.Value.Equals(false) || value.Value.Equals("false") || value.Value.Equals("no") || value.Value.Equals("") || value.Value.Equals("0"))
            return false;
        if (value.Value.Equals(1) || value.Value.Equals(true) || value.Value.Equals("true") || value.Value.Equals("yes") || value.Value.Equals("1"))
            return true;
        return Convert.ToBoolean(value.Value);
    }

    public static explicit operator byte(JsonValue value)
    {
        return Convert.ToByte(value.Value);
    }

    public static explicit operator char(JsonValue value)
    {
        return Convert.ToChar(value.Value);
    }

    public static explicit operator DateTime(JsonValue value)
    {
        return Convert.ToDateTime(value.Value);
    }

    public static explicit operator decimal(JsonValue value)
    {
        return Convert.ToDecimal(value.Value);
    }

    public static explicit operator double(JsonValue value)
    {
        return Convert.ToDouble(value.Value);
    }

    public static explicit operator short(JsonValue value)
    {
        return Convert.ToInt16(value.Value);
    }

    public static explicit operator int(JsonValue value)
    {
        return Convert.ToInt32(value.Value);
    }

    public static explicit operator long(JsonValue value)
    {
        return Convert.ToInt64(value.Value);
    }

    public static explicit operator sbyte(JsonValue value)
    {
        return Convert.ToSByte(value.Value);
    }

    public static explicit operator float(JsonValue value)
    {
        return Convert.ToSingle(value.Value);
    }

    public override string ToString()
    {
        return Value?.ToString()!;
    }

    public static explicit operator string(JsonValue value)
    {
        return Convert.ToString(value.Value)!;
    }

    public static explicit operator ushort(JsonValue value)
    {
        return Convert.ToUInt16(value.Value);
    }

    public static explicit operator uint(JsonValue value)
    {
        return Convert.ToUInt32(value.Value);
    }

    public static explicit operator ulong(JsonValue value)
    {
        return Convert.ToUInt64(value.Value);
    }

    public byte[] ToByteArray()
    {
        return Value is string text ? Convert.FromBase64String(text) : (byte[])Value!;
    }

    public byte[] ToByteArray(IFormatProvider? provider)
    {
        return Value is string text ? Convert.FromBase64String(text) : (byte[])Value!;
    }

    public static explicit operator byte[](JsonValue value)
    {
        return value.Value is string text ? Convert.FromBase64String(text) : (byte[])value.Value!;
    }
}