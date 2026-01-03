using UnityEngine;
using TMPro;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace Simple.Localize
{
    [ExecuteAlways]
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class SimpleLocalizedText : MonoBehaviour
    {
        [Tooltip("Select Table and Key")]
        public LocalizedString localizedString = new LocalizedString();

        private TextMeshProUGUI _textMeshPro;

        private void Awake()
        {
            _textMeshPro = GetComponent<TextMeshProUGUI>();
        }

        private void OnEnable()
        {
            if (_textMeshPro == null) _textMeshPro = GetComponent<TextMeshProUGUI>();

            // 이벤트 구독: 언어가 바뀌거나 키가 바뀌면 이 함수가 호출됨
            localizedString.StringChanged += UpdateText;
            
            // 데이터 갱신 요청
            localizedString.RefreshString(); 
        }

        private void OnDisable()
        {
            localizedString.StringChanged -= UpdateText;
        }

        private void OnValidate()
        {
            if (_textMeshPro == null) _textMeshPro = GetComponent<TextMeshProUGUI>();

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                // 에디터에서 키를 바꿨을 때 즉시 반영 시도
                UnityEditor.EditorApplication.delayCall += () => 
                {
                    if(this == null) return;
                    localizedString.RefreshString();
                };
            }
#endif
        }

        // StringChanged 이벤트 핸들러 (번역된 문자열이 넘어옴)
        private void UpdateText(string text)
        {
            if (_textMeshPro != null)
            {
                _textMeshPro.text = text;
                MarkDirtyInEditor();
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
        
        // 코드로 키 변경 시 사용
        public void SetKey(string tableName, string key)
        {
            localizedString.SetReference(tableName, key);
        }
    }
}