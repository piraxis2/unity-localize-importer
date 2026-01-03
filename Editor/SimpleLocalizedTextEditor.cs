using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine.Localization.Tables;
using UnityEditor.Localization;
using System.Collections.Generic;
using System.Linq;

namespace Simple.Localize.Editor
{
    [CustomEditor(typeof(SimpleLocalizedText))]
    public class SimpleLocalizedTextEditor : UnityEditor.Editor
    {
        private SimpleLocalizedText component;

        private void OnEnable()
        {
            component = (SimpleLocalizedText)target;
        }

        public override void OnInspectorGUI()
        {
            if (component == null) return;

            serializedObject.Update();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Localization Settings", EditorStyles.boldLabel);

            // 1. 테이블 선택
            DrawTableSelection();

            // 2. 키 선택
            DrawKeySelection();

            EditorGUILayout.Space();
            
            // GUI 변경 시 처리
            if (GUI.changed)
            {
                EditorUtility.SetDirty(component);
                component.SendMessage("OnValidate", SendMessageOptions.DontRequireReceiver);
            }
            
            // 기본 SerializedObject 적용은 필요 없지만(직접 수정하므로), 
            // 다른 프로퍼티(예: m_Script)가 있을 수 있으므로 유지
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawTableSelection()
        {
            var collections = LocalizationEditorSettings.GetStringTableCollections().ToList();
            var names = collections.Select(c => c.TableCollectionName).ToList();
            names.Insert(0, "None");

            string currentTableName = component.localizedString.TableReference.TableCollectionName;
            int currentIndex = 0;

            if (!string.IsNullOrEmpty(currentTableName))
            {
                currentIndex = names.IndexOf(currentTableName);
                if (currentIndex == -1) currentIndex = 0;
            }

            int newIndex = EditorGUILayout.Popup("Table Collection", currentIndex, names.ToArray());
            if (newIndex != currentIndex)
            {
                Undo.RecordObject(component, "Change Table Collection");
                if (newIndex == 0)
                {
                    component.localizedString.TableReference = default;
                }
                else
                {
                    component.localizedString.TableReference = collections[newIndex - 1].TableCollectionName;
                }
            }
        }

        private void DrawKeySelection()
        {
            string currentTableName = component.localizedString.TableReference.TableCollectionName;
            if (string.IsNullOrEmpty(currentTableName))
            {
                EditorGUILayout.HelpBox("Please select a Table Collection first.", MessageType.Info);
                return;
            }

            var collection = LocalizationEditorSettings.GetStringTableCollections()
                .FirstOrDefault(c => c.TableCollectionName == currentTableName);

            if (collection == null) return;

            string currentKey = "";
            long currentKeyId = component.localizedString.TableEntryReference.KeyId;
            
            // ID로 키 이름 찾기
            if (collection.SharedData != null)
            {
                var entry = collection.SharedData.GetEntry(currentKeyId);
                if (entry != null) currentKey = entry.Key;
                else currentKey = component.localizedString.TableEntryReference.Key; 
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Key");
            
            if (GUILayout.Button(string.IsNullOrEmpty(currentKey) ? "Select Key..." : currentKey, EditorStyles.popup))
            {
                var dropdown = new LocalizationKeyDropdown(new AdvancedDropdownState(), collection);
                dropdown.onKeySelected = (key, id) =>
                {
                    Undo.RecordObject(component, "Change Localization Key");
                    // ID로 할당하는 것이 가장 정확함
                    component.localizedString.TableEntryReference = id; 
                    
                    EditorUtility.SetDirty(component);
                    component.SendMessage("OnValidate", SendMessageOptions.DontRequireReceiver);
                };
                dropdown.Show(GUILayoutUtility.GetLastRect());
            }
            EditorGUILayout.EndHorizontal();
        }
    }

    public class LocalizationKeyDropdown : AdvancedDropdown
    {
        private StringTableCollection collection;
        public System.Action<string, long> onKeySelected;

        public LocalizationKeyDropdown(AdvancedDropdownState state, StringTableCollection collection) : base(state)
        {
            this.collection = collection;
        }

        protected override AdvancedDropdownItem BuildRoot()
        {
            var root = new AdvancedDropdownItem("Keys");
            
            if (collection.SharedData != null)
            {
                var entries = collection.SharedData.Entries.OrderBy(e => e.Key);
                foreach (var entry in entries)
                {
                    if (!string.IsNullOrEmpty(entry.Key))
                    {
                        root.AddChild(new AdvancedDropdownItem(entry.Key) { id = (int)entry.Id });
                    }
                }
            }

            return root;
        }

        protected override void ItemSelected(AdvancedDropdownItem item)
        {
            onKeySelected?.Invoke(item.name, item.id);
        }
    }
}