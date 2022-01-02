using System;

public abstract class ShaderPropertyIDAttributeBase : Attribute
{
	public enum Type
	{
		Any,
		Float,
		Color,
		Vector,
		Texture
	}

	public Type m_type;
	public string m_reference;
	
	public ShaderPropertyIDAttributeBase(string reference, Type type)
	{
		m_reference = reference;
		m_type = type;
	}
}

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class MaterialPropertyIDAttribute : ShaderPropertyIDAttributeBase
{
	/// <summary>
	/// Draw string as dropdown with properties from <paramref name="material"/>
	/// </summary>
	/// <param name="material">String that will be evaluated to <b>Material</b> reference.
	/// <br/>
	/// If set to <b>null or empty string</b> <i>first found <b>Material</b></i> from sibling fields and properties will be used</param>
	/// <param name="type">Which properties of type should be listed in dropdown</param>
	public MaterialPropertyIDAttribute(string material = null, Type type = Type.Any) : base(material, type) { }
}

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class ShaderPropertyIDAttribute : ShaderPropertyIDAttributeBase
{
	/// <summary>
	/// Draw string as dropdown with properties from <paramref name="shader"/>
	/// </summary>
	/// <param name="shader">String that will be evaluated to <b>Shader</b> reference.
	/// <br/>
	/// If set to <b>null or empty string</b> <i>first found <b>Shader</b></i> from sibling fields and properties will be used</param>
	/// <param name="type">Which properties of type should be listed in dropdown</param>
	public ShaderPropertyIDAttribute(string shader = null, Type type = Type.Any) : base(shader, type) { }
}


[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class RendererPropertyIDAttribute : ShaderPropertyIDAttributeBase
{
	/// <summary>
	/// Draw string as dropdown with properties from <paramref name="renderer"/>
	/// <br/>
	/// Dropdown will use <b>first</b> material from <paramref name="renderer"/>.
	/// To use other materials use <see cref="RendererPropertyIDParentListAttribute"/>
	/// </summary>
	/// <param name="renderer">String that will be evaluated to <b>Renderer</b> reference.
	/// <br/>
	/// If set to <b>null or empty string</b> <i>first found <b>Renderer</b></i> from sibling fields and properties will be used</param>
	/// <param name="type">Which properties of type should be listed in dropdown</param>
	public RendererPropertyIDAttribute(string renderer = null, Type type = Type.Any) : base(renderer, type) { }
}

public abstract class PropertyIDTypeOverrideBaseAttribute : Attribute
{
	public ShaderPropertyIDAttributeBase.Type m_type;
	
	public PropertyIDTypeOverrideBaseAttribute(ShaderPropertyIDAttributeBase.Type type)
	{
		m_type = type;
	}
}

public interface IAttributeMatch<T> where T : ShaderPropertyIDAttributeBase {}

public class MaterialsPropertyIDTypeOverrideAttribute : PropertyIDTypeOverrideBaseAttribute, IAttributeMatch<MaterialPropertyIDAttribute>
{
	/// <summary>
	/// Override Type of <see cref="MaterialPropertyIDAttribute"/> in child fields and properties
	/// </summary>
	/// <param name="type">Which properties of type should be listed in dropdown</param>
	public MaterialsPropertyIDTypeOverrideAttribute(ShaderPropertyIDAttributeBase.Type type) : base(type) { }
}

public class ShaderPropertyIDTypeOverrideAttribute : PropertyIDTypeOverrideBaseAttribute, IAttributeMatch<ShaderPropertyIDAttribute>
{
	/// <summary>
	/// Override Type of <see cref="ShaderPropertyIDAttribute"/> in child fields and properties
	/// </summary>
	/// <param name="type">Which properties of type should be listed in dropdown</param>
	public ShaderPropertyIDTypeOverrideAttribute(ShaderPropertyIDAttributeBase.Type type) : base(type) { }
}

public class RendererPropertyIDTypeOverrideAttribute : PropertyIDTypeOverrideBaseAttribute, IAttributeMatch<RendererPropertyIDAttribute>
{
	/// <summary>
	/// Override Type of <see cref="RendererPropertyIDAttribute"/> in child fields and properties
	/// </summary>
	/// <param name="type">Which properties of type should be listed in dropdown</param>
	public RendererPropertyIDTypeOverrideAttribute(ShaderPropertyIDAttributeBase.Type type) : base(type) { }
}

public abstract class PropertyIDReferenceOverrideBaseAttribute : Attribute
{
	public string m_reference;
	
	public PropertyIDReferenceOverrideBaseAttribute(string reference = null)
	{
		m_reference = reference;
	}
}

public class MaterialPropertyIDMaterialOverrideAttribute : PropertyIDReferenceOverrideBaseAttribute, IAttributeMatch<MaterialPropertyIDAttribute>
{
	/// <summary>
	/// Override material of <see cref="MaterialPropertyIDAttribute"/> in child fields and properties
	/// </summary>
	/// <param name="material">String that will be evaluated to <b>Material</b> reference.
	/// <br/>
	/// If set to <b>null or empty string</b> <i>first found <b>Material</b></i> from sibling fields and properties will be used</param>
	public MaterialPropertyIDMaterialOverrideAttribute(string material = null) : base(material) { }
}

public class ShaderPropertyIDShaderOverrideAttribute : PropertyIDReferenceOverrideBaseAttribute, IAttributeMatch<ShaderPropertyIDAttribute>
{
	/// <summary>
	/// Override shader of <see cref="ShaderPropertyIDAttribute"/> in child fields and properties
	/// </summary>
	/// <param name="shader">String that will be evaluated to <b>Shader</b> reference.
	/// <br/>
	/// If set to <b>null or empty string</b> <i>first found <b>Shader</b></i> from sibling fields and properties will be used</param>
	public ShaderPropertyIDShaderOverrideAttribute(string shader = null) : base(shader) { }
}

public class RendererPropertyIDRendererOverrideAttribute : PropertyIDReferenceOverrideBaseAttribute, IAttributeMatch<RendererPropertyIDAttribute>
{
	/// <summary>
	/// Override renderer of <see cref="RendererPropertyIDAttribute"/> in child fields and properties
	/// </summary>
	/// <param name="renderer">String that will be evaluated to <b>Renderer</b> reference.
	/// <br/>
	/// If set to <b>null or empty string</b> <i>first found <b>Renderer</b></i> from sibling fields and properties will be used</param>
	public RendererPropertyIDRendererOverrideAttribute(string renderer = null) : base(renderer) { }
}

/// <summary>
/// When added to list or array that contains string or class with fields or properties with <see cref="RendererPropertyIDAttribute"/>,
/// dropdown will use material of corresponding index from Renderer:<br/>
/// - properties and fields from index 0 will use material from index 0<br/>
/// - properties and fields from index 1 will use material from index 1<br/>
/// - etc.
/// </summary>
public class RendererPropertyIDParentListAttribute : RendererPropertyIDRendererOverrideAttribute { }