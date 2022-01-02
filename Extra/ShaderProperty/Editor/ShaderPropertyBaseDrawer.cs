using Sirenix.OdinInspector.Editor;
using UnityEngine;

public class ShaderPropertyBaseDrawer<T> : OdinValueDrawer<T> where T : ShaderPropertyBase
{
	private InspectorProperty m_property;

	protected override void Initialize() => m_property = Property.Children["m_property"];

	protected override void DrawPropertyLayout(GUIContent label) => m_property.Draw(label);
}