using System;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public abstract class ShaderPropertyBase
{
	protected int m_id = -1;
	
	protected void PropertyChanged() => m_id = -1;
	
	protected abstract string Property { get; }

	public static implicit operator int(ShaderPropertyBase property)
	{
		if (property.m_id < 0) 
			property.m_id = Shader.PropertyToID(property.Property);

		return property.m_id;
	}
}

[Serializable]
public class RendererProperty : ShaderPropertyBase
{
	[SerializeField, RendererPropertyID, OnValueChanged(nameof(PropertyChanged))]
	private string m_property;

	protected override string Property => m_property;
}

[Serializable]
public class MaterialProperty : ShaderPropertyBase
{
	[SerializeField, MaterialPropertyID, OnValueChanged(nameof(PropertyChanged))]
	private string m_property;

	protected override string Property => m_property;
}

[Serializable]
public class ShaderProperty : ShaderPropertyBase
{
	[SerializeField, ShaderPropertyID, OnValueChanged(nameof(PropertyChanged))]
	private string m_property;

	protected override string Property => m_property;
}