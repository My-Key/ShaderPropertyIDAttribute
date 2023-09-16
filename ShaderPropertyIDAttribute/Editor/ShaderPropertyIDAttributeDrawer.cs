using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector.Editor.ValueResolvers;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public abstract class ShaderPropertyIDAttributeDrawerBase<TAttribute, TResolver> : OdinAttributeDrawer<TAttribute, string>
	where TAttribute : ShaderPropertyIDAttributeBase
{
	private GUIContent m_buttonContent = new GUIContent();
	private Shader m_prevShader;
	private bool m_propertyFound;
	private bool m_correctType;

	private const string CANT_FIND = "Can't find in this property";
	private const string WRONG_TYPE = "Current parameter has wrong type";

	protected abstract (Shader, string) Shader { get; }

	protected ValueResolver<TResolver> m_resolver;

	private string m_referenceToResolve;
	private InspectorProperty m_propertyToResolve;

	private ShaderPropertyIDAttributeBase.Type m_type;

	protected override void Initialize()
	{
		base.Initialize();

		(m_propertyToResolve, m_referenceToResolve) =
			GetValueResolverOverride(Property, Attribute.m_reference, typeof(TResolver));
		m_type = GetTypeOverride();
		
		m_resolver = ValueResolver.Get<TResolver>(m_propertyToResolve, m_referenceToResolve);

		if (m_resolver.HasError)
			return;
		
		UpdateButtonContent();
	}

	public static (InspectorProperty property, string attribute)
		GetValueResolverOverride(InspectorProperty property, string reference, Type type)
	{
		var currentProperty = property;

		if (currentProperty.ParentType.IsArray)
			currentProperty = currentProperty.ParentValueProperty;

		PropertyIDReferenceOverrideBaseAttribute currentAttribute;
		var foundReference = reference;

		// Try to get override
		do
		{
			currentAttribute = GetAttributeForType(currentProperty.ParentValueProperty
					?.GetAttributes<PropertyIDReferenceOverrideBaseAttribute>());

			if (currentAttribute != null)
			{
				currentProperty = currentProperty.ParentValueProperty;
				foundReference = currentAttribute.m_reference;
			}
		} while (currentAttribute != null);
		
		if (!string.IsNullOrWhiteSpace(foundReference) || currentProperty.ParentValueProperty == null)
			return (currentProperty, foundReference);

		// If reference is null or empty, find first property of correct type
		foundReference = FindFirstSiblingProperty(currentProperty, foundReference, type);

		return (currentProperty, foundReference);
	}

	public static string FindFirstSiblingProperty(InspectorProperty currentProperty, string foundReference, Type type)
	{
		foreach (var child in currentProperty.ParentValueProperty.Children)
		{
			if (child.ValueEntry == null)
				continue;

			var value = child.ValueEntry.TypeOfValue;

			if (!value.InheritsFrom(type)) continue;

			foundReference = child.Name;
			return foundReference;
		}

		return foundReference;
	}

	private static TType GetAttributeForType<TType>(IEnumerable<TType> attributes) where TType : Attribute
	{
		foreach (var overrideAttribute in attributes)
		{
			if (overrideAttribute.GetType().ImplementsOrInherits(typeof(IAttributeMatch<TAttribute>)))
				return overrideAttribute;
		}

		return null;
	}

	private ShaderPropertyIDAttributeBase.Type GetTypeOverride()
	{
		var currentProperty = Property;

		if (currentProperty.ParentType.IsArray)
			currentProperty = currentProperty.ParentValueProperty;

		PropertyIDTypeOverrideBaseAttribute currentAttribute;
		var foundType = Attribute.m_type;

		do
		{
			currentAttribute = GetAttributeForType(currentProperty.ParentValueProperty
					?.GetAttributes<PropertyIDTypeOverrideBaseAttribute>());

			if (currentAttribute != null)
			{
				currentProperty = currentProperty.ParentValueProperty;
				foundType = currentAttribute.m_type;
			}
		} while (currentAttribute != null);

		return foundType;
	}

	private void UpdateButtonContent()
	{
		var (shader, _) = Shader;

		if (shader == null)
			return;

		var index = GetIndex(ValueEntry.SmartValue, shader);

		m_propertyFound = index >= 0;
		m_correctType = m_propertyFound && IsCorrectType(shader.GetPropertyType(index), m_type);

		SetButtonContent(shader);
	}

	private void SetButtonContent(Shader shader)
	{
		int index = GetIndex(ValueEntry.SmartValue, shader);

		m_buttonContent.text = index >= 0 ? shader.GetPropertyDescription(index) : ValueEntry.SmartValue;
	}

	protected override void DrawPropertyLayout(GUIContent label)
	{
		if (m_resolver.HasError)
		{
			m_resolver.DrawError();
			return;
		}

		var (shader, message) = Shader;

		var materialChanged = m_prevShader != shader;
		m_prevShader = shader;
		
		if (materialChanged)
			UpdateButtonContent();
		
		if (shader == null)
		{
			SirenixEditorGUI.ErrorMessageBox(message);
			return;
		}

		var rect = EditorGUILayout.GetControlRect(label != null);
		rect = label == null ? EditorGUI.IndentedRect(rect) : EditorGUI.PrefixLabel(rect, label);
		
		GUIHelper.PushColor(m_propertyFound && m_correctType ? Color.white : Color.red);

		if (!m_propertyFound || !m_correctType)
		{
			var tooltip = string.Empty;
			
			if (!m_correctType)
				tooltip = WRONG_TYPE;
			
			if (!m_propertyFound) 
				tooltip = CANT_FIND;

			m_buttonContent.tooltip = tooltip;
		}

		var clicked = EditorGUI.DropdownButton(rect, m_buttonContent, FocusType.Passive);
		
		GUIHelper.PopColor();
		
		if (!clicked)
			return;
		
		DrawSelector(shader, rect);
	}

	private void DrawSelector(Shader shader, Rect rect)
	{
		var selector = new ShaderPropertySelector(shader, m_type);
		selector.SetSelection(GetIndex(ValueEntry.SmartValue, shader));
		selector.ShowInPopup(rect.position);

		selector.SelectionConfirmed += x =>
		{
			ValueEntry.Property.Tree.DelayAction(() =>
			{
				int index = x.FirstOrDefault();

				SetSmartValue(index, shader);

				m_buttonContent.text = shader.GetPropertyDescription(index);
				m_propertyFound = true;
				m_correctType = true;
			});
		};
	}

	private int GetIndex(string value, Shader shader) =>
		string.IsNullOrEmpty(value) ? -1 : shader.FindPropertyIndex(value);

	private void SetSmartValue(int index, Shader shader)
	{
		ValueEntry.SmartValue = shader.GetPropertyName(index);
	}

	public static bool IsCorrectType(ShaderPropertyType shaderType, ShaderPropertyIDAttributeBase.Type type)
	{
		return type switch
		{
			ShaderPropertyIDAttributeBase.Type.Float => shaderType == ShaderPropertyType.Float ||
			                                          shaderType == ShaderPropertyType.Range,
			ShaderPropertyIDAttributeBase.Type.Color => shaderType == ShaderPropertyType.Color,
			ShaderPropertyIDAttributeBase.Type.Vector => shaderType == ShaderPropertyType.Vector,
			ShaderPropertyIDAttributeBase.Type.Texture => shaderType == ShaderPropertyType.Texture,
			_ => true
		};
	}
}

