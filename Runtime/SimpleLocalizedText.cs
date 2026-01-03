using UnityEngine;
using TMPro;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;

namespace Simple.Localize
{
    // 에디터에서도 미리보기가 가능하도록 설정
    [ExecuteAlways]
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class SimpleLocalizedText : MonoBehaviour
    {
        [Tooltip("엑셀(테이블)에 정의된 키 값")]
        public string localizationKey;

        [Tooltip("사용할 스트링 테이블 이름")]
        public string tableName = "Localize";

        private TextMeshProUGUI _textMeshPro;

        private void Awake()
        {
            _textMeshPro = GetComponent<TextMeshProUGUI>();
        }

        private void OnEnable()
        {
            // 언어 변경 이벤트 구독
            LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
            
            // 활성화 시 텍스트 갱신
            UpdateText();
        }

        private void OnDisable()
        {
            LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
        }
        
        // 인스펙터에서 값이 바뀌면 바로 갱신 (에디터 미리보기용)
        private void OnValidate()
        {
            // 컴포넌트 추가 직후에는 null일 수 있음
            if (_textMeshPro == null) _textMeshPro = GetComponent<TextMeshProUGUI>();
            
            // 에디터 모드에서도 텍스트 갱신 시도
            // (주의: LocalizationSettings가 초기화되지 않은 상태면 갱신되지 않을 수 있음)
            if (!Application.isPlaying)
            {
                UpdateText();
            }
        }

        private void OnLocaleChanged(Locale locale)
        {
            UpdateText();
        }

        public void UpdateText()
        {
            if (string.IsNullOrEmpty(localizationKey)) return;
            if (_textMeshPro == null) return;

            // 로컬라이제이션 시스템이 준비되지 않았으면 (특히 에디터 모드 초기 로딩 시) 건너뜀
            // 하지만 GetLocalizedStringAsync는 안전하게 동작하도록 설계됨
            
            try
            {
                var op = LocalizationSettings.StringDatabase.GetLocalizedStringAsync(tableName, localizationKey);
            
                if (op.IsDone)
                {
                    _textMeshPro.text = op.Result;
                }
                else
                {
                    op.Completed += (handle) =>
                    {
                        if (_textMeshPro != null)
                            _textMeshPro.text = handle.Result;
                    };
                }
            }
            catch (System.Exception)
            {
                // 에디터 등에서 테이블을 찾지 못하거나 초기화 전일 때 예외 무시
                // (사용자 경험을 해치지 않기 위해 조용히 실패)
            }
        }

        public void SetKey(string key)
        {
            localizationKey = key;
            UpdateText();
        }
    }
}
