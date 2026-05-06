using UnityEngine;
using TMPro;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using System.Collections.Generic;
using System;

namespace Simple.Localize
{
    // LocalizedAsset<T>를 상속받아 폰트 에셋을 다루는 클래스 정의
    [Serializable]
    public class LocalizedFont : LocalizedAsset<TMP_FontAsset> {}

    [ExecuteAlways]
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class SimpleLocalizedText : MonoBehaviour
    {
        [Header("Text Settings")]
        [Tooltip("Select Table and Key for Text")]
        public LocalizedString localizedString = new LocalizedString();

        [Tooltip("Arguments for Smart Strings (e.g. {0}, {name})")]
        public List<string> smartArguments = new List<string>();

        [Header("Font Settings (Optional)")]
        [Tooltip("Select Table and Key for Font Asset")]
        public LocalizedFont localizedFont = new LocalizedFont();

        private TextMeshProUGUI _textMeshPro;

        private void Awake()
        {
            _textMeshPro = GetComponent<TextMeshProUGUI>();
        }

        private void OnEnable()
        {
            if (_textMeshPro == null) _textMeshPro = GetComponent<TextMeshProUGUI>();

            // 1. 텍스트/폰트 이벤트 구독 (둘 중 하나만 변해도 전체 갱신하여 폰트-텍스트 순서 보장)
            localizedString.StringChanged += OnStringChanged;
            localizedFont.AssetChanged += OnFontChanged;

            // 2. 초기화 및 갱신
            Refresh();
        }

        private void OnDisable()
        {
            localizedString.StringChanged -= OnStringChanged;
            localizedFont.AssetChanged -= OnFontChanged;
        }

        private void OnStringChanged(string value) => UpdateText(value);
        private void OnFontChanged(TMP_FontAsset asset) => UpdateFont(asset);



        public void UpdateText()
        {
            Refresh();
        }

        public void Refresh()
        {
            // --- 폰트 갱신 (텍스트보다 먼저 적용하여 깨짐 방지) ---
            // 테이블이나 키가 설정되어 있을 때만 로드 시도
            if (!localizedFont.IsEmpty)
            {
                 var op = localizedFont.LoadAssetAsync();
                 if (op.IsDone)
                 {
                     UpdateFont(op.Result);
                 }
                 else if (Application.isPlaying)
                 {
                     UpdateFont(op.WaitForCompletion());
                 }
            }

            // --- 텍스트 갱신 ---
            // 테이블이나 키가 설정되어 있을 때만 로컬라이즈 시도
            if (!localizedString.IsEmpty)
            {
                if (smartArguments != null && smartArguments.Count > 0)
                {
                    localizedString.Arguments = smartArguments.ToArray();
                }
                else
                {
                    localizedString.Arguments = null;
                }

                if (Application.isPlaying)
                {
                    UpdateText(localizedString.GetLocalizedString());
                }
                else
                {
                    localizedString.RefreshString();
                }
            }


#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                // 에디터 비동기 처리 (텍스트)
                var opText = localizedString.GetLocalizedStringAsync();
                if (opText.IsDone) UpdateText(opText.Result);
                else opText.Completed += (handle) => UpdateText(handle.Result);
            }
#endif
        }

        private void UpdateText(string text)
        {
            if (_textMeshPro != null)
            {
                // 텍스트가 다를 때만 변경
                if (_textMeshPro.text != text)
                {
                    _textMeshPro.text = text;
                    MarkDirtyInEditor();
                }
            }
        }

        private void UpdateFont(TMP_FontAsset font)
        {
            if (_textMeshPro != null && font != null)
            {
                // 폰트가 다를 때만 변경
                if (_textMeshPro.font != font)
                {
                    _textMeshPro.font = font;
                    MarkDirtyInEditor();
                }
            }
        }

        private void MarkDirtyInEditor()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying && _textMeshPro != null)
            {
                UnityEditor.EditorUtility.SetDirty(_textMeshPro);
            }
#endif
        }

        // 런타임에서 인자 변경
        public void SetArgs(params string[] args)
        {
            if (smartArguments == null) smartArguments = new List<string>();
            smartArguments.Clear();
            smartArguments.AddRange(args);
            Refresh();
        }

        // 런타임에서 텍스트 키 변경
        public void SetKey(string tableName, string key)
        {
            localizedString.SetReference(tableName, key);
            Refresh();
        }

        // 런타임에서 폰트 키 변경
        public void SetFontKey(string tableName, string key)
        {
            localizedFont.SetReference(tableName, key);
            Refresh();
        }

        /// <summary>
        /// 텍스트와 폰트 정보를 한 번에 설정하고 갱신합니다. (중복 Refresh 방지)
        /// </summary>
        public void SetFull(string textTable, string textKey, string fontTable, string fontKey, params string[] args)
        {
            localizedFont.SetReference(fontTable, fontKey);
            localizedString.SetReference(textTable, textKey);

            if (args != null && args.Length > 0)
            {
                if (smartArguments == null) smartArguments = new List<string>();
                smartArguments.Clear();
                smartArguments.AddRange(args);
            }

            Refresh();
        }
    }
}
