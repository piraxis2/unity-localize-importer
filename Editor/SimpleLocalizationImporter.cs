using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.Localization;
using UnityEngine;
using UnityEngine.Localization.Tables;

namespace Simple.Localize.Editor
{
    // 엑셀 로더 데이터를 유니티 Localization 테이블로 가져오는 에디터 윈도우
    public class SimpleLocalizationImporter : EditorWindow
    {
        // UI에서 입력받을 설정 변수들
        private ScriptableObject sourceAsset;
        private StringTableCollection targetCollection;
        
        private string listFieldName = "Data"; // 엑셀 로더의 기본 리스트 이름
        private string keyFieldName = "Key";   // 키 컬럼 이름
        
        // 언어 매핑 정보 (예: KR -> ko)
        [Serializable]
        public class LocaleMapping
        {
            public string SourceColumn; // 엑셀의 컬럼명 (예: KR)
            public string LocaleCode;   // 유니티 Locale 코드 (예: ko)
        }

        private List<LocaleMapping> mappings = new List<LocaleMapping>();

        [MenuItem("Tools/Simple Localize/Importer")]
        public static void ShowWindow()
        {
            GetWindow<SimpleLocalizationImporter>("Localize Importer");
        }

        private void OnEnable()
        {
            // 기본 매핑 예시 추가
            if (mappings.Count == 0)
            {
                mappings.Add(new LocaleMapping { SourceColumn = "KR", LocaleCode = "ko" });
                mappings.Add(new LocaleMapping { SourceColumn = "EN", LocaleCode = "en" });
                mappings.Add(new LocaleMapping { SourceColumn = "JP", LocaleCode = "ja" });
            }
        }

        private void OnGUI()
        {
            GUILayout.Label("Simple Localization Importer", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // 1. 소스 데이터 (ScriptableObject)
            sourceAsset = (ScriptableObject)EditorGUILayout.ObjectField("Source Asset (Excel)", sourceAsset, typeof(ScriptableObject), false);
            
            // 2. 타겟 테이블 (Localization Table)
            targetCollection = (StringTableCollection)EditorGUILayout.ObjectField("Target String Table", targetCollection, typeof(StringTableCollection), false);

            EditorGUILayout.Space();
            GUILayout.Label("Field Settings", EditorStyles.boldLabel);
            
            // 3. 필드 이름 설정
            listFieldName = EditorGUILayout.TextField("List Field Name", listFieldName);
            keyFieldName = EditorGUILayout.TextField("Key Field Name", keyFieldName);

            EditorGUILayout.Space();
            GUILayout.Label("Locale Mapping (Excel Column -> Locale Code)", EditorStyles.boldLabel);

            // 4. 언어 매핑 UI
            for (int i = 0; i < mappings.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                mappings[i].SourceColumn = EditorGUILayout.TextField(mappings[i].SourceColumn);
                GUILayout.Label("->", GUILayout.Width(20));
                mappings[i].LocaleCode = EditorGUILayout.TextField(mappings[i].LocaleCode);
                
                if (GUILayout.Button("X", GUILayout.Width(20)))
                {
                    mappings.RemoveAt(i);
                    i--;
                }
                EditorGUILayout.EndHorizontal();
            }

            if (GUILayout.Button("Add Mapping"))
            {
                mappings.Add(new LocaleMapping());
            }

            EditorGUILayout.Space();

            // 5. 가져오기 버튼
            GUI.enabled = sourceAsset != null && targetCollection != null;
            if (GUILayout.Button("Import / Update Table", GUILayout.Height(40)))
            {
                ImportData();
            }
            GUI.enabled = true;
        }

        private void ImportData()
        {
            if (sourceAsset == null || targetCollection == null) return;

            // 리플렉션으로 리스트 데이터 가져오기
            var listField = sourceAsset.GetType().GetField(listFieldName, BindingFlags.Public | BindingFlags.Instance);
            if (listField == null)
            {
                Debug.LogError($"[SimpleLocalize] '{listFieldName}' 필드를 찾을 수 없습니다.");
                return;
            }

            var listValue = listField.GetValue(sourceAsset) as IList;
            if (listValue == null)
            {
                Debug.LogError($"[SimpleLocalize] '{listFieldName}'는 리스트가 아닙니다.");
                return;
            }

            Undo.RecordObject(targetCollection, "Import Localization Data");

            int updatedCount = 0;

            foreach (var item in listValue)
            {
                // 키 값 가져오기
                var keyField = item.GetType().GetField(keyFieldName, BindingFlags.Public | BindingFlags.Instance);
                if (keyField == null) continue;

                string key = keyField.GetValue(item)?.ToString();
                if (string.IsNullOrEmpty(key)) continue;

                // 공유 엔트리 생성 또는 가져오기
                var entry = targetCollection.SharedData.GetEntry(key);
                if (entry == null)
                {
                    entry = targetCollection.SharedData.AddKey(key);
                }

                // 각 언어별 값 업데이트
                foreach (var map in mappings)
                {
                    UpdateLocaleValue(item, map.SourceColumn, map.LocaleCode, entry.Id);
                }
                
                updatedCount++;
            }
            
            // 변경 사항 저장 (Dirty 설정)
            EditorUtility.SetDirty(targetCollection);
            if (targetCollection.SharedData != null)
                EditorUtility.SetDirty(targetCollection.SharedData);
            
            // 연결된 모든 StringTable도 저장 필요
            foreach (var table in targetCollection.StringTables)
            {
                if (table != null)
                    EditorUtility.SetDirty(table);
            }
            
            Debug.Log($"[SimpleLocalize] {updatedCount}개의 키가 성공적으로 업데이트되었습니다.");
        }

        private void UpdateLocaleValue(object itemData, string columnName, string localeCode, long entryId)
        {
            // 엑셀 데이터 객체에서 해당 언어 컬럼 값 읽기
            var contentField = itemData.GetType().GetField(columnName, BindingFlags.Public | BindingFlags.Instance);
            if (contentField == null) return; // 해당 컬럼 없으면 스킵

            string contentValue = contentField.GetValue(itemData)?.ToString();
            
            // 해당 로케일의 테이블 찾기
            var table = targetCollection.GetTable(localeCode) as StringTable;
            if (table == null)
            {
                // 테이블이 없으면 생성 시도하지 않고 경고 (보통 미리 생성되어 있어야 함)
                // Debug.LogWarning($"[SimpleLocalize] 로케일 '{localeCode}'에 해당하는 테이블을 찾을 수 없습니다.");
                return;
            }

            // 값 넣기
            table.AddEntry(entryId, contentValue ?? "");
        }
    }
}