public class MaterialPropertyIDAttributeDrawer : ShaderPropertyIDAttributeDrawerBase<MaterialPropertyIDAttribute, Material>
{
	private const string NULL_MESSAGE = "No material attached";

	protected override (Shader, string) Shader
	{
		get
		{
			var material = m_resolver.GetValue();

			return material != null ? (material.shader, string.Empty) : (null, NULL_MESSAGE);
		}
	}
}

public class ShaderPropertyIDAttributeDrawer : ShaderPropertyIDAttributeDrawerBase<ShaderPropertyIDAttribute, Shader>
{
	private const string NULL_MESSAGE = "No shader attached";

	protected override (Shader, string) Shader
	{
		get
		{
			var shader = m_resolver.GetValue();

			return (shader, shader != null ? string.Empty : NULL_MESSAGE);
		}
	}
}

public class RendererPropertyIDAttributeDrawer : ShaderPropertyIDAttributeDrawerBase<RendererPropertyIDAttribute, Renderer>
{
	private InspectorProperty m_parent;
	private bool m_isInList;

	protected override void Initialize()
	{
		base.Initialize();

		m_parent = GetParent(Property);

		m_isInList = m_parent != null;
	}

	private static InspectorProperty GetParent(InspectorProperty property)
	{
		return property.FindParent(
			inspectorProperty => inspectorProperty.GetAttribute<RendererPropertyIDParentListAttribute>() != null,
			true);
	}

