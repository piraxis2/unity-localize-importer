using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine.Localization.Tables;
using UnityEditor.Localization;
using System.Collections.Generic;
using System.Linq;
using TMPro; 
using UnityEngine.Localization; 
using UnityEngine.Localization.Settings; 

namespace Simple.Localize.Editor
{
    [CustomEditor(typeof(SimpleLocalizedText))]
    public class SimpleLocalizedTextEditor : UnityEditor.Editor
    {
        private SimpleLocalizedText component;

        private void OnEnable()
        {
            component = (SimpleLocalizedText)target;
            if (component == null) return;

            // 이미 초기화되어 있고 로케일도 선택되어 있다면 즉시 갱신
            if (LocalizationSettings.InitializationOperation.IsDone && LocalizationSettings.SelectedLocale != null)
            {
                RefreshPreview();
            }
            else
            {
                // 준비될 때까지 매 프레임 체크
                EditorApplication.update += CheckLocalizationReady;
            }
        }

        private void OnDisable()
        {
            EditorApplication.update -= CheckLocalizationReady;
        }

        private void CheckLocalizationReady()
        {
            // 컴포넌트가 사라졌으면 중단
            if (component == null)
            {
                EditorApplication.update -= CheckLocalizationReady;
                return;
            }

            // 초기화 완료 여부 체크
            var op = LocalizationSettings.InitializationOperation;
            if (op.IsDone)
            {
                // 초기화는 끝났는데 선택된 로케일이 없다면?
                if (LocalizationSettings.SelectedLocale == null)
                {
                    // 사용 가능한 로케일이 있는지 확인하고 강제로 첫 번째꺼 선택 시도
                    var settings = LocalizationEditorSettings.ActiveLocalizationSettings;
                    if (settings != null)
                    {
                        var available = settings.GetAvailableLocales();
                        if (available != null && available.Locales.Count > 0)
                        {
                            // 로케일 강제 선택 (에디터 상에서만)
                            LocalizationSettings.SelectedLocale = available.Locales[0];
                        }
                    }
                }

                // 이제 갱신 시도
                if (LocalizationSettings.SelectedLocale != null)
                {
                    RefreshPreview();
                    EditorApplication.update -= CheckLocalizationReady; // 성공했으니 체크 종료
                }
            }
        }

        private void RefreshPreview()
        {
            if (component == null) return;
            UpdatePreviewText(component.localizedString.TableReference, component.localizedString.TableEntryReference);
            
            // 폰트는 런타임 로직(Refresh)을 통해 갱신 시도
            if (!component.localizedFont.IsEmpty)
            {
                component.Refresh();
            }
        }

