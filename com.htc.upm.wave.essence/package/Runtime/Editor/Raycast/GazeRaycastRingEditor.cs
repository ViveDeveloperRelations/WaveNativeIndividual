// "Wave SDK 
// © 2020 HTC Corporation. All Rights Reserved.
//
// Unless otherwise required by copyright law and practice,
// upon the execution of HTC SDK license agreement,
// HTC grants you access to and use of the Wave SDK(s).
// You shall fully comply with all of HTC’s SDK license agreement terms and
// conditions signed by you and all SDK and API requirements,
// specifications, and documentation provided by HTC to You."

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace Wave.Essence.Raycast.Editor
{
    [CustomEditor(typeof(GazeRaycastRing))]
    public class GazeRaycastRingEditor : UnityEditor.Editor
    {
        /// Physics Raycaster options
        SerializedProperty m_IgnoreReversedGraphics, m_PhysicsCastDistance, m_PhysicsEventMask;
        /// RaycastRing options
        SerializedProperty m_PointerRingWidth, m_PointerCircleRadius, m_PointerDistance, m_PointerColor, m_ProgressColor, m_PointerMaterial, m_PointerRenderQueue, m_PointerSortingOrder, m_TimeToGaze;
        /// GazeRaycastRing options
        SerializedProperty m_InputEvent, m_ControlKey, m_AlwaysEnable;
        private void OnEnable()
        {
            /// Physics Raycaster options
            m_IgnoreReversedGraphics = serializedObject.FindProperty("m_IgnoreReversedGraphics");
            m_PhysicsCastDistance = serializedObject.FindProperty("m_PhysicsCastDistance");
            m_PhysicsEventMask = serializedObject.FindProperty("m_PhysicsEventMask");
            /// RaycastRing options
            m_PhysicsEventMask = serializedObject.FindProperty("m_PointerRingWidth");
            m_PointerCircleRadius = serializedObject.FindProperty("m_PointerCircleRadius");
            m_PointerDistance = serializedObject.FindProperty("m_PointerDistance");
            m_PointerColor = serializedObject.FindProperty("m_PointerColor");
            m_ProgressColor = serializedObject.FindProperty("m_ProgressColor");
            m_PointerMaterial = serializedObject.FindProperty("m_PointerMaterial");
            m_PointerRenderQueue = serializedObject.FindProperty("m_PointerRenderQueue");
            m_PointerSortingOrder = serializedObject.FindProperty("m_PointerSortingOrder");
            m_TimeToGaze = serializedObject.FindProperty("m_TimeToGaze");
            /// GazeRaycastRing options
            m_InputEvent = serializedObject.FindProperty("m_InputEvent");
            m_ControlKey = serializedObject.FindProperty("m_ControlKey");
            m_AlwaysEnable = serializedObject.FindProperty("m_AlwaysEnable");
        }
        bool PhysicsRaycasterOptions = false, RingOptions = false, GazeOptions = true;
		public override void OnInspectorGUI()
		{
			serializedObject.Update();
			GazeRaycastRing myScript = target as GazeRaycastRing;

            PhysicsRaycasterOptions = EditorGUILayout.Foldout(PhysicsRaycasterOptions, "Physics Raycaster Settings");
            if (PhysicsRaycasterOptions)
            {
                EditorGUILayout.PropertyField(m_IgnoreReversedGraphics);
                EditorGUILayout.PropertyField(m_PhysicsCastDistance);
                EditorGUILayout.PropertyField(m_PhysicsEventMask);
            }

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            RingOptions = EditorGUILayout.Foldout(RingOptions, "Ring Settings");
            if (RingOptions)
			{
                EditorGUILayout.PropertyField(m_PhysicsEventMask);
                EditorGUILayout.PropertyField(m_PointerCircleRadius);
                EditorGUILayout.PropertyField(m_PointerDistance);
                EditorGUILayout.PropertyField(m_PointerColor);
                EditorGUILayout.PropertyField(m_ProgressColor);
                EditorGUILayout.PropertyField(m_PointerMaterial);
                EditorGUILayout.PropertyField(m_PointerRenderQueue);
                EditorGUILayout.PropertyField(m_PointerSortingOrder);
            }

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            GazeOptions = EditorGUILayout.Foldout(GazeOptions, "Gaze Settings");
            if (GazeOptions)
			{
                // Moves m_TimeToGaze here thus developers can easily set the value.
                EditorGUILayout.PropertyField(m_TimeToGaze);
                EditorGUILayout.PropertyField(m_InputEvent);
                EditorGUILayout.PropertyField(m_ControlKey);
            }

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            EditorGUILayout.PropertyField(m_AlwaysEnable);

            serializedObject.ApplyModifiedProperties();
            if (GUI.changed)
                EditorUtility.SetDirty((GazeRaycastRing)target);
        }
    }
}
#endif