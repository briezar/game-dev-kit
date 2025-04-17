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

    public struct ShaderId
    {
        public int PropertyId { get; private set; }

        public ShaderId(string propertyName) => PropertyId = Shader.PropertyToID(propertyName);

        public static implicit operator ShaderId(int propertyId) => new() { PropertyId = propertyId };
        public static implicit operator ShaderId(string propertyName) => new(propertyName);
    }

    public static class ShaderIdExtension
    {
        public static T Get<T>(this IMaterialPropertyReadWrite<T> prop, Renderer renderer) => prop.Get(renderer.material);
        public static void Set<T>(this IMaterialPropertyReadWrite<T> prop, Renderer renderer, T value) => prop.Set(renderer.material, value);
    }

    /// <summary> Wrapper for better context </summary>
    public struct FloatShaderId : IMaterialPropertyReadWrite<float>
    {
        public int PropertyId { get; private set; }

        public FloatShaderId(string propertyName) => PropertyId = Shader.PropertyToID(propertyName);

        public readonly float Get(Material material) => material.GetFloat(PropertyId);
        public readonly void Set(Material material, float property) => material.SetFloat(PropertyId, property);

        public static implicit operator int(FloatShaderId id) => id.PropertyId;

        public static implicit operator FloatShaderId(int propertyId) => new() { PropertyId = propertyId };
        public static implicit operator FloatShaderId(string propertyName) => new(propertyName);
    }

    /// <summary> Wrapper for better context </summary>
    public struct ColorShaderId : IMaterialPropertyReadWrite<Color>
    {
        public int PropertyId { get; private set; }

        public ColorShaderId(string propertyName) => PropertyId = Shader.PropertyToID(propertyName);

        public readonly Color Get(Material material) => material.GetColor(PropertyId);
        public readonly void Set(Material material, Color property) => material.SetColor(PropertyId, property);

        public static implicit operator int(ColorShaderId id) => id.PropertyId;

        public static implicit operator ColorShaderId(int propertyId) => new() { PropertyId = propertyId };
        public static implicit operator ColorShaderId(string propertyName) => new(propertyName);
    }

    /// <summary> Wrapper for better context </summary>
    public struct TextureShaderId : IMaterialPropertyReadWrite<Texture>
    {
        public int PropertyId { get; private set; }

        public TextureShaderId(string propertyName) => PropertyId = Shader.PropertyToID(propertyName);

        public readonly Texture Get(Material material) => material.GetTexture(PropertyId);
        public readonly void Set(Material material, Texture texture) => material.SetTexture(PropertyId, texture);

        public static implicit operator int(TextureShaderId id) => id.PropertyId;

        public static implicit operator TextureShaderId(int propertyId) => new() { PropertyId = propertyId };
        public static implicit operator TextureShaderId(string propertyName) => new(propertyName);
    }
}
