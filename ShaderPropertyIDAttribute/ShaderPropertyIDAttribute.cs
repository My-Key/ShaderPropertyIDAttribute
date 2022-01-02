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
	public MaterialPropertyIDAttribute(string material = null, Type type = Type.Any) : base(material, type) { }
}

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class ShaderPropertyIDAttribute : ShaderPropertyIDAttributeBase
{
	public ShaderPropertyIDAttribute(string shader = null, Type type = Type.Any) : base(shader, type) { }
}


[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class RendererPropertyIDAttribute : ShaderPropertyIDAttributeBase
{
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
	public MaterialsPropertyIDTypeOverrideAttribute(ShaderPropertyIDAttributeBase.Type type) : base(type) { }
}

public class ShaderPropertyIDTypeOverrideAttribute : PropertyIDTypeOverrideBaseAttribute, IAttributeMatch<ShaderPropertyIDAttribute>
{
	public ShaderPropertyIDTypeOverrideAttribute(ShaderPropertyIDAttributeBase.Type type) : base(type) { }
}

public class RendererPropertyIDTypeOverrideAttribute : PropertyIDTypeOverrideBaseAttribute, IAttributeMatch<RendererPropertyIDAttribute>
{
	public RendererPropertyIDTypeOverrideAttribute(ShaderPropertyIDAttributeBase.Type type) : base(type) { }
}

public abstract class PropertyIDReferenceOverrideBaseAttribute : Attribute
{
	public string m_reference;
	
	public PropertyIDReferenceOverrideBaseAttribute(string reference = "")
	{
		m_reference = reference;
	}
}

public class MaterialPropertyIDMaterialOverrideAttribute : PropertyIDReferenceOverrideBaseAttribute, IAttributeMatch<MaterialPropertyIDAttribute>
{
	public MaterialPropertyIDMaterialOverrideAttribute(string material = "") : base(material) { }
}

public class ShaderPropertyIDShaderOverrideAttribute : PropertyIDReferenceOverrideBaseAttribute, IAttributeMatch<ShaderPropertyIDAttribute>
{
	public ShaderPropertyIDShaderOverrideAttribute(string shader = "") : base(shader) { }
}

public class RendererPropertyIDRendererOverrideAttribute : PropertyIDReferenceOverrideBaseAttribute, IAttributeMatch<RendererPropertyIDAttribute>
{
	public RendererPropertyIDRendererOverrideAttribute(string renderer = "") : base(renderer) { }
}

public class RendererPropertyIDParentListAttribute : RendererPropertyIDRendererOverrideAttribute { }