using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameDevKit
{
    public interface IMaterialPropertyReadWrite<TProperty>
    {
        int PropertyId { get; }
        TProperty Get(Material material);
        void Set(Material material, TProperty value);
    }

    /// <summary> Shader.PropertyToID wrapper for better context and type-safety </summary>
    public struct ShaderProperty
    {
        public int PropertyId { get; private set; }

        public ShaderProperty(string propertyName) => PropertyId = Shader.PropertyToID(propertyName);

        public static implicit operator ShaderProperty(int propertyId) => new() { PropertyId = propertyId };
        public static implicit operator ShaderProperty(string propertyName) => new(propertyName);
    }

    [Serializable]
    public struct SerializableShaderProperty
    {
        [SerializeField] private string _propertyName;

        private int? _propertyId;
        public int PropertyId => _propertyId ??= Shader.PropertyToID(_propertyName);

#if UNITY_EDITOR
        internal static class EditorProps
        {
            public static string PropertyName => nameof(_propertyName);
        }
#endif

        public SerializableShaderProperty(string propertyName)
        {
            _propertyName = propertyName;
            _propertyId = Shader.PropertyToID(propertyName);
        }

        public static implicit operator int(SerializableShaderProperty hash) => hash.PropertyId;
        public static implicit operator SerializableShaderProperty(string propertyName) => new(propertyName);
    }

    public static class ShaderPropertyExtensions
    {
        public static T Get<T>(this IMaterialPropertyReadWrite<T> prop, Renderer renderer) => prop.Get(renderer.material);
        public static void Set<T>(this IMaterialPropertyReadWrite<T> prop, Renderer renderer, T value) => prop.Set(renderer.material, value);
    }

    /// <summary> <inheritdoc cref="ShaderProperty"/> </summary>
    public struct IntShaderProperty : IMaterialPropertyReadWrite<int>
    {
        public int PropertyId { get; private set; }

        public IntShaderProperty(string propertyName) => PropertyId = Shader.PropertyToID(propertyName);

        public readonly int Get(Material material) => material.GetInteger(PropertyId);
        public readonly void Set(Material material, int property) => material.SetInteger(PropertyId, property);

        public static implicit operator int(IntShaderProperty id) => id.PropertyId;

        public static implicit operator IntShaderProperty(int propertyId) => new() { PropertyId = propertyId };
        public static implicit operator IntShaderProperty(string propertyName) => new(propertyName);
    }

    /// <summary> <inheritdoc cref="ShaderProperty"/> </summary>
    public struct FloatShaderProperty : IMaterialPropertyReadWrite<float>
    {
        public int PropertyId { get; private set; }

        public FloatShaderProperty(string propertyName) => PropertyId = Shader.PropertyToID(propertyName);

        public readonly float Get(Material material) => material.GetFloat(PropertyId);
        public readonly void Set(Material material, float property) => material.SetFloat(PropertyId, property);

        public static implicit operator int(FloatShaderProperty id) => id.PropertyId;

        public static implicit operator FloatShaderProperty(int propertyId) => new() { PropertyId = propertyId };
        public static implicit operator FloatShaderProperty(string propertyName) => new(propertyName);
    }

    /// <summary> <inheritdoc cref="ShaderProperty"/> </summary>
    public struct ColorShaderProperty : IMaterialPropertyReadWrite<Color>
    {
        public int PropertyId { get; private set; }

        public ColorShaderProperty(string propertyName) => PropertyId = Shader.PropertyToID(propertyName);

        public readonly Color Get(Material material) => material.GetColor(PropertyId);
        public readonly void Set(Material material, Color property) => material.SetColor(PropertyId, property);

        public static implicit operator int(ColorShaderProperty id) => id.PropertyId;

        public static implicit operator ColorShaderProperty(int propertyId) => new() { PropertyId = propertyId };
        public static implicit operator ColorShaderProperty(string propertyName) => new(propertyName);
    }

    /// <summary> <inheritdoc cref="ShaderProperty"/> </summary>
    public struct TextureShaderProperty : IMaterialPropertyReadWrite<Texture>
    {
        public int PropertyId { get; private set; }

        public TextureShaderProperty(string propertyName) => PropertyId = Shader.PropertyToID(propertyName);

        public readonly Texture Get(Material material) => material.GetTexture(PropertyId);
        public readonly void Set(Material material, Texture texture) => material.SetTexture(PropertyId, texture);

        public static implicit operator int(TextureShaderProperty id) => id.PropertyId;

        public static implicit operator TextureShaderProperty(int propertyId) => new() { PropertyId = propertyId };
        public static implicit operator TextureShaderProperty(string propertyName) => new(propertyName);
    }
}
