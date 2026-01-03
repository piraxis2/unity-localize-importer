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
        private SerializedProperty localizedStringProp;
        private SerializedProperty tableRefProp;
        private SerializedProperty tableEntryRefProp;

        private void OnEnable()
        {
            localizedStringProp = serializedObject.FindProperty("localizedString");
            tableRefProp = localizedStringProp.FindPropertyRelative("m_TableReference");
            tableEntryRefProp = localizedStringProp.FindPropertyRelative("m_TableEntryReference");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Localization Settings", EditorStyles.boldLabel);

            // 1. 테이블 선택
            DrawTableSelection();

            // 2. 키 선택
            DrawKeySelection();

            EditorGUILayout.Space();
            
            // 변경 사항 적용
            if (serializedObject.ApplyModifiedProperties())
            {
                // 값이 바뀌면 에디터 프리뷰 갱신 트리거
                ((SimpleLocalizedText)target).SendMessage("OnValidate", SendMessageOptions.DontRequireReceiver);
            }
        }

        private void DrawTableSelection()
        {
            var collections = LocalizationEditorSettings.GetStringTableCollections().ToList();
            var names = collections.Select(c => c.TableCollectionName).ToList();
            names.Insert(0, "None");

            string currentTableName = tableRefProp.FindPropertyRelative("m_TableCollectionName").stringValue;
            int currentIndex = 0;

            if (!string.IsNullOrEmpty(currentTableName))
            {
                currentIndex = names.IndexOf(currentTableName);
                if (currentIndex == -1) currentIndex = 0;
            }

            int newIndex = EditorGUILayout.Popup("Table Collection", currentIndex, names.ToArray());
            if (newIndex != currentIndex)
            {
                if (newIndex == 0)
                {
                    tableRefProp.FindPropertyRelative("m_TableCollectionName").stringValue = "";
                    tableRefProp.FindPropertyRelative("m_TableCollectionNameGuid").stringValue = "";
                }
                else
                {
                    var selected = collections[newIndex - 1];
                    tableRefProp.FindPropertyRelative("m_TableCollectionName").stringValue = selected.TableCollectionName;
                    tableRefProp.FindPropertyRelative("m_TableCollectionNameGuid").stringValue = selected.SharedData.TableCollectionNameGuid.ToString();
                }
            }
        }

        private void DrawKeySelection()
        {
            string currentTableName = tableRefProp.FindPropertyRelative("m_TableCollectionName").stringValue;
            if (string.IsNullOrEmpty(currentTableName))
            {
                EditorGUILayout.HelpBox("Please select a Table Collection first.", MessageType.Info);
                return;
            }

            var collection = LocalizationEditorSettings.GetStringTableCollections()
                .FirstOrDefault(c => c.TableCollectionName == currentTableName);

            if (collection == null) return;

            string currentKey = "";
            long currentKeyId = tableEntryRefProp.FindPropertyRelative("m_KeyId").longValue;
            
            // ID로 키 이름 찾기
            var entry = collection.SharedData.GetEntry(currentKeyId);
            if (entry != null) currentKey = entry.Key;
            else 
            {
                // ID로 못 찾으면 이름으로 시도
                currentKey = tableEntryRefProp.FindPropertyRelative("m_KeyName").stringValue;
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Key");
            
            if (GUILayout.Button(string.IsNullOrEmpty(currentKey) ? "Select Key..." : currentKey, EditorStyles.popup))
            {
                // 드롭다운 표시
                var dropdown = new LocalizationKeyDropdown(new AdvancedDropdownState(), collection);
                dropdown.onKeySelected = (key, id) =>
                {
                    tableEntryRefProp.FindPropertyRelative("m_KeyName").stringValue = key;
                    tableEntryRefProp.FindPropertyRelative("m_KeyId").longValue = id;
                    serializedObject.ApplyModifiedProperties();
                    ((SimpleLocalizedText)target).SendMessage("OnValidate", SendMessageOptions.DontRequireReceiver);
                };
                dropdown.Show(GUILayoutUtility.GetLastRect());
            }
            EditorGUILayout.EndHorizontal();
        }
    }

    // 고급 검색 드롭다운 (AdvancedDropdown)
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

            // 키 목록을 정렬해서 보여주면 더 찾기 쉽습니다.
            var entries = collection.SharedData.Entries.OrderBy(e => e.Key);

            foreach (var entry in entries)
            {
                if (!string.IsNullOrEmpty(entry.Key))
                {
                    root.AddChild(new AdvancedDropdownItem(entry.Key) { id = (int)entry.Id });
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