	private const string NULL_MESSAGE = "No renderer attached";
	private const string NO_MATERIAL = "No material in renderer";
	private const string TOO_BIG_INDEX = "Parent index bigger than materials count in renderer";

	protected override (Shader, string) Shader
	{
		get
		{
			var renderer = m_resolver.GetValue();

			if (renderer == null)
				return (null, NULL_MESSAGE);

			if (!m_isInList)
			{
				var sharedMaterial = renderer.sharedMaterial;

				return (sharedMaterial != null ? sharedMaterial.shader : null,
					sharedMaterial != null ? string.Empty : NO_MATERIAL);
			}

			var sharedMaterials = renderer.sharedMaterials;

			return sharedMaterials != null && sharedMaterials.Length > m_parent.Index
				? (sharedMaterials[m_parent.Index].shader, string.Empty)
				: (null, TOO_BIG_INDEX);
		}
	}
}

public class ShaderPropertySelector : OdinSelector<int>
{
	private Shader m_shader;
	private ShaderPropertyIDAttributeBase.Type m_type;
	
	public ShaderPropertySelector(Shader shader, ShaderPropertyIDAttributeBase.Type type)
	{
		m_shader = shader;
		m_type = type;

		SelectionTree.Selection.SupportsMultiSelect = false;
	}
	
	protected override void BuildSelectionTree(OdinMenuTree tree)
	{
		var count = m_shader.GetPropertyCount();
		
		for (int i = 0; i < count; i++)
		{
			if (ShaderUtil.IsShaderPropertyHidden(m_shader, i) ||
			    ShaderUtil.IsShaderPropertyNonModifiableTexureProperty(m_shader, i))
				continue;

			var type = m_shader.GetPropertyType(i);

			if (!RendererPropertyIDAttributeDrawer.IsCorrectType(type, m_type))
				continue;

			tree.Add($"{m_shader.GetPropertyDescription(i)} ({m_shader.GetPropertyType(i)})", i);
		}
	}
}

[DrawerPriority(10)]
public class RendererPropertyIDParentListAttributeDrawer : OdinAttributeDrawer<RendererPropertyIDParentListAttribute>
{
	private bool m_isListElement;
	
	private ValueResolver<Renderer> m_rendererResolver;

	private const string INDEX_TOO_BIG = "Index bigger than materials count";

	protected override bool CanDrawAttributeProperty(InspectorProperty property)
	{
		return property.ParentType.IsArray || property.ParentType.IsGenericType &&
			property.ParentType.GetGenericTypeDefinition() == typeof(List<>);
	}

	protected override void Initialize()
	{
		m_isListElement = Property.Parent != null && Property.Parent.ChildResolver is IOrderedCollectionResolver;

		var (propertyToResolve, referenceToResolve) =
			RendererPropertyIDAttributeDrawer.GetValueResolverOverride(Property, Attribute.m_reference,
				typeof(Renderer));

		m_rendererResolver = ValueResolver.Get<Renderer>(propertyToResolve, referenceToResolve);
	}

	protected override void DrawPropertyLayout(GUIContent label)
	{
		if (!m_isListElement)
		{
			if (m_rendererResolver.HasError)
				m_rendererResolver.DrawError();

			CallNextDrawer(label);
			return;
		}

		if (m_rendererResolver.HasError)
		{
			CallNextDrawer(label);
			return;
		}

		var renderer = m_rendererResolver.GetValue();

		if (renderer != null)
		{
			if (renderer.sharedMaterials.Length > Property.Index)
			{
				GUIHelper.PushGUIEnabled(false);
				GUIHelper.PushLabelWidth(25);

				EditorGUILayout.ObjectField(Property.Index.ToString(), renderer.sharedMaterials[Property.Index],
					typeof(Material), true);

				GUIHelper.PopLabelWidth();
				GUIHelper.PopGUIEnabled();
			}
			else
			{
				SirenixEditorGUI.WarningMessageBox(INDEX_TOO_BIG);
			}
		}

		CallNextDrawer(label);
	}
}