        public override void OnInspectorGUI()
        {
            if (component == null) return;

            serializedObject.Update();

            // --- Text Settings ---
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Text Localization", EditorStyles.boldLabel);
            
            DrawReferenceSelection("Text Table", "Text Key", component.localizedString, true);
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Smart String Arguments", EditorStyles.boldLabel);
            SerializedProperty argsProp = serializedObject.FindProperty("smartArguments");
            EditorGUILayout.PropertyField(argsProp, true);

            // --- Font Settings ---
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Font Localization (Optional)", EditorStyles.boldLabel);
            
            DrawReferenceSelection("Font Table", "Font Key", component.localizedFont, false);

            EditorGUILayout.Space();
            
            if (GUI.changed)
            {
                EditorUtility.SetDirty(component);
            }
            
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawReferenceSelection(string tableLabel, string keyLabel, LocalizedReference reference, bool isText)
        {
            // 1. Table Selection
            var collections = LocalizationEditorSettings.GetStringTableCollections().Cast<LocalizationTableCollection>().ToList();
            
            // AssetTableCollection (폰트 등)도 가져오기 위해
            if (!isText)
            {
                 collections = LocalizationEditorSettings.GetAssetTableCollections().Cast<LocalizationTableCollection>().ToList();
            }

            var names = collections.Select(c => c.TableCollectionName).ToList();
            names.Insert(0, "None");

            string currentTableName = reference.TableReference.TableCollectionName;
            int currentIndex = 0;

            if (!string.IsNullOrEmpty(currentTableName))
            {
                currentIndex = names.IndexOf(currentTableName);
                if (currentIndex == -1) currentIndex = 0;
            }

            int newIndex = EditorGUILayout.Popup(tableLabel, currentIndex, names.ToArray());
            if (newIndex != currentIndex)
            {
                Undo.RecordObject(component, $"Change {tableLabel}");
                if (newIndex == 0)
                {
                    reference.TableReference = default;
                }
                else
                {
                    reference.TableReference = collections[newIndex - 1].TableCollectionName;
                }
                
                // 텍스트면 프리뷰 갱신
                if(isText) UpdatePreviewText(reference.TableReference, reference.TableEntryReference);
            }

            // 2. Key Selection
            if (string.IsNullOrEmpty(reference.TableReference.TableCollectionName))
            {
                // 테이블 미선택 시 키 선택 불가
                return;
            }

            var collection = collections.FirstOrDefault(c => c.TableCollectionName == reference.TableReference.TableCollectionName);
            if (collection == null) return;

            string currentKey = "";
            long currentKeyId = reference.TableEntryReference.KeyId;
            
            if (collection.SharedData != null)
            {
                var entry = collection.SharedData.GetEntry(currentKeyId);
                if (entry != null) currentKey = entry.Key;
                else currentKey = reference.TableEntryReference.Key; 
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(keyLabel);
            
            if (GUILayout.Button(string.IsNullOrEmpty(currentKey) ? "Select Key..." : currentKey, EditorStyles.popup))
            {
                var dropdown = new LocalizationKeyDropdown(new AdvancedDropdownState(), collection);
                dropdown.onKeySelected = (key, id) =>
                {
                    Undo.RecordObject(component, $"Change {keyLabel}");
                    
                    reference.SetReference(collection.TableCollectionName, key);
                    
                    if (isText)
                    {
                        UpdatePreviewText(collection as StringTableCollection, id);
                    }
                    else
                    {
                         // 폰트의 경우 즉시 갱신 시도
                         component.Refresh();
                    }
                    
                    EditorUtility.SetDirty(component);
                    Repaint();
                };
                dropdown.Show(GUILayoutUtility.GetLastRect());
            }
            EditorGUILayout.EndHorizontal();
        }

        private void UpdatePreviewText(StringTableCollection collection, long keyId)
        {
            if (collection == null) return;

            Locale locale = LocalizationSettings.SelectedLocale;
            
            if (locale == null)
            {
                var settings = LocalizationEditorSettings.ActiveLocalizationSettings;
                if (settings != null)
                {
                    var availableLocales = settings.GetAvailableLocales();
                    if (availableLocales != null && availableLocales.Locales.Count > 0)
                    {
                        locale = availableLocales.Locales[0];
                    }
                }
            }
            
            if (locale == null) return;

            var table = collection.GetTable(locale.Identifier) as StringTable;
            if (table != null)
            {
                var entry = table.GetEntry(keyId);
                if (entry != null)
                {
                    var tmpro = component.GetComponent<TextMeshProUGUI>();
                    if (tmpro != null)
                    {
                        // 텍스트가 실제로 변경되었을 때만 적용 및 Dirty 처리
                        if (tmpro.text != entry.LocalizedValue)
                        {
                            Undo.RecordObject(tmpro, "Update Preview Text");
                            tmpro.text = entry.LocalizedValue;
                            EditorUtility.SetDirty(tmpro);
                        }
                    }
                }
            }
        }
        
        private void UpdatePreviewText(TableReference tableRef, TableEntryReference entryRef)
        {
            var collection = LocalizationEditorSettings.GetStringTableCollection(tableRef);
            if (collection != null)
            {
                UpdatePreviewText(collection, entryRef.KeyId);
            }
        }
    }

    public class LocalizationKeyDropdown : AdvancedDropdown
    {
        private LocalizationTableCollection collection;
        public System.Action<string, long> onKeySelected;

        public LocalizationKeyDropdown(AdvancedDropdownState state, LocalizationTableCollection collection) : base(state)
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