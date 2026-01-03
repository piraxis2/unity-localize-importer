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

            DrawTableSelection();
            DrawKeySelection();

            EditorGUILayout.Space();
            
            if (GUI.changed)
            {
                EditorUtility.SetDirty(component);
                component.localizedString.RefreshString();
                component.UpdateText();
            }
            
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
                component.localizedString.RefreshString();
                component.UpdateText();
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
                    
                    // SetReference를 사용하여 명시적으로 갱신
                    component.localizedString.SetReference(collection.TableCollectionName, key);
                    
                    EditorUtility.SetDirty(component);
                    
                    // 타이밍 이슈 해결: 선택 처리가 완전히 끝난 후 갱신 시도
                    EditorApplication.delayCall += () => 
                    {
                        if (component != null) 
                        {
                            component.UpdateText();
                            // 갱신 후 다시 한 번 저장/리페인트 (텍스트 바뀐거 반영)
                            EditorUtility.SetDirty(component);
                            
                            // SceneView 강제 갱신 (선택 사항)
                            UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
                        }
                    };
                    
                    Repaint();
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
